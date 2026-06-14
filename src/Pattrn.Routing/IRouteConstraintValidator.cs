namespace Pattrn.Routing;

/// <summary>
/// Validates one preserved route constraint against one captured route value.
/// </summary>
/// <remarks>
/// Constraint validators run after the generic index has produced a structural route match. They are intentionally scoped to the routing companion package so the core matcher
/// remains unaware of route-template semantics.
/// </remarks>
public interface IRouteConstraintValidator
{
    /// <summary>
    /// Validates a captured route value against a parsed constraint token.
    /// </summary>
    /// <param name="value">The captured route value to validate.</param>
    /// <param name="constraint">The constraint token being evaluated.</param>
    /// <param name="failureMessage">A human-readable failure message when validation fails; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the value satisfies the constraint; otherwise, <see langword="false"/>.</returns>
    bool IsMatch(string value, RouteConstraint constraint, out string? failureMessage);
}
