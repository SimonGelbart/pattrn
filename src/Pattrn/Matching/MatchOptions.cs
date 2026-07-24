namespace Pattrn.Matching;

/// <summary>
/// Controls how a compiled <see cref="PattrnIndex{TSegment, TValue}"/> matches registered patterns.
/// </summary>
/// <remarks>
/// Match operations are selected by their method names. These options only
/// control whether equal values from distinct registrations are retained.
/// </remarks>
public readonly struct MatchOptions : IEquatable<MatchOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MatchOptions"/> struct.
    /// </summary>
    /// <param name="duplicateValueMatchMode">The duplicate value behavior used by the compiled index.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an option value is not defined.</exception>
    public MatchOptions(
        DuplicateValueMatchMode duplicateValueMatchMode = DuplicateValueMatchMode.Deduplicate)
    {
        if (!Enum.IsDefined(duplicateValueMatchMode))
        {
            throw new ArgumentOutOfRangeException(nameof(duplicateValueMatchMode), duplicateValueMatchMode, "The duplicate match mode is not defined.");
        }

        DuplicateValueMatchMode = duplicateValueMatchMode;
    }

    /// <summary>
    /// Gets the default match options: exact-length matching with deduplication enabled.
    /// </summary>
    public static MatchOptions Default { get; } = new();

    /// <summary>
    /// Gets match options that preserve duplicate value registrations while keeping exact-length matching.
    /// </summary>
    public static MatchOptions PreserveDuplicates { get; } = new(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates);

    /// <summary>
    /// Gets the duplicate value behavior used by the compiled index.
    /// </summary>
    public DuplicateValueMatchMode DuplicateValueMatchMode { get; }

    /// <summary>
    /// Gets a value indicating whether values reached through overlapping patterns are emitted once.
    /// </summary>
    public bool DeduplicateValues => DuplicateValueMatchMode == DuplicateValueMatchMode.Deduplicate;

    /// <inheritdoc />
    public bool Equals(MatchOptions other)
    {
        return DuplicateValueMatchMode == other.DuplicateValueMatchMode;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is MatchOptions other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => (int)DuplicateValueMatchMode;

    /// <summary>
    /// Compares two <see cref="MatchOptions"/> values for equality.
    /// </summary>
    public static bool operator ==(MatchOptions left, MatchOptions right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="MatchOptions"/> values for inequality.
    /// </summary>
    public static bool operator !=(MatchOptions left, MatchOptions right) => !left.Equals(right);
}
