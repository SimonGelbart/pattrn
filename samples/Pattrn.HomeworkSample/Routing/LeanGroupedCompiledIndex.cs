using System.Buffers;
using System.Collections.Immutable;

namespace Homework.Routing;

/// <summary>Minimal immutable literal/wildcard prefix matcher for the grouped experiment.</summary>
internal sealed class LeanGroupedCompiledIndex : IGroupedCompiledIndex
{
    private const int ChildLookupThreshold = 8;
    private const int NoIndex = -1;

    private readonly LeanNode[] _nodes;
    private readonly LeanChild[] _children;
    private readonly int[] _lookupSlots;
    private readonly ImmutableArray<Subscription>[] _buckets;
    private readonly int _maxFrontierWidth;
    private readonly int _logicalSubscriptionCount;

    private LeanGroupedCompiledIndex(
        LeanNode[] nodes,
        LeanChild[] children,
        int[] lookupSlots,
        ImmutableArray<Subscription>[] buckets,
        int maxFrontierWidth,
        int logicalSubscriptionCount)
    {
        _nodes = nodes;
        _children = children;
        _lookupSlots = lookupSlots;
        _buckets = buckets;
        _maxFrontierWidth = maxFrontierWidth;
        _logicalSubscriptionCount = logicalSubscriptionCount;
    }

    internal static LeanGroupedCompiledIndex Compile(
        GroupedPattrnSubscriptionIndex.PatternBucketSet patternBucketSet)
    {
        var root = new BuildNode();
        foreach (var bucket in patternBucketSet.Buckets)
        {
            var node = root;
            foreach (var part in bucket.Pattern.Parts)
            {
                if (part == "*")
                {
                    node.Wildcard ??= new BuildNode();
                    node = node.Wildcard;
                }
                else
                {
                    if (!node.Exact.TryGetValue(part, out var child))
                    {
                        child = new BuildNode();
                        node.Exact.Add(part, child);
                    }

                    node = child;
                }
            }

            node.Bucket = bucket;
        }

        var builder = new Builder(patternBucketSet.LogicalSubscriptionCount);
        var rootIndex = builder.AddNode(root, depth: 0);
        if (rootIndex != 0)
        {
            throw new InvalidOperationException("The lean matcher root must be node zero.");
        }

        return builder.ToIndex();
    }

    public int StructuralPatternCount => _buckets.Length;
    public int LogicalSubscriptionCount => _logicalSubscriptionCount;

    public Subscription[] Find(ReadOnlySpan<string> path)
    {
        var capacity = GetMatchCountUpperBound(path);
        if (capacity == 0)
        {
            return [];
        }

        var result = new Subscription[capacity];
        if (!TryFind(path, result, out var written))
        {
            throw new InvalidOperationException("The lean matcher returned an insufficient owning buffer.");
        }

        return written == result.Length ? result : result[..written];
    }

    public bool TryFind(ReadOnlySpan<string> path, Span<Subscription> destination, out int written)
    {
        if (_buckets.Length == 0)
        {
            written = 0;
            return true;
        }

        var frontier = ArrayPool<int>.Shared.Rent(Math.Max(1, _maxFrontierWidth));
        var nextFrontier = ArrayPool<int>.Shared.Rent(Math.Max(1, _maxFrontierWidth));
        var matchingBuckets = ArrayPool<int>.Shared.Rent(_buckets.Length);
        try
        {
            Collect(path, frontier, nextFrontier, matchingBuckets, out var bucketCount, out var required);
            if (destination.Length < required)
            {
                written = required;
                return false;
            }

            Flatten(matchingBuckets.AsSpan(0, bucketCount), destination);
            written = required;
            return true;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(frontier);
            ArrayPool<int>.Shared.Return(nextFrontier);
            ArrayPool<int>.Shared.Return(matchingBuckets);
        }
    }

    public int GetMatchCountUpperBound(ReadOnlySpan<string> path)
    {
        if (_buckets.Length == 0)
        {
            return 0;
        }

        var frontier = ArrayPool<int>.Shared.Rent(Math.Max(1, _maxFrontierWidth));
        var nextFrontier = ArrayPool<int>.Shared.Rent(Math.Max(1, _maxFrontierWidth));
        var matchingBuckets = ArrayPool<int>.Shared.Rent(_buckets.Length);
        try
        {
            Collect(path, frontier, nextFrontier, matchingBuckets, out _, out var required);
            return required;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(frontier);
            ArrayPool<int>.Shared.Return(nextFrontier);
            ArrayPool<int>.Shared.Return(matchingBuckets);
        }
    }

    private void Collect(
        ReadOnlySpan<string> path,
        int[] frontier,
        int[] nextFrontier,
        int[] matchingBuckets,
        out int bucketCount,
        out int subscriptionCount)
    {
        frontier[0] = 0;
        var frontierCount = 1;
        bucketCount = 0;
        subscriptionCount = 0;
        AddBucket(_nodes[0].BucketIndex, matchingBuckets, ref bucketCount, ref subscriptionCount);

        for (var depth = 0; depth < path.Length && frontierCount > 0; depth++)
        {
            var nextCount = 0;
            for (var frontierIndex = 0; frontierIndex < frontierCount; frontierIndex++)
            {
                var node = _nodes[frontier[frontierIndex]];
                var exactChild = FindExactChild(node, path[depth]);
                if (exactChild != NoIndex)
                {
                    nextFrontier[nextCount++] = exactChild;
                }

                if (node.WildcardChild != NoIndex)
                {
                    nextFrontier[nextCount++] = node.WildcardChild;
                }
            }

            (frontier, nextFrontier) = (nextFrontier, frontier);
            frontierCount = nextCount;
            for (var frontierIndex = 0; frontierIndex < frontierCount; frontierIndex++)
            {
                AddBucket(
                    _nodes[frontier[frontierIndex]].BucketIndex,
                    matchingBuckets,
                    ref bucketCount,
                    ref subscriptionCount);
            }
        }
    }

    private void AddBucket(
        int bucketIndex,
        int[] matchingBuckets,
        ref int bucketCount,
        ref int subscriptionCount)
    {
        if (bucketIndex == NoIndex)
        {
            return;
        }

        matchingBuckets[bucketCount++] = bucketIndex;
        subscriptionCount += _buckets[bucketIndex].Length;
    }

    private int FindExactChild(LeanNode node, string segment)
    {
        if (node.ChildCount == 0)
        {
            return NoIndex;
        }

        if (node.FirstLookupSlot != NoIndex)
        {
            var slot = node.FirstLookupSlot + (StringComparer.Ordinal.GetHashCode(segment) & node.LookupMask);
            while (true)
            {
                var childIndex = _lookupSlots[slot];
                if (childIndex == NoIndex)
                {
                    return NoIndex;
                }

                if (StringComparer.Ordinal.Equals(_children[childIndex].Segment, segment))
                {
                    return _children[childIndex].NodeIndex;
                }

                slot = node.FirstLookupSlot + ((slot - node.FirstLookupSlot + 1) & node.LookupMask);
            }
        }

        for (var index = node.FirstChild; index < node.FirstChild + node.ChildCount; index++)
        {
            if (StringComparer.Ordinal.Equals(_children[index].Segment, segment))
            {
                return _children[index].NodeIndex;
            }
        }

        return NoIndex;
    }

    private void Flatten(ReadOnlySpan<int> matchingBuckets, Span<Subscription> destination)
    {
        var offset = 0;
        for (var bucketIndex = 0; bucketIndex < matchingBuckets.Length; bucketIndex++)
        {
            foreach (var subscription in _buckets[matchingBuckets[bucketIndex]])
            {
                destination[offset++] = subscription;
            }
        }
    }

    private sealed class BuildNode
    {
        internal Dictionary<string, BuildNode> Exact { get; } = new(StringComparer.Ordinal);
        internal BuildNode? Wildcard { get; set; }
        internal GroupedPattrnSubscriptionIndex.SubscriptionBucket? Bucket { get; set; }
    }

    private sealed class Builder
    {
        private readonly List<LeanNode> _nodes = [];
        private readonly List<LeanChild> _children = [];
        private readonly List<int> _lookupSlots = [];
        private readonly List<ImmutableArray<Subscription>> _buckets = [];
        private readonly List<int> _depthCounts = [];
        private readonly int _logicalSubscriptionCount;

        internal Builder(int logicalSubscriptionCount)
        {
            _logicalSubscriptionCount = logicalSubscriptionCount;
        }

        internal int AddNode(BuildNode source, int depth)
        {
            var nodeIndex = _nodes.Count;
            _nodes.Add(default);
            while (_depthCounts.Count <= depth)
            {
                _depthCounts.Add(0);
            }

            _depthCounts[depth]++;
            var firstChild = _children.Count;
            var exact = source.Exact.ToArray();
            Array.Sort(exact, static (left, right) => StringComparer.Ordinal.Compare(left.Key, right.Key));
            for (var exactIndex = 0; exactIndex < exact.Length; exactIndex++)
            {
                _children.Add(new LeanChild(exact[exactIndex].Key, NoIndex));
            }

            for (var exactIndex = 0; exactIndex < exact.Length; exactIndex++)
            {
                var childNodeIndex = AddNode(exact[exactIndex].Value, depth + 1);
                _children[firstChild + exactIndex] = new LeanChild(
                    exact[exactIndex].Key,
                    childNodeIndex);
            }

            var wildcardChild = NoIndex;
            if (source.Wildcard is not null)
            {
                wildcardChild = _nodes.Count;
                AddNode(source.Wildcard, depth + 1);
            }

            var bucketIndex = NoIndex;
            if (source.Bucket is not null)
            {
                bucketIndex = _buckets.Count;
                _buckets.Add(source.Bucket.ToImmutableArray());
            }

            var childCount = exact.Length;
            var firstLookupSlot = NoIndex;
            var lookupMask = 0;
            if (childCount >= ChildLookupThreshold)
            {
                (firstLookupSlot, lookupMask) = AddLookup(firstChild, childCount);
            }

            _nodes[nodeIndex] = new LeanNode(
                firstChild,
                childCount,
                wildcardChild,
                bucketIndex,
                firstLookupSlot,
                lookupMask);
            return nodeIndex;
        }

        internal LeanGroupedCompiledIndex ToIndex() => new(
            [.. _nodes],
            [.. _children],
            [.. _lookupSlots],
            [.. _buckets],
            _depthCounts.Count == 0 ? 1 : _depthCounts.Max(),
            _logicalSubscriptionCount);

        private (int FirstSlot, int Mask) AddLookup(int firstChild, int childCount)
        {
            var tableLength = 1;
            while (tableLength < childCount * 2)
            {
                tableLength <<= 1;
            }

            var firstSlot = _lookupSlots.Count;
            var mask = tableLength - 1;
            for (var index = 0; index < tableLength; index++)
            {
                _lookupSlots.Add(NoIndex);
            }

            for (var childIndex = firstChild; childIndex < firstChild + childCount; childIndex++)
            {
                var slot = firstSlot + (StringComparer.Ordinal.GetHashCode(_children[childIndex].Segment) & mask);
                while (_lookupSlots[slot] != NoIndex)
                {
                    slot = firstSlot + ((slot - firstSlot + 1) & mask);
                }

                _lookupSlots[slot] = childIndex;
            }

            return (firstSlot, mask);
        }
    }

    private readonly struct LeanNode
    {
        internal LeanNode(
            int firstChild,
            int childCount,
            int wildcardChild,
            int bucketIndex,
            int firstLookupSlot,
            int lookupMask)
        {
            FirstChild = firstChild;
            ChildCount = childCount;
            WildcardChild = wildcardChild;
            BucketIndex = bucketIndex;
            FirstLookupSlot = firstLookupSlot;
            LookupMask = lookupMask;
        }

        internal int FirstChild { get; }
        internal int ChildCount { get; }
        internal int WildcardChild { get; }
        internal int BucketIndex { get; }
        internal int FirstLookupSlot { get; }
        internal int LookupMask { get; }
    }

    private readonly struct LeanChild
    {
        internal LeanChild(string segment, int nodeIndex)
        {
            Segment = segment;
            NodeIndex = nodeIndex;
        }

        internal string Segment { get; }
        internal int NodeIndex { get; }
    }
}
