using TUnit.Assertions.Enums;

namespace Pattrn.Routing.Tests.Templates;

public sealed class RoutePatternTests
{
    [Test]
    public async Task ParseConvertsRouteTemplateToGenericPatternSegments()
    {
        var pattern = RoutePattern.Parse("/customers/{customerId}/orders/{orderId}");

        await Assert.That(pattern.Length).IsEqualTo(4);
        await Assert.That(pattern[0]).IsEqualTo(PatternSegment<string>.Literal("customers"));
        await Assert.That(pattern[1]).IsEqualTo(PatternSegment<string>.Parameter("customerId"));
        await Assert.That(pattern[2]).IsEqualTo(PatternSegment<string>.Literal("orders"));
        await Assert.That(pattern[3]).IsEqualTo(PatternSegment<string>.Parameter("orderId"));
    }

    [Test]
    public async Task ParseConvertsTerminalCatchAllToGenericPatternSegment()
    {
        var pattern = RoutePattern.Parse("/files/{*path}");

        await Assert.That(pattern.Length).IsEqualTo(2);
        await Assert.That(pattern[0]).IsEqualTo(PatternSegment<string>.Literal("files"));
        await Assert.That(pattern[1]).IsEqualTo(PatternSegment<string>.CatchAll("path"));
    }

    [Test]
    public async Task ParseTreatsStarAsLiteralNotCoreWildcardToken()
    {
        var pattern = RoutePattern.Parse("/files/*");

        await Assert.That(pattern.Length).IsEqualTo(2);
        await Assert.That(pattern[1]).IsEqualTo(PatternSegment<string>.Literal("*"));
    }

    [Test]
    public async Task ParseSupportsRootPattern()
    {
        await Assert.That(RoutePattern.Parse("/")).IsEquivalentTo(Array.Empty<PatternSegment<string>>(), CollectionOrdering.Matching);
        await Assert.That(RoutePattern.Parse("")).IsEquivalentTo(Array.Empty<PatternSegment<string>>(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task SplitPathSupportsRootPath()
    {
        await Assert.That(RoutePattern.SplitPath("/")).IsEquivalentTo(Array.Empty<string>(), CollectionOrdering.Matching);
        await Assert.That(RoutePattern.SplitPath("")).IsEquivalentTo(Array.Empty<string>(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task SplitPathTrimsOneLeadingAndTrailingSlash()
    {
        await Assert.That(RoutePattern.SplitPath("/orders/123/")).IsEquivalentTo(["orders", "123"], CollectionOrdering.Matching);
        await Assert.That(RoutePattern.SplitPath("orders/123")).IsEquivalentTo(["orders", "123"], CollectionOrdering.Matching);
    }


    [Test]
    public async Task GetPathSegmentCountCountsTrimmedSegments()
    {
        await Assert.That(RoutePattern.GetPathSegmentCount("/orders/123/")).IsEqualTo(2);
        await Assert.That(RoutePattern.GetPathSegmentCount("/")).IsEqualTo(0);
        await Assert.That(RoutePattern.GetPathSegmentCount("")).IsEqualTo(0);
    }

    [Test]
    public async Task SplitPathCanWriteToCallerProvidedSpan()
    {
        var destination = new string[2];

        var written = RoutePattern.SplitPath("/orders/123/", destination);

        await Assert.That(written).IsEqualTo(2);
        await Assert.That(destination).IsEquivalentTo(["orders", "123"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task SplitPathThrowsWhenDestinationSpanIsTooSmall()
    {
        var destination = new string[1];

        await Assert.That(() => RoutePattern.SplitPath("/orders/123", destination)).Throws<ArgumentException>();
    }

    [Test]
    public async Task TrySplitPathReturnsFalseWithoutWritingWhenDestinationSpanIsTooSmall()
    {
        var destination = new[] { "existing" };

        var success = RoutePattern.TrySplitPath("/orders/123", destination, out var written);

        await Assert.That(success).IsFalse().Because("Expected split to fail when the destination is too small.");
        await Assert.That(written).IsEqualTo(0);
        await Assert.That(destination[0]).IsEqualTo("existing");
    }

    [Test]
    public async Task TrySplitPathWritesSegmentsWhenDestinationSpanIsLargeEnough()
    {
        var destination = new string[3];

        var success = RoutePattern.TrySplitPath("/files/a/b", destination, out var written);

        await Assert.That(success).IsTrue().Because("Expected split to succeed.");
        await Assert.That(written).IsEqualTo(3);
        await Assert.That(destination).IsEquivalentTo(["files", "a", "b"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ParsePreservesConstraintsButCompilesStructuralParameter()
    {
        var pattern = RoutePattern.Parse("/orders/{id:int:min(1)}");

        await Assert.That(pattern.Length).IsEqualTo(2);
        await Assert.That(pattern[0]).IsEqualTo(PatternSegment<string>.Literal("orders"));
        await Assert.That(pattern[1]).IsEqualTo(PatternSegment<string>.Parameter("id"));

        var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1)}");
        var parameter = template.Segments[1].Parameter!;
        await Assert.That(parameter.Name).IsEqualTo("id");
        await Assert.That(parameter.Constraints.Count).IsEqualTo(2);
        await Assert.That(parameter.Constraints[0].Name).IsEqualTo("int");
        await Assert.That(parameter.Constraints[1].Name).IsEqualTo("min");
        await Assert.That(parameter.Constraints[1].Argument).IsEqualTo("1");
    }

    [Test]
    public async Task ParseTemplatePreservesOptionalAndDefaultMetadata()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int?}");

        await Assert.That(template.Text).IsEqualTo("/archive/{year:int}/{month:int?}");
        await Assert.That(template.Segments.Count).IsEqualTo(3);
        await Assert.That(template.Segments[1].Parameter!.IsOptional).IsFalse().Because("Year should be required.");
        await Assert.That(template.Segments[2].Parameter!.IsOptional).IsTrue().Because("Month should be optional.");
        await Assert.That(template.HasOptionalSegments).IsTrue().Because("Template should report optional segments.");

        var defaulted = RoutePattern.ParseTemplate("/reports/{format=json}");
        await Assert.That(defaulted.Segments[1].Parameter!.DefaultValue).IsEqualTo("json");
        await Assert.That(defaulted.Segments[1].Parameter!.HasDefaultValue).IsTrue().Because("Default metadata should be preserved.");
    }

    [Test]
    public async Task ExpandCreatesVariantsForOptionalSuffixParameters()
    {
        var expanded = RoutePattern.Expand("/archive/{year:int}/{month:int?}");

        await Assert.That(expanded.Length).IsEqualTo(2);
        await Assert.That(expanded[0].Length).IsEqualTo(2);
        await Assert.That(expanded[0][0]).IsEqualTo(PatternSegment<string>.Literal("archive"));
        await Assert.That(expanded[0][1]).IsEqualTo(PatternSegment<string>.Parameter("year"));
        await Assert.That(expanded[1].Length).IsEqualTo(3);
        await Assert.That(expanded[1][0]).IsEqualTo(PatternSegment<string>.Literal("archive"));
        await Assert.That(expanded[1][1]).IsEqualTo(PatternSegment<string>.Parameter("year"));
        await Assert.That(expanded[1][2]).IsEqualTo(PatternSegment<string>.Parameter("month"));
    }


    [Test]
    public async Task ExpandDetailedKeepsExpansionMetadataLinkedToOriginalTemplate()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int=6}/{day:int?}");

        var expansions = template.ExpandDetailed();

        await Assert.That(expansions.Length).IsEqualTo(3);
        await Assert.That(expansions[0].Template).IsEqualTo(template);
        await Assert.That(expansions[0].ExpansionIndex).IsEqualTo(0);
        await Assert.That(expansions[0].IncludedSegmentCount).IsEqualTo(2);
        await Assert.That(expansions[0].IsFullTemplate).IsFalse().Because("The shortest expansion omits optional/defaulted suffix parameters.");
        await Assert.That(expansions[0].Pattern.Length).IsEqualTo(2);
        await Assert.That(expansions[0].Pattern[0]).IsEqualTo(PatternSegment<string>.Literal("archive"));
        await Assert.That(expansions[0].Pattern[1]).IsEqualTo(PatternSegment<string>.Parameter("year"));
        await Assert.That(expansions[0].OmittedParameters.Count).IsEqualTo(2);
        await Assert.That(expansions[0].OmittedParameters[0].Name).IsEqualTo("month");
        await Assert.That(expansions[0].OmittedParameters[0].DefaultValue).IsEqualTo("6");
        await Assert.That(expansions[0].OmittedParameters[1].Name).IsEqualTo("day");

        await Assert.That(expansions[1].ExpansionIndex).IsEqualTo(1);
        await Assert.That(expansions[1].IncludedSegmentCount).IsEqualTo(3);
        await Assert.That(expansions[1].Pattern.Length).IsEqualTo(3);
        await Assert.That(expansions[1].Pattern[0]).IsEqualTo(PatternSegment<string>.Literal("archive"));
        await Assert.That(expansions[1].Pattern[1]).IsEqualTo(PatternSegment<string>.Parameter("year"));
        await Assert.That(expansions[1].Pattern[2]).IsEqualTo(PatternSegment<string>.Parameter("month"));
        await Assert.That(expansions[1].OmittedParameters.Count).IsEqualTo(1);
        await Assert.That(expansions[1].OmittedParameters[0].Name).IsEqualTo("day");

        await Assert.That(expansions[2].ExpansionIndex).IsEqualTo(2);
        await Assert.That(expansions[2].IncludedSegmentCount).IsEqualTo(4);
        await Assert.That(expansions[2].IsFullTemplate).IsTrue().Because("The final expansion includes the full template.");
        await Assert.That(expansions[2].Pattern.Length).IsEqualTo(4);
        await Assert.That(expansions[2].Pattern[0]).IsEqualTo(PatternSegment<string>.Literal("archive"));
        await Assert.That(expansions[2].Pattern[1]).IsEqualTo(PatternSegment<string>.Parameter("year"));
        await Assert.That(expansions[2].Pattern[2]).IsEqualTo(PatternSegment<string>.Parameter("month"));
        await Assert.That(expansions[2].Pattern[3]).IsEqualTo(PatternSegment<string>.Parameter("day"));
        await Assert.That(expansions[2].OmittedParameters.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AddRouteUsesSamePatternIdentityForExpandedOptionalVariants()
    {
        var builder = PattrnIndex<string, string>.Builder();
        builder.AddRoute("/archive/{year:int}/{month:int=6}/{day:int?}", "archive", name: "archive-template");
        var index = builder.Build();

        var shortMatches = index.MatchDetailedToArray(RoutePattern.SplitPath("/archive/2026"));
        var fullMatches = index.MatchDetailedToArray(RoutePattern.SplitPath("/archive/2026/7/14"));

        await Assert.That(shortMatches.Length).IsEqualTo(1);
        await Assert.That(shortMatches[0].Captures.Length).IsEqualTo(1);
        await Assert.That(shortMatches[0].Captures[0].Name).IsEqualTo("year");

        await Assert.That(fullMatches.Length).IsEqualTo(1);
        await Assert.That(fullMatches[0].Captures.Length).IsEqualTo(3);
        await Assert.That(fullMatches[0].Captures[2].Name).IsEqualTo("day");
    }

    [Test]
    public async Task TryParseTemplateReturnsDiagnosticsForUnsupportedSyntax()
    {
        var success = RoutePattern.TryParseTemplate("/orders/{id?}/items", out var template, out var diagnostics);

        await Assert.That(success).IsFalse().Because("Expected parse to fail when optional parameters are not a suffix.");
        await Assert.That(template).IsNull();
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Code).IsEqualTo("ROUTE014");
        await Assert.That(diagnostics[0].SegmentIndex).IsEqualTo(1);
    }

    [Test]
    [Arguments("/orders/{order-id}", 1)]
    [Arguments("/{9id}", 0)]
    [Arguments("/{id.name}", 0)]
    [Arguments("/files/{*path-name}", 1)]
    [Arguments("/{\uD801}", 0)]
    [Arguments("/{\uDC00}", 0)]
    public async Task TryParseTemplateRejectsInvalidCaptureNamesWithRoute015(string pattern, int segmentIndex)
    {
        var success = RouteTemplate.TryParse(pattern, out var template, out var diagnostics);

        await Assert.That(success).IsFalse().Because("Expected route template parsing to reject invalid capture name.");
        await Assert.That(template).IsNull();
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Code).IsEqualTo("ROUTE015");
        await Assert.That(diagnostics[0].SegmentIndex).IsEqualTo(segmentIndex);

        success = RoutePattern.TryParseTemplate(pattern, out template, out diagnostics);

        await Assert.That(success).IsFalse().Because("Expected route pattern parsing to reject invalid capture name.");
        await Assert.That(template).IsNull();
        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Code).IsEqualTo("ROUTE015");
        await Assert.That(diagnostics[0].SegmentIndex).IsEqualTo(segmentIndex);
    }

    [Test]
    public async Task SupplementaryPlaneParameterNameParsesCompilesAndExpands()
    {
        var success = RoutePattern.TryParseTemplate("/orders/{𐐀id}", out var template, out var diagnostics);

        await Assert.That(success).IsTrue().Because("Expected supplementary-plane letter parameter name to parse.");
        await Assert.That(diagnostics.Length).IsEqualTo(0);
        await Assert.That(template!.Compile()[1]).IsEqualTo(PatternSegment<string>.Parameter("𐐀id"));

        var expanded = RoutePattern.Expand("/orders/{𐐀id}")[0];
        await Assert.That(expanded.Length).IsEqualTo(2);
        await Assert.That(expanded[0]).IsEqualTo(PatternSegment<string>.Literal("orders"));
        await Assert.That(expanded[1]).IsEqualTo(PatternSegment<string>.Parameter("𐐀id"));

        var detailedPattern = RoutePattern.ExpandDetailed("/orders/{𐐀id}")[0].Pattern;
        await Assert.That(detailedPattern.Length).IsEqualTo(2);
        await Assert.That(detailedPattern[0]).IsEqualTo(PatternSegment<string>.Literal("orders"));
        await Assert.That(detailedPattern[1]).IsEqualTo(PatternSegment<string>.Parameter("𐐀id"));
    }

    [Test]
    public async Task InvalidCaptureNamesThrowFromParseApis()
    {
        await Assert.That(() => RouteTemplate.Parse("/orders/{order-id}")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders/{order-id}")).Throws<ArgumentException>();
    }

    [Test]
    public async Task AddRouteRejectsInvalidCaptureNamesBeforeMutatingBuilder()
    {
        var builder = PattrnIndex<string, string>.Builder();
        builder.AddRoute("/orders/{id}", "existing");
        var patternCount = builder.PatternCount;
        var registrationCount = builder.RegistrationCount;

        await Assert.That(() => builder.AddRoute("/orders/{order-id}", "invalid")).Throws<ArgumentException>();

        await Assert.That(builder.PatternCount).IsEqualTo(patternCount);
        await Assert.That(builder.RegistrationCount).IsEqualTo(registrationCount);
    }

    [Test]
    public async Task SuccessfulParseCompilesAndExpandsWithoutCaptureNameException()
    {
        var template = RoutePattern.ParseTemplate("/archive/{year}/{month=07}/{day?}");

        await Assert.That(template.Compile().Length).IsEqualTo(4);
        await Assert.That(template.Expand().Length).IsEqualTo(3);
        await Assert.That(template.ExpandDetailed().Length).IsEqualTo(3);
    }

    [Test]
    public async Task ParseRejectsInvalidRouteSyntax()
    {
        await Assert.That(() => RoutePattern.Parse("/orders/{}")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders/{ }")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/files/{*path}/tail")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders//{id}")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("//orders/{id}")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders/{id}//")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders/{id?}/items")).Throws<ArgumentException>();
    }
}

