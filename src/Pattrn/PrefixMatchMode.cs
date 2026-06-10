namespace Pattrn;

/// <summary>
/// Defines whether shorter registered patterns can match the beginning of longer input paths.
/// </summary>
public enum PrefixMatchMode
{
    /// <summary>
    /// Registered patterns match only input paths with the same segment length.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Registered patterns can match the beginning of longer input paths.
    /// </summary>
    IncludePrefixPatterns = 1,
}
