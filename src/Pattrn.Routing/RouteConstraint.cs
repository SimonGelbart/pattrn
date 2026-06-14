namespace Pattrn.Routing;

/// <summary>
/// Represents a framework-neutral route parameter constraint token such as <c>int</c>, <c>guid</c>, or <c>min(1)</c>.
/// </summary>
/// <remarks>
/// The routing package preserves constraint text but does not evaluate constraints. Constraint validation belongs above the generic core matcher.
/// </remarks>
public sealed class RouteConstraint
{
    internal RouteConstraint(string text, string name, string? argument)
    {
        Text = text;
        Name = name;
        Argument = argument;
    }

    /// <summary>
    /// Gets the original constraint token text without the leading <c>:</c> separator.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the constraint name. For <c>min(1)</c>, this is <c>min</c>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional constraint argument text. For <c>min(1)</c>, this is <c>1</c>.
    /// </summary>
    public string? Argument { get; }
}
