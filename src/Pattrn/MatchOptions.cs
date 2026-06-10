namespace Pattrn;

/// <summary>
/// Controls how a compiled <see cref="PattrnIndex{TSegment, TValue}"/> matches registered patterns.
/// </summary>
/// <remarks>
/// The default value is exact-length matching with value deduplication enabled.
/// </remarks>
public readonly struct MatchOptions : IEquatable<MatchOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MatchOptions"/> struct.
    /// </summary>
    /// <param name="prefixMatchMode">The prefix matching behavior used by the compiled index.</param>
    /// <param name="duplicateValueMatchMode">The duplicate value behavior used by the compiled index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an option value is not defined.</exception>
    public MatchOptions(
        PrefixMatchMode prefixMatchMode = PrefixMatchMode.Disabled,
        DuplicateValueMatchMode duplicateValueMatchMode = DuplicateValueMatchMode.Deduplicate)
    {
        if (!Enum.IsDefined(prefixMatchMode))
        {
            throw new ArgumentOutOfRangeException(nameof(prefixMatchMode), prefixMatchMode, "The prefix match mode is not defined.");
        }

        if (!Enum.IsDefined(duplicateValueMatchMode))
        {
            throw new ArgumentOutOfRangeException(nameof(duplicateValueMatchMode), duplicateValueMatchMode, "The duplicate match mode is not defined.");
        }

        PrefixMatchMode = prefixMatchMode;
        DuplicateValueMatchMode = duplicateValueMatchMode;
    }

    /// <summary>
    /// Gets the default match options: exact-length matching with deduplication enabled.
    /// </summary>
    public static MatchOptions Default { get; } = new();

    /// <summary>
    /// Gets match options that enable prefix matching while keeping deduplication enabled.
    /// </summary>
    public static MatchOptions Prefix { get; } = new(PrefixMatchMode.IncludePrefixPatterns);

    /// <summary>
    /// Gets match options that preserve duplicate value registrations while keeping exact-length matching.
    /// </summary>
    public static MatchOptions PreserveDuplicates { get; } = new(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates);

    /// <summary>
    /// Gets the prefix matching behavior used by the compiled index.
    /// </summary>
    public PrefixMatchMode PrefixMatchMode { get; }

    /// <summary>
    /// Gets the duplicate value behavior used by the compiled index.
    /// </summary>
    public DuplicateValueMatchMode DuplicateValueMatchMode { get; }

    /// <summary>
    /// Gets a value indicating whether a pattern can match the beginning of a longer input path.
    /// </summary>
    public bool IncludePrefixMatches => PrefixMatchMode == PrefixMatchMode.IncludePrefixPatterns;

    /// <summary>
    /// Gets a value indicating whether values reached through overlapping patterns are emitted once.
    /// </summary>
    public bool DeduplicateValues => DuplicateValueMatchMode == DuplicateValueMatchMode.Deduplicate;

    /// <summary>
    /// Deconstructs the options into their individual mode values.
    /// </summary>
    /// <param name="prefixMatchMode">The prefix matching mode.</param>
    /// <param name="duplicateValueMatchMode">The duplicate value mode.</param>
    public void Deconstruct(out PrefixMatchMode prefixMatchMode, out DuplicateValueMatchMode duplicateValueMatchMode)
    {
        prefixMatchMode = PrefixMatchMode;
        duplicateValueMatchMode = DuplicateValueMatchMode;
    }

    /// <inheritdoc />
    public bool Equals(MatchOptions other)
    {
        return PrefixMatchMode == other.PrefixMatchMode
            && DuplicateValueMatchMode == other.DuplicateValueMatchMode;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MatchOptions other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(PrefixMatchMode, DuplicateValueMatchMode);

    /// <summary>
    /// Compares two <see cref="MatchOptions"/> values for equality.
    /// </summary>
    public static bool operator ==(MatchOptions left, MatchOptions right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="MatchOptions"/> values for inequality.
    /// </summary>
    public static bool operator !=(MatchOptions left, MatchOptions right) => !left.Equals(right);
}
