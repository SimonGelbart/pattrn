namespace Pattrn;

/// <summary>
/// Describes one segment in a generic segmented pattern.
/// </summary>
/// <typeparam name="TSegment">The segment value type used by registered patterns and incoming paths.</typeparam>
/// <remarks>
/// <para>
/// This type is domain-neutral. It does not parse route strings, URL paths, filesystem paths, or topic names.
/// Optional string/route syntax should be translated into these generic segments by higher-level packages.
/// </para>
/// <para>
/// The default value is an anonymous single-segment wildcard, which makes <c>default(PatternSegment&lt;TSegment&gt;)</c> safe to use.
/// </para>
/// </remarks>
public readonly struct PatternSegment<TSegment> : IEquatable<PatternSegment<TSegment>>
    where TSegment : notnull
{
    private readonly TSegment _literal;
    private readonly string? _parameterName;

    private PatternSegment(PatternSegmentKind kind, TSegment literal, string? parameterName)
    {
        Kind = kind;
        _literal = literal;
        _parameterName = parameterName;
    }

    private static readonly PatternSegment<TSegment> AnonymousWildcard = new(PatternSegmentKind.Wildcard, default!, null);
    private static readonly PatternSegment<TSegment> AnonymousCatchAll = new(PatternSegmentKind.CatchAll, default!, null);

    /// <summary>
    /// Gets the kind of pattern segment.
    /// </summary>
    public PatternSegmentKind Kind { get; }

    /// <summary>
    /// Gets a value indicating whether this segment matches one exact literal segment.
    /// </summary>
    public bool IsLiteral => Kind == PatternSegmentKind.Literal;

    /// <summary>
    /// Gets a value indicating whether this segment matches any single input segment.
    /// </summary>
    public bool IsWildcard => Kind == PatternSegmentKind.Wildcard;

    /// <summary>
    /// Gets a value indicating whether this segment matches any single input segment and carries a parameter name.
    /// </summary>
    public bool IsParameter => Kind == PatternSegmentKind.Parameter;

    /// <summary>
    /// Gets a value indicating whether this segment matches zero or more remaining input segments.
    /// </summary>
    public bool IsCatchAll => Kind == PatternSegmentKind.CatchAll;

    /// <summary>
    /// Gets the literal segment value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this segment is not a literal segment.</exception>
    public TSegment LiteralValue
    {
        get
        {
            if (Kind != PatternSegmentKind.Literal)
            {
                throw new InvalidOperationException("Only literal pattern segments have a literal value.");
            }

            return _literal;
        }
    }

    /// <summary>
    /// Gets the name for named parameter and named catch-all segments, or <see langword="null"/> for literal and anonymous wildcard segments.
    /// </summary>
    public string? ParameterName => Kind is PatternSegmentKind.Parameter or PatternSegmentKind.CatchAll ? _parameterName : null;

    /// <summary>
    /// Creates a literal pattern segment that matches one exact segment value.
    /// </summary>
    /// <param name="value">The exact segment value to match.</param>
    /// <returns>A literal pattern segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static PatternSegment<TSegment> Literal(TSegment value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new PatternSegment<TSegment>(PatternSegmentKind.Literal, value, null);
    }

    /// <summary>
    /// Creates an anonymous single-segment wildcard pattern segment.
    /// </summary>
    /// <returns>A wildcard pattern segment.</returns>
    public static PatternSegment<TSegment> Wildcard() => AnonymousWildcard;

    /// <summary>
    /// Creates a named single-segment parameter pattern segment.
    /// </summary>
    /// <param name="name">The logical parameter name.</param>
    /// <returns>A named parameter pattern segment.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    public static PatternSegment<TSegment> Parameter(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PatternSegment<TSegment>(PatternSegmentKind.Parameter, default!, name);
    }

    /// <summary>
    /// Creates an anonymous terminal catch-all pattern segment that matches zero or more remaining input segments.
    /// </summary>
    /// <returns>An anonymous catch-all pattern segment.</returns>
    public static PatternSegment<TSegment> CatchAll() => AnonymousCatchAll;

    /// <summary>
    /// Creates a named terminal catch-all pattern segment that matches zero or more remaining input segments.
    /// </summary>
    /// <param name="name">The logical catch-all parameter name.</param>
    /// <returns>A named catch-all pattern segment.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
    public static PatternSegment<TSegment> CatchAll(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new PatternSegment<TSegment>(PatternSegmentKind.CatchAll, default!, name);
    }

    /// <summary>
    /// Deconstructs the segment into its kind, literal value, and parameter name.
    /// </summary>
    /// <param name="kind">The segment kind.</param>
    /// <param name="literalValue">The literal value for literal segments, or <see langword="default"/> for non-literal segments.</param>
    /// <param name="parameterName">The name for named parameters and named catch-alls, or <see langword="null"/> otherwise.</param>
    public void Deconstruct(out PatternSegmentKind kind, out TSegment literalValue, out string? parameterName)
    {
        kind = Kind;
        literalValue = _literal;
        parameterName = ParameterName;
    }

    /// <inheritdoc />
    public bool Equals(PatternSegment<TSegment> other)
    {
        if (Kind != other.Kind)
        {
            return false;
        }

        return Kind switch
        {
            PatternSegmentKind.Literal => EqualityComparer<TSegment>.Default.Equals(_literal, other._literal),
            PatternSegmentKind.Parameter or PatternSegmentKind.CatchAll => string.Equals(_parameterName, other._parameterName, StringComparison.Ordinal),
            _ => true
        };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PatternSegment<TSegment> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Kind switch
        {
            PatternSegmentKind.Literal => HashCode.Combine(Kind, EqualityComparer<TSegment>.Default.GetHashCode(_literal)),
            PatternSegmentKind.Parameter or PatternSegmentKind.CatchAll => HashCode.Combine(Kind, _parameterName is null ? 0 : StringComparer.Ordinal.GetHashCode(_parameterName)),
            _ => (int)Kind
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Kind switch
        {
            PatternSegmentKind.Literal => _literal.ToString() ?? string.Empty,
            PatternSegmentKind.Parameter => "{" + _parameterName + "}",
            PatternSegmentKind.CatchAll => _parameterName is null ? "**" : "{*" + _parameterName + "}",
            _ => "*"
        };
    }

    /// <summary>
    /// Compares two pattern segments for equality.
    /// </summary>
    public static bool operator ==(PatternSegment<TSegment> left, PatternSegment<TSegment> right) => left.Equals(right);

    /// <summary>
    /// Compares two pattern segments for inequality.
    /// </summary>
    public static bool operator !=(PatternSegment<TSegment> left, PatternSegment<TSegment> right) => !left.Equals(right);
}
