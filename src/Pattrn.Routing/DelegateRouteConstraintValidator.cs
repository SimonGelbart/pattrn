namespace Pattrn.Routing;

/// <summary>
/// Adapts a delegate into an <see cref="IRouteConstraintValidator"/>.
/// </summary>
public sealed class DelegateRouteConstraintValidator : IRouteConstraintValidator
{
    private readonly Func<string, RouteConstraint, bool> _validator;

    /// <summary>
    /// Initializes a new delegate-backed route constraint validator.
    /// </summary>
    /// <param name="validator">The validation delegate.</param>
    public DelegateRouteConstraintValidator(Func<string, RouteConstraint, bool> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <inheritdoc />
    public bool IsMatch(string value, RouteConstraint constraint, out string? failureMessage)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(constraint);

        var isMatch = _validator(value, constraint);
        failureMessage = isMatch ? null : $"Value '{value}' does not satisfy route constraint '{constraint.Text}'.";
        return isMatch;
    }
}
