namespace Pattrn.Patterns;

/// <summary>
/// Describes one named capture slice written by caller-buffer detailed matching APIs.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <remarks>
/// The slice does not own captured values. Use <see cref="StartSegmentIndex"/> and <see cref="SegmentCount"/>
/// to read the captured range from the original input path without allocations.
/// </remarks>
public readonly record struct PatternCaptureSlice<TSegment>(string Name, int StartSegmentIndex, int SegmentCount)
    where TSegment : notnull
{
    /// <summary>
    /// Gets the zero-based input segment index where this capture starts.
    /// </summary>
    public int SegmentIndex => StartSegmentIndex;
}
