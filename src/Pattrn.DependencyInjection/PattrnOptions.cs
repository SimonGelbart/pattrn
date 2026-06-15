namespace Pattrn.DependencyInjection;

/// <summary>
/// Configures how a compiled <see cref="PattrnIndex{TSegment, TValue}"/> is created by dependency-injection registration extensions.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
internal sealed class PattrnOptions<TSegment, TValue>
    where TSegment : notnull
{
    private TSegment _wildcardSegment = default!;
    private bool _hasWildcardSegment;

    /// <summary>
    /// Gets or sets the segment value that represents a single-segment wildcard in tokenized registered patterns.
    /// </summary>
    /// <exception cref="ArgumentNullException">The assigned value is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The property is read before a wildcard segment was configured.</exception>
    public TSegment WildcardSegment
    {
        get
        {
            if (!_hasWildcardSegment)
            {
                throw new InvalidOperationException("No wildcard segment token is configured for this tokenless path pattern index registration.");
            }

            return _wildcardSegment;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _wildcardSegment = value;
            _hasWildcardSegment = true;
        }
    }

    /// <summary>
    /// Gets or sets the matching options used when compiling the index.
    /// </summary>
    public MatchOptions MatchOptions { get; set; } = MatchOptions.Default;

    /// <summary>
    /// Gets or sets the comparer used for exact segment lookup and optional tokenized wildcard detection.
    /// </summary>
    public IEqualityComparer<TSegment>? SegmentComparer { get; set; }

    /// <summary>
    /// Gets or sets the comparer used for removal and optional match-result deduplication.
    /// </summary>
    public IEqualityComparer<TValue>? ValueComparer { get; set; }

    internal bool HasWildcardSegment => _hasWildcardSegment;

    internal static PattrnOptions<TSegment, TValue> FromValues(
        TSegment wildcardSegment,
        bool hasWildcardSegment,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer,
        IEqualityComparer<TValue>? valueComparer)
    {
        var options = new PattrnOptions<TSegment, TValue>
        {
            MatchOptions = matchOptions,
            SegmentComparer = segmentComparer,
            ValueComparer = valueComparer
        };

        if (hasWildcardSegment)
        {
            options.WildcardSegment = wildcardSegment;
        }

        return options;
    }

    internal PattrnIndexBuilder<TSegment, TValue> CreateBuilder()
    {
        return _hasWildcardSegment
            ? PattrnIndex<TSegment, TValue>.Builder(WildcardSegment, SegmentComparer, ValueComparer)
            : PattrnIndex<TSegment, TValue>.Builder(SegmentComparer, ValueComparer);
    }
}
