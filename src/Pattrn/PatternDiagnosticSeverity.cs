namespace Pattrn;

/// <summary>
/// Describes the severity of a pattern diagnostic.
/// </summary>
public enum PatternDiagnosticSeverity
{
    /// <summary>
    /// The diagnostic is informational and matching remains deterministic.
    /// </summary>
    Info = 0,

    /// <summary>
    /// The diagnostic highlights a pattern set that is valid but likely deserves review.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// The diagnostic represents an invalid pattern set.
    /// </summary>
    Error = 2
}
