namespace Pattrn.Routing;

internal static class RouteTemplateParser
{
    internal static bool TryParse(string template, out RouteTemplate? routeTemplate, out RouteTemplateDiagnostic[] diagnostics)
    {
        var diagnosticList = new List<RouteTemplateDiagnostic>();
        string[] segmentTexts;
        try
        {
            segmentTexts = RoutePattern.SplitTemplateSegments(template);
        }
        catch (ArgumentException exception)
        {
            routeTemplate = null;
            diagnostics = [new RouteTemplateDiagnostic("ROUTE001", exception.Message)];
            return false;
        }

        var segments = new RouteSegment[segmentTexts.Length];
        for (var i = 0; i < segmentTexts.Length; i++)
        {
            if (!TryParseSegment(segmentTexts[i], i, segmentTexts.Length, diagnosticList, out var segment))
            {
                routeTemplate = null;
                diagnostics = [.. diagnosticList];
                return false;
            }

            segments[i] = segment;
        }

        ValidateOptionalSuffix(segments, diagnosticList);
        if (diagnosticList.Count > 0)
        {
            routeTemplate = null;
            diagnostics = [.. diagnosticList];
            return false;
        }

        routeTemplate = new RouteTemplate(template, segments, []);
        diagnostics = [];
        return true;
    }

    private static bool TryParseSegment(
        string text,
        int index,
        int segmentCount,
        List<RouteTemplateDiagnostic> diagnostics,
        out RouteSegment segment)
    {
        segment = null!;

        if (text.Length < 2 || text[0] != '{' || text[^1] != '}')
        {
            if (text.IndexOfAny(['{', '}']) >= 0)
            {
                diagnostics.Add(new RouteTemplateDiagnostic("ROUTE002", "Complex route segments that mix literals and parameters are not supported by this framework-neutral parser.", index));
                return false;
            }

            segment = RouteSegment.ForLiteral(text);
            return true;
        }

        var body = text[1..^1];
        if (body.Length == 0)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE003", "Route parameter names cannot be empty.", index));
            return false;
        }

        if (body.IndexOfAny(['{', '}']) >= 0)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE004", "Route parameter segments cannot contain nested braces.", index));
            return false;
        }

        if (!TryParseParameter(body, index, segmentCount, diagnostics, out var parameter))
        {
            return false;
        }

        segment = RouteSegment.ForParameter(parameter);
        return true;
    }

    private static bool TryParseParameter(
        string body,
        int segmentIndex,
        int segmentCount,
        List<RouteTemplateDiagnostic> diagnostics,
        out RouteParameter parameter)
    {
        parameter = null!;

        var isCatchAll = false;
        if (body.StartsWith("**", StringComparison.Ordinal))
        {
            isCatchAll = true;
            body = body[2..];
        }
        else if (body.StartsWith('*'))
        {
            isCatchAll = true;
            body = body[1..];
        }

        if (isCatchAll && segmentIndex != segmentCount - 1)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE005", "Route catch-all parameters must be terminal.", segmentIndex));
            return false;
        }

        var isOptional = false;
        if (body.EndsWith('?'))
        {
            isOptional = true;
            body = body[..^1];
        }

        var defaultValue = default(string);
        var defaultIndex = IndexOfTopLevel(body, '=');
        if (defaultIndex >= 0)
        {
            defaultValue = body[(defaultIndex + 1)..];
            body = body[..defaultIndex];
            if (defaultValue.Length == 0)
            {
                diagnostics.Add(new RouteTemplateDiagnostic("ROUTE006", "Route parameter default values cannot be empty.", segmentIndex));
                return false;
            }
        }

        var tokens = SplitTopLevel(body, ':');
        if (tokens is null)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE007", "Route parameter constraint parentheses are unbalanced.", segmentIndex));
            return false;
        }

        var name = tokens[0];
        if (string.IsNullOrWhiteSpace(name))
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE008", "Route parameter names cannot be empty or whitespace.", segmentIndex));
            return false;
        }

        if (name.IndexOf('*') >= 0)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE009", "A route catch-all marker must appear at the start of a parameter segment.", segmentIndex));
            return false;
        }

        if (isCatchAll && (isOptional || defaultValue is not null || tokens.Count > 1))
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE010", "Route catch-all parameters cannot declare optional markers, defaults, or constraints in this framework-neutral parser.", segmentIndex));
            return false;
        }

        var constraints = new RouteConstraint[tokens.Count - 1];
        for (var i = 1; i < tokens.Count; i++)
        {
            if (!TryParseConstraint(tokens[i], segmentIndex, diagnostics, out var constraint))
            {
                return false;
            }

            constraints[i - 1] = constraint;
        }

        parameter = new RouteParameter(name, isCatchAll, isOptional, defaultValue, constraints);
        return true;
    }

    private static bool TryParseConstraint(
        string text,
        int segmentIndex,
        List<RouteTemplateDiagnostic> diagnostics,
        out RouteConstraint constraint)
    {
        constraint = null!;
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE011", "Route parameter constraints cannot be empty.", segmentIndex));
            return false;
        }

        var openIndex = text.IndexOf('(', StringComparison.Ordinal);
        if (openIndex < 0)
        {
            constraint = new RouteConstraint(text, text, null);
            return true;
        }

        if (!text.EndsWith(')') || openIndex == 0)
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE012", "Route parameter constraints with arguments must use name(argument) syntax.", segmentIndex));
            return false;
        }

        var name = text[..openIndex];
        if (string.IsNullOrWhiteSpace(name))
        {
            diagnostics.Add(new RouteTemplateDiagnostic("ROUTE013", "Route parameter constraint names cannot be empty.", segmentIndex));
            return false;
        }

        var argument = text[(openIndex + 1)..^1];
        constraint = new RouteConstraint(text, name, argument);
        return true;
    }

    private static void ValidateOptionalSuffix(RouteSegment[] segments, List<RouteTemplateDiagnostic> diagnostics)
    {
        var seenRequiredAfterOptional = false;
        for (var i = segments.Length - 1; i >= 0; i--)
        {
            var canOmit = segments[i].Parameter?.CanOmit == true;
            if (!canOmit)
            {
                seenRequiredAfterOptional = true;
                continue;
            }

            if (seenRequiredAfterOptional)
            {
                diagnostics.Add(new RouteTemplateDiagnostic("ROUTE014", "Optional or defaulted route parameters must form a contiguous suffix.", i));
            }
        }
    }

    private static int IndexOfTopLevel(string value, char separator)
    {
        var depth = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (current == '(')
            {
                depth++;
            }
            else if (current == ')' && depth > 0)
            {
                depth--;
            }
            else if (current == separator && depth == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private static List<string>? SplitTopLevel(string value, char separator)
    {
        var result = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];
            if (current == '(')
            {
                depth++;
            }
            else if (current == ')')
            {
                depth--;
                if (depth < 0)
                {
                    return null;
                }
            }
            else if (current == separator && depth == 0)
            {
                result.Add(value[start..i]);
                start = i + 1;
            }
        }

        if (depth != 0)
        {
            return null;
        }

        result.Add(value[start..]);
        return result;
    }
}
