#pragma warning disable CS1591
namespace Pattrn.Matching;

/// <summary>
/// Convenience overloads for segmented path indexes.
/// </summary>
public static partial class PattrnIndexExtensions
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
}

