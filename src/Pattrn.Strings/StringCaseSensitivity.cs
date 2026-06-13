namespace Pattrn;

/// <summary>
/// Describes the built-in string segment comparison policy used by <see cref="StringNormalizationOptions"/>.
/// </summary>
public enum StringCaseSensitivity
{
    /// <summary>
    /// Segments are compared with ordinal case-sensitive semantics.
    /// </summary>
    Ordinal = 0,

    /// <summary>
    /// Segments are compared with ordinal case-insensitive semantics.
    /// </summary>
    OrdinalIgnoreCase = 1
}
