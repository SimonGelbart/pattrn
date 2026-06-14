namespace Pattrn.Routing;

/// <summary>
/// Stores route constraint validators by constraint name.
/// </summary>
/// <remarks>
/// The default registry contains framework-neutral validators for common scalar route constraints. Create a new registry and register custom validators when an application
/// needs domain-specific constraint names.
/// </remarks>
public sealed class RouteConstraintValidatorRegistry
{
    private readonly Dictionary<string, IRouteConstraintValidator> _validators;

    /// <summary>
    /// Initializes an empty route constraint validator registry.
    /// </summary>
    public RouteConstraintValidatorRegistry()
        : this(new Dictionary<string, IRouteConstraintValidator>(StringComparer.OrdinalIgnoreCase))
    {
    }

    private RouteConstraintValidatorRegistry(Dictionary<string, IRouteConstraintValidator> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// Gets the built-in framework-neutral route constraint validators.
    /// </summary>
    public static RouteConstraintValidatorRegistry Default { get; } = CreateDefault();

    /// <summary>
    /// Creates a new registry populated with the built-in framework-neutral validators.
    /// </summary>
    /// <returns>A mutable registry initialized with the built-in validators.</returns>
    public static RouteConstraintValidatorRegistry CreateDefault()
    {
        var registry = new RouteConstraintValidatorRegistry();
        registry.Add("int", RouteConstraintValidators.Int32);
        registry.Add("long", RouteConstraintValidators.Int64);
        registry.Add("guid", RouteConstraintValidators.Guid);
        registry.Add("bool", RouteConstraintValidators.Boolean);
        registry.Add("datetime", RouteConstraintValidators.DateTime);
        registry.Add("decimal", RouteConstraintValidators.Decimal);
        registry.Add("alpha", RouteConstraintValidators.Alpha);
        registry.Add("min", RouteConstraintValidators.Min);
        registry.Add("max", RouteConstraintValidators.Max);
        registry.Add("length", RouteConstraintValidators.Length);
        registry.Add("regex", RouteConstraintValidators.Regex);
        return registry;
    }

    /// <summary>
    /// Adds or replaces a route constraint validator.
    /// </summary>
    /// <param name="name">The constraint name.</param>
    /// <param name="validator">The validator that evaluates the constraint.</param>
    /// <returns>The current registry, allowing fluent registration chains.</returns>
    public RouteConstraintValidatorRegistry Add(string name, IRouteConstraintValidator validator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(validator);

        _validators[name] = validator;
        return this;
    }

    /// <summary>
    /// Adds or replaces a delegate-backed route constraint validator.
    /// </summary>
    /// <param name="name">The constraint name.</param>
    /// <param name="validator">The validation delegate.</param>
    /// <returns>The current registry, allowing fluent registration chains.</returns>
    public RouteConstraintValidatorRegistry Add(string name, Func<string, RouteConstraint, bool> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        return Add(name, new DelegateRouteConstraintValidator(validator));
    }

    /// <summary>
    /// Attempts to get a registered validator by constraint name.
    /// </summary>
    /// <param name="name">The constraint name.</param>
    /// <param name="validator">The validator when found; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when a validator was found; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValidator(string name, out IRouteConstraintValidator? validator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _validators.TryGetValue(name, out validator);
    }
}
