using System.Collections.Immutable;

namespace Pattrn;

/// <summary>
/// Convenience detailed match result returned by <see cref="PattrnIndex{TSegment, TValue}.MatchDetailedToArray"/>.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public readonly record struct PatternMatchDetailed<TSegment, TValue>(
    TValue Value,
    PatternMatchKind Kind,
    int PatternSegmentCount,
    int ConsumedSegmentCount,
    ImmutableArray<PatternCapture<TSegment>> Captures)
    where TSegment : notnull
{
    internal PatternMatchDetailed(
        TValue value,
        PatternMatchKind kind,
        int patternSegmentCount,
        int consumedSegmentCount,
        ImmutableArray<PatternCapture<TSegment>> captures,
        string? patternId,
        int registrationOrder,
        int specificity)
        : this(value, kind, patternSegmentCount, consumedSegmentCount, captures)
    {
        PatternId = patternId;
        RegistrationOrder = registrationOrder;
        Specificity = specificity;
    }

    /// <summary>
    /// Gets the optional caller-provided pattern identity associated with the matched registration.
    /// </summary>
    public string? PatternId { get; init; }

    /// <summary>
    /// Gets the zero-based order assigned when the registration was accepted by the builder.
    /// </summary>
    public int RegistrationOrder { get; init; } = -1;

    /// <summary>
    /// Gets the specificity score assigned to the pattern.
    /// </summary>
    public int Specificity { get; init; }

    /// <inheritdoc />
    public bool Equals(PatternMatchDetailed<TSegment, TValue> other)
    {
        if (!EqualityComparer<TValue>.Default.Equals(Value, other.Value)
            || Kind != other.Kind
            || PatternSegmentCount != other.PatternSegmentCount
            || ConsumedSegmentCount != other.ConsumedSegmentCount
            || !string.Equals(PatternId, other.PatternId, StringComparison.Ordinal)
            || RegistrationOrder != other.RegistrationOrder
            || Specificity != other.Specificity)
        {
            return false;
        }

        if (Captures.IsDefault || other.Captures.IsDefault)
        {
            return Captures.IsDefault == other.Captures.IsDefault;
        }

        return Captures.SequenceEqual(other.Captures);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Value);
        hash.Add(Kind);
        hash.Add(PatternSegmentCount);
        hash.Add(ConsumedSegmentCount);
        if (!Captures.IsDefault)
        {
            foreach (var capture in Captures)
            {
                hash.Add(capture);
            }
        }

        hash.Add(PatternId, StringComparer.Ordinal);
        hash.Add(RegistrationOrder);
        hash.Add(Specificity);
        return hash.ToHashCode();
    }
}
