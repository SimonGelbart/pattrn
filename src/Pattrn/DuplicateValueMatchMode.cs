namespace Pattrn;

/// <summary>
/// Defines how duplicate values reached through overlapping matching patterns are emitted.
/// </summary>
public enum DuplicateValueMatchMode
{
    /// <summary>
    /// Emit each value at most once according to the configured value comparer.
    /// </summary>
    Deduplicate = 0,

    /// <summary>
    /// Emit every value registration reached during matching, including duplicates.
    /// </summary>
    PreserveDuplicates = 1,
}
