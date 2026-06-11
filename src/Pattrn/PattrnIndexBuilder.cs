namespace Pattrn;

/// <summary>
/// Builds an immutable <see cref="PattrnIndex{TSegment, TValue}"/> from segmented path patterns.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// The builder is mutable and is not thread-safe. The compiled index returned by <see cref="Build"/> is immutable
/// and can be shared safely between concurrent readers.
/// </remarks>
public sealed class PattrnIndexBuilder<TSegment, TValue>
    where TSegment : notnull
{
    private readonly BuilderNode<TSegment, TValue> _root;
    private readonly bool _usesWildcardSegmentToken;
    private int _patternCount;
    private int _registrationCount;
    private int _nextRegistrationOrder;
    private DuplicatePatternRegistrationBehavior _duplicatePatternBehavior;
    private Func<PatternDiagnostic<TSegment>, bool>? _buildValidationPredicate;

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
        _root = new BuilderNode<TSegment, TValue>(segmentComparer);
    }

    /// <summary>
    /// Gets the segment value that represents a single-segment wildcard in tokenized pattern registrations.
    /// </summary>
    /// <remarks>
    /// This value is meaningful only when <see cref="UsesWildcardSegmentToken"/> is <see langword="true"/>.
    /// Tokenless builders use explicit <see cref="PatternSegment{TSegment}"/> values and treat all <c>Add(...)</c> segments as literals.
    /// </remarks>
    public TSegment WildcardSegment { get; }

    /// <summary>
    /// Gets a value indicating whether tokenized <c>Add(...)</c> registrations interpret <see cref="WildcardSegment"/> as a wildcard.
    /// </summary>
    public bool UsesWildcardSegmentToken => _usesWildcardSegmentToken;

    /// <summary>
    /// Gets the comparer used for exact segment lookup and optional tokenized wildcard detection.
    /// </summary>
    public IEqualityComparer<TSegment> SegmentComparer { get; }

    /// <summary>
    /// Gets the comparer used for removal and optional match-result deduplication.
    /// </summary>
    public IEqualityComparer<TValue> ValueComparer { get; }

    /// <summary>
    /// Gets the number of distinct patterns currently stored in the builder.
    /// </summary>
    public int PatternCount => _patternCount;

    /// <summary>
    /// Gets the number of pattern/value registrations currently stored in the builder.
    /// </summary>
    public int RegistrationCount => _registrationCount;

    /// <summary>
    /// Gets the maximum number of values that a single match operation emitted by a built index can emit.
    /// </summary>
    public int MatchCountUpperBound => _registrationCount;

    /// <summary>
    /// Gets the policy used when registering a value for a structural pattern that already exists.
    /// </summary>
    public DuplicatePatternRegistrationBehavior DuplicatePatternRegistrationBehavior => _duplicatePatternBehavior;

    /// <summary>
    /// Creates a new mutable tokenless builder.
    /// </summary>
    /// <param name="segmentComparer">The comparer used for exact segment lookup.</param>
    /// <param name="valueComparer">The comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable builder.</returns>
    /// <remarks>
    /// Tokenless builders make explicit <see cref="PatternSegment{TSegment}"/> registrations the primary model.
    /// The convenience <c>Add(...)</c> overload registers literal-only patterns on a tokenless builder.
    /// Use <c>AddPattern(...)</c> to register wildcards, parameters, and catch-alls.
    /// </remarks>
    public static PattrnIndexBuilder<TSegment, TValue> Create(
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return new PattrnIndexBuilder<TSegment, TValue>(
            default!,
            usesWildcardSegmentToken: false,
            segmentComparer ?? EqualityComparer<TSegment>.Default,
            valueComparer ?? EqualityComparer<TValue>.Default);
    }

    /// <summary>
    /// Creates a new mutable builder that treats a reserved segment value as a single-segment wildcard in tokenized registrations.
    /// </summary>
    /// <param name="wildcardSegment">The segment value that represents a single-segment wildcard in tokenized registered patterns.</param>
    /// <param name="segmentComparer">The comparer used for exact segment lookup and wildcard detection.</param>
    /// <param name="valueComparer">The comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wildcardSegment"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This overload is retained as a convenience for callers that intentionally reserve a wildcard token.
    /// Prefer the tokenless <see cref="Create(IEqualityComparer{TSegment}?, IEqualityComparer{TValue}?)"/> overload plus explicit <see cref="PatternSegment{TSegment}"/> registrations for new core usage.
    /// </remarks>
    public static PattrnIndexBuilder<TSegment, TValue> Create(
        TSegment wildcardSegment,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        ArgumentNullException.ThrowIfNull(wildcardSegment);

        return new PattrnIndexBuilder<TSegment, TValue>(
            wildcardSegment,
            usesWildcardSegmentToken: true,
            segmentComparer ?? EqualityComparer<TSegment>.Default,
            valueComparer ?? EqualityComparer<TValue>.Default);
    }

    /// <summary>
    /// Sets the policy used when registering a value for a structural pattern that already exists.
    /// </summary>
    /// <param name="behavior">The duplicate-pattern behavior to use for subsequent registrations.</param>
    /// <returns>The current builder, allowing fluent configuration chains.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="behavior"/> is not a defined behavior.</exception>
    /// <remarks>
    /// This policy applies at registration time and is independent of <see cref="DuplicateValueMatchMode"/>,
    /// which controls duplicate values emitted by a compiled index at match time.
    /// </remarks>
    public PattrnIndexBuilder<TSegment, TValue> UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior behavior)
    {
        if (!Enum.IsDefined(behavior))
        {
            throw new ArgumentOutOfRangeException(nameof(behavior), "Unknown duplicate pattern behavior.");
        }

        _duplicatePatternBehavior = behavior;
        return this;
    }

    /// <summary>
    /// Enables build validation that rejects diagnostics at or above the specified severity.
    /// </summary>
    /// <param name="minimumSeverity">The minimum diagnostic severity that should fail <see cref="Build"/>.</param>
    /// <returns>The current builder, allowing fluent configuration chains.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minimumSeverity"/> is not a defined severity.</exception>
    public PattrnIndexBuilder<TSegment, TValue> ValidateOnBuild(
        PatternDiagnosticSeverity minimumSeverity = PatternDiagnosticSeverity.Warning)
    {
        if (!Enum.IsDefined(minimumSeverity))
        {
            throw new ArgumentOutOfRangeException(nameof(minimumSeverity), "Unknown diagnostic severity.");
        }

        _buildValidationPredicate = diagnostic => diagnostic.Severity >= minimumSeverity;
        return this;
    }

    /// <summary>
    /// Enables build validation with a caller-supplied diagnostic rejection predicate.
    /// </summary>
    /// <param name="rejectDiagnostic">A predicate that returns <see langword="true"/> for diagnostics that should fail <see cref="Build"/>.</param>
    /// <returns>The current builder, allowing fluent configuration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rejectDiagnostic"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The predicate is evaluated only during <see cref="Build"/>. Matching remains unaffected. This hook is intended for
    /// domain-specific strictness in companion packages or applications without putting domain rules into the core.
    /// </remarks>
    public PattrnIndexBuilder<TSegment, TValue> ValidateOnBuild(
        Func<PatternDiagnostic<TSegment>, bool> rejectDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(rejectDiagnostic);

        _buildValidationPredicate = rejectDiagnostic;
        return this;
    }

    /// <summary>
    /// Disables build validation.
    /// </summary>
    /// <returns>The current builder, allowing fluent configuration chains.</returns>
    public PattrnIndexBuilder<TSegment, TValue> DisableBuildValidation()
    {
        _buildValidationPredicate = null;
        return this;
    }

    /// <summary>
    /// Registers a value for the specified segmented pattern.
    /// </summary>
    /// <param name="pattern">The pattern to register. On tokenized builders, segments equal to the configured wildcard segment match any single input segment; on tokenless builders, all segments are literals.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    public PattrnIndexBuilder<TSegment, TValue> Add(ReadOnlySpan<TSegment> pattern, TValue value, string? patternId = null)
    {
        var node = _root;
        var hasWildcard = false;
        var score = 0;

        foreach (var segment in pattern)
        {
            if (_usesWildcardSegmentToken && SegmentComparer.Equals(segment, WildcardSegment))
            {
                node.WildcardChild ??= new BuilderNode<TSegment, TValue>(SegmentComparer);
                node = node.WildcardChild;
                hasWildcard = true;
                score += 10;
                continue;
            }

            node = node.GetOrAddChild(segment);
            score += 100;
        }

        var kind = hasWildcard ? PatternMatchKind.Wildcard : PatternMatchKind.Exact;
        AddRegistration(node, value, new BuilderRegistrationMetadata([], kind, score, patternId));
        return this;
    }

    /// <summary>
    /// Registers a value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to register. Literal segments match exact values; wildcard and parameter segments match any single input segment.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <remarks>
    /// This overload is domain-neutral and does not parse route strings. Named parameters are matched as single-segment wildcard branches and are exposed by the detailed match-result API.
    /// Unlike the legacy segment overload, <see cref="PatternSegment{TSegment}.Literal(TSegment)"/> can register a literal value equal to <see cref="WildcardSegment"/>.
    /// </remarks>
    public PattrnIndexBuilder<TSegment, TValue> AddPattern(ReadOnlySpan<PatternSegment<TSegment>> pattern, TValue value, string? patternId = null)
    {
        var node = _root;
        var captures = new List<CaptureDescriptor>();
        var hasParameter = false;
        var hasWildcard = false;
        var hasCatchAll = false;
        var score = 0;

        for (var i = 0; i < pattern.Length; i++)
        {
            var segment = pattern[i];
            switch (segment.Kind)
            {
                case PatternSegmentKind.Literal:
                    node = node.GetOrAddChild(segment.LiteralValue);
                    score += 100;
                    break;

                case PatternSegmentKind.Parameter:
                    node.WildcardChild ??= new BuilderNode<TSegment, TValue>(SegmentComparer);
                    node = node.WildcardChild;
                    captures.Add(new CaptureDescriptor(segment.ParameterName!, i));
                    hasParameter = true;
                    score += 50;
                    break;

                case PatternSegmentKind.Wildcard:
                    node.WildcardChild ??= new BuilderNode<TSegment, TValue>(SegmentComparer);
                    node = node.WildcardChild;
                    hasWildcard = true;
                    score += 10;
                    break;

                case PatternSegmentKind.CatchAll:
                    if (i != pattern.Length - 1)
                    {
                        throw new ArgumentException("Catch-all pattern segments must be terminal.", nameof(pattern));
                    }

                    node.CatchAllChild ??= new BuilderNode<TSegment, TValue>(SegmentComparer);
                    node = node.CatchAllChild;
                    if (segment.ParameterName is not null)
                    {
                        captures.Add(new CaptureDescriptor(segment.ParameterName, i, isCatchAll: true));
                    }

                    hasCatchAll = true;
                    score += 1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(pattern), "Unknown pattern segment kind.");
            }
        }

        var kind = GetMatchKind(hasParameter, hasWildcard, hasCatchAll);
        AddRegistration(node, value, new BuilderRegistrationMetadata([.. captures], kind, score, patternId));
        return this;
    }

    /// <summary>
    /// Registers a value for the specified segmented pattern.
    /// </summary>
    /// <param name="pattern">The pattern to register. Segments equal to the configured wildcard segment match any single input segment.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    public PattrnIndexBuilder<TSegment, TValue> Add(ReadOnlyMemory<TSegment> pattern, TValue value, string? patternId = null) => Add(pattern.Span, value, patternId);


    /// <summary>
    /// Registers a value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to register. Literal segments match exact values; wildcard and parameter segments match any single input segment.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    public PattrnIndexBuilder<TSegment, TValue> AddPattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern, TValue value, string? patternId = null) => AddPattern(pattern.Span, value, patternId);

    /// <summary>
    /// Registers a value for the specified segmented pattern.
    /// </summary>
    /// <param name="pattern">The pattern to register. Segments equal to the configured wildcard segment match any single input segment.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public PattrnIndexBuilder<TSegment, TValue> Add(IEnumerable<TSegment> pattern, TValue value, string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is TSegment[] array)
        {
            return Add(array.AsSpan(), value, patternId);
        }

        return Add(pattern.ToArray().AsSpan(), value, patternId);
    }


    /// <summary>
    /// Registers a value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to register. Literal segments match exact values; wildcard and parameter segments match any single input segment.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public PattrnIndexBuilder<TSegment, TValue> AddPattern(IEnumerable<PatternSegment<TSegment>> pattern, TValue value, string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is PatternSegment<TSegment>[] array)
        {
            return AddPattern(array.AsSpan(), value, patternId);
        }

        return AddPattern(pattern.ToArray().AsSpan(), value, patternId);
    }

    /// <summary>
    /// Registers a sequence of pattern/value registrations.
    /// </summary>
    /// <param name="registrations">The registrations to add.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is <see langword="null"/>.</exception>
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


    /// <summary>
    /// Registers a sequence of identified pattern/value registrations.
    /// </summary>
    /// <param name="registrations">The identified registrations to add.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is <see langword="null"/>.</exception>
    public PattrnIndexBuilder<TSegment, TValue> AddRange(
        IEnumerable<(IEnumerable<TSegment> Pattern, TValue Value, string? PatternId)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        foreach (var (pattern, value, patternId) in registrations)
        {
            Add(pattern, value, patternId);
        }

        return this;
    }


    /// <summary>
    /// Registers a sequence of generic pattern-segment/value registrations.
    /// </summary>
    /// <param name="registrations">The registrations to add.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Registers a sequence of identified generic pattern-segment/value registrations.
    /// </summary>
    /// <param name="registrations">The identified registrations to add.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="registrations"/> is <see langword="null"/>.</exception>
    public PattrnIndexBuilder<TSegment, TValue> AddPatternRange(
        IEnumerable<(IEnumerable<PatternSegment<TSegment>> Pattern, TValue Value, string? PatternId)> registrations)
    {
        ArgumentNullException.ThrowIfNull(registrations);

        foreach (var (pattern, value, patternId) in registrations)
        {
            AddPattern(pattern, value, patternId);
        }

        return this;
    }

    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    public bool Contains(ReadOnlySpan<TSegment> pattern)
    {
        var node = FindNode(pattern);
        return node?.Values is { Count: > 0 };
    }


    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsPattern(ReadOnlySpan<PatternSegment<TSegment>> pattern)
    {
        var node = FindNode(pattern);
        return node?.Values is { Count: > 0 };
    }

    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    public bool Contains(ReadOnlyMemory<TSegment> pattern) => Contains(pattern.Span);


    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    public bool ContainsPattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern) => ContainsPattern(pattern.Span);

    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public bool Contains(IEnumerable<TSegment> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is TSegment[] array)
        {
            return Contains(array.AsSpan());
        }

        return Contains(pattern.ToArray().AsSpan());
    }


    /// <summary>
    /// Determines whether the builder currently contains at least one value for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to find.</param>
    /// <returns><see langword="true"/> when the pattern exists; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public bool ContainsPattern(IEnumerable<PatternSegment<TSegment>> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is PatternSegment<TSegment>[] array)
        {
            return ContainsPattern(array.AsSpan());
        }

        return ContainsPattern(pattern.ToArray().AsSpan());
    }

    /// <summary>
    /// Removes one matching pattern/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(ReadOnlySpan<TSegment> pattern, TValue value)
    {
        var frames = TraverseWithParents(pattern, out var node);
        if (node is null || node.Values is null)
        {
            return false;
        }

        for (var i = 0; i < node.Values.Count; i++)
        {
            if (!ValueComparer.Equals(node.Values[i], value))
            {
                continue;
            }

            node.Values.RemoveAt(i);
            node.Metadata?.RemoveAt(i);
            _registrationCount--;

            if (node.Values.Count == 0)
            {
                node.Values = null;
                node.Metadata = null;
                _patternCount--;
                PruneEmptyBranches(frames);
            }

            return true;
        }

        return false;
    }


    /// <summary>
    /// Removes one matching generic pattern-segment/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    public bool RemovePattern(ReadOnlySpan<PatternSegment<TSegment>> pattern, TValue value)
    {
        var frames = TraverseWithParents(pattern, out var node);
        if (node is null || node.Values is null)
        {
            return false;
        }

        for (var i = 0; i < node.Values.Count; i++)
        {
            if (!ValueComparer.Equals(node.Values[i], value))
            {
                continue;
            }

            node.Values.RemoveAt(i);
            node.Metadata?.RemoveAt(i);
            _registrationCount--;

            if (node.Values.Count == 0)
            {
                node.Values = null;
                node.Metadata = null;
                _patternCount--;
                PruneEmptyBranches(frames);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes one matching pattern/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    public bool Remove(ReadOnlyMemory<TSegment> pattern, TValue value) => Remove(pattern.Span, value);


    /// <summary>
    /// Removes one matching generic pattern-segment/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    public bool RemovePattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern, TValue value) => RemovePattern(pattern.Span, value);

    /// <summary>
    /// Removes one matching pattern/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public bool Remove(IEnumerable<TSegment> pattern, TValue value)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is TSegment[] array)
        {
            return Remove(array.AsSpan(), value);
        }

        return Remove(pattern.ToArray().AsSpan(), value);
    }


    /// <summary>
    /// Removes one matching generic pattern-segment/value registration from the builder.
    /// </summary>
    /// <param name="pattern">The pattern to remove.</param>
    /// <param name="value">The value to remove from the pattern.</param>
    /// <returns><see langword="true"/> when a registration was removed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public bool RemovePattern(IEnumerable<PatternSegment<TSegment>> pattern, TValue value)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is PatternSegment<TSegment>[] array)
        {
            return RemovePattern(array.AsSpan(), value);
        }

        return RemovePattern(pattern.ToArray().AsSpan(), value);
    }

    /// <summary>
    /// Removes every value registered for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    public int RemoveAll(ReadOnlySpan<TSegment> pattern)
    {
        var frames = TraverseWithParents(pattern, out var node);
        if (node?.Values is not { Count: > 0 } values)
        {
            return 0;
        }

        var removedCount = values.Count;
        node.Values = null;
        node.Metadata = null;
        _patternCount--;
        _registrationCount -= removedCount;
        PruneEmptyBranches(frames);
        return removedCount;
    }


    /// <summary>
    /// Removes every value registered for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    public int RemoveAllPattern(ReadOnlySpan<PatternSegment<TSegment>> pattern)
    {
        var frames = TraverseWithParents(pattern, out var node);
        if (node?.Values is not { Count: > 0 } values)
        {
            return 0;
        }

        var removedCount = values.Count;
        node.Values = null;
        node.Metadata = null;
        _patternCount--;
        _registrationCount -= removedCount;
        PruneEmptyBranches(frames);
        return removedCount;
    }

    /// <summary>
    /// Removes every value registered for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    public int RemoveAll(ReadOnlyMemory<TSegment> pattern) => RemoveAll(pattern.Span);


    /// <summary>
    /// Removes every value registered for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    public int RemoveAllPattern(ReadOnlyMemory<PatternSegment<TSegment>> pattern) => RemoveAllPattern(pattern.Span);

    /// <summary>
    /// Removes every value registered for the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public int RemoveAll(IEnumerable<TSegment> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is TSegment[] array)
        {
            return RemoveAll(array.AsSpan());
        }

        return RemoveAll(pattern.ToArray().AsSpan());
    }


    /// <summary>
    /// Removes every value registered for the specified generic pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern whose values should be removed.</param>
    /// <returns>The number of pattern/value registrations removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    public int RemoveAllPattern(IEnumerable<PatternSegment<TSegment>> pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern is PatternSegment<TSegment>[] array)
        {
            return RemoveAllPattern(array.AsSpan());
        }

        return RemoveAllPattern(pattern.ToArray().AsSpan());
    }


    /// <summary>
    /// Gets advisory diagnostics for the currently registered pattern set.
    /// </summary>
    /// <returns>An array of diagnostics. The array is empty when no diagnostics were found.</returns>
    /// <remarks>
    /// Diagnostics are domain-neutral and do not change matching behavior. They can be used by higher-level packages
    /// to reject duplicates, ambiguous parameter names, or overlapping wildcard/catch-all registrations according to
    /// domain-specific policy.
    /// </remarks>
    public PatternDiagnostic<TSegment>[] GetDiagnostics()
    {
        if (_patternCount == 0)
        {
            return [];
        }

        var diagnostics = new List<PatternDiagnostic<TSegment>>();
        var stack = new Stack<DiagnosticFrame>();
        stack.Push(new DiagnosticFrame(_root, []));

        while (stack.Count > 0)
        {
            var frame = stack.Pop();
            AddNodeDiagnostics(frame.Node, frame.Pattern, diagnostics);

            if (frame.Node.CatchAllChild is not null)
            {
                stack.Push(new DiagnosticFrame(
                    frame.Node.CatchAllChild,
                    Append(frame.Pattern, GetCatchAllPatternSegment(frame.Node.CatchAllChild))));
            }

            if (frame.Node.WildcardChild is not null)
            {
                stack.Push(new DiagnosticFrame(
                    frame.Node.WildcardChild,
                    Append(frame.Pattern, PatternSegment<TSegment>.Wildcard())));
            }

            if (frame.Node.Children is not null)
            {
                foreach (var (segment, child) in frame.Node.Children)
                {
                    stack.Push(new DiagnosticFrame(
                        child,
                        Append(frame.Pattern, PatternSegment<TSegment>.Literal(segment))));
                }
            }
        }

        return diagnostics.Count == 0 ? [] : [.. diagnostics];
    }

    /// <summary>
    /// Removes every pattern and value from the builder.
    /// </summary>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    public PattrnIndexBuilder<TSegment, TValue> Clear()
    {
        _root.Clear();
        _patternCount = 0;
        _registrationCount = 0;
        _nextRegistrationOrder = 0;
        return this;
    }



    private static PatternSegment<TSegment>[] Append(
        PatternSegment<TSegment>[] pattern,
        PatternSegment<TSegment> segment)
    {
        var result = new PatternSegment<TSegment>[pattern.Length + 1];
        Array.Copy(pattern, result, pattern.Length);
        result[^1] = segment;
        return result;
    }

    private static PatternSegment<TSegment> GetCatchAllPatternSegment(BuilderNode<TSegment, TValue> catchAllNode)
    {
        if (catchAllNode.Metadata is not null)
        {
            foreach (var metadata in catchAllNode.Metadata)
            {
                foreach (var capture in metadata.Captures)
                {
                    if (capture.IsCatchAll)
                    {
                        return PatternSegment<TSegment>.CatchAll(capture.Name);
                    }
                }
            }
        }

        return PatternSegment<TSegment>.CatchAll();
    }

    private static PatternSegment<TSegment>[] ApplyCaptureNames(
        PatternSegment<TSegment>[] pattern,
        BuilderRegistrationMetadata metadata)
    {
        if (metadata.Captures.Length == 0)
        {
            return pattern;
        }

        var result = new PatternSegment<TSegment>[pattern.Length];
        Array.Copy(pattern, result, pattern.Length);

        foreach (var capture in metadata.Captures)
        {
            if ((uint)capture.SegmentIndex >= (uint)result.Length)
            {
                continue;
            }

            result[capture.SegmentIndex] = capture.IsCatchAll
                ? PatternSegment<TSegment>.CatchAll(capture.Name)
                : PatternSegment<TSegment>.Parameter(capture.Name);
        }

        return result;
    }

    private static PatternSegment<TSegment>[] GetDiagnosticPattern(
        BuilderNode<TSegment, TValue> node,
        PatternSegment<TSegment>[] structuralPattern)
    {
        return node.Metadata is { Count: > 0 } metadata
            ? ApplyCaptureNames(structuralPattern, metadata[0])
            : structuralPattern;
    }

    private static bool HasAnyRegistration(BuilderNode<TSegment, TValue>? node)
    {
        if (node is null)
        {
            return false;
        }

        var stack = new Stack<BuilderNode<TSegment, TValue>>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.Values is { Count: > 0 })
            {
                return true;
            }

            if (current.CatchAllChild is not null)
            {
                stack.Push(current.CatchAllChild);
            }

            if (current.WildcardChild is not null)
            {
                stack.Push(current.WildcardChild);
            }

            if (current.Children is not null)
            {
                foreach (var child in current.Children.Values)
                {
                    stack.Push(child);
                }
            }
        }

        return false;
    }

    private static bool HasAnyRegistrationOutsideCatchAll(BuilderNode<TSegment, TValue> node)
    {
        if (node.Values is { Count: > 0 })
        {
            return true;
        }

        if (HasAnyRegistration(node.WildcardChild))
        {
            return true;
        }

        if (node.Children is not null)
        {
            foreach (var child in node.Children.Values)
            {
                if (HasAnyRegistration(child))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasAmbiguousParameterNames(BuilderNode<TSegment, TValue> node)
    {
        if (node.Metadata is not { Count: > 1 } metadata)
        {
            return false;
        }

        var names = new Dictionary<CaptureKey, string>();
        foreach (var registration in metadata)
        {
            foreach (var capture in registration.Captures)
            {
                var key = new CaptureKey(capture.SegmentIndex, capture.IsCatchAll);
                if (names.TryGetValue(key, out var existingName))
                {
                    if (!string.Equals(existingName, capture.Name, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    continue;
                }

                names.Add(key, capture.Name);
            }
        }

        return false;
    }

    private static void AddNodeDiagnostics(
        BuilderNode<TSegment, TValue> node,
        PatternSegment<TSegment>[] structuralPattern,
        List<PatternDiagnostic<TSegment>> diagnostics)
    {
        var diagnosticPattern = GetDiagnosticPattern(node, structuralPattern);

        if (node.Values is { Count: > 1 } values)
        {
            diagnostics.Add(new PatternDiagnostic<TSegment>(
                PatternDiagnosticKind.DuplicatePattern,
                PatternDiagnosticSeverity.Warning,
                diagnosticPattern,
                "Multiple values are registered for the same structural pattern.",
                values.Count));
        }

        if (HasAmbiguousParameterNames(node))
        {
            diagnostics.Add(new PatternDiagnostic<TSegment>(
                PatternDiagnosticKind.AmbiguousParameterNames,
                PatternDiagnosticSeverity.Warning,
                diagnosticPattern,
                "Structurally equivalent parameterized patterns use different parameter names.",
                node.Values?.Count ?? 0));
        }

        if (node.WildcardChild is not null && node.ChildCount > 0 && HasAnyRegistration(node.WildcardChild))
        {
            diagnostics.Add(new PatternDiagnostic<TSegment>(
                PatternDiagnosticKind.OverlappingWildcard,
                PatternDiagnosticSeverity.Info,
                Append(structuralPattern, PatternSegment<TSegment>.Wildcard()),
                "A wildcard branch overlaps one or more literal branches at the same position; deterministic ranking still prefers literal matches.",
                0));
        }

        if (node.CatchAllChild is not null && HasAnyRegistration(node.CatchAllChild) && HasAnyRegistrationOutsideCatchAll(node))
        {
            diagnostics.Add(new PatternDiagnostic<TSegment>(
                PatternDiagnosticKind.OverlappingCatchAll,
                PatternDiagnosticSeverity.Info,
                Append(structuralPattern, GetCatchAllPatternSegment(node.CatchAllChild)),
                "A terminal catch-all overlaps a shorter or more specific pattern below the same prefix; deterministic ranking still prefers more specific matches.",
                0));
        }
    }

    private static PatternMatchKind GetMatchKind(bool hasParameter, bool hasWildcard, bool hasCatchAll)
    {
        var nonLiteralKinds = 0;
        if (hasParameter)
        {
            nonLiteralKinds++;
        }

        if (hasWildcard)
        {
            nonLiteralKinds++;
        }

        if (hasCatchAll)
        {
            nonLiteralKinds++;
        }

        if (nonLiteralKinds > 1)
        {
            return PatternMatchKind.Mixed;
        }

        if (hasCatchAll)
        {
            return PatternMatchKind.CatchAll;
        }

        if (hasParameter)
        {
            return PatternMatchKind.Parameter;
        }

        return hasWildcard ? PatternMatchKind.Wildcard : PatternMatchKind.Exact;
    }

    private void AddRegistration(
        BuilderNode<TSegment, TValue> node,
        TValue value,
        BuilderRegistrationMetadata metadata)
    {
        if (node.Values is null || node.Values.Count == 0)
        {
            node.Values = [];
            node.Metadata = [];
            _patternCount++;
            AddRegistrationToExistingPattern(node, value, metadata);
            return;
        }

        switch (_duplicatePatternBehavior)
        {
            case DuplicatePatternRegistrationBehavior.Append:
                EnsureMetadataList(node);
                AddRegistrationToExistingPattern(node, value, metadata);
                return;

            case DuplicatePatternRegistrationBehavior.Throw:
                throw new InvalidOperationException("A registration already exists for the same structural pattern. Configure DuplicatePatternRegistrationBehavior.Append, Replace, or Ignore to allow duplicates explicitly.");

            case DuplicatePatternRegistrationBehavior.Replace:
                _registrationCount -= node.Values.Count;
                node.Values.Clear();
                node.Metadata?.Clear();
                node.Metadata ??= [];
                AddRegistrationToExistingPattern(node, value, metadata);
                return;

            case DuplicatePatternRegistrationBehavior.Ignore:
                return;

            default:
                throw new InvalidOperationException("The builder has an invalid duplicate pattern behavior.");
        }
    }

    private void AddRegistrationToExistingPattern(
        BuilderNode<TSegment, TValue> node,
        TValue value,
        BuilderRegistrationMetadata metadata)
    {
        node.Values!.Add(value);
        node.Metadata!.Add(metadata.WithRegistrationOrder(_nextRegistrationOrder));
        _nextRegistrationOrder++;
        _registrationCount++;
    }

    private static void EnsureMetadataList(BuilderNode<TSegment, TValue> node)
    {
        if (node.Metadata is not null)
        {
            return;
        }

        node.Metadata = [];
        for (var i = 0; i < node.Values!.Count; i++)
        {
            node.Metadata.Add(BuilderRegistrationMetadata.Exact);
        }
    }

    /// <summary>
    /// Compiles the current builder contents into an immutable index.
    /// </summary>
    /// <param name="options">The matching options used by the compiled index.</param>
    /// <returns>An immutable index safe for concurrent matching.</returns>
    public PattrnIndex<TSegment, TValue> Build(MatchOptions options = default)
    {
        ValidateBuildIfNeeded();

        var index = CompiledIndex<TSegment, TValue>.FromBuilder(_root, SegmentComparer, ValueComparer, options.DeduplicateValues);
        return new PattrnIndex<TSegment, TValue>(
            index,
            _patternCount,
            _registrationCount,
            options,
            SegmentComparer,
            ValueComparer);
    }

    private void ValidateBuildIfNeeded()
    {
        if (_buildValidationPredicate is null)
        {
            return;
        }

        var diagnostics = GetDiagnostics();
        var rejected = diagnostics.Where(_buildValidationPredicate).ToArray();
        if (rejected.Length == 0)
        {
            return;
        }

        var first = rejected[0];
        throw new InvalidOperationException(
            $"Build validation failed with {rejected.Length} diagnostic(s). First diagnostic: {first.Kind} ({first.Severity}) - {first.Message}");
    }

    private BuilderNode<TSegment, TValue>? FindNode(ReadOnlySpan<TSegment> pattern)
    {
        var node = _root;

        foreach (var segment in pattern)
        {
            if (_usesWildcardSegmentToken && SegmentComparer.Equals(segment, WildcardSegment))
            {
                if (node.WildcardChild is null)
                {
                    return null;
                }

                node = node.WildcardChild;
                continue;
            }

            if (!node.TryGetChild(segment, out var child))
            {
                return null;
            }

            node = child!;
        }

        return node;
    }


    private BuilderNode<TSegment, TValue>? FindNode(ReadOnlySpan<PatternSegment<TSegment>> pattern)
    {
        var node = _root;

        for (var i = 0; i < pattern.Length; i++)
        {
            var segment = pattern[i];
            switch (segment.Kind)
            {
                case PatternSegmentKind.Literal:
                    if (!node.TryGetChild(segment.LiteralValue, out var child))
                    {
                        return null;
                    }

                    node = child!;
                    break;

                case PatternSegmentKind.Parameter:
                case PatternSegmentKind.Wildcard:
                    if (node.WildcardChild is null)
                    {
                        return null;
                    }

                    node = node.WildcardChild;
                    break;

                case PatternSegmentKind.CatchAll:
                    if (i != pattern.Length - 1)
                    {
                        return null;
                    }

                    if (node.CatchAllChild is null)
                    {
                        return null;
                    }

                    node = node.CatchAllChild;
                    break;

                default:
                    return null;
            }
        }

        return node;
    }


    private BuilderFrame[] TraverseWithParents(ReadOnlySpan<TSegment> pattern, out BuilderNode<TSegment, TValue>? node)
    {
        node = _root;

        if (pattern.IsEmpty)
        {
            return [];
        }

        var frames = new BuilderFrame[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            var parent = node!;
            var segment = pattern[i];

            if (_usesWildcardSegmentToken && SegmentComparer.Equals(segment, WildcardSegment))
            {
                if (parent.WildcardChild is null)
                {
                    node = null;
                    return [];
                }

                frames[i] = new BuilderFrame(parent, segment, BuilderFrameKind.Wildcard);
                node = parent.WildcardChild;
                continue;
            }

            if (!parent.TryGetChild(segment, out var child))
            {
                node = null;
                return [];
            }

            frames[i] = new BuilderFrame(parent, segment, BuilderFrameKind.Literal);
            node = child!;
        }

        return frames;
    }


    private BuilderFrame[] TraverseWithParents(ReadOnlySpan<PatternSegment<TSegment>> pattern, out BuilderNode<TSegment, TValue>? node)
    {
        node = _root;

        if (pattern.IsEmpty)
        {
            return [];
        }

        var frames = new BuilderFrame[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            var parent = node!;
            var segment = pattern[i];
            switch (segment.Kind)
            {
                case PatternSegmentKind.Literal:
                    if (!parent.TryGetChild(segment.LiteralValue, out var child))
                    {
                        node = null;
                        return [];
                    }

                    frames[i] = new BuilderFrame(parent, segment.LiteralValue, BuilderFrameKind.Literal);
                    node = child!;
                    break;

                case PatternSegmentKind.Parameter:
                case PatternSegmentKind.Wildcard:
                    if (parent.WildcardChild is null)
                    {
                        node = null;
                        return [];
                    }

                    frames[i] = new BuilderFrame(parent, default!, BuilderFrameKind.Wildcard);
                    node = parent.WildcardChild;
                    break;

                case PatternSegmentKind.CatchAll:
                    if (i != pattern.Length - 1 || parent.CatchAllChild is null)
                    {
                        node = null;
                        return [];
                    }

                    frames[i] = new BuilderFrame(parent, default!, BuilderFrameKind.CatchAll);
                    node = parent.CatchAllChild;
                    break;

                default:
                    node = null;
                    return [];
            }
        }

        return frames;
    }

    private void PruneEmptyBranches(ReadOnlySpan<BuilderFrame> frames)
    {
        if (frames.IsEmpty)
        {
            return;
        }

        for (var i = frames.Length - 1; i >= 0; i--)
        {
            var frame = frames[i];
            var child = frame.Kind switch
            {
                BuilderFrameKind.Wildcard => frame.Parent.WildcardChild,
                BuilderFrameKind.CatchAll => frame.Parent.CatchAllChild,
                _ => frame.Parent.GetChildOrDefault(frame.Segment)
            };

            if (child is null || !child.IsEmpty)
            {
                return;
            }

            switch (frame.Kind)
            {
                case BuilderFrameKind.Wildcard:
                    frame.Parent.WildcardChild = null;
                    break;

                case BuilderFrameKind.CatchAll:
                    frame.Parent.CatchAllChild = null;
                    break;

                default:
                    _ = frame.Parent.RemoveChild(frame.Segment);
                    break;
            }
        }
    }


    private readonly struct DiagnosticFrame
    {
        internal DiagnosticFrame(BuilderNode<TSegment, TValue> node, PatternSegment<TSegment>[] pattern)
        {
            Node = node;
            Pattern = pattern;
        }

        internal BuilderNode<TSegment, TValue> Node { get; }

        internal PatternSegment<TSegment>[] Pattern { get; }
    }

    private readonly struct CaptureKey : IEquatable<CaptureKey>
    {
        internal CaptureKey(int segmentIndex, bool isCatchAll)
        {
            SegmentIndex = segmentIndex;
            IsCatchAll = isCatchAll;
        }

        private int SegmentIndex { get; }

        private bool IsCatchAll { get; }

        public bool Equals(CaptureKey other) => SegmentIndex == other.SegmentIndex && IsCatchAll == other.IsCatchAll;

        public override bool Equals(object? obj) => obj is CaptureKey other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(SegmentIndex, IsCatchAll);
    }

    private enum BuilderFrameKind
    {
        Literal,
        Wildcard,
        CatchAll
    }

    private readonly struct BuilderFrame
    {
        internal BuilderFrame(BuilderNode<TSegment, TValue> parent, TSegment segment, BuilderFrameKind kind)
        {
            Parent = parent;
            Segment = segment;
            Kind = kind;
        }

        internal BuilderNode<TSegment, TValue> Parent { get; }

        internal TSegment Segment { get; }

        internal BuilderFrameKind Kind { get; }
    }
}
