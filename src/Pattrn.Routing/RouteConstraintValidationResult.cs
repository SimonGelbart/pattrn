namespace Pattrn.Routing;

/// <summary>
/// Represents the result of validating preserved route constraints against structural match captures.
/// </summary>
public sealed class RouteConstraintValidationResult
{
    internal static readonly RouteConstraintValidationResult SuccessResult = new([]);

    internal RouteConstraintValidationResult(RouteConstraintValidationFailure[] failures)
    {
        Failures = failures;
    }

    /// <summary>
    /// Gets a value indicating whether every evaluated route constraint accepted its captured value.
    /// </summary>
    public bool IsValid => Failures.Count == 0;

    /// <summary>
    /// Gets route constraint validation failures.
    /// </summary>
    public IReadOnlyList<RouteConstraintValidationFailure> Failures { get; }
}
