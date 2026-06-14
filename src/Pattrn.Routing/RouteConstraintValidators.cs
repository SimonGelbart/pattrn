using System.Globalization;
using System.Text.RegularExpressions;

namespace Pattrn.Routing;

/// <summary>
/// Provides framework-neutral built-in route constraint validators.
/// </summary>
public static class RouteConstraintValidators
{
    /// <summary>
    /// Validates that a captured value can be parsed as a 32-bit signed integer.
    /// </summary>
    public static IRouteConstraintValidator Int32 { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid 32-bit integer.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value can be parsed as a 64-bit signed integer.
    /// </summary>
    public static IRouteConstraintValidator Int64 { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid 64-bit integer.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value can be parsed as a GUID.
    /// </summary>
    public static IRouteConstraintValidator Guid { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (System.Guid.TryParse(value, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid GUID.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value can be parsed as a Boolean.
    /// </summary>
    public static IRouteConstraintValidator Boolean { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (bool.TryParse(value, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid Boolean.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value can be parsed as a date/time using invariant culture.
    /// </summary>
    public static IRouteConstraintValidator DateTime { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (System.DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid date/time.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value can be parsed as a decimal number using invariant culture.
    /// </summary>
    public static IRouteConstraintValidator Decimal { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        failure = $"Value '{value}' is not a valid decimal number.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value contains only letters and is not empty.
    /// </summary>
    public static IRouteConstraintValidator Alpha { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (value.Length > 0 && value.All(char.IsLetter))
        {
            return true;
        }

        failure = $"Value '{value}' must contain only letters.";
        return false;
    });

    /// <summary>
    /// Validates that a captured numeric value is greater than or equal to the constraint argument.
    /// </summary>
    public static IRouteConstraintValidator Min { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (!TryParseDecimalArgument(constraint, out var min, out failure))
        {
            return false;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed >= min)
        {
            return true;
        }

        failure = $"Value '{value}' must be greater than or equal to {constraint.Argument}.";
        return false;
    });

    /// <summary>
    /// Validates that a captured numeric value is less than or equal to the constraint argument.
    /// </summary>
    public static IRouteConstraintValidator Max { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (!TryParseDecimalArgument(constraint, out var max, out failure))
        {
            return false;
        }

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed <= max)
        {
            return true;
        }

        failure = $"Value '{value}' must be less than or equal to {constraint.Argument}.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value has exactly the length specified by the constraint argument.
    /// </summary>
    public static IRouteConstraintValidator Length { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (!int.TryParse(constraint.Argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expectedLength) || expectedLength < 0)
        {
            failure = $"Constraint '{constraint.Text}' requires a non-negative integer argument.";
            return false;
        }

        if (value.Length == expectedLength)
        {
            return true;
        }

        failure = $"Value '{value}' must have length {constraint.Argument}.";
        return false;
    });

    /// <summary>
    /// Validates that a captured value satisfies the regular expression supplied as the constraint argument.
    /// </summary>
    public static IRouteConstraintValidator Regex { get; } = new BuiltInValidator(static (string value, RouteConstraint constraint, ref string? failure) =>
    {
        if (string.IsNullOrEmpty(constraint.Argument))
        {
            failure = $"Constraint '{constraint.Text}' requires a regular expression argument.";
            return false;
        }

        try
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(value, constraint.Argument, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250)))
            {
                return true;
            }
        }
        catch (ArgumentException exception)
        {
            failure = $"Constraint '{constraint.Text}' has an invalid regular expression: {exception.Message}";
            return false;
        }
        catch (RegexMatchTimeoutException)
        {
            failure = $"Constraint '{constraint.Text}' timed out while evaluating its regular expression.";
            return false;
        }

        failure = $"Value '{value}' does not match regular expression constraint '{constraint.Text}'.";
        return false;
    });

    private static bool TryParseDecimalArgument(RouteConstraint constraint, out decimal parsed, out string? failure)
    {
        if (decimal.TryParse(constraint.Argument, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
        {
            failure = null;
            return true;
        }

        failure = $"Constraint '{constraint.Text}' requires a decimal argument.";
        return false;
    }

    private sealed class BuiltInValidator : IRouteConstraintValidator
    {
        private readonly BuiltInValidationDelegate _validator;

        internal BuiltInValidator(BuiltInValidationDelegate validator)
        {
            _validator = validator;
        }

        public bool IsMatch(string value, RouteConstraint constraint, out string? failureMessage)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(constraint);

            failureMessage = null;
            return _validator(value, constraint, ref failureMessage);
        }
    }

    private delegate bool BuiltInValidationDelegate(string value, RouteConstraint constraint, ref string? failureMessage);
}
