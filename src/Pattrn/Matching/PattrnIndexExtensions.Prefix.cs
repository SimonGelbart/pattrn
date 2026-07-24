#pragma warning disable CS1591
namespace Pattrn.Matching;

public static partial class PattrnIndexExtensions
{
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
}

