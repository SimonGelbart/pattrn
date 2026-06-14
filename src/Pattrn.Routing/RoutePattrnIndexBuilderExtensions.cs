namespace Pattrn.Routing;

/// <summary>
/// Builder helpers for registering route-like string templates without adding route syntax to the core package.
/// </summary>
public static class RoutePattrnIndexBuilderExtensions
{
    /// <summary>
    /// Registers a value for a route-like pattern template.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="builder">The builder to mutate.</param>
    /// <param name="pattern">The route-like pattern template.</param>
    /// <param name="value">The value returned when the pattern matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    public static PattrnIndexBuilder<string, TValue> AddRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value,
        string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddRoute(RoutePattern.ParseTemplate(pattern), value, patternId);
    }

    /// <summary>
    /// Registers a value for a parsed route template.
    /// </summary>
    /// <typeparam name="TValue">The stored value type.</typeparam>
    /// <param name="builder">The builder to mutate.</param>
    /// <param name="template">The parsed route template.</param>
    /// <param name="value">The value returned when the template matches.</param>
    /// <param name="patternId">An optional caller-provided identity for this route template registration.</param>
    /// <returns>The current builder, allowing fluent registration chains.</returns>
    /// <remarks>
    /// Optional/defaulted suffix expansions are registered with the same <paramref name="patternId"/> so detailed core matches remain linked to the caller's route identity.
    /// Use <see cref="RouteTemplate.ExpandDetailed"/> when a caller also needs to inspect which optional/defaulted parameters were omitted by each generated structural pattern.
    /// </remarks>
    public static PattrnIndexBuilder<string, TValue> AddRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        RouteTemplate template,
        TValue value,
        string? patternId = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(template);

        foreach (var expansion in template.ExpandDetailed())
        {
            builder.AddPattern(expansion.Pattern, value, patternId);
        }

        return builder;
    }

    /// <summary>
    /// Determines whether the builder contains at least one registration for a route-like pattern template.
    /// </summary>
    public static bool ContainsRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.ContainsRoute(RoutePattern.ParseTemplate(pattern));
    }

    /// <summary>
    /// Determines whether the builder contains at least one registration for a parsed route template.
    /// </summary>
    public static bool ContainsRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        RouteTemplate template)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(template);

        foreach (var expansion in template.ExpandDetailed())
        {
            if (builder.ContainsPattern(expansion.Pattern))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes one value registration for a route-like pattern template.
    /// </summary>
    public static bool RemoveRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern,
        TValue value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.RemoveRoute(RoutePattern.ParseTemplate(pattern), value);
    }

    /// <summary>
    /// Removes one value registration for a parsed route template.
    /// </summary>
    public static bool RemoveRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        RouteTemplate template,
        TValue value)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(template);

        var removed = false;
        foreach (var expansion in template.ExpandDetailed())
        {
            removed |= builder.RemovePattern(expansion.Pattern, value);
        }

        return removed;
    }

    /// <summary>
    /// Removes all value registrations for a route-like pattern template.
    /// </summary>
    public static int RemoveAllRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        string pattern)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.RemoveAllRoute(RoutePattern.ParseTemplate(pattern));
    }

    /// <summary>
    /// Removes all value registrations for a parsed route template.
    /// </summary>
    public static int RemoveAllRoute<TValue>(
        this PattrnIndexBuilder<string, TValue> builder,
        RouteTemplate template)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(template);

        var removed = 0;
        foreach (var expansion in template.ExpandDetailed())
        {
            removed += builder.RemoveAllPattern(expansion.Pattern);
        }

        return removed;
    }
}
