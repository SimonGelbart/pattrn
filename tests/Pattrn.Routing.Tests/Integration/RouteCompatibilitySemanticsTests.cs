using TUnit.Assertions.Enums;

namespace Pattrn.Routing.Tests.Integration;

public sealed class RouteCompatibilitySemanticsTests
{
    [Test]
    public async Task RouteMatchingKeepsLiteralBeforeParameterBeforeCatchAllOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/orders/{*tail}", "catch-all")
            .AddRoute("/orders/{id}", "parameter")
            .AddRoute("/orders/new", "literal")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchRouteDetailedToArray("/orders/new");

        await Assert.That(matches.Select(match => match.Value)).IsEquivalentTo(["literal", "parameter", "catch-all"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task RouteCapturesKeepSegmentValuesWithoutJoiningOrDecoding()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/files/{*path}", "files")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/files/a/b%20c/d.txt");

        await Assert.That(matches.Length).IsEqualTo(1);
        await Assert.That(matches[0].Captures.Length).IsEqualTo(1);

        var capture = matches[0].Captures[0];
        await Assert.That(capture.Name).IsEqualTo("path");
        await Assert.That(capture.Values).IsEquivalentTo(["a", "b%20c", "d.txt"], CollectionOrdering.Matching);
        await Assert.That(capture.StartSegmentIndex).IsEqualTo(1);
    }

    [Test]
    public async Task RouteParserPreservesDeferredConstraintSyntaxWithoutEvaluatingIt()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int}");

        await Assert.That(template.Segments[1].Parameter!.Constraints.Count).IsEqualTo(1);
        await Assert.That(template.Segments[1].Parameter!.Constraints[0].Name).IsEqualTo("int");

        await Assert.That(() => RoutePattern.Parse("/files/{*path}/tail")).Throws<ArgumentException>();
        await Assert.That(() => RoutePattern.Parse("/orders//{id}")).Throws<ArgumentException>();
    }

    [Test]
    public async Task OptionalRouteSuffixExpandsIntoMultipleStructuralRegistrations()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id?}", "orders", name: "orders-optional-id")
            .Build(MatchOptions.PreserveDuplicates);

        var rootMatches = index.MatchRouteDetailedToArray("/orders");
        var itemMatches = index.MatchRouteDetailedToArray("/orders/123");

        await Assert.That(rootMatches.Length).IsEqualTo(1);
        await Assert.That(itemMatches.Length).IsEqualTo(1);
        await Assert.That(itemMatches[0].Captures[0]).IsEqualTo(new PatternCapture<string>("id", "123", 1));
    }
}

public sealed class RouteIdentityTests
{
    [Test]
    public async Task AddRouteFlowsPatternIdentityToDetailedMatches()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id}", "handler", name: "orders-by-id")
            .Build();

        var match = index.MatchDetailedToArray(["orders", "123"]).Single();

        await Assert.That(match.Captures[0]).IsEqualTo(new PatternCapture<string>("id", "123", 1));
    }
}

