using System.Collections.Immutable;

#pragma warning disable CS1591
namespace Pattrn.Builders;

/// <summary>Single-writer mutable source of canonical registrations.</summary>
/// <typeparam name="TSegment">The segment type used by patterns and inputs.</typeparam>
/// <typeparam name="TValue">The registered value type.</typeparam>
public sealed class PattrnIndexBuilder<TSegment, TValue>
    where TSegment : notnull
{
    private readonly List<PattrnRegistration<TSegment, TValue>> _registrations = [];
    private readonly bool _usesWildcardSegmentToken;

    private PattrnIndexBuilder(
        TSegment wildcardSegment,
        bool usesWildcardSegmentToken,
        IEqualityComparer<TSegment> segmentComparer,
        IEqualityComparer<TValue> valueComparer)
    {
        WildcardSegment = wildcardSegment;
        _usesWildcardSegmentToken = usesWildcardSegmentToken;
        SegmentComparer = segmentComparer;
        ValueComparer = valueComparer;
    }

    public TSegment WildcardSegment { get; }
    public bool UsesWildcardSegmentToken => _usesWildcardSegmentToken;
    public IEqualityComparer<TSegment> SegmentComparer { get; }
    public IEqualityComparer<TValue> ValueComparer { get; }
    public int RegistrationCount => _registrations.Count;
    public int PatternCount => CountDistinctPatterns(_registrations, SegmentComparer);
    public int MatchCountUpperBound => RegistrationCount;

    public static PattrnIndexBuilder<TSegment, TValue> Create(
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
        => new(
            default!,
            usesWildcardSegmentToken: false,
            segmentComparer ?? EqualityComparer<TSegment>.Default,
            valueComparer ?? EqualityComparer<TValue>.Default);

    public static PattrnIndexBuilder<TSegment, TValue> Create(
        TSegment wildcardSegment,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        ArgumentNullException.ThrowIfNull(wildcardSegment);
        return new(
            wildcardSegment,
            usesWildcardSegmentToken: true,
            segmentComparer ?? EqualityComparer<TSegment>.Default,
            valueComparer ?? EqualityComparer<TValue>.Default);
    }

    public RegistrationId Add(PattrnRegistration<TSegment, TValue> registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (registration.Id.Value == Guid.Empty || _registrations.Any(existing => existing.Id == registration.Id))
        {
            throw new ArgumentException("Registration identity must be non-empty and unique in the builder.", nameof(registration));
        }

        _registrations.Add(registration);
        return registration.Id;
    }

    public RegistrationId Add(
        IReadOnlyList<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
        => Add(PattrnRegistration<TSegment, TValue>.Create(pattern, value, name));

    public PattrnIndexBuilder<TSegment, TValue> Add(
        ReadOnlySpan<TSegment> pattern,
        TValue value,
        string? name = null)
    {
        var canonical = new PatternSegment<TSegment>[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            canonical[i] = _usesWildcardSegmentToken && SegmentComparer.Equals(pattern[i], WildcardSegment)
                ? PatternSegment<TSegment>.Wildcard()
                : PatternSegment<TSegment>.Literal(pattern[i]);
        }

        Add(PattrnRegistration<TSegment, TValue>.Create(canonical, value, name));
        return this;
    }

    public PattrnIndexBuilder<TSegment, TValue> Add(
        TSegment[] pattern,
        TValue value,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Add(pattern.AsSpan(), value, name);
    }

    public PattrnIndexBuilder<TSegment, TValue> Add(
        ReadOnlyMemory<TSegment> pattern,
        TValue value,
        string? name = null)
        => Add(pattern.Span, value, name);

    public PattrnIndexBuilder<TSegment, TValue> Add(
        IEnumerable<TSegment> pattern,
        TValue value,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Add(pattern is TSegment[] array ? array.AsSpan() : pattern.ToArray().AsSpan(), value, name);
    }

    public PattrnIndexBuilder<TSegment, TValue> AddPattern(
        ReadOnlySpan<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
    {
        Add(PattrnRegistration<TSegment, TValue>.Create(pattern.ToArray(), value, name));
        return this;
    }

    public PattrnIndexBuilder<TSegment, TValue> AddPattern(
        ReadOnlyMemory<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
        => AddPattern(pattern.Span, value, name);

    public PattrnIndexBuilder<TSegment, TValue> AddPattern(
        IEnumerable<PatternSegment<TSegment>> pattern,
        TValue value,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return AddPattern(pattern is PatternSegment<TSegment>[] array ? array.AsSpan() : pattern.ToArray().AsSpan(), value, name);
    }

    public PattrnIndexBuilder<TSegment, TValue> AddRange(
        IEnumerable<(IEnumerable<TSegment> Pattern, TValue Value)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        foreach (var (pattern, value) in registrations)
        {
            Add(pattern, value);
        }

        return this;
    }

    public PattrnIndexBuilder<TSegment, TValue> AddRange(
        IEnumerable<(IEnumerable<TSegment> Pattern, TValue Value, string? Name)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        foreach (var (pattern, value, name) in registrations)
        {
            Add(pattern, value, name);
        }

        return this;
    }

    public PattrnIndexBuilder<TSegment, TValue> AddPatternRange(
        IEnumerable<(IEnumerable<PatternSegment<TSegment>> Pattern, TValue Value)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        foreach (var (pattern, value) in registrations)
        {
            AddPattern(pattern, value);
        }

        return this;
    }

    public PattrnIndexBuilder<TSegment, TValue> AddPatternRange(
        IEnumerable<(IEnumerable<PatternSegment<TSegment>> Pattern, TValue Value, string? Name)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        foreach (var (pattern, value, name) in registrations)
        {
            AddPattern(pattern, value, name);
        }

        return this;
    }

    public bool Replace(PattrnRegistration<TSegment, TValue> updatedRegistration)
    {
        ArgumentNullException.ThrowIfNull(updatedRegistration);
        var index = _registrations.FindIndex(registration => registration.Id == updatedRegistration.Id);
        if (index < 0)
        {
            return false;
        }

        _registrations[index] = updatedRegistration;
        return true;
    }

    public bool Remove(RegistrationId id)
    {
        var index = _registrations.FindIndex(registration => registration.Id == id);
        if (index < 0)
        {
            return false;
        }

        _registrations.RemoveAt(index);
        return true;
    }

    public void Clear() => _registrations.Clear();

    public ImmutableArray<PattrnRegistration<TSegment, TValue>> ToRegistrations() => [.. _registrations];

    public bool Contains(ReadOnlySpan<TSegment> pattern)
    {
        var canonical = new PatternSegment<TSegment>[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            canonical[i] = _usesWildcardSegmentToken && SegmentComparer.Equals(pattern[i], WildcardSegment)
                ? PatternSegment<TSegment>.Wildcard()
                : PatternSegment<TSegment>.Literal(pattern[i]);
        }

        return ContainsPattern(canonical);
    }

    public bool ContainsPattern(ReadOnlySpan<PatternSegment<TSegment>> pattern)
    {
        var requested = pattern.ToArray();
        return _registrations.Any(registration => SamePattern(registration.Pattern, requested.ToImmutableArray(), SegmentComparer));
    }

    public bool Contains(ReadOnlyMemory<TSegment> pattern) => Contains(pattern.Span);
    public bool ContainsPattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern) => ContainsPattern(pattern.Span);

    public bool Contains(IEnumerable<TSegment> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Contains(pattern is TSegment[] array ? array.AsSpan() : pattern.ToArray().AsSpan());
    }

    public bool ContainsPattern(IEnumerable<PatternSegment<TSegment>> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return ContainsPattern(pattern is PatternSegment<TSegment>[] array ? array.AsSpan() : pattern.ToArray().AsSpan());
    }

    public PattrnIndex<TSegment, TValue> Build(PattrnCompileOptions? options = null)
        => PattrnIndex<TSegment, TValue>.Compile(ToRegistrations(), options, SegmentComparer, ValueComparer);

    public PattrnIndex<TSegment, TValue> Build(MatchOptions matchOptions)
        => PattrnIndex<TSegment, TValue>.Compile(ToRegistrations(), PattrnCompileOptions.Default, matchOptions, SegmentComparer, ValueComparer);

    public PattrnCompileResult<TSegment, TValue> BuildWithDiagnostics(PattrnCompileOptions? options = null)
        => PattrnIndex<TSegment, TValue>.CompileWithDiagnostics(ToRegistrations(), options, SegmentComparer, ValueComparer);

    public PattrnCompileResult<TSegment, TValue> BuildWithDiagnostics(
        PattrnCompileOptions? compileOptions,
        MatchOptions matchOptions)
        => PattrnIndex<TSegment, TValue>.CompileWithDiagnostics(ToRegistrations(), compileOptions, matchOptions, SegmentComparer, ValueComparer);

    private static int CountDistinctPatterns(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        IEqualityComparer<TSegment> comparer)
    {
        var distinct = new List<ImmutableArray<PatternSegment<TSegment>>>();
        foreach (var registration in registrations)
        {
            if (!distinct.Any(existing => SamePattern(existing, registration.Pattern, comparer)))
            {
                distinct.Add(registration.Pattern);
            }
        }

        return distinct.Count;
    }

    private static bool SamePattern(
        ImmutableArray<PatternSegment<TSegment>> left,
        ImmutableArray<PatternSegment<TSegment>> right,
        IEqualityComparer<TSegment> comparer)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            left[i].Deconstruct(out var leftKind, out var leftLiteral, out var leftName);
            right[i].Deconstruct(out var rightKind, out var rightLiteral, out var rightName);
            if (leftKind != rightKind
                || (leftKind == PatternSegmentKind.Literal && !comparer.Equals(leftLiteral, rightLiteral))
                || !string.Equals(leftName, rightName, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
