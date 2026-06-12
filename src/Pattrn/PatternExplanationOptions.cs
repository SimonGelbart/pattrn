namespace Pattrn;

/// <summary>
/// Configures diagnostics-oriented explanation matching.
/// </summary>
/// <remarks>
/// Explanation options apply only to <see cref="PattrnIndex{TSegment, TValue}.Explain(ReadOnlySpan{TSegment}, PatternExplanationOptions)"/>.
/// They do not affect the hot <c>Match</c>, <c>TryMatch</c>, or <c>MatchToArray</c> APIs.
/// </remarks>
public readonly struct PatternExplanationOptions : IEquatable<PatternExplanationOptions>
{
    /// <summary>
    /// Initializes a new explanation options value.
    /// </summary>
    /// <param name="includeRejectedCandidates">Whether to collect rejected-branch diagnostics while explaining a match.</param>
    public PatternExplanationOptions(bool includeRejectedCandidates = false)
    {
        IncludeRejectedCandidates = includeRejectedCandidates;
    }

    /// <summary>
    /// Gets the default explanation options. Rejected-candidate diagnostics are disabled by default.
    /// </summary>
    public static PatternExplanationOptions Default => default;

    /// <summary>
    /// Gets an explanation options value that collects rejected-branch diagnostics.
    /// </summary>
    public static PatternExplanationOptions IncludeRejections => new(includeRejectedCandidates: true);

    /// <summary>
    /// Gets a value indicating whether rejected-branch diagnostics should be collected.
    /// </summary>
    /// <remarks>
    /// Rejected-candidate diagnostics are intentionally opt-in because they require additional traversal and allocation.
    /// </remarks>
    public bool IncludeRejectedCandidates { get; }

    /// <inheritdoc />
    public bool Equals(PatternExplanationOptions other) => IncludeRejectedCandidates == other.IncludeRejectedCandidates;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PatternExplanationOptions other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => IncludeRejectedCandidates.GetHashCode();

    /// <summary>
    /// Compares two explanation options values for equality.
    /// </summary>
    public static bool operator ==(PatternExplanationOptions left, PatternExplanationOptions right) => left.Equals(right);

    /// <summary>
    /// Compares two explanation options values for inequality.
    /// </summary>
    public static bool operator !=(PatternExplanationOptions left, PatternExplanationOptions right) => !left.Equals(right);

    /// <summary>
    /// Deconstructs this options value.
    /// </summary>
    /// <param name="includeRejectedCandidates">Whether rejected-branch diagnostics should be collected.</param>
    public void Deconstruct(out bool includeRejectedCandidates)
    {
        includeRejectedCandidates = IncludeRejectedCandidates;
    }
}
