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
    /// Gets the captured value for a single-segment capture.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this capture contains zero or multiple segments. Use <see cref="Values"/> for multi-segment and zero-segment captures.</exception>
    public TSegment Value
    {
        get
        {
            if (Values.Length != 1)
            {
                throw new InvalidOperationException("PatternCapture.Value is only available for single-segment captures. Use Values for zero-segment or multi-segment captures.");
            }

            return Values[0];
        }
    }

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
