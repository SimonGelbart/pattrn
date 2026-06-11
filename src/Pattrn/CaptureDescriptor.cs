namespace Pattrn;

internal readonly struct CaptureDescriptor
{
    internal CaptureDescriptor(string name, int segmentIndex, bool isCatchAll = false)
    {
        Name = name;
        SegmentIndex = segmentIndex;
        IsCatchAll = isCatchAll;
    }

    internal string Name { get; }

    internal int SegmentIndex { get; }

    internal bool IsCatchAll { get; }
}
