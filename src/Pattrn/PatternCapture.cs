using System.Collections.Immutable;

namespace Pattrn;

/// <summary>
/// Describes one named capture produced while matching a generic pattern.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
public readonly record struct PatternCapture<TSegment>(string Name, ImmutableArray<TSegment> Values, int StartSegmentIndex)
    where TSegment : notnull
{
    /// <summary>
    /// Initializes a new single-segment capture.
    /// </summary>
    public PatternCapture(string name, TSegment value, int startSegmentIndex)
        : this(name, ImmutableArray.Create(value), startSegmentIndex)
    {
    }

    /// <summary>
    /// Gets the first captured input segment value.
    /// </summary>
    public TSegment Value => Values[0];

    /// <summary>
    /// Gets the zero-based input segment index where this capture starts.
    /// </summary>
    public int SegmentIndex => StartSegmentIndex;

    /// <summary>
    /// Gets the number of input segments captured by this capture.
    /// </summary>
    public int SegmentCount => Values.Length;

    /// <inheritdoc />
    public bool Equals(PatternCapture<TSegment> other)
    {
        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && Values.SequenceEqual(other.Values)
            && StartSegmentIndex == other.StartSegmentIndex;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        foreach (var value in Values)
        {
            hash.Add(value);
        }

        hash.Add(StartSegmentIndex);
        return hash.ToHashCode();
    }
}
