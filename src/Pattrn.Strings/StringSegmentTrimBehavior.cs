namespace Pattrn;

/// <summary>
/// Describes optional whitespace trimming for separated string helpers.
/// </summary>
public enum StringSegmentTrimBehavior
{
    /// <summary>
    /// Segments are used exactly as produced by splitting and normalization.
    /// </summary>
    None = 0,

    /// <summary>
    /// Leading and trailing whitespace is removed from each segment before empty-segment validation.
    /// </summary>
    TrimWhitespace = 1
}
