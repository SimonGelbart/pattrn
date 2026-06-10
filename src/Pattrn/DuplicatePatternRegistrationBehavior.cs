namespace Pattrn;

/// <summary>
/// Describes how a builder handles a new registration for a structural pattern that already has values.
/// </summary>
public enum DuplicatePatternRegistrationBehavior
{
    /// <summary>
    /// Append the new value to the existing structural pattern. This preserves the historical builder behavior.
    /// </summary>
    Append = 0,

    /// <summary>
    /// Throw an exception when a structural pattern already has at least one value.
    /// </summary>
    Throw = 1,

    /// <summary>
    /// Replace all existing values for the structural pattern with the new value.
    /// </summary>
    Replace = 2,

    /// <summary>
    /// Keep existing values and ignore the new registration when the structural pattern already exists.
    /// </summary>
    Ignore = 3
}
