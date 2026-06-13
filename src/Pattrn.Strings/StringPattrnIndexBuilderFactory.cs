namespace Pattrn;

/// <summary>
/// Provides type-inference friendly factories for <see cref="StringPattrnIndexBuilder{TValue}"/>.
/// </summary>
public static class StringPattrnIndexBuilder
{
    /// <summary>
    /// Creates a tokenless string-path builder.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> Create<TValue>(
        StringNormalizationOptions? options = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.Create(options, valueComparer);
    }

    /// <summary>
    /// Creates a tokenless string-path builder for the specified separator.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> Create<TValue>(
        char separator,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.Create(separator, valueComparer);
    }

    /// <summary>
    /// Creates a dotted-name string-path builder.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> CreateDotted<TValue>(
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.CreateDotted(valueComparer);
    }

    /// <summary>
    /// Creates a slash-separated string-path builder.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> CreateSlash<TValue>(
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.CreateSlash(valueComparer);
    }

    /// <summary>
    /// Creates a tokenized string-path builder that treats one literal token in string registrations as a wildcard segment.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> CreateTokenized<TValue>(
        string wildcardSegment,
        StringNormalizationOptions? options = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.CreateTokenized(wildcardSegment, options, valueComparer);
    }

    /// <summary>
    /// Creates a tokenized string-path builder for the specified separator.
    /// </summary>
    public static StringPattrnIndexBuilder<TValue> CreateTokenized<TValue>(
        char separator,
        string wildcardSegment,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return StringPattrnIndexBuilder<TValue>.CreateTokenized(separator, wildcardSegment, valueComparer);
    }
}
