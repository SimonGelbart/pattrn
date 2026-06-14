namespace Pattrn.Routing;

/// <summary>
/// Configures route constraint validation.
/// </summary>
public sealed class RouteConstraintValidationOptions
{
    /// <summary>
    /// Gets the default route constraint validation options.
    /// </summary>
    public static RouteConstraintValidationOptions Default { get; } = new();

    /// <summary>
    /// Gets the constraint validator registry used during validation.
    /// </summary>
    public RouteConstraintValidatorRegistry ValidatorRegistry { get; init; } = RouteConstraintValidatorRegistry.Default;

    /// <summary>
    /// Gets a value indicating whether unknown constraints should be treated as accepted.
    /// </summary>
    /// <remarks>
    /// The default is <see langword="false"/> so validation fails closed when a template contains a constraint that the configured registry cannot evaluate.
    /// </remarks>
    public bool AllowUnknownConstraints { get; init; }
}
