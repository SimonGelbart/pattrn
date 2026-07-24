#pragma warning disable CS1591
namespace Pattrn.Matching;

public static partial class PattrnIndexExtensions
{
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
}

