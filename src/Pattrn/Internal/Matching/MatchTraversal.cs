using System.Buffers;

namespace Pattrn.Internal.Matching;

/// <summary>Identifies the accepted path depths included in a candidate traversal.</summary>
internal enum CandidateTraversalKind
{
    Exact,
    AllPrefix
}

/// <summary>Owns a pooled, ordered candidate selection for the lifetime of a projection operation.</summary>
internal ref struct TraversalCandidates<TValue>
{
    private MatchCandidate<TValue>[]? _rented;
    private readonly int _start;

    internal TraversalCandidates(
        MatchCandidate<TValue>[] rented,
        int start,
        int count,
        int captureCountUpperBound)
    {
        _rented = rented;
        _start = start;
        Count = count;
        CaptureCountUpperBound = captureCountUpperBound;
    }

    internal readonly int Count { get; }

    internal readonly int CaptureCountUpperBound { get; }

    internal readonly ReadOnlySpan<MatchCandidate<TValue>> Candidates
        => _rented is null
            ? ReadOnlySpan<MatchCandidate<TValue>>.Empty
            : _rented.AsSpan(_start, Count);

    internal void Dispose()
    {
        var rented = _rented;
        _rented = null;
        if (rented is not null)
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }
}

/// <summary>Allocation-conscious traversal and ranking over immutable compiled storage.</summary>
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

    internal TraversalCandidates<TValue> TraverseCandidates(
        ReadOnlySpan<TSegment> path,
        CandidateTraversalKind kind,
        bool bestPrefix)
    {
        var capacity = CountCandidates(path, kind);
        if (capacity == 0)
        {
            return default;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, kind, candidates);
            SortCandidates(candidates[..count], kind);

            var start = 0;
            if (bestPrefix)
            {
                var selected = SelectDeepestPrefixCandidates(candidates[..count]);
                start = count - selected.Length;
                count = selected.Length;
            }

            var captureCountUpperBound = CountCaptures(candidates.Slice(start, count));
            return new TraversalCandidates<TValue>(rented, start, count, captureCountUpperBound);
        }
        catch
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
            throw;
        }
    }

    internal int CountCandidates(ReadOnlySpan<TSegment> path, CandidateTraversalKind kind)
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

                if (kind == CandidateTraversalKind.AllPrefix)
                {
                    count += GetValueCount(frame.NodeIndex);
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

    internal int CountCaptures(ReadOnlySpan<TSegment> path, CandidateTraversalKind kind)
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
                    count += GetCaptureCountIncludingTerminalCatchAll(frame.NodeIndex);
                    continue;
                }

                if (kind == CandidateTraversalKind.AllPrefix)
                {
                    count += GetCaptureCount(frame.NodeIndex);
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

    internal int CountBestPrefixCandidates(ReadOnlySpan<TSegment> path)
    {
        using var candidates = TraverseCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: true);
        return candidates.Count;
    }

    internal int CountBestPrefixCaptures(ReadOnlySpan<TSegment> path)
    {
        using var candidates = TraverseCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: true);
        return candidates.CaptureCountUpperBound;
    }

    private int CollectCandidatesInto(
        ReadOnlySpan<TSegment> path,
        CandidateTraversalKind kind,
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

                if (kind == CandidateTraversalKind.AllPrefix)
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

    private static void SortCandidates(Span<MatchCandidate<TValue>> candidates, CandidateTraversalKind kind)
    {
        for (var i = 1; i < candidates.Length; i++)
        {
            var candidate = candidates[i];
            var j = i - 1;
            while (j >= 0 && CompareCandidates(candidate, candidates[j], kind) < 0)
            {
                candidates[j + 1] = candidates[j];
                j--;
            }

            candidates[j + 1] = candidate;
        }
    }

    private static ReadOnlySpan<MatchCandidate<TValue>> SelectDeepestPrefixCandidates(
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

    private int GetValueCountIncludingTerminalCatchAll(int nodeIndex)
    {
        var count = GetValueCount(nodeIndex);
        ref readonly var node = ref _index.Nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetValueCount(node.CatchAllChild);
        }

        return count;
    }

    private int GetCaptureCountIncludingTerminalCatchAll(int nodeIndex)
    {
        var count = GetCaptureCount(nodeIndex);
        ref readonly var node = ref _index.Nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetCaptureCount(node.CatchAllChild);
        }

        return count;
    }

    private int GetValueCount(int nodeIndex) => _index.Nodes[nodeIndex].ValueCount;

    private int GetCaptureCount(int nodeIndex)
    {
        var count = 0;
        ref readonly var node = ref _index.Nodes[nodeIndex];
        var details = _index.ValueDetails.AsSpan(node.FirstValue, node.ValueCount);
        for (var i = 0; i < details.Length; i++)
        {
            count += details[i].CaptureCount;
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

    private static int CountCaptures(ReadOnlySpan<MatchCandidate<TValue>> candidates)
    {
        var count = 0;
        for (var i = 0; i < candidates.Length; i++)
        {
            count += candidates[i].Detail.CaptureCount;
        }

        return count;
    }

    private int AddCandidates(
        int nodeIndex,
        int depth,
        int pathLength,
        Span<MatchCandidate<TValue>> destination,
        int count)
    {
        ref readonly var node = ref _index.Nodes[nodeIndex];
        var values = _index.Values.AsSpan(node.FirstValue, node.ValueCount);
        var details = _index.ValueDetails.AsSpan(node.FirstValue, node.ValueCount);
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
        CandidateTraversalKind kind)
    {
        if (kind == CandidateTraversalKind.AllPrefix)
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
