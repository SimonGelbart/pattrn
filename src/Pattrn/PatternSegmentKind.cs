namespace Pattrn;

/// <summary>
/// Identifies how a <see cref="PatternSegment{TSegment}"/> participates in pattern matching.
/// </summary>
/// <remarks>
/// The numeric values are not part of the matching contract and should not be used for ranking.
/// </remarks>
public enum PatternSegmentKind
{
    /// <summary>
    /// Matches any single input segment without assigning it a name.
    /// </summary>
    Wildcard = 0,

    /// <summary>
    /// Matches one exact segment value.
    /// </summary>
    Literal = 1,

    /// <summary>
    /// Matches any single input segment and assigns the segment a logical name for richer match APIs.
    /// </summary>
    Parameter = 2,

    /// <summary>
    /// Matches zero or more remaining input segments. Catch-all segments must be terminal.
    /// </summary>
    CatchAll = 3
}
