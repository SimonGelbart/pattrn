namespace Pattrn.Routing;

/// <summary>
/// Describes a route constraint validation failure.
/// </summary>
public sealed class RouteConstraintValidationFailure
{
    internal RouteConstraintValidationFailure(
        string code,
        string parameterName,
        RouteConstraint constraint,
        string? value,
        int templateSegmentIndex,
        int pathSegmentIndex,
        string message)
    {
        Code = code;
        ParameterName = parameterName;
        Constraint = constraint;
        Value = value;
        TemplateSegmentIndex = templateSegmentIndex;
        PathSegmentIndex = pathSegmentIndex;
        Message = message;
    }

    /// <summary>
    /// Gets the stable routing diagnostic code for this failure.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the route parameter name whose constraint failed.
    /// </summary>
    public string ParameterName { get; }

    /// <summary>
    /// Gets the constraint token that failed validation.
    /// </summary>
    public RouteConstraint Constraint { get; }

    /// <summary>
    /// Gets the captured value that failed validation, or <see langword="null"/> when the capture was missing.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Gets the zero-based route-template segment index for the constrained parameter.
    /// </summary>
    public int TemplateSegmentIndex { get; }

    /// <summary>
    /// Gets the zero-based runtime path segment index for the captured value, or <c>-1</c> when the capture was missing.
    /// </summary>
    public int PathSegmentIndex { get; }

    /// <summary>
    /// Gets a human-readable failure message.
    /// </summary>
    public string Message { get; }
}
