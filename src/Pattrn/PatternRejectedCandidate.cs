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
    internal PatternRejectedCandidate(int pathDepth, string reason)
    {
        if (pathDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pathDepth), pathDepth, "Path depth must be non-negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        PathDepth = pathDepth;
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
    /// Gets a domain-neutral reason explaining why the branch did not produce an accepted match.
    /// </summary>
    public string Reason { get; }
}
