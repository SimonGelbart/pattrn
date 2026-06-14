namespace Pattrn.Routing;

/// <summary>
/// Validates framework-neutral route-template constraints against captures produced by structural matching.
/// </summary>
public static class RouteConstraintValidation
{
    /// <summary>
    /// Validates preserved route constraints against detailed match captures.
    /// </summary>
    /// <typeparam name="TValue">The matched value type.</typeparam>
    /// <param name="template">The parsed route template that produced the structural registration.</param>
    /// <param name="match">The structural match to validate.</param>
    /// <param name="options">Optional validation options.</param>
    /// <returns>The constraint validation result.</returns>
    public static RouteConstraintValidationResult Validate<TValue>(
        RouteTemplate template,
        PatternMatchResult<string, TValue> match,
        RouteConstraintValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(match);
        return Validate(template, match.Captures, options);
    }

    /// <summary>
    /// Validates preserved route constraints against named captures.
    /// </summary>
    /// <param name="template">The parsed route template that produced the structural registration.</param>
    /// <param name="captures">The structural match captures to validate.</param>
    /// <param name="options">Optional validation options.</param>
    /// <returns>The constraint validation result.</returns>
    public static RouteConstraintValidationResult Validate(
        RouteTemplate template,
        IReadOnlyList<PatternCapture<string>> captures,
        RouteConstraintValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(captures);

        options ??= RouteConstraintValidationOptions.Default;
        var failures = new List<RouteConstraintValidationFailure>();

        for (var segmentIndex = 0; segmentIndex < template.Segments.Count; segmentIndex++)
        {
            var parameter = template.Segments[segmentIndex].Parameter;
            if (parameter is null || parameter.Constraints.Count == 0)
            {
                continue;
            }

            var hasCapture = TryFindCapture(captures, parameter.Name, out var capture);
            if (!hasCapture)
            {
                if (parameter.CanOmit)
                {
                    continue;
                }

                foreach (var constraint in parameter.Constraints)
                {
                    failures.Add(new RouteConstraintValidationFailure(
                        "ROUTECONSTRAINT001",
                        parameter.Name,
                        constraint,
                        null,
                        segmentIndex,
                        -1,
                        $"Route parameter '{parameter.Name}' was not captured before constraint '{constraint.Text}' was evaluated."));
                }

                continue;
            }

            foreach (var constraint in parameter.Constraints)
            {
                if (!options.ValidatorRegistry.TryGetValidator(constraint.Name, out var validator))
                {
                    if (options.AllowUnknownConstraints)
                    {
                        continue;
                    }

                    failures.Add(new RouteConstraintValidationFailure(
                        "ROUTECONSTRAINT002",
                        parameter.Name,
                        constraint,
                        capture.Value,
                        segmentIndex,
                        capture.SegmentIndex,
                        $"No route constraint validator is registered for constraint '{constraint.Name}'."));
                    continue;
                }

                if (!validator!.IsMatch(capture.Value, constraint, out var failureMessage))
                {
                    failures.Add(new RouteConstraintValidationFailure(
                        "ROUTECONSTRAINT003",
                        parameter.Name,
                        constraint,
                        capture.Value,
                        segmentIndex,
                        capture.SegmentIndex,
                        failureMessage ?? $"Value '{capture.Value}' does not satisfy route constraint '{constraint.Text}'."));
                }
            }
        }

        return failures.Count == 0
            ? RouteConstraintValidationResult.SuccessResult
            : new RouteConstraintValidationResult([.. failures]);
    }

    private static bool TryFindCapture(IReadOnlyList<PatternCapture<string>> captures, string name, out PatternCapture<string> capture)
    {
        for (var i = 0; i < captures.Count; i++)
        {
            if (string.Equals(captures[i].Name, name, StringComparison.Ordinal))
            {
                capture = captures[i];
                return true;
            }
        }

        capture = default;
        return false;
    }
}
