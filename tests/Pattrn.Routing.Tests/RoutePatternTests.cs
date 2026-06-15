using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RoutePatternTests
{
    [Test]
    public void ParseConvertsRouteTemplateToGenericPatternSegments()
    {
        var pattern = RoutePattern.Parse("/customers/{customerId}/orders/{orderId}");

        ShouldEqual(pattern.Length, 4);
        ShouldEqual(pattern[0], PatternSegment<string>.Literal("customers"));
        ShouldEqual(pattern[1], PatternSegment<string>.Parameter("customerId"));
        ShouldEqual(pattern[2], PatternSegment<string>.Literal("orders"));
        ShouldEqual(pattern[3], PatternSegment<string>.Parameter("orderId"));
    }

    [Test]
    public void ParseConvertsTerminalCatchAllToGenericPatternSegment()
    {
        var pattern = RoutePattern.Parse("/files/{*path}");

        ShouldEqual(pattern.Length, 2);
        ShouldEqual(pattern[0], PatternSegment<string>.Literal("files"));
        ShouldEqual(pattern[1], PatternSegment<string>.CatchAll("path"));
    }

    [Test]
    public void ParseTreatsStarAsLiteralNotCoreWildcardToken()
    {
        var pattern = RoutePattern.Parse("/files/*");

        ShouldEqual(pattern.Length, 2);
        ShouldEqual(pattern[1], PatternSegment<string>.Literal("*"));
    }

    [Test]
    public void ParseSupportsRootPattern()
    {
        ShouldSequenceEqual(RoutePattern.Parse("/"), []);
        ShouldSequenceEqual(RoutePattern.Parse(""), []);
    }

    [Test]
    public void SplitPathSupportsRootPath()
    {
        ShouldSequenceEqual(RoutePattern.SplitPath("/"), []);
        ShouldSequenceEqual(RoutePattern.SplitPath(""), []);
    }

    [Test]
    public void SplitPathTrimsOneLeadingAndTrailingSlash()
    {
        ShouldSequenceEqual(RoutePattern.SplitPath("/orders/123/"), ["orders", "123"]);
        ShouldSequenceEqual(RoutePattern.SplitPath("orders/123"), ["orders", "123"]);
    }


    [Test]
    public void GetPathSegmentCountCountsTrimmedSegments()
    {
        ShouldEqual(RoutePattern.GetPathSegmentCount("/orders/123/"), 2);
        ShouldEqual(RoutePattern.GetPathSegmentCount("/"), 0);
        ShouldEqual(RoutePattern.GetPathSegmentCount(""), 0);
    }

    [Test]
    public void SplitPathCanWriteToCallerProvidedSpan()
    {
        var destination = new string[2];

        var written = RoutePattern.SplitPath("/orders/123/", destination);

        ShouldEqual(written, 2);
        ShouldSequenceEqual(destination, ["orders", "123"]);
    }

    [Test]
    public void SplitPathThrowsWhenDestinationSpanIsTooSmall()
    {
        var destination = new string[1];

        ShouldThrow<ArgumentException>(() => RoutePattern.SplitPath("/orders/123", destination));
    }

    [Test]
    public void TrySplitPathReturnsFalseWithoutWritingWhenDestinationSpanIsTooSmall()
    {
        var destination = new[] { "existing" };

        var success = RoutePattern.TrySplitPath("/orders/123", destination, out var written);

        ShouldBeFalse(success, "Expected split to fail when the destination is too small.");
        ShouldEqual(written, 0);
        ShouldEqual(destination[0], "existing");
    }

    [Test]
    public void TrySplitPathWritesSegmentsWhenDestinationSpanIsLargeEnough()
    {
        var destination = new string[3];

        var success = RoutePattern.TrySplitPath("/files/a/b", destination, out var written);

        ShouldBeTrue(success, "Expected split to succeed.");
        ShouldEqual(written, 3);
        ShouldSequenceEqual(destination, ["files", "a", "b"]);
    }

    [Test]
    public void ParsePreservesConstraintsButCompilesStructuralParameter()
    {
        var pattern = RoutePattern.Parse("/orders/{id:int:min(1)}");

        ShouldEqual(pattern.Length, 2);
        ShouldEqual(pattern[0], PatternSegment<string>.Literal("orders"));
        ShouldEqual(pattern[1], PatternSegment<string>.Parameter("id"));

        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1)}");
        var parameter = template.Segments[1].Parameter!;
        ShouldEqual(parameter.Name, "id");
        ShouldEqual(parameter.Constraints.Count, 2);
        ShouldEqual(parameter.Constraints[0].Name, "int");
        ShouldEqual(parameter.Constraints[1].Name, "min");
        ShouldEqual(parameter.Constraints[1].Argument, "1");
    }

    [Test]
    public void ParseTemplatePreservesOptionalAndDefaultMetadata()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int?}");

        ShouldEqual(template.Text, "/archive/{year:int}/{month:int?}");
        ShouldEqual(template.Segments.Count, 3);
        ShouldBeFalse(template.Segments[1].Parameter!.IsOptional, "Year should be required.");
        ShouldBeTrue(template.Segments[2].Parameter!.IsOptional, "Month should be optional.");
        ShouldBeTrue(template.HasOptionalSegments, "Template should report optional segments.");

        var defaulted = RoutePattern.ParseTemplate("/reports/{format=json}");
        ShouldEqual(defaulted.Segments[1].Parameter!.DefaultValue, "json");
        ShouldBeTrue(defaulted.Segments[1].Parameter!.HasDefaultValue, "Default metadata should be preserved.");
    }

    [Test]
    public void ExpandCreatesVariantsForOptionalSuffixParameters()
    {
        var expanded = RoutePattern.Expand("/archive/{year:int}/{month:int?}");

        ShouldEqual(expanded.Length, 2);
        ShouldSequenceEqual(expanded[0], [PatternSegment<string>.Literal("archive"), PatternSegment<string>.Parameter("year")]);
        ShouldSequenceEqual(expanded[1], [PatternSegment<string>.Literal("archive"), PatternSegment<string>.Parameter("year"), PatternSegment<string>.Parameter("month")]);
    }


    [Test]
    public void ExpandDetailedKeepsExpansionMetadataLinkedToOriginalTemplate()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int=6}/{day:int?}");

        var expansions = template.ExpandDetailed();

        ShouldEqual(expansions.Length, 3);
        ShouldEqual(expansions[0].Template, template);
        ShouldEqual(expansions[0].ExpansionIndex, 0);
        ShouldEqual(expansions[0].IncludedSegmentCount, 2);
        ShouldBeFalse(expansions[0].IsFullTemplate, "The shortest expansion omits optional/defaulted suffix parameters.");
        ShouldSequenceEqual(expansions[0].Pattern, [PatternSegment<string>.Literal("archive"), PatternSegment<string>.Parameter("year")]);
        ShouldEqual(expansions[0].OmittedParameters.Count, 2);
        ShouldEqual(expansions[0].OmittedParameters[0].Name, "month");
        ShouldEqual(expansions[0].OmittedParameters[0].DefaultValue, "6");
        ShouldEqual(expansions[0].OmittedParameters[1].Name, "day");

        ShouldEqual(expansions[1].ExpansionIndex, 1);
        ShouldEqual(expansions[1].IncludedSegmentCount, 3);
        ShouldSequenceEqual(expansions[1].Pattern, [PatternSegment<string>.Literal("archive"), PatternSegment<string>.Parameter("year"), PatternSegment<string>.Parameter("month")]);
        ShouldEqual(expansions[1].OmittedParameters.Count, 1);
        ShouldEqual(expansions[1].OmittedParameters[0].Name, "day");

        ShouldEqual(expansions[2].ExpansionIndex, 2);
        ShouldEqual(expansions[2].IncludedSegmentCount, 4);
        ShouldBeTrue(expansions[2].IsFullTemplate, "The final expansion includes the full template.");
        ShouldSequenceEqual(expansions[2].Pattern, [PatternSegment<string>.Literal("archive"), PatternSegment<string>.Parameter("year"), PatternSegment<string>.Parameter("month"), PatternSegment<string>.Parameter("day")]);
        ShouldEqual(expansions[2].OmittedParameters.Count, 0);
    }

    [Test]
    public void AddRouteUsesSamePatternIdentityForExpandedOptionalVariants()
    {
        var builder = PattrnIndex<string, string>.Builder();
        builder.AddRoute("/archive/{year:int}/{month:int=6}/{day:int?}", "archive", patternId: "archive-template");
        var index = builder.Build();

        var shortMatches = index.MatchDetailed(RoutePattern.SplitPath("/archive/2026"));
        var fullMatches = index.MatchDetailed(RoutePattern.SplitPath("/archive/2026/7/14"));

        ShouldEqual(shortMatches.Length, 1);
        ShouldEqual(shortMatches[0].PatternId, "archive-template");
        ShouldEqual(shortMatches[0].Captures.Count, 1);
        ShouldEqual(shortMatches[0].Captures[0].Name, "year");

        ShouldEqual(fullMatches.Length, 1);
        ShouldEqual(fullMatches[0].PatternId, "archive-template");
        ShouldEqual(fullMatches[0].Captures.Count, 3);
        ShouldEqual(fullMatches[0].Captures[2].Name, "day");
    }

    [Test]
    public void TryParseTemplateReturnsDiagnosticsForUnsupportedSyntax()
    {
        var success = RoutePattern.TryParseTemplate("/orders/{id?}/items", out var template, out var diagnostics);

        ShouldBeFalse(success, "Expected parse to fail when optional parameters are not a suffix.");
        ShouldEqual(template, null);
        ShouldEqual(diagnostics.Length, 1);
        ShouldEqual(diagnostics[0].Code, "ROUTE014");
        ShouldEqual(diagnostics[0].SegmentIndex, 1);
    }

    [Test]
    public void ParseRejectsInvalidRouteSyntax()
    {
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders/{}"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders/{ }"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/files/{*path}/tail"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders//{id}"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("//orders/{id}"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders/{id}//"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders/{id?}/items"));
    }
}
