#pragma warning disable CS1591
namespace Pattrn.Matching;

public static partial class PattrnIndexExtensions
{
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
