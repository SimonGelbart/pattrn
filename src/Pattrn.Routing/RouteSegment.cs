namespace Pattrn.Routing;

/// <summary>
/// Represents one parsed route-template segment.
/// </summary>
public sealed class RouteSegment
{
    private RouteSegment(RouteSegmentKind kind, string? literal, RouteParameter? parameter)
    {
        Kind = kind;
        Literal = literal;
        Parameter = parameter;
    }

    /// <summary>
    /// Gets the parsed segment kind.
    /// </summary>
    public RouteSegmentKind Kind { get; }

    /// <summary>
    /// Gets the literal segment text when <see cref="Kind"/> is <see cref="RouteSegmentKind.Literal"/>; otherwise, <see langword="null"/>.
    /// </summary>
    public string? Literal { get; }

    /// <summary>
    /// Gets the parsed route parameter when <see cref="Kind"/> is <see cref="RouteSegmentKind.Parameter"/>; otherwise, <see langword="null"/>.
    /// </summary>
    public RouteParameter? Parameter { get; }

    internal static RouteSegment ForLiteral(string literal) => new(RouteSegmentKind.Literal, literal, null);

    internal static RouteSegment ForParameter(RouteParameter parameter) => new(RouteSegmentKind.Parameter, null, parameter);

    /// <summary>
    /// Converts this route segment to the generic core pattern-segment model.
    /// </summary>
    /// <returns>The generic pattern segment represented by this route segment.</returns>
    public PatternSegment<string> ToPatternSegment()
    {
        return Kind switch
        {
            RouteSegmentKind.Literal => PatternSegment<string>.Literal(Literal!),
            RouteSegmentKind.Parameter when Parameter!.IsCatchAll => PatternSegment<string>.CatchAll(Parameter.Name),
            RouteSegmentKind.Parameter => PatternSegment<string>.Parameter(Parameter!.Name),
            _ => throw new InvalidOperationException("Unknown route segment kind.")
        };
    }
}
