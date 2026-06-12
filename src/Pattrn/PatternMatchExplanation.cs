namespace Pattrn;

/// <summary>
/// Diagnostic explanation returned by <see cref="PattrnIndex{TSegment, TValue}.Explain(ReadOnlySpan{TSegment}, PatternExplanationOptions)"/>.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// Explanation results are intentionally allocation-friendly for humans and tools, not for hot matching loops.
/// Use <c>Match</c>, <c>TryMatch</c>, or <c>MatchToArray</c> for hot paths.
/// </remarks>
public sealed class PatternMatchExplanation<TSegment, TValue>
    where TSegment : notnull
{
    internal PatternMatchExplanation(
        TSegment[] path,
        PatternMatchResult<TSegment, TValue>[] matches,
        PatternRejectedCandidate[] rejectedCandidates,
        MatchOptions matchOptions,
        PatternExplanationOptions explanationOptions,
        int matchCountUpperBound,
        int captureCountUpperBound)
    {
        Path = path;
        Matches = matches;
        RejectedCandidates = rejectedCandidates;
        MatchOptions = matchOptions;
        ExplanationOptions = explanationOptions;
        MatchCountUpperBound = matchCountUpperBound;
        CaptureCountUpperBound = captureCountUpperBound;
    }

    /// <summary>
    /// Gets a copy of the input path that was explained.
    /// </summary>
    public IReadOnlyList<TSegment> Path { get; }

    /// <summary>
    /// Gets the accepted detailed matches for the input path.
    /// </summary>
    public IReadOnlyList<PatternMatchResult<TSegment, TValue>> Matches { get; }

    /// <summary>
    /// Gets diagnostic rejected-branch hints, when requested through <see cref="PatternExplanationOptions.IncludeRejectedCandidates"/>.
    /// </summary>
    public IReadOnlyList<PatternRejectedCandidate> RejectedCandidates { get; }

    /// <summary>
    /// Gets the matching options used by the explained index.
    /// </summary>
    public MatchOptions MatchOptions { get; }

    /// <summary>
    /// Gets the explanation options used to produce this result.
    /// </summary>
    public PatternExplanationOptions ExplanationOptions { get; }

    /// <summary>
    /// Gets the path-specific upper bound for accepted match values at explanation time.
    /// </summary>
    public int MatchCountUpperBound { get; }

    /// <summary>
    /// Gets the path-specific upper bound for named captures at explanation time.
    /// </summary>
    public int CaptureCountUpperBound { get; }

    /// <summary>
    /// Gets a value indicating whether the input produced at least one accepted match.
    /// </summary>
    public bool HasMatches => Matches.Count > 0;

    /// <summary>
    /// Gets the number of accepted detailed matches.
    /// </summary>
    public int MatchCount => Matches.Count;
}
