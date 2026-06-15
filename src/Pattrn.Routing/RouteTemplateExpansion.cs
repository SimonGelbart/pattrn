namespace Pattrn.Routing;

/// <summary>
/// Represents one structural expansion of a framework-neutral route template.
/// </summary>
/// <remarks>
/// Optional and defaulted route parameters are expanded in the routing companion before registration with the generic core matcher.
/// This model keeps each generated core pattern linked to the original route template and records which optional/defaulted parameters were omitted.
/// </remarks>
public sealed class RouteTemplateExpansion
{
    internal RouteTemplateExpansion(
        RouteTemplate template,
        int expansionIndex,
        int includedSegmentCount,
        PatternSegment<string>[] pattern,
        RouteParameter[] omittedParameters)
    {
        Template = template;
        ExpansionIndex = expansionIndex;
        IncludedSegmentCount = includedSegmentCount;
        Pattern = pattern;
        OmittedParameters = omittedParameters;
    }

    /// <summary>
    /// Gets the original parsed route template that produced this expansion.
    /// </summary>
    public RouteTemplate Template { get; }

    /// <summary>
    /// Gets the zero-based expansion index. Expansions are ordered from shortest omitted-suffix variant to the full template.
    /// </summary>
    public int ExpansionIndex { get; }

    /// <summary>
    /// Gets the number of original template segments included in this expansion.
    /// </summary>
    public int IncludedSegmentCount { get; }

    /// <summary>
    /// Gets the generic core pattern segments produced for this expansion.
    /// </summary>
    public PatternSegment<string>[] Pattern { get; }

    /// <summary>
    /// Gets the optional or defaulted parameters omitted by this expansion.
    /// </summary>
    public IReadOnlyList<RouteParameter> OmittedParameters { get; }

    /// <summary>
    /// Gets a value indicating whether this expansion includes every segment from the original template.
    /// </summary>
    public bool IsFullTemplate => OmittedParameters.Count == 0;
}
