#pragma warning disable CS1591
namespace Pattrn.Matching;

/// <summary>
/// Convenience overloads for segmented path indexes.
/// </summary>
public static class PattrnIndexExtensions
{
    /// <summary>
    /// Gets a path-specific upper bound for a memory-backed path.
    /// </summary>
    public static int GetMatchCountUpperBound<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetMatchCountUpperBound(path.Span);
    }

    /// <summary>
    /// Gets a path-specific prefix match upper bound for a memory-backed path.
    /// </summary>
    public static int GetPrefixMatchCountUpperBound<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetPrefixMatchCountUpperBound(path.Span);
    }

    public static bool TryMatchValues<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<TValue> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchValues(path.Span, destination, out written);
    }

    public static bool TryMatch<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatch<TValue>> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatch(path.Span, destination, out written);
    }

    public static bool TryMatchPrefixValues<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<TValue> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchPrefixValues(path.Span, destination, out written);
    }

    public static bool TryMatchPrefix<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatch<TValue>> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchPrefix(path.Span, destination, out written);
    }

    public static bool TryEnumeratePrefixMatches<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatch<TValue>> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryEnumeratePrefixMatches(path.Span, destination, out written);
    }

    public static bool TryEnumeratePrefixValues<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<TValue> destination,
        out int written)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryEnumeratePrefixValues(path.Span, destination, out written);
    }

    /// <summary>
    /// Gets a path-specific capture upper bound for a memory-backed path.
    /// </summary>
    public static int GetCaptureCountUpperBound<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetCaptureCountUpperBound(path.Span);
    }

    /// <summary>
    /// Gets a path-specific best-prefix capture upper bound for a memory-backed path.
    /// </summary>
    public static int GetPrefixCaptureCountUpperBound<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetPrefixCaptureCountUpperBound(path.Span);
    }

    /// <summary>
    /// Gets a path-specific all-prefix capture upper bound for a memory-backed path.
    /// </summary>
    public static int GetEnumeratePrefixCaptureCountUpperBound<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.GetEnumeratePrefixCaptureCountUpperBound(path.Span);
    }

    /// <summary>
    /// Matches a memory-backed path and writes detailed matches and captures into caller-provided destination spans.
    /// </summary>
    public static int MatchDetailed<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
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
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchDetailed(path.Span, matches, captures, out matchesWritten, out capturesWritten);
    }

    public static bool TryMatchPrefixDetailed<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryMatchPrefixDetailed(path.Span, matches, captures, out matchesWritten, out capturesWritten);
    }

    public static bool TryEnumeratePrefixDetailed<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.TryEnumeratePrefixDetailed(path.Span, matches, captures, out matchesWritten, out capturesWritten);
    }

    /// <summary>
    /// Explains matching behavior for a memory-backed path using diagnostics-oriented allocation-friendly results.
    /// </summary>
    public static PatternMatchExplanation<TSegment, TValue> Explain<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
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
        this PattrnIndex<TSegment, TValue> index,
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
    public static PatternMatchDetailed<TSegment, TValue>[] MatchDetailedToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchDetailedToArray(path.Span);
    }

    /// <summary>
    /// Matches an enumerable path and returns detailed matches as a new array.
    /// </summary>
    public static PatternMatchDetailed<TSegment, TValue>[] MatchDetailedToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
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

    public static PatternMatchDetailed<TSegment, TValue>[] MatchPrefixDetailedToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchPrefixDetailedToArray(path.Span);
    }

    public static PatternMatchDetailed<TSegment, TValue>[] EnumeratePrefixDetailedToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.EnumeratePrefixDetailedToArray(path.Span);
    }

    public static PatternMatch<TValue>[] MatchPrefixToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);
        return index.MatchPrefixToArray(path is TSegment[] array ? array.AsSpan() : path.ToArray().AsSpan());
    }

    public static PatternMatch<TValue>[] EnumeratePrefixMatchesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.EnumeratePrefixMatchesToArray(path.Span);
    }

    public static TValue[] MatchPrefixValuesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);
        return index.MatchPrefixValuesToArray(path is TSegment[] array ? array.AsSpan() : path.ToArray().AsSpan());
    }

    public static TValue[] EnumeratePrefixValuesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.EnumeratePrefixValuesToArray(path.Span);
    }

    /// <summary>
    /// Matches a memory-backed path and returns matching values as a new array.
    /// </summary>
    public static PatternMatch<TValue>[] MatchToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchToArray(path.Span);
    }

    public static TValue[] MatchValuesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchValuesToArray(path.Span);
    }

    /// <summary>
    /// Matches prefix registrations for a memory-backed path and returns matching values as a new array.
    /// </summary>
    public static PatternMatch<TValue>[] MatchPrefixToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchPrefixToArray(path.Span);
    }

    public static TValue[] MatchPrefixValuesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        ReadOnlyMemory<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        return index.MatchPrefixValuesToArray(path.Span);
    }

    /// <summary>
    /// Matches an enumerable path and returns matching values as a new array.
    /// </summary>
    public static PatternMatch<TValue>[] MatchToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
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

    public static TValue[] MatchValuesToArray<TSegment, TValue>(
        this PattrnIndex<TSegment, TValue> index,
        IEnumerable<TSegment> path)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(path);
        return index.MatchValuesToArray(path is TSegment[] array ? array.AsSpan() : path.ToArray().AsSpan());
    }
}
