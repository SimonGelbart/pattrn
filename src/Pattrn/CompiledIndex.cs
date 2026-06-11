namespace Pattrn;

internal sealed class CompiledIndex<TSegment, TValue>
    where TSegment : notnull
{
    private CompiledIndex(
        CompiledNode[] nodes,
        CompiledChild<TSegment>[] children,
        int[] childLookupSlots,
        TValue[] values,
        CompiledValueDetail[] valueDetails,
        CaptureDescriptor[] captureDescriptors,
        bool hasWildcardBranches)
    {
        Nodes = nodes;
        Children = children;
        ChildLookupSlots = childLookupSlots;
        Values = values;
        ValueDetails = valueDetails;
        CaptureDescriptors = captureDescriptors;
        HasWildcardBranches = hasWildcardBranches;
    }

    internal CompiledNode[] Nodes { get; }

    internal CompiledChild<TSegment>[] Children { get; }

    internal int[] ChildLookupSlots { get; }

    internal TValue[] Values { get; }

    internal CompiledValueDetail[] ValueDetails { get; }

    internal CaptureDescriptor[] CaptureDescriptors { get; }

    internal bool HasWildcardBranches { get; }

    internal static CompiledIndex<TSegment, TValue> FromBuilder(
        BuilderNode<TSegment, TValue> source,
        IEqualityComparer<TSegment> segmentComparer,
        IEqualityComparer<TValue> valueComparer,
        bool deduplicateValues)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(segmentComparer);
        ArgumentNullException.ThrowIfNull(valueComparer);

        var builder = new Builder(segmentComparer, valueComparer, deduplicateValues);
        builder.Compile(source);
        return builder.ToIndex();
    }

    private sealed class Builder
    {
        private const int ChildLookupThreshold = 8;

        private readonly IEqualityComparer<TSegment> _segmentComparer;
        private readonly IEqualityComparer<TValue> _valueComparer;
        private readonly bool _deduplicateValues;
        private readonly List<CompiledNode> _nodes = [];
        private readonly List<CompiledChild<TSegment>> _children = [];
        private readonly List<int> _childLookupSlots = [];
        private readonly List<TValue> _values = [];
        private readonly List<CompiledValueDetail> _valueDetails = [];
        private readonly List<CaptureDescriptor> _captureDescriptors = [];
        private bool _hasWildcardBranches;

        internal Builder(
            IEqualityComparer<TSegment> segmentComparer,
            IEqualityComparer<TValue> valueComparer,
            bool deduplicateValues)
        {
            _segmentComparer = segmentComparer;
            _valueComparer = valueComparer;
            _deduplicateValues = deduplicateValues;
        }

        internal void Compile(BuilderNode<TSegment, TValue> source)
        {
            var rootIndex = AddNodePlaceholder();
            var pendingNodes = new List<PendingNode> { new(source, rootIndex) };

            while (pendingNodes.Count > 0)
            {
                var pendingIndex = pendingNodes.Count - 1;
                var pendingNode = pendingNodes[pendingIndex];
                pendingNodes.RemoveAt(pendingIndex);

                AddNode(pendingNode.Source, pendingNode.NodeIndex, pendingNodes);
            }
        }

        internal CompiledIndex<TSegment, TValue> ToIndex()
        {
            return new CompiledIndex<TSegment, TValue>(
                [.. _nodes],
                [.. _children],
                [.. _childLookupSlots],
                [.. _values],
                [.. _valueDetails],
                [.. _captureDescriptors],
                _hasWildcardBranches);
        }

        private int AddNodePlaceholder()
        {
            var nodeIndex = _nodes.Count;
            _nodes.Add(default);
            return nodeIndex;
        }

        private void AddNode(
            BuilderNode<TSegment, TValue> source,
            int nodeIndex,
            List<PendingNode> pendingNodes)
        {
            var firstValue = _values.Count;
            var valueCount = _deduplicateValues
                ? AddDeduplicatedValues(source.Values, source.Metadata)
                : AddValues(source.Values, source.Metadata);

            var wildcardChild = CompiledNode.NoNode;
            if (source.WildcardChild is not null)
            {
                wildcardChild = AddNodePlaceholder();
                pendingNodes.Add(new PendingNode(source.WildcardChild, wildcardChild));
                _hasWildcardBranches = true;
            }

            var catchAllChild = CompiledNode.NoNode;
            if (source.CatchAllChild is not null)
            {
                catchAllChild = AddNodePlaceholder();
                pendingNodes.Add(new PendingNode(source.CatchAllChild, catchAllChild));
                _hasWildcardBranches = true;
            }

            var childCount = source.ChildCount;
            var firstChild = _children.Count;
            if (source.Children is not null)
            {
                for (var i = 0; i < childCount; i++)
                {
                    _children.Add(default);
                }

                var childOffset = 0;
                foreach (var (segment, child) in source.Children)
                {
                    var childNodeIndex = AddNodePlaceholder();
                    _children[firstChild + childOffset] = new CompiledChild<TSegment>(segment, childNodeIndex);
                    pendingNodes.Add(new PendingNode(child, childNodeIndex));
                    childOffset++;
                }
            }

            var firstLookupSlot = CompiledNode.NoLookup;
            var lookupMask = 0;
            if (childCount >= ChildLookupThreshold)
            {
                (firstLookupSlot, lookupMask) = AddChildLookup(firstChild, childCount);
            }

            _nodes[nodeIndex] = new CompiledNode(
                firstChild,
                childCount,
                wildcardChild,
                catchAllChild,
                firstValue,
                valueCount,
                firstLookupSlot,
                lookupMask);
        }

        private (int FirstSlot, int Mask) AddChildLookup(int firstChild, int childCount)
        {
            var tableLength = GetPowerOfTwoAtLeast(childCount * 2);
            var mask = tableLength - 1;
            var firstSlot = _childLookupSlots.Count;

            for (var i = 0; i < tableLength; i++)
            {
                _childLookupSlots.Add(CompiledNode.NoNode);
            }

            for (var childIndex = firstChild; childIndex < firstChild + childCount; childIndex++)
            {
                var child = _children[childIndex];
                var slot = firstSlot + (_segmentComparer.GetHashCode(child.Segment) & mask);

                while (_childLookupSlots[slot] != CompiledNode.NoNode)
                {
                    slot = firstSlot + ((slot - firstSlot + 1) & mask);
                }

                _childLookupSlots[slot] = childIndex;
            }

            return (firstSlot, mask);
        }

        private static int GetPowerOfTwoAtLeast(int value)
        {
            var result = 1;
            while (result < value)
            {
                result <<= 1;
            }

            return result;
        }

        private int AddValues(List<TValue>? values, List<BuilderRegistrationMetadata>? metadata)
        {
            if (values is null || values.Count == 0)
            {
                return 0;
            }

            var orderedIndexes = GetSpecificityOrderedIndexes(values.Count, metadata);
            for (var i = 0; i < orderedIndexes.Length; i++)
            {
                var registrationIndex = orderedIndexes[i];
                _values.Add(values[registrationIndex]);
                AddValueDetail(GetMetadata(metadata, registrationIndex));
            }

            return values.Count;
        }

        private int AddDeduplicatedValues(List<TValue>? values, List<BuilderRegistrationMetadata>? metadata)
        {
            if (values is null || values.Count == 0)
            {
                return 0;
            }

            var start = _values.Count;
            var orderedIndexes = GetSpecificityOrderedIndexes(values.Count, metadata);
            for (var i = 0; i < orderedIndexes.Length; i++)
            {
                var registrationIndex = orderedIndexes[i];
                var candidate = values[registrationIndex];
                var isDuplicate = false;

                for (var j = start; j < _values.Count; j++)
                {
                    if (!_valueComparer.Equals(_values[j], candidate))
                    {
                        continue;
                    }

                    isDuplicate = true;
                    break;
                }

                if (!isDuplicate)
                {
                    _values.Add(candidate);
                    AddValueDetail(GetMetadata(metadata, registrationIndex));
                }
            }

            return _values.Count - start;
        }

        private static int[] GetSpecificityOrderedIndexes(int count, List<BuilderRegistrationMetadata>? metadata)
        {
            var indexes = new int[count];
            for (var i = 0; i < count; i++)
            {
                indexes[i] = i;
            }

            Array.Sort(indexes, (left, right) =>
            {
                var leftScore = GetMetadata(metadata, left).Score;
                var rightScore = GetMetadata(metadata, right).Score;
                var scoreComparison = rightScore.CompareTo(leftScore);
                return scoreComparison != 0 ? scoreComparison : left.CompareTo(right);
            });

            return indexes;
        }

        private void AddValueDetail(BuilderRegistrationMetadata metadata)
        {
            var firstCapture = _captureDescriptors.Count;
            var captureCount = metadata.Captures.Length;

            for (var i = 0; i < captureCount; i++)
            {
                _captureDescriptors.Add(metadata.Captures[i]);
            }

            _valueDetails.Add(new CompiledValueDetail(
                firstCapture,
                captureCount,
                metadata.Kind,
                metadata.Score,
                metadata.PatternId,
                metadata.RegistrationOrder));
        }

        private static BuilderRegistrationMetadata GetMetadata(List<BuilderRegistrationMetadata>? metadata, int index)
        {
            return metadata is not null && index < metadata.Count
                ? metadata[index]
                : BuilderRegistrationMetadata.Exact;
        }

        private readonly struct PendingNode
        {
            internal PendingNode(BuilderNode<TSegment, TValue> source, int nodeIndex)
            {
                Source = source;
                NodeIndex = nodeIndex;
            }

            internal BuilderNode<TSegment, TValue> Source { get; }

            internal int NodeIndex { get; }
        }
    }
}
