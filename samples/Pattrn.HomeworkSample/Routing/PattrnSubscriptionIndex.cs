using System.Collections.Immutable;
using Pattrn.Diagnostics;
using Pattrn.Matching;
using Pattrn.Patterns;
using Pattrn.Registrations;

namespace Homework.Routing;

/// <summary>
/// Adapts Homework subscriptions to immutable Pattrn indexes published as
/// complete snapshots.
/// </summary>
public sealed class PattrnSubscriptionIndex : ISubscriptionIndex
{
    private static readonly PattrnCompileOptions CompileOptions = new()
    {
        DuplicatePatternPolicy = DuplicatePatternPolicy.Allow,
    };

    private readonly object _writeLock = new();
    private readonly Dictionary<MessageTypeId, RegistrationSet> _registrationsByType = [];
    private PattrnSubscriptionSnapshot _snapshot = PattrnSubscriptionSnapshot.Empty;

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
            var candidates = new Dictionary<MessageTypeId, RegistrationSet>();

            foreach (var subscription in additions)
            {
                ArgumentNullException.ThrowIfNull(subscription);
                var registrations = GetCandidate(candidates, subscription.MessageTypeId);
                registrations.Add(subscription);
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
            var candidates = new Dictionary<MessageTypeId, RegistrationSet>();

            foreach (var subscription in removals)
            {
                ArgumentNullException.ThrowIfNull(subscription);
                if (!_registrationsByType.ContainsKey(subscription.MessageTypeId))
                {
                    continue;
                }

                var registrations = GetCandidate(candidates, subscription.MessageTypeId);
                registrations.Remove(subscription);
            }

            RemoveUnchangedCandidates(candidates);
            Publish(candidates);
        }
    }

    public void RemoveSubscriptionsForConsumer(ClientId consumer)
    {
        lock (_writeLock)
        {
            var candidates = new Dictionary<MessageTypeId, RegistrationSet>();

            foreach (var pair in _registrationsByType)
            {
                var registrations = pair.Value.Clone();
                if (registrations.RemoveConsumer(consumer) > 0)
                {
                    candidates.Add(pair.Key, registrations);
                }
            }

            Publish(candidates);
        }
    }

    public IEnumerable<Subscription> FindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent) =>
        Volatile.Read(ref _snapshot).FindSubscriptions(messageTypeId, routingContent);

    internal PattrnSubscriptionSnapshot CaptureSnapshot() => Volatile.Read(ref _snapshot);

    internal bool TryFindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent,
        Span<Subscription> destination,
        out int written) =>
        Volatile.Read(ref _snapshot).TryFindSubscriptions(messageTypeId, routingContent, destination, out written);

    internal int GetMatchCountUpperBound(MessageTypeId messageTypeId, MessageRoutingContent routingContent) =>
        Volatile.Read(ref _snapshot).GetMatchCountUpperBound(messageTypeId, routingContent);

    private RegistrationSet GetCandidate(
        Dictionary<MessageTypeId, RegistrationSet> candidates,
        MessageTypeId messageTypeId)
    {
        if (candidates.TryGetValue(messageTypeId, out var candidate))
        {
            return candidate;
        }

        candidate = _registrationsByType.TryGetValue(messageTypeId, out var current)
            ? current.Clone()
            : new RegistrationSet();
        candidates.Add(messageTypeId, candidate);
        return candidate;
    }

    private void RemoveUnchangedCandidates(Dictionary<MessageTypeId, RegistrationSet> candidates)
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

    private void Publish(Dictionary<MessageTypeId, RegistrationSet> candidates)
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
                continue;
            }

            nextIndexes[pair.Key] = Compile(pair.Value);
        }

        var nextSnapshot = new PattrnSubscriptionSnapshot(nextIndexes.ToImmutable());

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

    private static PattrnIndex<string, Subscription> Compile(RegistrationSet registrations)
    {
        var canonical = new PattrnRegistration<string, Subscription>[registrations.Count];
        for (var index = 0; index < registrations.Count; index++)
        {
            var subscription = registrations[index];
            canonical[index] = PattrnRegistration<string, Subscription>.Create(
                ToPattern(subscription.ContentPattern),
                subscription);
        }

        return PattrnIndex<string, Subscription>.Compile(
            canonical,
            CompileOptions,
            StringComparer.Ordinal,
            EqualityComparer<Subscription>.Default);
    }

    private static PatternSegment<string>[] ToPattern(ContentPattern contentPattern)
    {
        var parts = contentPattern.Parts;
        var pattern = new PatternSegment<string>[parts.Count];
        for (var index = 0; index < parts.Count; index++)
        {
            pattern[index] = parts[index] == "*"
                ? PatternSegment<string>.Wildcard()
                : PatternSegment<string>.Literal(parts[index]);
        }

        return pattern;
    }

    private sealed class RegistrationSet
    {
        private readonly List<Subscription> _ordered;
        private readonly HashSet<Subscription> _membership;

        internal RegistrationSet()
        {
            _ordered = [];
            _membership = [];
        }

        private RegistrationSet(List<Subscription> ordered, HashSet<Subscription> membership)
        {
            _ordered = ordered;
            _membership = membership;
        }

        internal int Count => _ordered.Count;
        internal Subscription this[int index] => _ordered[index];

        internal bool Add(Subscription subscription)
        {
            if (!_membership.Add(subscription))
            {
                return false;
            }

            _ordered.Add(subscription);
            return true;
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

        internal RegistrationSet Clone() =>
            new([.. _ordered], new HashSet<Subscription>(_membership));

        internal bool SetEquals(RegistrationSet other) => _membership.SetEquals(other._membership);
    }
}

internal sealed class PattrnSubscriptionSnapshot
{
    internal static PattrnSubscriptionSnapshot Empty { get; } = new(
        ImmutableDictionary<MessageTypeId, PattrnIndex<string, Subscription>>.Empty);

    internal PattrnSubscriptionSnapshot(
        ImmutableDictionary<MessageTypeId, PattrnIndex<string, Subscription>> indexes)
    {
        Indexes = indexes;
    }

    internal ImmutableDictionary<MessageTypeId, PattrnIndex<string, Subscription>> Indexes { get; }

    internal Subscription[] FindSubscriptions(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent)
    {
        if (!Indexes.TryGetValue(messageTypeId, out var index))
        {
            return [];
        }

        var path = GetPath(routingContent);
        return index.EnumeratePrefixValuesToArray(path);
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

        var path = GetPath(routingContent);
        return index.TryEnumeratePrefixValues(path, destination, out written);
    }

    internal int GetMatchCountUpperBound(
        MessageTypeId messageTypeId,
        MessageRoutingContent routingContent)
    {
        if (!Indexes.TryGetValue(messageTypeId, out var index))
        {
            return 0;
        }

        return index.GetEnumeratePrefixMatchCountUpperBound(GetPath(routingContent));
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
