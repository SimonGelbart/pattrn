namespace Pattrn;

/// <summary>
/// Identifies the stable, machine-consumable reason kind for a rejected candidate reported by runtime explanation.
/// </summary>
/// <remarks>
/// Member names and numeric values are compatibility-significant. The human-readable
/// <see cref="PatternRejectedCandidate.Reason"/> text remains best-effort debugging information.
/// </remarks>
public enum PatternRejectedCandidateReasonKind
{
    /// <summary>
    /// No typed reason was supplied.
    /// </summary>
    None = 0,

    /// <summary>
    /// The current input segment did not match any literal branch considered at this depth.
    /// </summary>
    LiteralMismatch = 1,

    /// <summary>
    /// The input ended before a considered branch reached a terminal registration.
    /// </summary>
    PathTooShort = 5,

    /// <summary>
    /// No compiled branch considered at this depth matched the current input segment.
    /// </summary>
    BranchNotMatched = 7
}
