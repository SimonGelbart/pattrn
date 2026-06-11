namespace Pattrn;

internal readonly struct CompiledChild<TSegment>
    where TSegment : notnull
{
    internal CompiledChild(TSegment segment, int nodeIndex)
    {
        Segment = segment;
        NodeIndex = nodeIndex;
    }

    internal TSegment Segment { get; }

    internal int NodeIndex { get; }
}
