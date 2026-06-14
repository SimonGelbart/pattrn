namespace Pattrn.Routing;

/// <summary>
/// Represents a parsed, framework-neutral route template.
/// </summary>
/// <remarks>
/// The template model preserves literals, parameters, catch-alls, optional markers, defaults, and constraint tokens. It compiles structural route shape into
/// <see cref="PatternSegment{TSegment}"/> values, but it does not evaluate route constraints or apply framework-specific routing semantics.
/// </remarks>
public sealed class RouteTemplate
{
    internal RouteTemplate(string text, RouteSegment[] segments, RouteTemplateDiagnostic[] diagnostics)
    {
        Text = text;
        Segments = segments;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Gets the original template text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the parsed route-template segments.
    /// </summary>
    public IReadOnlyList<RouteSegment> Segments { get; }

    /// <summary>
    /// Gets diagnostics produced while parsing or validating the template.
    /// </summary>
    public IReadOnlyList<RouteTemplateDiagnostic> Diagnostics { get; }

    /// <summary>
    /// Gets a value indicating whether the template contains optional or defaulted parameters that can be expanded into multiple generic patterns.
    /// </summary>
    public bool HasOptionalSegments => Segments.Any(static segment => segment.Parameter?.CanOmit == true);

    /// <summary>
    /// Parses a framework-neutral route template.
    /// </summary>
    /// <param name="template">The route template to parse. Leading and trailing slashes are ignored.</param>
    /// <returns>The parsed route template.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the route template is invalid or uses unsupported route syntax.</exception>
    public static RouteTemplate Parse(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (TryParse(template, out var routeTemplate, out var diagnostics))
        {
            return routeTemplate!;
        }

        var first = diagnostics.Length == 0
            ? new RouteTemplateDiagnostic("ROUTE000", "The route template is invalid.")
            : diagnostics[0];
        throw new ArgumentException(first.Message, nameof(template));
    }

    /// <summary>
    /// Attempts to parse a framework-neutral route template without throwing for syntax diagnostics.
    /// </summary>
    /// <param name="template">The route template to parse. Leading and trailing slashes are ignored.</param>
    /// <param name="routeTemplate">The parsed route template when parsing succeeds; otherwise, <see langword="null"/>.</param>
    /// <param name="diagnostics">The diagnostics produced by parsing and validation.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is <see langword="null"/>.</exception>
    public static bool TryParse(string template, out RouteTemplate? routeTemplate, out RouteTemplateDiagnostic[] diagnostics)
    {
        ArgumentNullException.ThrowIfNull(template);
        return RouteTemplateParser.TryParse(template, out routeTemplate, out diagnostics);
    }


    /// <summary>
    /// Validates this template's preserved constraints against named structural match captures.
    /// </summary>
    /// <param name="captures">The captures produced by a structural match for this template.</param>
    /// <param name="options">Optional route constraint validation options.</param>
    /// <returns>The route constraint validation result.</returns>
    public RouteConstraintValidationResult ValidateConstraints(
        IReadOnlyList<PatternCapture<string>> captures,
        RouteConstraintValidationOptions? options = null)
    {
        return RouteConstraintValidation.Validate(this, captures, options);
    }

    /// <summary>
    /// Validates this template's preserved constraints against a detailed structural match result.
    /// </summary>
    /// <typeparam name="TValue">The matched value type.</typeparam>
    /// <param name="match">The detailed structural match result to validate.</param>
    /// <param name="options">Optional route constraint validation options.</param>
    /// <returns>The route constraint validation result.</returns>
    public RouteConstraintValidationResult ValidateConstraints<TValue>(
        PatternMatchResult<string, TValue> match,
        RouteConstraintValidationOptions? options = null)
    {
        return RouteConstraintValidation.Validate(this, match, options);
    }

    /// <summary>
    /// Compiles this route template into one generic pattern, preserving all structural segments.
    /// </summary>
    /// <returns>The generic core pattern segments represented by this template.</returns>
    /// <remarks>
    /// Optional or defaulted parameters are included in the returned pattern. Use <see cref="Expand"/> when optional/defaulted suffix parameters should be omitted as additional registrations.
    /// </remarks>
    public PatternSegment<string>[] Compile()
    {
        var compiled = new PatternSegment<string>[Segments.Count];
        for (var i = 0; i < compiled.Length; i++)
        {
            compiled[i] = Segments[i].ToPatternSegment();
        }

        return compiled;
    }

    /// <summary>
    /// Expands optional/defaulted suffix parameters into one or more generic core patterns.
    /// </summary>
    /// <returns>The generic core patterns represented by this template.</returns>
    /// <remarks>
    /// Optional/defaulted parameters must form a contiguous suffix. This keeps expansion deterministic and avoids framework-specific interpretation of omitted middle segments.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when optional/defaulted parameters are not a contiguous suffix.</exception>
    public PatternSegment<string>[][] Expand()
    {
        var detailed = ExpandDetailed();
        var expanded = new PatternSegment<string>[detailed.Length][];
        for (var i = 0; i < detailed.Length; i++)
        {
            expanded[i] = detailed[i].Pattern;
        }

        return expanded;
    }

    /// <summary>
    /// Expands optional/defaulted suffix parameters into one or more generic core patterns with expansion metadata.
    /// </summary>
    /// <returns>The route-template expansions represented by this template.</returns>
    /// <remarks>
    /// Expansions are ordered from shortest omitted-suffix variant to the full template. Each expansion retains a link to this route template
    /// and records which optional/defaulted parameters were omitted before registration with the generic core matcher.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when optional/defaulted parameters are not a contiguous suffix.</exception>
    public RouteTemplateExpansion[] ExpandDetailed()
    {
        var optionalStart = GetOptionalSuffixStart();
        var optionalCount = Segments.Count - optionalStart;
        var expanded = new RouteTemplateExpansion[optionalCount + 1];
        for (var expansionIndex = 0; expansionIndex < expanded.Length; expansionIndex++)
        {
            var length = optionalStart + expansionIndex;
            var compiled = new PatternSegment<string>[length];
            for (var i = 0; i < length; i++)
            {
                compiled[i] = Segments[i].ToPatternSegment();
            }

            var omittedCount = Segments.Count - length;
            var omitted = new RouteParameter[omittedCount];
            for (var i = 0; i < omitted.Length; i++)
            {
                omitted[i] = Segments[length + i].Parameter!;
            }

            expanded[expansionIndex] = new RouteTemplateExpansion(this, expansionIndex, length, compiled, omitted);
        }

        return expanded;
    }

    private int GetOptionalSuffixStart()
    {
        var optionalStart = Segments.Count;
        while (optionalStart > 0 && Segments[optionalStart - 1].Parameter?.CanOmit == true)
        {
            optionalStart--;
        }

        for (var i = 0; i < optionalStart; i++)
        {
            if (Segments[i].Parameter?.CanOmit == true)
            {
                throw new InvalidOperationException("Optional or defaulted route parameters must form a contiguous suffix before they can be expanded into generic patterns.");
            }
        }

        return optionalStart;
    }
}
