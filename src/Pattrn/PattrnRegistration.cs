using System.Collections.Immutable;
#pragma warning disable CS1591

namespace Pattrn;

/// <summary>The immutable canonical input to Pattrn compilation.</summary>
public sealed record PattrnRegistration<TSegment, TValue>
    where TSegment : notnull
{
    public PattrnRegistration(
        RegistrationId id,
        IReadOnlyList<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        if (id == default)
        {
            throw new ArgumentException("Registration identity must be non-empty.", nameof(id));
        }

        Id = id;
        Pattern = pattern.ToImmutableArray();
        Value = value;
        Name = name;
    }

    public RegistrationId Id { get; }
    public ImmutableArray<PatternSegment<TSegment>> Pattern { get; }
    public TValue Value { get; }
    public string? Name { get; }

    public static PattrnRegistration<TSegment, TValue> Create(
        IReadOnlyList<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null) =>
        new(RegistrationId.New(), pattern, value, name);
}
