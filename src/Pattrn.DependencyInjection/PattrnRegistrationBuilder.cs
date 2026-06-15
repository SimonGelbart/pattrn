using Microsoft.Extensions.DependencyInjection;

namespace Pattrn.DependencyInjection;

/// <summary>
/// Provides a fluent configuration surface for registering a dependency-injected <see cref="PattrnIndex{TSegment, TValue}"/>.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
public sealed class PattrnRegistrationBuilder<TSegment, TValue>
    where TSegment : notnull
{
    private readonly List<Action<PattrnIndexBuilder<TSegment, TValue>>> _builderConfigurations = [];
    private readonly PattrnOptions<TSegment, TValue> _options = new();
    private bool _includeRegisteredSources;

    internal PattrnRegistrationBuilder(string? name)
    {
        if (name is not null)
        {
            ThrowIfInvalidName(name);
        }

        Name = name;
    }

    /// <summary>
    /// Gets the registered index name, or <see langword="null"/> for the default unnamed index.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets a value indicating whether this registration targets the default unnamed index.
    /// </summary>
    public bool IsDefault => Name is null;

    /// <summary>
    /// Gets a value indicating whether this registration targets a named index.
    /// </summary>
    public bool IsNamed => Name is not null;

    /// <summary>
    /// Configures the segment value that represents a single-segment wildcard in tokenized registered patterns.
    /// </summary>
    /// <remarks>
    /// Omit this configuration for a tokenless builder and use explicit <see cref="PatternSegment{TSegment}"/> registrations through <see cref="Configure(Action{PattrnIndexBuilder{TSegment, TValue}})"/> or registration sources.
    /// </remarks>
    public PattrnRegistrationBuilder<TSegment, TValue> UseWildcard(TSegment wildcardSegment)
    {
        _options.WildcardSegment = wildcardSegment;
        return this;
    }

    /// <summary>
    /// Configures the matching options used when compiling the index.
    /// </summary>
    public PattrnRegistrationBuilder<TSegment, TValue> UseMatchOptions(MatchOptions matchOptions)
    {
        _options.MatchOptions = matchOptions;
        return this;
    }

    /// <summary>
    /// Configures the comparer used for exact segment lookup and optional tokenized wildcard detection.
    /// </summary>
    public PattrnRegistrationBuilder<TSegment, TValue> UseSegmentComparer(IEqualityComparer<TSegment>? segmentComparer)
    {
        _options.SegmentComparer = segmentComparer;
        return this;
    }

    /// <summary>
    /// Configures the comparer used for removal and optional match-result deduplication.
    /// </summary>
    public PattrnRegistrationBuilder<TSegment, TValue> UseValueComparer(IEqualityComparer<TValue>? valueComparer)
    {
        _options.ValueComparer = valueComparer;
        return this;
    }

    /// <summary>
    /// Adds a builder configuration callback that contributes pattern/value registrations.
    /// </summary>
    public PattrnRegistrationBuilder<TSegment, TValue> Configure(
        Action<PattrnIndexBuilder<TSegment, TValue>> configureBuilder)
    {
        ArgumentNullException.ThrowIfNull(configureBuilder);
        _builderConfigurations.Add(configureBuilder);
        return this;
    }

    /// <summary>
    /// Includes all registered <see cref="IPattrnRegistrationSource{TSegment, TValue}"/> instances when this index is built.
    /// </summary>
    public PattrnRegistrationBuilder<TSegment, TValue> FromRegisteredSources()
    {
        _includeRegisteredSources = true;
        return this;
    }

    internal PattrnIndexRegistration<TSegment, TValue> BuildRegistration()
    {
        var options = PattrnOptions<TSegment, TValue>.FromValues(
            _options.HasWildcardSegment ? _options.WildcardSegment : default!,
            _options.HasWildcardSegment,
            _options.MatchOptions,
            _options.SegmentComparer,
            _options.ValueComparer);

        return new PattrnIndexRegistration<TSegment, TValue>(
            Name,
            options,
            _builderConfigurations.ToArray(),
            _includeRegisteredSources);
    }

    private static void ThrowIfInvalidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("The index name cannot be null, empty, or white space.", nameof(name));
        }
    }
}

internal sealed class PattrnIndexRegistration<TSegment, TValue>
    where TSegment : notnull
{
    private readonly Action<PattrnIndexBuilder<TSegment, TValue>>[] _builderConfigurations;
    private readonly bool _includeRegisteredSources;

    internal PattrnIndexRegistration(
        string? name,
        PattrnOptions<TSegment, TValue> options,
        Action<PattrnIndexBuilder<TSegment, TValue>>[] builderConfigurations,
        bool includeRegisteredSources)
    {
        Name = name;
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _builderConfigurations = builderConfigurations ?? throw new ArgumentNullException(nameof(builderConfigurations));
        _includeRegisteredSources = includeRegisteredSources;
    }

    internal string? Name { get; }

    internal PattrnOptions<TSegment, TValue> Options { get; }

    internal void Configure(IServiceProvider serviceProvider, PattrnIndexBuilder<TSegment, TValue> builder)
    {
        foreach (var configureBuilder in _builderConfigurations)
        {
            configureBuilder(builder);
        }

        if (!_includeRegisteredSources)
        {
            return;
        }

        var context = new PattrnRegistrationContext<TSegment, TValue>(builder, Name);
        foreach (var source in serviceProvider.GetServices<IPattrnRegistrationSource<TSegment, TValue>>())
        {
            source.AddRegistrations(context);
        }
    }
}
