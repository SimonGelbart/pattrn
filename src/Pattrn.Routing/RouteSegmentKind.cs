namespace Pattrn.Routing;

/// <summary>
/// Identifies the kind of a parsed route-template segment.
/// </summary>
public enum RouteSegmentKind
{
    /// <summary>
    /// A literal route segment.
    /// </summary>
    Literal = 0,

    /// <summary>
    /// A parameter or catch-all route segment.
    /// </summary>
    Parameter = 1
}
