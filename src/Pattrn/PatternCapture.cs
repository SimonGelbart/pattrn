namespace Pattrn;

/// <summary>
/// Describes one named segment captured while matching a generic pattern.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
public readonly struct PatternCapture<TSegment> : IEquatable<PatternCapture<TSegment>>
    where TSegment : notnull
{
    /// <summary>
    /// Initializes a new captured segment value.
    /// </summary>
    /// <param name="name">The parameter name assigned by the registered pattern.</param>
    /// <param name="value">The input segment value captured for the parameter.</param>
    /// <param name="segmentIndex">The zero-based input segment index captured for the parameter.</param>
    public PatternCapture(string name, TSegment value, int segmentIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        if (segmentIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentIndex), segmentIndex, "Segment index must be non-negative.");
        }

        Name = name;
        Value = value;
        SegmentIndex = segmentIndex;
    }

    /// <summary>
    /// Gets the parameter name assigned by the registered pattern.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the input segment value captured for the parameter.
    /// </summary>
    public TSegment Value { get; }

    /// <summary>
    /// Gets the zero-based input segment index captured for the parameter.
    /// </summary>
    public int SegmentIndex { get; }

    /// <inheritdoc />
    public bool Equals(PatternCapture<TSegment> other)
    {
        return string.Equals(Name, other.Name, StringComparison.Ordinal)
            && EqualityComparer<TSegment>.Default.Equals(Value, other.Value)
            && SegmentIndex == other.SegmentIndex;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PatternCapture<TSegment> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(Name), Value, SegmentIndex);

    /// <inheritdoc />
    public override string ToString() => Name + "=" + (Value?.ToString() ?? string.Empty);

    /// <summary>
    /// Compares two captures for equality.
    /// </summary>
    public static bool operator ==(PatternCapture<TSegment> left, PatternCapture<TSegment> right) => left.Equals(right);

    /// <summary>
    /// Compares two captures for inequality.
    /// </summary>
    public static bool operator !=(PatternCapture<TSegment> left, PatternCapture<TSegment> right) => !left.Equals(right);
}
