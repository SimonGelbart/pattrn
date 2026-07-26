#pragma warning disable CS1591
namespace Pattrn.Matching;

public static partial class PattrnIndexExtensions
{
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
}
