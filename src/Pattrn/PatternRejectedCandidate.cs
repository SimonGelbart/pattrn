namespace Pattrn;

/// <summary>
/// Describes a branch considered by diagnostic explanation matching that did not produce an accepted match.
/// </summary>
/// <remarks>
/// Rejected candidates are diagnostic hints rather than part of the hot matching contract. They intentionally avoid
/// domain-specific concepts such as HTTP methods, filesystem semantics, authorization rules, or route constraints.
/// </remarks>
public sealed class PatternRejectedCandidate
{
    internal PatternRejectedCandidate(int pathDepth, PatternRejectedCandidateReasonKind reasonKind, string reason)
    {
        if (pathDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pathDepth), pathDepth, "Path depth must be non-negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        PathDepth = pathDepth;
        ReasonKind = reasonKind;
        Reason = reason;
    }

    /// <summary>
    /// Gets the zero-based input depth where this branch stopped matching.
    /// </summary>
    /// <remarks>
    /// A value equal to the input path length means traversal reached the end of the input but did not find a terminal registration.
    /// </remarks>
    public int PathDepth { get; }

    /// <summary>
    /// Gets the stable, machine-consumable reason kind explaining why the branch did not produce an accepted match.
    /// </summary>
    public PatternRejectedCandidateReasonKind ReasonKind { get; }

    /// <summary>
    /// Gets best-effort human-readable text explaining why the branch did not produce an accepted match.
    /// </summary>
    /// <remarks>
    /// Exact wording is intended for debugging and is not a compatibility contract. Use <see cref="ReasonKind"/> for machine-consumable decisions.
    /// </remarks>
    public string Reason { get; }
}
