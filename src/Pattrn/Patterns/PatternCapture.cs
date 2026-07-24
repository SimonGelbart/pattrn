using System.Collections.Immutable;

namespace Pattrn.Patterns;

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
    /// <exception cref="InvalidOperationException">Thrown when this capture contains zero or multiple segments, or is uninitialized. Use <see cref="Values"/> for multi-segment and zero-segment captures.</exception>
    public TSegment Value
    {
        get
        {
            if (Values.IsDefault || Values.Length != 1)
            {
                throw new InvalidOperationException("PatternCapture.Value is only available for initialized single-segment captures. Use Values for zero-segment or multi-segment captures.");
            }

            return Values[0];
        }
    }

    /// <summary>
    /// Gets the zero-based input segment index where this capture starts.
    /// </summary>
    public int SegmentIndex => StartSegmentIndex;

    /// <summary>
    /// Gets the number of input segments captured by this capture, or <c>0</c> when the capture is uninitialized.
    /// </summary>
    public int SegmentCount => Values.IsDefault ? 0 : Values.Length;

    /// <inheritdoc />
    public bool Equals(PatternCapture<TSegment> other)
    {
        if (!string.Equals(Name, other.Name, StringComparison.Ordinal)
            || StartSegmentIndex != other.StartSegmentIndex)
        {
            return false;
        }

        if (Values.IsDefault || other.Values.IsDefault)
        {
            return Values.IsDefault == other.Values.IsDefault;
        }

        return Values.SequenceEqual(other.Values);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        if (!Values.IsDefault)
        {
            foreach (var value in Values)
            {
                hash.Add(value);
            }
        }

        hash.Add(StartSegmentIndex);
        return hash.ToHashCode();
    }
}
