namespace Pattrn.Routing;

/// <summary>
/// Represents a parsed route parameter segment.
/// </summary>
public sealed class RouteParameter
{
    internal RouteParameter(
        string name,
        bool isCatchAll,
        bool isOptional,
        string? defaultValue,
        RouteConstraint[] constraints)
    {
        Name = name;
        IsCatchAll = isCatchAll;
        IsOptional = isOptional;
        DefaultValue = defaultValue;
        Constraints = constraints;
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether this parameter is a terminal catch-all parameter.
    /// </summary>
    public bool IsCatchAll { get; }

    /// <summary>
    /// Gets a value indicating whether this parameter has an explicit <c>?</c> optional marker.
    /// </summary>
    public bool IsOptional { get; }

    /// <summary>
    /// Gets the optional default value text, when the template used <c>=</c> default syntax.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Gets a value indicating whether this parameter has a default value.
    /// </summary>
    public bool HasDefaultValue => DefaultValue is not null;

    /// <summary>
    /// Gets a value indicating whether this parameter can be omitted when the template is expanded into generic core patterns.
    /// </summary>
    public bool CanOmit => IsOptional || HasDefaultValue;

    /// <summary>
    /// Gets preserved constraint tokens. The generic matcher does not evaluate these constraints.
    /// </summary>
    public IReadOnlyList<RouteConstraint> Constraints { get; }
}
