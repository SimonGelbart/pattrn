namespace Pattrn;

/// <summary>
/// Provides an ergonomic string-facing builder over the generic string-segment core builder.
/// </summary>
/// <typeparam name="TValue">The stored value type.</typeparam>
/// <remarks>
/// This facade owns a <see cref="StringNormalizationOptions"/> instance and applies it consistently during registration
/// and matching. Domain-specific string semantics such as URL decoding, filesystem normalization, route constraints, and
/// globbing remain outside this package.
/// </remarks>
public sealed class StringPattrnIndexBuilder<TValue>
{
    private StringPattrnIndexBuilder(
        StringNormalizationOptions options,
        PattrnIndexBuilder<string, TValue> coreBuilder)
    {
        Options = options;
        CoreBuilder = coreBuilder;
    }

    /// <summary>
    /// Gets the normalization options applied by this builder.
    /// </summary>
    public StringNormalizationOptions Options { get; }

    /// <summary>
    /// Gets the underlying generic string-segment builder for advanced segmented operations.
    /// </summary>
    public PattrnIndexBuilder<string, TValue> CoreBuilder { get; }

    /// <summary>
    /// Creates a tokenless string-path builder.
    /// </summary>
    /// <param name="options">The string normalization options. Slash-separated defaults are used when omitted.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> Create(
        StringNormalizationOptions? options = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        options ??= StringNormalizationOptions.Slash;
        return new StringPattrnIndexBuilder<TValue>(options, options.CreateBuilder(valueComparer));
    }

    /// <summary>
    /// Creates a tokenless string-path builder for the specified separator.
    /// </summary>
    /// <param name="separator">The separator used to split registered patterns and matched paths.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> Create(
        char separator,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return Create(new StringNormalizationOptions(separator), valueComparer);
    }

    /// <summary>
    /// Creates a dotted-name string-path builder.
    /// </summary>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new dotted-name string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> CreateDotted(IEqualityComparer<TValue>? valueComparer = null)
    {
        return Create(StringNormalizationOptions.Dotted, valueComparer);
    }

    /// <summary>
    /// Creates a slash-separated string-path builder.
    /// </summary>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new slash-separated string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> CreateSlash(IEqualityComparer<TValue>? valueComparer = null)
    {
        return Create(StringNormalizationOptions.Slash, valueComparer);
    }

    /// <summary>
    /// Creates a tokenized string-path builder that treats one literal token in <see cref="Add"/> registrations as a wildcard segment.
    /// </summary>
    /// <param name="wildcardSegment">The reserved wildcard token used by tokenized <see cref="Add"/> registrations.</param>
    /// <param name="options">The string normalization options. Slash-separated defaults are used when omitted.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new tokenized string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> CreateTokenized(
        string wildcardSegment,
        StringNormalizationOptions? options = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        options ??= StringNormalizationOptions.Slash;
        var normalizedWildcardSegment = options.NormalizeSegmentValue(wildcardSegment, nameof(wildcardSegment));
        return new StringPattrnIndexBuilder<TValue>(
            options,
            options.CreateTokenizedBuilder(normalizedWildcardSegment, valueComparer));
    }

    /// <summary>
    /// Creates a tokenized string-path builder for the specified separator.
    /// </summary>
    /// <param name="separator">The separator used to split registered patterns and matched paths.</param>
    /// <param name="wildcardSegment">The reserved wildcard token used by tokenized <see cref="Add"/> registrations.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new tokenized string-path builder.</returns>
    public static StringPattrnIndexBuilder<TValue> CreateTokenized(
        char separator,
        string wildcardSegment,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return CreateTokenized(wildcardSegment, new StringNormalizationOptions(separator), valueComparer);
    }

    /// <summary>
    /// Configures duplicate pattern registration behavior on the underlying builder.
    /// </summary>
    /// <param name="behavior">The duplicate registration behavior to use.</param>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> UseDuplicatePatternRegistrationBehavior(
        DuplicatePatternRegistrationBehavior behavior)
    {
        CoreBuilder.UseDuplicatePatternRegistrationBehavior(behavior);
        return this;
    }

    /// <summary>
    /// Enables build validation with the specified minimum diagnostic severity.
    /// </summary>
    /// <param name="minimumSeverity">The minimum diagnostic severity that should fail <see cref="Build"/>.</param>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> ValidateOnBuild(
        PatternDiagnosticSeverity minimumSeverity = PatternDiagnosticSeverity.Warning)
    {
        CoreBuilder.ValidateOnBuild(minimumSeverity);
        return this;
    }

    /// <summary>
    /// Enables build validation with a caller-supplied diagnostic rejection predicate.
    /// </summary>
    /// <param name="rejectDiagnostic">A predicate that returns <see langword="true"/> for diagnostics that should fail <see cref="Build"/>.</param>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> ValidateOnBuild(
        Func<PatternDiagnostic<string>, bool> rejectDiagnostic)
    {
        CoreBuilder.ValidateOnBuild(rejectDiagnostic);
        return this;
    }

    /// <summary>
    /// Disables build validation on the underlying builder.
    /// </summary>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> DisableBuildValidation()
    {
        CoreBuilder.DisableBuildValidation();
        return this;
    }

    /// <summary>
    /// Registers a normalized string pattern.
    /// </summary>
    /// <param name="pattern">The separated string pattern.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current string-path builder.</returns>
    /// <remarks>
    /// On tokenless builders, all string segments are literals. On tokenized builders, segments equal to the configured wildcard token
    /// use the core tokenized wildcard convenience path. Use the explicit pattern registration overloads when wildcard, parameter, or catch-all
    /// segments are required without reserving a string token.
    /// </remarks>
    public StringPattrnIndexBuilder<TValue> Add(string pattern, TValue value, string? patternId = null)
    {
        CoreBuilder.Add(Options.Split(pattern, nameof(pattern)), value, patternId);
        return this;
    }

    /// <summary>
    /// Registers an explicit pattern-segment pattern after normalizing literal string segments.
    /// </summary>
    /// <param name="pattern">The explicit string pattern segments.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> AddPattern(
        ReadOnlySpan<PatternSegment<string>> pattern,
        TValue value,
        string? patternId = null)
    {
        CoreBuilder.AddPattern(NormalizePattern(pattern), value, patternId);
        return this;
    }

    /// <summary>
    /// Registers an explicit pattern-segment pattern after normalizing literal string segments.
    /// </summary>
    /// <param name="pattern">The explicit string pattern segments.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> AddPattern(
        IEnumerable<PatternSegment<string>> pattern,
        TValue value,
        string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return AddPattern(pattern.ToArray().AsSpan(), value, patternId);
    }

    /// <summary>
    /// Determines whether at least one value is registered for the normalized string pattern.
    /// </summary>
    /// <param name="pattern">The separated string pattern.</param>
    /// <returns><see langword="true"/> when the pattern is registered; otherwise, <see langword="false"/>.</returns>
    public bool Contains(string pattern)
    {
        return CoreBuilder.Contains(Options.Split(pattern, nameof(pattern)));
    }

    /// <summary>
    /// Removes one normalized string pattern/value registration.
    /// </summary>
    /// <param name="pattern">The separated string pattern.</param>
    /// <param name="value">The value to remove from that pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(string pattern, TValue value)
    {
        return CoreBuilder.Remove(Options.Split(pattern, nameof(pattern)), value);
    }

    /// <summary>
    /// Removes every value registered for the normalized string pattern.
    /// </summary>
    /// <param name="pattern">The separated string pattern.</param>
    /// <returns>The number of removed values.</returns>
    public int RemoveAll(string pattern)
    {
        return CoreBuilder.RemoveAll(Options.Split(pattern, nameof(pattern)));
    }

    /// <summary>
    /// Clears every registration from the underlying builder.
    /// </summary>
    /// <returns>The current string-path builder.</returns>
    public StringPattrnIndexBuilder<TValue> Clear()
    {
        CoreBuilder.Clear();
        return this;
    }

    /// <summary>
    /// Builds an immutable string-path index using the specified match options.
    /// </summary>
    /// <param name="options">The matching options for the compiled index.</param>
    /// <returns>An immutable string-path index wrapper.</returns>
    public StringPattrnIndex<TValue> Build(MatchOptions options = default)
    {
        return new StringPattrnIndex<TValue>(CoreBuilder.Build(options), Options);
    }

    private PatternSegment<string>[] NormalizePattern(ReadOnlySpan<PatternSegment<string>> pattern)
    {
        var normalized = new PatternSegment<string>[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            var segment = pattern[i];
            normalized[i] = segment.Kind switch
            {
                PatternSegmentKind.Literal => PatternSegment<string>.Literal(
                    Options.NormalizeSegmentValue(segment.LiteralValue, nameof(pattern))),
                PatternSegmentKind.Parameter => segment.ParameterName is null
                    ? PatternSegment<string>.Wildcard()
                    : PatternSegment<string>.Parameter(segment.ParameterName),
                PatternSegmentKind.CatchAll => segment.ParameterName is null
                    ? PatternSegment<string>.CatchAll()
                    : PatternSegment<string>.CatchAll(segment.ParameterName),
                _ => PatternSegment<string>.Wildcard()
            };
        }

        return normalized;
    }
}
