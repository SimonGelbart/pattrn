namespace Pattrn;

/// <summary>Canonical registration convenience methods for mutable builders.</summary>
public static class PattrnIndexBuilderCanonicalExtensions
{
    /// <summary>Adds an explicit-segment registration and returns its generated identity.</summary>
    public static RegistrationId Add<TSegment, TValue>(
        this PattrnIndexBuilder<TSegment, TValue> builder,
        IReadOnlyList<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
        where TSegment : notnull
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Add(PattrnRegistration<TSegment, TValue>.Create(pattern, value, name));
    }
}
