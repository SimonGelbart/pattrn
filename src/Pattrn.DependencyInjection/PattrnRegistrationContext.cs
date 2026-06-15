namespace Pattrn.DependencyInjection;

/// <summary>
/// Provides contextual information to an <see cref="IPattrnRegistrationSource{TSegment, TValue}"/> while a dependency-injected index is being built.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public sealed class PattrnRegistrationContext<TSegment, TValue>
    where TSegment : notnull
{
    private readonly PattrnIndexBuilder<TSegment, TValue> _builder;

    internal PattrnRegistrationContext(
        PattrnIndexBuilder<TSegment, TValue> builder,
        string? name)
    {
        _builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Name = name;
    }

    /// <summary>
    /// Gets the registered index name, or <see langword="null"/> for the default unnamed index.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets a value indicating whether the current index is named.
    /// </summary>
    public bool IsNamed => Name is not null;

    /// <summary>
    /// Gets a value indicating whether the current index is the default unnamed index.
    /// </summary>
    public bool IsDefault => Name is null;

    /// <summary>
    /// Adds a literal/wildcard pattern registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> Add(ReadOnlySpan<TSegment> pattern, TValue value)
    {
        _builder.Add(pattern, value);
        return this;
    }

    /// <summary>
    /// Adds a literal/wildcard pattern registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> Add(ReadOnlyMemory<TSegment> pattern, TValue value)
    {
        _builder.Add(pattern, value);
        return this;
    }

    /// <summary>
    /// Adds a literal/wildcard pattern registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> Add(IEnumerable<TSegment> pattern, TValue value)
    {
        _builder.Add(pattern, value);
        return this;
    }

    /// <summary>
    /// Adds a generic pattern-segment registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> AddPattern(ReadOnlySpan<PatternSegment<TSegment>> pattern, TValue value)
    {
        _builder.AddPattern(pattern, value);
        return this;
    }

    /// <summary>
    /// Adds a generic pattern-segment registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> AddPattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern, TValue value)
    {
        _builder.AddPattern(pattern, value);
        return this;
    }

    /// <summary>
    /// Adds a generic pattern-segment registration and returns this context for fluent source implementations.
    /// </summary>
    public PattrnRegistrationContext<TSegment, TValue> AddPattern(IEnumerable<PatternSegment<TSegment>> pattern, TValue value)
    {
        _builder.AddPattern(pattern, value);
        return this;
    }
}
