namespace Pattrn;

/// <summary>
/// Describes one detailed match result produced by the index.
/// </summary>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// Captures are stored in a caller-provided capture span. <see cref="CaptureStart"/> and <see cref="CaptureCount"/>
/// identify the slice belonging to this match.
/// </remarks>
public readonly struct PatternMatch<TValue> : IEquatable<PatternMatch<TValue>>
{
    /// <summary>
    /// Initializes a new detailed match descriptor.
    /// </summary>
    /// <param name="value">The matched registration value.</param>
    /// <param name="kind">The shape of the pattern that produced the match.</param>
    /// <param name="captureStart">The starting capture index for this match in the shared capture span.</param>
    /// <param name="captureCount">The number of captures that belong to this match.</param>
    /// <param name="consumedSegmentCount">The number of input segments consumed by this match.</param>
    /// <param name="patternSegmentCount">The number of segments in the pattern that produced the match.</param>
    public PatternMatch(
        TValue value,
        PatternMatchKind kind,
        int captureStart,
        int captureCount,
        int consumedSegmentCount = 0,
        int patternSegmentCount = 0)
    {
        if (captureStart < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureStart), captureStart, "Capture start must be non-negative.");
        }

        if (captureCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureCount), captureCount, "Capture count must be non-negative.");
        }

        if (consumedSegmentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(consumedSegmentCount), consumedSegmentCount, "Consumed segment count must be non-negative.");
        }

        if (patternSegmentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(patternSegmentCount), patternSegmentCount, "Pattern segment count must be non-negative.");
        }

        Value = value;
        Kind = kind;
        CaptureStart = captureStart;
        CaptureCount = captureCount;
        ConsumedSegmentCount = consumedSegmentCount;
        PatternSegmentCount = patternSegmentCount;
    }

    /// <summary>
    /// Gets the matched registration value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Gets the number of input segments consumed by this match.
    /// </summary>
    public int ConsumedSegmentCount { get; }

    /// <summary>
    /// Gets the number of segments in the pattern that produced the match.
    /// </summary>
    public int PatternSegmentCount { get; }

    /// <summary>
    /// Gets the shape of the pattern that produced the match.
    /// </summary>
    public PatternMatchKind Kind { get; }

    /// <summary>
    /// Gets the starting capture index for this match in the shared capture span.
    /// </summary>
    public int CaptureStart { get; }

    /// <summary>
    /// Gets the number of captures that belong to this match.
    /// </summary>
    public int CaptureCount { get; }

    /// <inheritdoc />
    public bool Equals(PatternMatch<TValue> other)
    {
        return EqualityComparer<TValue>.Default.Equals(Value, other.Value)
            && ConsumedSegmentCount == other.ConsumedSegmentCount
            && PatternSegmentCount == other.PatternSegmentCount
            && Kind == other.Kind
            && CaptureStart == other.CaptureStart
            && CaptureCount == other.CaptureCount;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PatternMatch<TValue> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Value);
        hash.Add(ConsumedSegmentCount);
        hash.Add(PatternSegmentCount);
        hash.Add(Kind);
        hash.Add(CaptureStart);
        hash.Add(CaptureCount);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Value} ({Kind})";
    }

    /// <summary>
    /// Compares two detailed match descriptors for equality.
    /// </summary>
    public static bool operator ==(PatternMatch<TValue> left, PatternMatch<TValue> right) => left.Equals(right);

    /// <summary>
    /// Compares two detailed match descriptors for inequality.
    /// </summary>
    public static bool operator !=(PatternMatch<TValue> left, PatternMatch<TValue> right) => !left.Equals(right);
}
