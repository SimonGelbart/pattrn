namespace Pattrn;

/// <summary>
/// Convenience detailed match result returned by <see cref="PattrnIndex{TSegment, TValue}.MatchDetailedToArray"/>.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public sealed class PatternMatchResult<TSegment, TValue>
    where TSegment : notnull
{
    internal PatternMatchResult(
        TValue value,
        string? patternId,
        int registrationOrder,
        PatternMatchKind kind,
        int specificity,
        PatternCapture<TSegment>[] captures)
    {
        Value = value;
        PatternId = patternId;
        RegistrationOrder = registrationOrder;
        Kind = kind;
        Specificity = specificity;
        Captures = captures;
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
    public int RegistrationOrder { get; }

    /// <summary>
    /// Gets the shape of the pattern that produced the match.
    /// </summary>
    public PatternMatchKind Kind { get; }

    /// <summary>
    /// Gets the specificity score assigned to the pattern.
    /// </summary>
    public int Specificity { get; }

    /// <summary>
    /// Gets the named captures for this match.
    /// </summary>
    public IReadOnlyList<PatternCapture<TSegment>> Captures { get; }
}
