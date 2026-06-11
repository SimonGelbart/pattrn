namespace Pattrn;

/// <summary>
/// Describes the shape of a registered pattern that produced a detailed match.
/// </summary>
public enum PatternMatchKind
{
    /// <summary>
    /// The matched pattern contains only literal segments.
    /// </summary>
    Exact = 0,

    /// <summary>
    /// The matched pattern contains one or more anonymous wildcard segments and no named parameters or catch-alls.
    /// </summary>
    Wildcard = 1,

    /// <summary>
    /// The matched pattern contains one or more named parameters and no anonymous wildcards or catch-alls.
    /// </summary>
    Parameter = 2,

    /// <summary>
    /// The matched pattern contains one terminal catch-all and no other non-literal segments.
    /// </summary>
    CatchAll = 3,

    /// <summary>
    /// The matched pattern combines multiple non-literal segment kinds.
    /// </summary>
    Mixed = 4
}
