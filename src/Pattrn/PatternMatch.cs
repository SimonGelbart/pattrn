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
    /// <param name="specificity">The specificity score assigned to the pattern.</param>
    /// <param name="captureStart">The starting capture index for this match in the shared capture span.</param>
    /// <param name="captureCount">The number of captures that belong to this match.</param>
    /// <param name="patternId">The optional caller-provided pattern identity associated with the matched registration.</param>
    /// <param name="registrationOrder">The zero-based order assigned when the registration was accepted by the builder, or <c>-1</c> for manually created descriptors.</param>
    /// <param name="consumedSegmentCount">The number of input segments consumed by this match.</param>
    public PatternMatch(
        TValue value,
        PatternMatchKind kind,
        int specificity,
        int captureStart,
        int captureCount,
        string? patternId = null,
        int registrationOrder = -1,
        int consumedSegmentCount = 0)
    {
        if (captureStart < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureStart), captureStart, "Capture start must be non-negative.");
        }

        if (captureCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureCount), captureCount, "Capture count must be non-negative.");
        }

        if (registrationOrder < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(registrationOrder), registrationOrder, "Registration order must be -1 or non-negative.");
        }

        if (consumedSegmentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(consumedSegmentCount), consumedSegmentCount, "Consumed segment count must be non-negative.");
        }

        Value = value;
        Kind = kind;
        Specificity = specificity;
        CaptureStart = captureStart;
        CaptureCount = captureCount;
        PatternId = patternId;
        RegistrationOrder = registrationOrder;
        ConsumedSegmentCount = consumedSegmentCount;
    }

    /// <summary>
    /// Gets the matched registration value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Gets the optional caller-provided pattern identity associated with the matched registration.
    /// </summary>
    public string? PatternId { get; }

    /// <summary>
    /// Gets the zero-based order assigned when the registration was accepted by the builder.
    /// </summary>
    /// <remarks>
    /// A value of <c>-1</c> means the descriptor was created manually rather than by a compiled index.
    /// </remarks>
    public int RegistrationOrder { get; }

    /// <summary>
    /// Gets the number of input segments consumed by this match.
    /// </summary>
    public int ConsumedSegmentCount { get; }

    /// <summary>
    /// Gets the shape of the pattern that produced the match.
    /// </summary>
    public PatternMatchKind Kind { get; }

    /// <summary>
    /// Gets the specificity score assigned to the pattern.
    /// </summary>
    public int Specificity { get; }

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
            && string.Equals(PatternId, other.PatternId, StringComparison.Ordinal)
            && RegistrationOrder == other.RegistrationOrder
            && ConsumedSegmentCount == other.ConsumedSegmentCount
            && Kind == other.Kind
            && Specificity == other.Specificity
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
        hash.Add(PatternId, StringComparer.Ordinal);
        hash.Add(RegistrationOrder);
        hash.Add(ConsumedSegmentCount);
        hash.Add(Kind);
        hash.Add(Specificity);
        hash.Add(CaptureStart);
        hash.Add(CaptureCount);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var identity = PatternId is null ? $"registration {RegistrationOrder}" : $"pattern '{PatternId}', registration {RegistrationOrder}";
        return $"{Value} ({Kind}, specificity {Specificity}, {identity})";
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
