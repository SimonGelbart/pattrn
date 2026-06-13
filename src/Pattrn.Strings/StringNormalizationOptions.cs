namespace Pattrn;

/// <summary>
/// Defines explicit string splitting and normalization behavior for <c>Pattrn.Strings</c> helpers.
/// </summary>
/// <remarks>
/// These options intentionally live in the string companion package, not in the generic core. The core remains an
/// already-segmented matcher; callers choose a comparer and split or normalize strings before registration and matching.
/// </remarks>
public sealed class StringNormalizationOptions
{
    /// <summary>
    /// Initializes a new string normalization options instance for the specified separator.
    /// </summary>
    /// <param name="separator">The separator used to split input strings into segments.</param>
    public StringNormalizationOptions(char separator)
    {
        Separator = separator;
    }

    /// <summary>
    /// Gets slash-separated defaults that reject empty segments and compare segments case-sensitively.
    /// </summary>
    public static StringNormalizationOptions Slash => new('/');

    /// <summary>
    /// Gets dotted-name defaults that reject empty segments and compare segments case-sensitively.
    /// </summary>
    public static StringNormalizationOptions Dotted => new('.');

    /// <summary>
    /// Gets the separator used to split input strings into segments.
    /// </summary>
    public char Separator { get; }

    /// <summary>
    /// Gets the built-in case-sensitivity policy used when creating string-segment builders.
    /// </summary>
    public StringCaseSensitivity CaseSensitivity { get; init; } = StringCaseSensitivity.Ordinal;

    /// <summary>
    /// Gets how empty segments are handled after splitting, trimming, and custom normalization.
    /// </summary>
    public StringEmptySegmentBehavior EmptySegmentBehavior { get; init; } = StringEmptySegmentBehavior.Reject;

    /// <summary>
    /// Gets whether each segment is trimmed before empty-segment validation and custom normalization.
    /// </summary>
    public StringSegmentTrimBehavior TrimBehavior { get; init; } = StringSegmentTrimBehavior.None;

    /// <summary>
    /// Gets an optional caller-supplied segment normalization delegate.
    /// </summary>
    /// <remarks>
    /// The delegate is applied after optional trimming and before the final empty-segment check. It must not return <see langword="null"/>.
    /// </remarks>
    public Func<string, string>? NormalizeSegment { get; init; }

    /// <summary>
    /// Creates a segment comparer that matches <see cref="CaseSensitivity"/>.
    /// </summary>
    /// <returns>A string comparer suitable for a string-segment builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="CaseSensitivity"/> is not a defined value.</exception>
    public IEqualityComparer<string> CreateSegmentComparer()
    {
        return CaseSensitivity switch
        {
            StringCaseSensitivity.Ordinal => StringComparer.Ordinal,
            StringCaseSensitivity.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => throw new ArgumentOutOfRangeException(nameof(CaseSensitivity), "Unknown string case-sensitivity policy.")
        };
    }

    /// <summary>
    /// Creates a tokenless string-segment builder configured with this options object's segment comparer.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable string-segment builder.</returns>
    public PattrnIndexBuilder<string, TValue> CreateBuilder<TValue>(IEqualityComparer<TValue>? valueComparer = null)
    {
        return PattrnIndexBuilder<string, TValue>.Create(CreateSegmentComparer(), valueComparer);
    }

    /// <summary>
    /// Creates an ergonomic tokenless string-path builder configured with this options object.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable string-path builder facade.</returns>
    public StringPattrnIndexBuilder<TValue> CreateStringBuilder<TValue>(IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.Create(this, valueComparer);
    }

    /// <summary>
    /// Creates a tokenized string-segment builder configured with this options object's segment comparer.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="wildcardSegment">The reserved wildcard token used by tokenized <c>Add(...)</c> registrations.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable string-segment builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wildcardSegment"/> is <see langword="null"/>.</exception>
    public PattrnIndexBuilder<string, TValue> CreateTokenizedBuilder<TValue>(
        string wildcardSegment,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        ArgumentNullException.ThrowIfNull(wildcardSegment);
        return PattrnIndexBuilder<string, TValue>.Create(wildcardSegment, CreateSegmentComparer(), valueComparer);
    }

    /// <summary>
    /// Creates an ergonomic tokenized string-path builder configured with this options object.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="wildcardSegment">The reserved wildcard token used by tokenized string registrations.</param>
    /// <param name="valueComparer">The optional comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable tokenized string-path builder facade.</returns>
    public StringPattrnIndexBuilder<TValue> CreateTokenizedStringBuilder<TValue>(
        string wildcardSegment,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.CreateTokenized(wildcardSegment, this, valueComparer);
    }

    /// <summary>
    /// Normalizes one string segment according to this options object.
    /// </summary>
    /// <param name="segment">The segment value to normalize.</param>
    /// <param name="parameterName">The parameter name used in validation exceptions.</param>
    /// <returns>The normalized segment.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="segment"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when empty segments are rejected and the normalized segment is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an enum option is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="NormalizeSegment"/> returns <see langword="null"/>.</exception>
    public string NormalizeSegmentValue(string segment, string parameterName = "segment")
    {
        ArgumentNullException.ThrowIfNull(segment);
        Validate();

        var normalized = TrimBehavior == StringSegmentTrimBehavior.TrimWhitespace
            ? segment.Trim()
            : segment;

        if (NormalizeSegment is not null)
        {
            normalized = NormalizeSegment(normalized)
                ?? throw new InvalidOperationException("String segment normalization delegates must not return null.");
        }

        if (normalized.Length == 0 && EmptySegmentBehavior == StringEmptySegmentBehavior.Reject)
        {
            throw new ArgumentException("String segments must not be empty.", parameterName);
        }

        return normalized;
    }

    /// <summary>
    /// Splits and normalizes a separated string into segments according to this options object.
    /// </summary>
    /// <param name="value">The separated string to split.</param>
    /// <param name="parameterName">The parameter name used in validation exceptions.</param>
    /// <returns>The normalized segment array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when empty segments are rejected and the value contains one.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an enum option is not a defined value.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="NormalizeSegment"/> returns <see langword="null"/>.</exception>
    public string[] Split(string value, string parameterName = "value")
    {
        ArgumentNullException.ThrowIfNull(value);
        Validate();

        var rawSegments = value.Split(Separator);
        var segments = new List<string>(rawSegments.Length);
        foreach (var rawSegment in rawSegments)
        {
            var segment = NormalizeSegmentValue(rawSegment, parameterName);
            if (segment.Length == 0)
            {
                continue;
            }

            segments.Add(segment);
        }

        return segments.Count == 0 ? [] : [.. segments];
    }

    private void Validate()
    {
        if (!Enum.IsDefined(CaseSensitivity))
        {
            throw new ArgumentOutOfRangeException(nameof(CaseSensitivity), "Unknown string case-sensitivity policy.");
        }

        if (!Enum.IsDefined(EmptySegmentBehavior))
        {
            throw new ArgumentOutOfRangeException(nameof(EmptySegmentBehavior), "Unknown empty-segment behavior.");
        }

        if (!Enum.IsDefined(TrimBehavior))
        {
            throw new ArgumentOutOfRangeException(nameof(TrimBehavior), "Unknown string segment trim behavior.");
        }
    }
}
