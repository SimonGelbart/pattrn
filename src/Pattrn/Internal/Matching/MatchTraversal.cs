using System.Buffers;

namespace Pattrn.Internal.Matching;

/// <summary>Allocation-free traversal over immutable compiled storage.</summary>
internal ref struct MatchTraversal<TSegment, TValue>
    where TSegment : notnull
{
    private readonly CompiledIndex<TSegment, TValue> _index;
    private readonly IEqualityComparer<TSegment> _segmentComparer;

    internal MatchTraversal(
        CompiledIndex<TSegment, TValue> index,
        IEqualityComparer<TSegment> segmentComparer)
    {
        _index = index;
        _segmentComparer = segmentComparer;
    }

    internal List<MatchCandidate<TValue>> CollectCandidates(
        ReadOnlySpan<TSegment> path,
        bool prefix)
    {
        var candidates = new List<MatchCandidate<TValue>>();
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    AddCandidates(frame.NodeIndex, frame.Depth, path.Length, candidates);
                    ref readonly var node = ref _index.Nodes[frame.NodeIndex];
                    if (node.CatchAllChild != CompiledNode.NoNode)
                    {
                        AddCandidates(node.CatchAllChild, frame.Depth, path.Length, candidates);
                    }

                    continue;
                }

                if (prefix)
                {
                    AddCandidates(frame.NodeIndex, frame.Depth, path.Length, candidates);
                }

                ref readonly var currentNode = ref _index.Nodes[frame.NodeIndex];
                if (currentNode.CatchAllChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(currentNode.CatchAllChild, path.Length));
                }

                if (currentNode.WildcardChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(currentNode.WildcardChild, frame.Depth + 1));
                }

                if (TryGetExactChild(frame.NodeIndex, path[frame.Depth], out var exactChildNodeIndex))
                {
                    stack.Push(new TraversalFrame(exactChildNodeIndex, frame.Depth + 1));
                }
            }
        }
        finally
        {
            stack.Dispose();
        }

        candidates.Sort((left, right) => CompareCandidates(left, right, prefix));
        return candidates;
    }

    internal int CollectCandidatesInto(
        ReadOnlySpan<TSegment> path,
        bool prefix,
        Span<MatchCandidate<TValue>> destination)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    count = AddCandidates(frame.NodeIndex, frame.Depth, path.Length, destination, count);
                    ref readonly var node = ref _index.Nodes[frame.NodeIndex];
                    if (node.CatchAllChild != CompiledNode.NoNode)
                    {
                        count = AddCandidates(node.CatchAllChild, frame.Depth, path.Length, destination, count);
                    }

                    continue;
                }

                if (prefix)
                {
                    count = AddCandidates(frame.NodeIndex, frame.Depth, path.Length, destination, count);
                }

                ref readonly var currentNode = ref _index.Nodes[frame.NodeIndex];
                if (currentNode.CatchAllChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(currentNode.CatchAllChild, path.Length));
                }

                if (currentNode.WildcardChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(currentNode.WildcardChild, frame.Depth + 1));
                }

                if (TryGetExactChild(frame.NodeIndex, path[frame.Depth], out var exactChildNodeIndex))
                {
                    stack.Push(new TraversalFrame(exactChildNodeIndex, frame.Depth + 1));
                }
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    internal int Count(ReadOnlySpan<TSegment> path, bool prefix)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    count += GetValueCountIncludingTerminalCatchAll(frame.NodeIndex);
                    continue;
                }

                if (prefix)
                {
                    count += GetValues(frame.NodeIndex).Length;
                }

                ref readonly var node = ref _index.Nodes[frame.NodeIndex];
                if (node.CatchAllChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(node.CatchAllChild, path.Length));
                }

                if (node.WildcardChild != CompiledNode.NoNode)
                {
                    stack.Push(new TraversalFrame(node.WildcardChild, frame.Depth + 1));
                }

                if (TryGetExactChild(frame.NodeIndex, path[frame.Depth], out var exactChildNodeIndex))
                {
                    stack.Push(new TraversalFrame(exactChildNodeIndex, frame.Depth + 1));
                }
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    internal List<MatchCandidate<TValue>> SelectBestPrefixCandidates(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: true);
        if (candidates.Count == 0)
        {
            return candidates;
        }

        var depth = candidates[^1].ConsumedSegmentCount;
        candidates.RemoveAll(candidate => candidate.ConsumedSegmentCount != depth);
        return candidates;
    }

    internal static ReadOnlySpan<MatchCandidate<TValue>> SelectDeepestPrefixCandidates(
        Span<MatchCandidate<TValue>> candidates)
    {
        if (candidates.IsEmpty)
        {
            return ReadOnlySpan<MatchCandidate<TValue>>.Empty;
        }

        var deepestDepth = candidates[^1].ConsumedSegmentCount;
        var first = candidates.Length - 1;
        while (first > 0 && candidates[first - 1].ConsumedSegmentCount == deepestDepth)
        {
            first--;
        }

        return candidates[first..];
    }

    private ReadOnlySpan<TValue> GetValues(int nodeIndex)
    {
        ref readonly var node = ref _index.Nodes[nodeIndex];
        return _index.Values.AsSpan(node.FirstValue, node.ValueCount);
    }

    private ReadOnlySpan<CompiledValueDetail> GetValueDetails(int nodeIndex)
    {
        ref readonly var node = ref _index.Nodes[nodeIndex];
        return _index.ValueDetails.AsSpan(node.FirstValue, node.ValueCount);
    }

    private int GetValueCountIncludingTerminalCatchAll(int nodeIndex)
    {
        var count = GetValues(nodeIndex).Length;
        ref readonly var node = ref _index.Nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetValues(node.CatchAllChild).Length;
        }

        return count;
    }

    private bool TryGetExactChild(int nodeIndex, TSegment segment, out int childNodeIndex)
    {
        ref readonly var node = ref _index.Nodes[nodeIndex];
        if (node.HasLookup)
        {
            var slotOffset = _segmentComparer.GetHashCode(segment) & node.LookupMask;
            for (var probeCount = 0; probeCount <= node.LookupMask; probeCount++)
            {
                var childIndex = _index.ChildLookupSlots[node.FirstLookupSlot + slotOffset];
                if (childIndex == CompiledNode.NoNode)
                {
                    break;
                }

                ref readonly var child = ref _index.Children[childIndex];
                if (_segmentComparer.Equals(child.Segment, segment))
                {
                    childNodeIndex = child.NodeIndex;
                    return true;
                }

                slotOffset = (slotOffset + 1) & node.LookupMask;
            }
        }
        else
        {
            var end = node.FirstChild + node.ChildCount;
            for (var i = node.FirstChild; i < end; i++)
            {
                ref readonly var child = ref _index.Children[i];
                if (_segmentComparer.Equals(child.Segment, segment))
                {
                    childNodeIndex = child.NodeIndex;
                    return true;
                }
            }
        }

        childNodeIndex = CompiledNode.NoNode;
        return false;
    }

    private void AddCandidates(
        int nodeIndex,
        int depth,
        int pathLength,
        List<MatchCandidate<TValue>> candidates)
    {
        var values = GetValues(nodeIndex);
        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            candidates.Add(new MatchCandidate<TValue>(
                values[i],
                details[i],
                depth,
                details[i].TerminalCatchAllSegmentIndex == pathLength));
        }
    }

    private int AddCandidates(
        int nodeIndex,
        int depth,
        int pathLength,
        Span<MatchCandidate<TValue>> destination,
        int count)
    {
        var values = GetValues(nodeIndex);
        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            destination[count++] = new MatchCandidate<TValue>(
                values[i],
                details[i],
                depth,
                details[i].TerminalCatchAllSegmentIndex == pathLength);
        }

        return count;
    }

    private static int CompareCandidates(
        MatchCandidate<TValue> left,
        MatchCandidate<TValue> right,
        bool prefix)
    {
        if (prefix)
        {
            var depth = left.ConsumedSegmentCount.CompareTo(right.ConsumedSegmentCount);
            if (depth != 0)
            {
                return depth;
            }
        }

        if (left.IsZeroLengthCatchAll != right.IsZeroLengthCatchAll)
        {
            return left.IsZeroLengthCatchAll ? 1 : -1;
        }

        var score = right.Detail.Score.CompareTo(left.Detail.Score);
        return score != 0 ? score : left.Detail.RegistrationOrder.CompareTo(right.Detail.RegistrationOrder);
    }

    private readonly struct TraversalFrame
    {
        internal TraversalFrame(int nodeIndex, int depth)
        {
            NodeIndex = nodeIndex;
            Depth = depth;
        }

        internal int NodeIndex { get; }

        internal int Depth { get; }
    }

    private ref struct TraversalStack
    {
        private Span<TraversalFrame> _items;
        private TraversalFrame[]? _rentedItems;
        private int _count;

        internal TraversalStack(Span<TraversalFrame> initialItems)
        {
            _items = initialItems;
            _rentedItems = null;
            _count = 0;
        }

        internal readonly bool IsEmpty => _count == 0;

        internal void Push(TraversalFrame frame)
        {
            if (_count == _items.Length)
            {
                Grow();
            }

            _items[_count++] = frame;
        }

        internal TraversalFrame Pop() => _items[--_count];

        internal void Dispose()
        {
            if (_rentedItems is not null)
            {
                ArrayPool<TraversalFrame>.Shared.Return(_rentedItems);
                _rentedItems = null;
            }

            _items = default;
            _count = 0;
        }

        private void Grow()
        {
            var newLength = _items.Length == 0 ? 4 : _items.Length * 2;
            var rentedItems = ArrayPool<TraversalFrame>.Shared.Rent(newLength);
            _items[.._count].CopyTo(rentedItems);
            if (_rentedItems is not null)
            {
                ArrayPool<TraversalFrame>.Shared.Return(_rentedItems);
            }

            _items = rentedItems;
            _rentedItems = rentedItems;
        }
    }
}
