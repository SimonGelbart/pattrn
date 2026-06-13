namespace Pattrn;

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
        string? patternId = null)
    {
        return builder.AddSeparated(pattern, value, separator, patternId);
    }

    /// <summary>
    /// Registers a separated string pattern on a string-segment builder.
    /// </summary>
    public static PattrnIndexBuilder<string, TValue> AddSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        char separator,
        string? patternId = null)
    {
        return builder.AddSeparated(pattern, value, new StringNormalizationOptions(separator), patternId);
    }

    /// <summary>
    /// Registers a separated string pattern on a string-segment builder using explicit normalization options.
    /// </summary>
    public static PattrnIndexBuilder<string, TValue> AddSeparated<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        StringNormalizationOptions options,
        string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.Add(options.Split(pattern, nameof(pattern)), value, patternId);
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
        return builder.Remove(options.Split(pattern, nameof(pattern)), value);
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
        return builder.RemoveAll(options.Split(pattern, nameof(pattern)));
    }

    /// <summary>
    /// Gets a path-specific upper bound for a dotted string path against a string-segment index.
    /// </summary>
    public static int GetDottedMatchCountUpperBound<TValue>(
        this IPattrnIndex<string, TValue> index,
        string path,
        char separator = '.')
    {
        return index.GetSeparatedMatchCountUpperBound(path, separator);
    }

    /// <summary>
    /// Gets a path-specific upper bound for a separated string path against a string-segment index.
    /// </summary>
    public static int GetSeparatedMatchCountUpperBound<TValue>(
        this IPattrnIndex<string, TValue> index,
        string path,
        char separator)
    {
        return index.GetSeparatedMatchCountUpperBound(path, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Gets a path-specific upper bound for a separated string path against a string-segment index using explicit normalization options.
    /// </summary>
    public static int GetSeparatedMatchCountUpperBound<TValue>(
        this IPattrnIndex<string, TValue> index,
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
        this IPattrnIndex<string, TValue> index,
        string path,
        char separator = '.')
    {
        return index.MatchSeparatedToArray(path, separator);
    }

    /// <summary>
    /// Matches a separated string path against a string-segment index and returns matching values as an array.
    /// </summary>
    public static TValue[] MatchSeparatedToArray<TValue>(
        this IPattrnIndex<string, TValue> index,
        string path,
        char separator)
    {
        return index.MatchSeparatedToArray(path, new StringNormalizationOptions(separator));
    }

    /// <summary>
    /// Matches a separated string path against a string-segment index and returns matching values as an array using explicit normalization options.
    /// </summary>
    public static TValue[] MatchSeparatedToArray<TValue>(
        this IPattrnIndex<string, TValue> index,
        string path,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.MatchToArray(options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Matches a dotted string path and writes matching values into the caller-provided destination span.
    /// </summary>
    public static int MatchDotted<TValue>(
        this IPattrnIndex<string, TValue> index,
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
        this IPattrnIndex<string, TValue> index,
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
        this IPattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.Match(options.Split(path, nameof(path)), destination);
    }

    /// <summary>
    /// Attempts to match a dotted string path and write values into the caller-provided destination span.
    /// </summary>
    public static bool TryMatchDotted<TValue>(
        this IPattrnIndex<string, TValue> index,
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
        this IPattrnIndex<string, TValue> index,
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
        this IPattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        out int written,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(options);
        return index.TryMatch(options.Split(path, nameof(path)), destination, out written);
    }
}
