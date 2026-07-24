namespace Pattrn.Internal.Storage;

internal sealed class CompiledIndex<TSegment, TValue>
    where TSegment : notnull
{
    internal CompiledIndex(
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
}
