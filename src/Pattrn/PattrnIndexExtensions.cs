namespace Pattrn;

/// <summary>
/// Convenience overloads for segmented path indexes.
/// </summary>
public static class PattrnIndexExtensions
{
    /// <summary>
    /// Gets a path-specific upper bound for a memory-backed path.
    /// </summary>
    public static int GetMatchCountUpperBound<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetMatchCountUpperBound(path.Span);
    }

    /// <summary>
    /// Matches a memory-backed path and writes matching values into the caller-provided destination span.
    /// </summary>
    public static int Match<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<TValue> destination)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.Match(path.Span, destination);
    }

    /// <summary>
    /// Attempts to match a memory-backed path and write matching values into the caller-provided destination span.
    /// </summary>
    public static bool TryMatch<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<TValue> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatch(path.Span, destination, out written);
    }

    /// <summary>
    /// Gets a path-specific capture upper bound for a memory-backed path.
    /// </summary>
    public static int GetCaptureCountUpperBound<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetCaptureCountUpperBound(path.Span);
    }

    /// <summary>
    /// Matches a memory-backed path and writes detailed matches and captures into caller-provided destination spans.
    /// </summary>
    public static int MatchDetailed<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int capturesWritten)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchDetailed(path.Span, matches, captures, out capturesWritten);
    }

    /// <summary>
    /// Attempts to match a memory-backed path and write detailed matches and captures into caller-provided destination spans.
    /// </summary>
    public static bool TryMatchDetailed<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchDetailed(path.Span, matches, captures, out matchesWritten, out capturesWritten);
    }

    /// <summary>
    /// Explains matching behavior for a memory-backed path using diagnostics-oriented allocation-friendly results.
    /// </summary>
    public static PatternMatchExplanation<TSegment, TValue> Explain<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        PatternExplanationOptions options = default)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.Explain(path.Span, options);
    }

    /// <summary>
    /// Explains matching behavior for an enumerable path using diagnostics-oriented allocation-friendly results.
    /// </summary>
    public static PatternMatchExplanation<TSegment, TValue> Explain<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path,
        PatternExplanationOptions options = default)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);

        if (path is TSegment[] array)
        {
            return index.Explain(array.AsSpan(), options);
        }

        return index.Explain(path.ToArray().AsSpan(), options);
    }

    /// <summary>
    /// Matches a memory-backed path and returns detailed matches as a new array.
    /// </summary>
    public static PatternMatchResult<TSegment, TValue>[] MatchDetailed<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchDetailed(path.Span);
    }

    /// <summary>
    /// Matches a memory-backed path and returns detailed matches as a new array.
    /// </summary>
    public static PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchDetailedToArray(path.Span);
    }

    /// <summary>
    /// Matches an enumerable path and returns detailed matches as a new array.
    /// </summary>
    public static PatternMatchResult<TSegment, TValue>[] MatchDetailed<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);

        if (path is TSegment[] array)
        {
            return index.MatchDetailed(array.AsSpan());
        }

        return index.MatchDetailed(path.ToArray().AsSpan());
    }

    /// <summary>
    /// Matches an enumerable path and returns detailed matches as a new array.
    /// </summary>
    public static PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);

        if (path is TSegment[] array)
        {
            return index.MatchDetailedToArray(array.AsSpan());
        }

        return index.MatchDetailedToArray(path.ToArray().AsSpan());
    }

    /// <summary>
    /// Matches a memory-backed path and returns matching values as a new array.
    /// </summary>
    public static TValue[] MatchToArray<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchToArray(path.Span);
    }

    /// <summary>
    /// Matches an enumerable path and returns matching values as a new array.
    /// </summary>
    public static TValue[] MatchToArray<TSegment, TValue>(
        this IPattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);

        if (path is TSegment[] array)
        {
            return index.MatchToArray(array.AsSpan());
        }

        return index.MatchToArray(path.ToArray().AsSpan());
    }
}
