namespace Pattrn;

/// <summary>
/// Describes how separated string helpers handle empty string segments produced by splitting.
/// </summary>
public enum StringEmptySegmentBehavior
{
    /// <summary>
    /// Empty segments are rejected with an <see cref="ArgumentException"/>.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// Empty segments are omitted from the produced segment list.
    /// </summary>
    Ignore = 1
}
