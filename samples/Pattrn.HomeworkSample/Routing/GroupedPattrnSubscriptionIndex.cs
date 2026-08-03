using System.Buffers;
using System.Collections.Immutable;
using Pattrn.Diagnostics;
using Pattrn.Matching;
using Pattrn.Patterns;
using Pattrn.Registrations;

namespace Homework.Routing;

/// <summary>
/// Experimental grouped adapter that compiles one matcher registration per
/// structural content pattern and stores immutable subscription buckets.
/// </summary>
public sealed class GroupedPattrnSubscriptionIndex : ISubscriptionIndex
{
    private static readonly PattrnCompileOptions CompileOptions = new()
    {
        DuplicatePatternPolicy = DuplicatePatternPolicy.Allow,
    };

    private readonly object _writeLock = new();
    private readonly GroupedMatcherMode _matcherMode;
    private readonly Dictionary<MessageTypeId, PatternBucketSet> _registrationsByType = [];
    private GroupedSubscriptionSnapshot _snapshot = GroupedSubscriptionSnapshot.Empty;

    public GroupedPattrnSubscriptionIndex()
        : this(GroupedMatcherMode.General)
    {
    }

    internal GroupedPattrnSubscriptionIndex(GroupedMatcherMode matcherMode)
    {
        if (!Enum.IsDefined(matcherMode))
        {
            throw new ArgumentOutOfRangeException(nameof(matcherMode));
        }

        _matcherMode = matcherMode;
    }

    public void AddSubscriptions(IEnumerable<Subscription> subscriptions)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);
        var additions = subscriptions.ToArray();
        if (additions.Length == 0)
        {
            return;
        }

        lock (_writeLock)
        {
            var candidates = new Dictionary<MessageTypeId, PatternBucketSet>();
            foreach (var subscription in additions)
            {
                ArgumentNullException.ThrowIfNull(subscription);
                GetCandidate(candidates, subscription.MessageTypeId).Add(subscription);
            }

            RemoveUnchangedCandidates(candidates);
            Publish(candidates);
        }
    }

    public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
    {
        ArgumentNullException.ThrowIfNull(subscriptions);
        var removals = subscriptions.ToArray();
        if (removals.Length == 0)
        {
            return;
        }

        lock (_writeLock)
        {
            var candidates = new Dictionary<MessageTypeId, PatternBucketSet>();
            foreach (var subscription in removals)
            {
                ArgumentNullException.ThrowIfNull(subscription);
                if (!_registrationsByType.ContainsKey(subscription.MessageTypeId))
                {
                    continue;
                }

                GetCandidate(candidates, subscription.MessageTypeId).Remove(subscription);
            }

            RemoveUnchangedCandidates(candidates);
            Publish(candidates);
        }
    }

    public void RemoveSubscriptionsForConsumer(ClientId consumer)
    {
        lock (_writeLock)
        {
            var candidates = new Dictionary<MessageTypeId, PatternBucketSet>();
            foreach (var pair in _registrationsByType)
            {
                var candidate = pair.Value.Clone();
                if (candidate.RemoveConsumer(consumer) > 0)
                {
                    candidates.Add(pair.Key, candidate);
                }
            }

            Publish(candidates);
        }
    }

    public IEnumerable<Subscription> FindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent) =>
        Volatile.Read(ref _snapshot).FindSubscriptions(messageTypeId, routingContent);

    internal GroupedSubscriptionSnapshot CaptureSnapshot() => Volatile.Read(ref _snapshot);

    internal bool TryFindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent,
        Span<Subscription> destination,
        out int written) =>
        Volatile.Read(ref _snapshot).TryFindSubscriptions(messageTypeId, routingContent, destination, out written);

    internal int GetMatchCountUpperBound(MessageTypeId messageTypeId, MessageRoutingContent routingContent) =>
        Volatile.Read(ref _snapshot).GetMatchCountUpperBound(messageTypeId, routingContent);

    internal int StructuralPatternCount => Volatile.Read(ref _snapshot).StructuralPatternCount;
    internal int LogicalSubscriptionCount => Volatile.Read(ref _snapshot).LogicalSubscriptionCount;

    private PatternBucketSet GetCandidate(
        Dictionary<MessageTypeId, PatternBucketSet> candidates,
        MessageTypeId messageTypeId)
    {
        if (candidates.TryGetValue(messageTypeId, out var candidate))
        {
            return candidate;
        }

        candidate = _registrationsByType.TryGetValue(messageTypeId, out var current)
            ? current.Clone()
            : new PatternBucketSet();
        candidates.Add(messageTypeId, candidate);
        return candidate;
    }

    private void RemoveUnchangedCandidates(Dictionary<MessageTypeId, PatternBucketSet> candidates)
    {
        foreach (var messageTypeId in candidates.Keys.ToArray())
        {
            if (_registrationsByType.TryGetValue(messageTypeId, out var current)
                ? current.SetEquals(candidates[messageTypeId])
                : candidates[messageTypeId].Count == 0)
            {
                candidates.Remove(messageTypeId);
            }
        }
    }

    private void Publish(Dictionary<MessageTypeId, PatternBucketSet> candidates)
    {
        if (candidates.Count == 0)
        {
            return;
        }

        var currentSnapshot = Volatile.Read(ref _snapshot);
        var nextIndexes = currentSnapshot.Indexes.ToBuilder();
        foreach (var pair in candidates)
        {
            if (pair.Value.Count == 0)
            {
                nextIndexes.Remove(pair.Key);
            }
            else
            {
                nextIndexes[pair.Key] = GroupedCompiledIndex.Compile(pair.Value, _matcherMode);
            }
        }

        var nextSnapshot = new GroupedSubscriptionSnapshot(nextIndexes.ToImmutable());
        foreach (var pair in candidates)
        {
            if (pair.Value.Count == 0)
            {
                _registrationsByType.Remove(pair.Key);
            }
            else
            {
                _registrationsByType[pair.Key] = pair.Value;
            }
        }

        Volatile.Write(ref _snapshot, nextSnapshot);
    }

    internal enum GroupedMatcherMode
    {
        General,
        Lean,
    }

    internal sealed class PatternBucketSet
    {
        private readonly Dictionary<ContentPatternKey, SubscriptionBucket> _buckets;

        internal PatternBucketSet()
        {
            _buckets = new(ContentPatternKeyComparer.Instance);
        }

        private PatternBucketSet(Dictionary<ContentPatternKey, SubscriptionBucket> buckets)
        {
            _buckets = buckets;
        }

        internal int Count => _buckets.Count;
        internal int LogicalSubscriptionCount => _buckets.Values.Sum(bucket => bucket.Count);
        internal IEnumerable<SubscriptionBucket> Buckets => _buckets.Values;

        internal void Add(Subscription subscription)
        {
            var key = ContentPatternKey.Create(subscription.ContentPattern);
            if (!_buckets.TryGetValue(key, out var bucket))
            {
                bucket = new SubscriptionBucket(key);
                _buckets.Add(key, bucket);
            }

            bucket.Add(subscription);
        }

        internal void Remove(Subscription subscription)
        {
            var key = ContentPatternKey.Create(subscription.ContentPattern);
            if (!_buckets.TryGetValue(key, out var bucket) || !bucket.Remove(subscription))
            {
                return;
            }

            if (bucket.Count == 0)
            {
                _buckets.Remove(key);
            }
        }

        internal int RemoveConsumer(ClientId consumer)
        {
            var removed = 0;
            foreach (var key in _buckets.Keys.ToArray())
            {
                var bucket = _buckets[key];
                removed += bucket.RemoveConsumer(consumer);
                if (bucket.Count == 0)
                {
                    _buckets.Remove(key);
                }
            }

            return removed;
        }

        internal PatternBucketSet Clone()
        {
            var buckets = new Dictionary<ContentPatternKey, SubscriptionBucket>(ContentPatternKeyComparer.Instance);
            foreach (var pair in _buckets)
            {
                buckets.Add(pair.Key, pair.Value.Clone());
            }

            return new PatternBucketSet(buckets);
        }

        internal bool SetEquals(PatternBucketSet other)
        {
            if (_buckets.Count != other._buckets.Count)
            {
                return false;
            }

            foreach (var pair in _buckets)
            {
                if (!other._buckets.TryGetValue(pair.Key, out var otherBucket)
                    || !pair.Value.SetEquals(otherBucket))
                {
                    return false;
                }
            }

            return true;
        }
    }

    internal sealed class SubscriptionBucket
    {
        private readonly List<Subscription> _ordered;
        private readonly HashSet<Subscription> _membership;

        internal SubscriptionBucket(ContentPatternKey pattern)
        {
            Pattern = pattern;
            _ordered = [];
            _membership = [];
        }

        private SubscriptionBucket(
            ContentPatternKey pattern,
            List<Subscription> ordered,
            HashSet<Subscription> membership)
        {
            Pattern = pattern;
            _ordered = ordered;
            _membership = membership;
        }

        internal ContentPatternKey Pattern { get; }
        internal int Count => _ordered.Count;
        internal Subscription this[int index] => _ordered[index];

        internal void Add(Subscription subscription)
        {
            if (_membership.Add(subscription))
            {
                _ordered.Add(subscription);
            }
        }

        internal bool Remove(Subscription subscription)
        {
            if (!_membership.Remove(subscription))
            {
                return false;
            }

            _ordered.Remove(subscription);
            return true;
        }

        internal int RemoveConsumer(ClientId consumer)
        {
            var removed = _ordered.RemoveAll(subscription => subscription.ConsumerId.Equals(consumer));
            if (removed > 0)
            {
                _membership.RemoveWhere(subscription => subscription.ConsumerId.Equals(consumer));
            }

            return removed;
        }

        internal SubscriptionBucket Clone() =>
            new(Pattern, [.. _ordered], new HashSet<Subscription>(_membership));

        internal bool SetEquals(SubscriptionBucket other) => _membership.SetEquals(other._membership);
        internal ImmutableArray<Subscription> ToImmutableArray() => [.. _ordered];
    }

    internal readonly struct ContentPatternKey
    {
        private ContentPatternKey(ImmutableArray<string> parts)
        {
            Parts = parts;
        }

        internal ImmutableArray<string> Parts { get; }

        internal static ContentPatternKey Create(ContentPattern pattern)
        {
            var parts = pattern.Parts;
            var copy = ImmutableArray.CreateBuilder<string>(parts.Count);
            for (var index = 0; index < parts.Count; index++)
            {
                copy.Add(parts[index]);
            }

            return new ContentPatternKey(copy.MoveToImmutable());
        }
    }

    private sealed class ContentPatternKeyComparer : IEqualityComparer<ContentPatternKey>
    {
        internal static ContentPatternKeyComparer Instance { get; } = new();

        public bool Equals(ContentPatternKey left, ContentPatternKey right)
        {
            if (left.Parts.Length != right.Parts.Length)
            {
                return false;
            }

            for (var index = 0; index < left.Parts.Length; index++)
            {
                if (!StringComparer.Ordinal.Equals(left.Parts[index], right.Parts[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(ContentPatternKey key)
        {
            var hash = new HashCode();
            hash.Add(key.Parts.Length);
            foreach (var part in key.Parts)
            {
                hash.Add(part, StringComparer.Ordinal);
            }

            return hash.ToHashCode();
        }
    }
}

internal sealed class GroupedSubscriptionSnapshot
{
    internal static GroupedSubscriptionSnapshot Empty { get; } = new(
        ImmutableDictionary<MessageTypeId, GroupedCompiledIndex>.Empty);

    internal GroupedSubscriptionSnapshot(
        ImmutableDictionary<MessageTypeId, GroupedCompiledIndex> indexes)
    {
        Indexes = indexes;
        StructuralPatternCount = indexes.Values.Sum(index => index.StructuralPatternCount);
        LogicalSubscriptionCount = indexes.Values.Sum(index => index.LogicalSubscriptionCount);
    }

    internal ImmutableDictionary<MessageTypeId, GroupedCompiledIndex> Indexes { get; }
    internal int StructuralPatternCount { get; }
    internal int LogicalSubscriptionCount { get; }

    internal Subscription[] FindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent)
    {
        if (!Indexes.TryGetValue(messageTypeId, out var index))
        {
            return [];
        }

        return index.Find(GetPath(routingContent));
    }

    internal bool TryFindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent,
        Span<Subscription> destination,
        out int written)
    {
        if (!Indexes.TryGetValue(messageTypeId, out var index))
        {
            written = 0;
            return true;
        }

        return index.TryFind(GetPath(routingContent), destination, out written);
    }

    internal int GetMatchCountUpperBound(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent)
    {
        return Indexes.TryGetValue(messageTypeId, out var index)
            ? index.GetMatchCountUpperBound(GetPath(routingContent))
            : 0;
    }

    private static ReadOnlySpan<string> GetPath(MessageRoutingContent routingContent)
    {
        var parts = routingContent.Parts;
        if (parts is null || parts.Count == 0)
        {
            return ReadOnlySpan<string>.Empty;
        }

        if (parts is string[] array)
        {
            return array;
        }

        var copy = new string[parts.Count];
        for (var index = 0; index < parts.Count; index++)
        {
            copy[index] = parts[index];
        }

        return copy;
    }
}

internal interface IGroupedCompiledIndex
{
    int StructuralPatternCount { get; }
    int LogicalSubscriptionCount { get; }
    Subscription[] Find(ReadOnlySpan<string> path);
    bool TryFind(ReadOnlySpan<string> path, Span<Subscription> destination, out int written);
    int GetMatchCountUpperBound(ReadOnlySpan<string> path);
}

internal sealed class GroupedCompiledIndex : IGroupedCompiledIndex
{
    private readonly IGroupedCompiledIndex _inner;

    private GroupedCompiledIndex(IGroupedCompiledIndex inner)
    {
        _inner = inner;
    }

    internal static GroupedCompiledIndex Compile(
        GroupedPattrnSubscriptionIndex.PatternBucketSet patternBucketSet,
        GroupedPattrnSubscriptionIndex.GroupedMatcherMode matcherMode)
    {
        ArgumentNullException.ThrowIfNull(patternBucketSet);
        return new(matcherMode switch
        {
            GroupedPattrnSubscriptionIndex.GroupedMatcherMode.General =>
                GeneralGroupedCompiledIndex.Compile(patternBucketSet),
            GroupedPattrnSubscriptionIndex.GroupedMatcherMode.Lean =>
                LeanGroupedCompiledIndex.Compile(patternBucketSet),
            _ => throw new ArgumentOutOfRangeException(nameof(matcherMode)),
        });
    }

    public int StructuralPatternCount => _inner.StructuralPatternCount;
    public int LogicalSubscriptionCount => _inner.LogicalSubscriptionCount;
    public Subscription[] Find(ReadOnlySpan<string> path) => _inner.Find(path);
    public bool TryFind(ReadOnlySpan<string> path, Span<Subscription> destination, out int written) => _inner.TryFind(path, destination, out written);
    public int GetMatchCountUpperBound(ReadOnlySpan<string> path) => _inner.GetMatchCountUpperBound(path);
}

internal sealed class GeneralGroupedCompiledIndex : IGroupedCompiledIndex
{
    private static readonly PattrnCompileOptions CompileOptions = new()
    {
        DuplicatePatternPolicy = DuplicatePatternPolicy.Allow,
    };

    private readonly PattrnIndex<string, GroupedPattrnSubscriptionIndex.SubscriptionBucket> _index;
    private readonly int _logicalSubscriptionCount;

    private GeneralGroupedCompiledIndex(
        PattrnIndex<string, GroupedPattrnSubscriptionIndex.SubscriptionBucket> index,
        int logicalSubscriptionCount)
    {
        _index = index;
        _logicalSubscriptionCount = logicalSubscriptionCount;
    }

    internal static GeneralGroupedCompiledIndex Compile(
        GroupedPattrnSubscriptionIndex.PatternBucketSet patternBucketSet)
    {
        var buckets = patternBucketSet.Buckets.ToArray();
        var registrations = new PattrnRegistration<string, GroupedPattrnSubscriptionIndex.SubscriptionBucket>[buckets.Length];
        for (var index = 0; index < buckets.Length; index++)
        {
            registrations[index] = PattrnRegistration<string, GroupedPattrnSubscriptionIndex.SubscriptionBucket>.Create(
                ToPattern(buckets[index].Pattern),
                buckets[index]);
        }

        return new(
            PattrnIndex<string, GroupedPattrnSubscriptionIndex.SubscriptionBucket>.Compile(
                registrations,
                CompileOptions,
                StringComparer.Ordinal,
                EqualityComparer<GroupedPattrnSubscriptionIndex.SubscriptionBucket>.Default),
            patternBucketSet.LogicalSubscriptionCount);
    }

    public int StructuralPatternCount => _index.PatternCount;
    public int LogicalSubscriptionCount => _logicalSubscriptionCount;

    public Subscription[] Find(ReadOnlySpan<string> path)
    {
        var bucketCapacity = _index.GetEnumeratePrefixMatchCountUpperBound(path);
        if (bucketCapacity == 0)
        {
            return [];
        }

        var buckets = new GroupedPattrnSubscriptionIndex.SubscriptionBucket[bucketCapacity];
        if (!_index.TryEnumeratePrefixValues(path, buckets, out var bucketCount))
        {
            throw new InvalidOperationException("The grouped general matcher returned an insufficient bucket buffer.");
        }

        var subscriptionCount = GetSubscriptionCount(buckets.AsSpan(0, bucketCount));
        var result = new Subscription[subscriptionCount];
        Flatten(buckets.AsSpan(0, bucketCount), result);
        return result;
    }

    public bool TryFind(ReadOnlySpan<string> path, Span<Subscription> destination, out int written)
    {
        var bucketCapacity = _index.GetEnumeratePrefixMatchCountUpperBound(path);
        if (bucketCapacity == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<GroupedPattrnSubscriptionIndex.SubscriptionBucket>.Shared.Rent(bucketCapacity);
        try
        {
            if (!_index.TryEnumeratePrefixValues(path, rented, out var bucketCount))
            {
                written = 0;
                return false;
            }

            var subscriptions = rented.AsSpan(0, bucketCount);
            var required = GetSubscriptionCount(subscriptions);
            if (destination.Length < required)
            {
                written = required;
                return false;
            }

            Flatten(subscriptions, destination);
            written = required;
            return true;
        }
        finally
        {
            ArrayPool<GroupedPattrnSubscriptionIndex.SubscriptionBucket>.Shared.Return(rented, clearArray: true);
        }
    }

    public int GetMatchCountUpperBound(ReadOnlySpan<string> path)
    {
        var bucketCapacity = _index.GetEnumeratePrefixMatchCountUpperBound(path);
        if (bucketCapacity == 0)
        {
            return 0;
        }

        var buckets = new GroupedPattrnSubscriptionIndex.SubscriptionBucket[bucketCapacity];
        if (!_index.TryEnumeratePrefixValues(path, buckets, out var bucketCount))
        {
            return _logicalSubscriptionCount;
        }

        return GetSubscriptionCount(buckets.AsSpan(0, bucketCount));
    }

    private static int GetSubscriptionCount(
        ReadOnlySpan<GroupedPattrnSubscriptionIndex.SubscriptionBucket> buckets)
    {
        var count = 0;
        for (var index = 0; index < buckets.Length; index++)
        {
            count += buckets[index].Count;
        }

        return count;
    }

    private static void Flatten(
        ReadOnlySpan<GroupedPattrnSubscriptionIndex.SubscriptionBucket> buckets,
        Span<Subscription> destination)
    {
        var offset = 0;
        for (var bucketIndex = 0; bucketIndex < buckets.Length; bucketIndex++)
        {
            var bucket = buckets[bucketIndex];
            for (var subscriptionIndex = 0; subscriptionIndex < bucket.Count; subscriptionIndex++)
            {
                destination[offset++] = bucket[subscriptionIndex];
            }
        }
    }

    private static PatternSegment<string>[] ToPattern(
        GroupedPattrnSubscriptionIndex.ContentPatternKey key)
    {
        var pattern = new PatternSegment<string>[key.Parts.Length];
        for (var index = 0; index < key.Parts.Length; index++)
        {
            pattern[index] = key.Parts[index] == "*"
                ? PatternSegment<string>.Wildcard()
                : PatternSegment<string>.Literal(key.Parts[index]);
        }

        return pattern;
    }
}
