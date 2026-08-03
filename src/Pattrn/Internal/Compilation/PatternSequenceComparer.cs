using System.Collections.Immutable;

namespace Pattrn.Internal.Compilation;

/// <summary>Compares canonical pattern sequences using the configured segment comparer.</summary>
internal sealed class PatternSequenceComparer<TSegment> : IEqualityComparer<ImmutableArray<PatternSegment<TSegment>>>
    where TSegment : notnull
{
    private readonly IEqualityComparer<TSegment> _segmentComparer;

    internal PatternSequenceComparer(IEqualityComparer<TSegment> segmentComparer)
    {
        ArgumentNullException.ThrowIfNull(segmentComparer);
        _segmentComparer = segmentComparer;
    }

    public bool Equals(
        ImmutableArray<PatternSegment<TSegment>> left,
        ImmutableArray<PatternSegment<TSegment>> right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var index = 0; index < left.Length; index++)
        {
            var leftSegment = left[index];
            var rightSegment = right[index];
            if (leftSegment.Kind != rightSegment.Kind
                || (leftSegment.IsLiteral && !_segmentComparer.Equals(leftSegment.LiteralValue, rightSegment.LiteralValue))
                || !StringComparer.Ordinal.Equals(leftSegment.ParameterName, rightSegment.ParameterName))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(ImmutableArray<PatternSegment<TSegment>> pattern)
    {
        var hash = new HashCode();
        hash.Add(pattern.Length);

        for (var index = 0; index < pattern.Length; index++)
        {
            var segment = pattern[index];
            hash.Add(segment.Kind);
            if (segment.IsLiteral)
            {
                hash.Add(segment.LiteralValue, _segmentComparer);
            }

            hash.Add(segment.ParameterName, StringComparer.Ordinal);
        }

        return hash.ToHashCode();
    }
}
