namespace Pattrn.Strings;

/// <summary>
/// Convenience helpers for separated string paths.
/// </summary>
/// <remarks>
/// These helpers allocate because they split strings into segments. Prefer the span-based core APIs for hot paths.
/// Use <see cref="StringNormalizationOptions"/> overloads when callers need explicit trimming, empty-segment, case-sensitivity, or segment-normalization behavior.
/// </remarks>
public static class PattrnStringExtensions
{
    /// <summary>
    /// Registers a dotted string pattern on a string-segment builder.
    /// </summary>
    public static PattrnIndexBuilder<string, TValue> AddDotted<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        char separator = '.',
        string? name = null)
    {
        return builder.AddSeparated(pattern, value, separator, name);
    }

    /// <summary>
    /// Registers a separated string pattern on a string-segment builder.
    /// </summary>
    public static PattrnIndexBuilder<string, TValue> AddSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        char separator,
        string? name = null)
    {
        return builder.AddSeparated(pattern, value, new StringNormalizationOptions(separator), name);
    }

    /// <summary>
    /// Registers a separated string pattern on a string-segment builder using explicit normalization options.
    /// </summary>
    public static PattrnIndexBuilder<string, TValue> AddSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        StringNormalizationOptions options,
        string? name = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.Add(options.Split(pattern, nameof(pattern)), value, name);
    }

    /// <summary>
    /// Removes one dotted string pattern/value registration from a string-segment builder.
    /// </summary>
    public static bool RemoveDotted<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        char separator = '.')
    {
        return builder.RemoveSeparated(pattern, value, separator);
    }

    /// <summary>
    /// Removes one separated string pattern/value registration from a string-segment builder.
    /// </summary>
    public static bool RemoveSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        char separator)
    {
        return builder.RemoveSeparated(pattern, value, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Removes one separated string pattern/value registration from a string-segment builder using explicit normalization options.
    /// </summary>
    public static bool RemoveSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        var segments = options.Split(pattern, nameof(pattern));
        var canonical = CanonicalizeLiteralPattern(builder, segments);
        var registration = builder.ToRegistrations().FirstOrDefault(candidate =>
            SamePattern(candidate.Pattern, canonical, builder.SegmentComparer)
            && builder.ValueComparer.Equals(candidate.Value, value));
        return registration is not null && builder.Remove(registration.Id);
    }

    /// <summary>
    /// Determines whether a string-segment builder contains at least one value for the specified separated pattern.
    /// </summary>
    public static bool ContainsSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        char separator)
    {
        return builder.ContainsSeparated(pattern, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Determines whether a string-segment builder contains at least one value for the specified separated pattern using explicit normalization options.
    /// </summary>
    public static bool ContainsSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.Contains(options.Split(pattern, nameof(pattern)));
    }

    /// <summary>
    /// Removes every value registered for the specified separated pattern.
    /// </summary>
    public static int RemoveAllSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        char separator)
    {
        return builder.RemoveAllSeparated(pattern, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Removes every value registered for the specified separated pattern using explicit normalization options.
    /// </summary>
    public static int RemoveAllSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        var canonical = CanonicalizeLiteralPattern(builder, options.Split(pattern, nameof(pattern)));
        var registrations = builder.ToRegistrations()
            .Where(candidate => SamePattern(candidate.Pattern, canonical, builder.SegmentComparer))
            .Select(candidate => candidate.Id)
            .ToArray();
        foreach (var id in registrations)
        {
            builder.Remove(id);
        }

        return registrations.Length;
    }

    private static PatternSegment<string>[] CanonicalizeLiteralPattern<TValue>(
        PattrnIndexBuilder<string, TValue> builder,
        ReadOnlySpan<string> segments)
    {
        var canonical = new PatternSegment<string>[segments.Length];
        for (var i = 0; i < segments.Length; i++)
        {
            canonical[i] = builder.UsesWildcardSegmentToken
                && builder.SegmentComparer.Equals(segments[i], builder.WildcardSegment)
                ? PatternSegment<string>.Wildcard()
                : PatternSegment<string>.Literal(segments[i]);
        }

        return canonical;
    }

    private static bool SamePattern(
        IReadOnlyList<PatternSegment<string>> left,
        IReadOnlyList<PatternSegment<string>> right,
        IEqualityComparer<string> comparer)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
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

    /// <summary>
    /// Gets a path-specific upper bound for a dotted string path against a string-segment index.
    /// </summary>
    public static int GetDottedMatchCountUpperBound<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        char separator = '.')
    {
        return index.GetSeparatedMatchCountUpperBound(path, separator);
    }

    /// <summary>
    /// Gets a path-specific upper bound for a separated string path against a string-segment index.
    /// </summary>
    public static int GetSeparatedMatchCountUpperBound<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        char separator)
    {
        return index.GetSeparatedMatchCountUpperBound(path, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Gets a path-specific upper bound for a separated string path against a string-segment index using explicit normalization options.
    /// </summary>
    public static int GetSeparatedMatchCountUpperBound<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.GetMatchCountUpperBound(options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Matches a dotted string path against a string-segment index and returns matching values as an array.
    /// </summary>
    public static TValue[] MatchDottedToArray<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        char separator = '.')
    {
        return index.MatchSeparatedToArray(path, separator);
    }

    /// <summary>
    /// Matches a separated string path against a string-segment index and returns matching values as an array.
    /// </summary>
    public static TValue[] MatchSeparatedToArray<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        char separator)
    {
        return index.MatchSeparatedToArray(path, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Matches a separated string path against a string-segment index and returns matching values as an array using explicit normalization options.
    /// </summary>
    public static TValue[] MatchSeparatedToArray<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.MatchValuesToArray(options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Matches a dotted string path and writes matching values into the caller-provided destination span.
    /// </summary>
    public static int MatchDotted<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        char separator = '.')
    {
        return index.MatchSeparated(path, destination, separator);
    }

    /// <summary>
    /// Matches a separated string path and writes matching values into the caller-provided destination span.
    /// </summary>
    public static int MatchSeparated<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        char separator)
    {
        return index.MatchSeparated(path, destination, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Matches a separated string path and writes matching values into the caller-provided destination span using explicit normalization options.
    /// </summary>
    public static int MatchSeparated<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        if (index.TryMatchValues(options.Split(path, nameof(path)), destination, out var written))
        {
            return written;
        }

        throw new ArgumentException("The destination span is too small to hold all matched values.", nameof(destination));
    }

    /// <summary>
    /// Attempts to match a dotted string path and write values into the caller-provided destination span.
    /// </summary>
    public static bool TryMatchDotted<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        out int written,
        char separator = '.')
    {
        return index.TryMatchSeparated(path, destination, out written, separator);
    }

    /// <summary>
    /// Attempts to match a separated string path and write values into the caller-provided destination span.
    /// </summary>
    public static bool TryMatchSeparated<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        out int written,
        char separator)
    {
        return index.TryMatchSeparated(path, destination, out written, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Attempts to match a separated string path and write values into the caller-provided destination span using explicit normalization options.
    /// </summary>
    public static bool TryMatchSeparated<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        out int written,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.TryMatchValues(options.Split(path, nameof(path)), destination, out written);
    }
}
