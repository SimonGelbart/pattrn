namespace Pattrn.Internal.Explanation;

/// <summary>Collects diagnostics-only rejected-candidate information.</summary>
internal static class ExplanationCollector<TSegment, TValue>
    where TSegment : notnull
{
    internal static PatternRejectedCandidate[] Collect(
        CompiledIndex<TSegment, TValue> index,
        IEqualityComparer<TSegment> segmentComparer,
        ReadOnlySpan<TSegment> path)
    {
        var rejectedCandidates = new List<PatternRejectedCandidate>();
        if (!index.HasWildcardBranches)
        {
            CollectExactOnly(index, segmentComparer, path, rejectedCandidates);
            return [.. rejectedCandidates];
        }

        var stack = new Stack<(int NodeIndex, int Depth)>();
        stack.Push((0, 0));
        while (stack.Count > 0)
        {
            var (nodeIndex, depth) = stack.Pop();
            ref readonly var node = ref index.Nodes[nodeIndex];

            if (depth == path.Length)
            {
                if (GetValueCountIncludingTerminalCatchAll(index, nodeIndex) == 0)
                {
                    rejectedCandidates.Add(new(
                        depth,
                        PatternRejectedCandidateReasonKind.PathTooShort,
                        "The input ended before this branch reached a terminal registration."));
                }

                continue;
            }

            var hadCandidate = false;
            if (node.CatchAllChild != CompiledNode.NoNode)
            {
                hadCandidate = true;
                stack.Push((node.CatchAllChild, path.Length));
            }

            if (node.WildcardChild != CompiledNode.NoNode)
            {
                hadCandidate = true;
                stack.Push((node.WildcardChild, depth + 1));
            }

            if (TryGetExactChild(index, segmentComparer, nodeIndex, path[depth], out var exactChildNodeIndex))
            {
                hadCandidate = true;
                stack.Push((exactChildNodeIndex, depth + 1));
            }

            if (!hadCandidate)
            {
                rejectedCandidates.Add(new(
                    depth,
                    PatternRejectedCandidateReasonKind.BranchNotMatched,
                    "No literal, wildcard, or catch-all branch matched this input segment."));
            }
        }

        return [.. rejectedCandidates];
    }

    private static void CollectExactOnly(
        CompiledIndex<TSegment, TValue> index,
        IEqualityComparer<TSegment> segmentComparer,
        ReadOnlySpan<TSegment> path,
        List<PatternRejectedCandidate> rejectedCandidates)
    {
        var nodeIndex = 0;
        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(index, segmentComparer, nodeIndex, path[depth], out var childNodeIndex))
            {
                rejectedCandidates.Add(new(
                    depth,
                    PatternRejectedCandidateReasonKind.LiteralMismatch,
                    "No literal branch matched this input segment."));
                return;
            }

            nodeIndex = childNodeIndex;
        }

        if (GetValues(index, nodeIndex).IsEmpty)
        {
            rejectedCandidates.Add(new(
                path.Length,
                PatternRejectedCandidateReasonKind.PathTooShort,
                "The input ended at a compiled node that has no terminal registration."));
        }
    }

    private static ReadOnlySpan<TValue> GetValues(CompiledIndex<TSegment, TValue> index, int nodeIndex)
    {
        ref readonly var node = ref index.Nodes[nodeIndex];
        return index.Values.AsSpan(node.FirstValue, node.ValueCount);
    }

    private static int GetValueCountIncludingTerminalCatchAll(
        CompiledIndex<TSegment, TValue> index,
        int nodeIndex)
    {
        var count = GetValues(index, nodeIndex).Length;
        ref readonly var node = ref index.Nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetValues(index, node.CatchAllChild).Length;
        }

        return count;
    }

    private static bool TryGetExactChild(
        CompiledIndex<TSegment, TValue> index,
        IEqualityComparer<TSegment> segmentComparer,
        int nodeIndex,
        TSegment segment,
        out int childNodeIndex)
    {
        ref readonly var node = ref index.Nodes[nodeIndex];
        if (node.HasLookup)
        {
            var slotOffset = segmentComparer.GetHashCode(segment) & node.LookupMask;
            for (var probeCount = 0; probeCount <= node.LookupMask; probeCount++)
            {
                var childIndex = index.ChildLookupSlots[node.FirstLookupSlot + slotOffset];
                if (childIndex == CompiledNode.NoNode)
                {
                    break;
                }

                ref readonly var child = ref index.Children[childIndex];
                if (segmentComparer.Equals(child.Segment, segment))
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
                ref readonly var child = ref index.Children[i];
                if (segmentComparer.Equals(child.Segment, segment))
                {
                    childNodeIndex = child.NodeIndex;
                    return true;
                }
            }
        }

        childNodeIndex = CompiledNode.NoNode;
        return false;
    }
}
