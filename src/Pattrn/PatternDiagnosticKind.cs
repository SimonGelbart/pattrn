namespace Pattrn;

/// <summary>
/// Describes the kind of builder diagnostic reported for registered patterns.
/// </summary>
public enum PatternDiagnosticKind
{
    /// <summary>
    /// Multiple values are registered for the same structural pattern.
    /// </summary>
    DuplicatePattern = 0,

    /// <summary>
    /// Structurally equivalent parameterized patterns use different parameter names.
    /// </summary>
    AmbiguousParameterNames = 1,

    /// <summary>
    /// A wildcard branch overlaps one or more literal branches at the same position.
    /// </summary>
    OverlappingWildcard = 2,

    /// <summary>
    /// A terminal catch-all overlaps a shorter or more specific pattern below the same prefix.
    /// </summary>
    OverlappingCatchAll = 3
}
