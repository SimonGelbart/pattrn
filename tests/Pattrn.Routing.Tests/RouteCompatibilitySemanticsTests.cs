using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RouteCompatibilitySemanticsTests
{
    [Test]
    public void RouteMatchingKeepsLiteralBeforeParameterBeforeCatchAllOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/orders/{*tail}", "catch-all")
            .AddRoute("/orders/{id}", "parameter")
            .AddRoute("/orders/new", "literal")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchRouteDetailedToArray("/orders/new");

        ShouldSequenceEqual(matches.Select(match => match.Value), ["literal", "parameter", "catch-all"]);
        ShouldBeTrue(matches[0].Specificity > matches[1].Specificity, "Literal routes should be more specific than parameter routes.");
        ShouldBeTrue(matches[1].Specificity > matches[2].Specificity, "Parameter routes should be more specific than catch-all routes.");
    }

    [Test]
    public void RouteCapturesKeepSegmentValuesWithoutJoiningOrDecoding()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/files/{*path}", "files")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/files/a/b%20c/d.txt");

        ShouldEqual(matches.Length, 1);
        ShouldSequenceEqual(
            matches[0].Captures,
            [
                new PatternCapture<string>("path", "a", 1),
                new PatternCapture<string>("path", "b%20c", 2),
                new PatternCapture<string>("path", "d.txt", 3)
            ]);
    }

    [Test]
    public void RouteParserPreservesDeferredConstraintSyntaxWithoutEvaluatingIt()
    {
        var template = RoutePattern.ParseTemplate("/orders/{id:int}");

        ShouldEqual(template.Segments[1].Parameter!.Constraints.Count, 1);
        ShouldEqual(template.Segments[1].Parameter!.Constraints[0].Name, "int");

        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/files/{*path}/tail"));
        ShouldThrow<ArgumentException>(() => RoutePattern.Parse("/orders//{id}"));
    }

    [Test]
    public void OptionalRouteSuffixExpandsIntoMultipleStructuralRegistrations()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id?}", "orders", patternId: "orders-optional-id")
            .Build(MatchOptions.PreserveDuplicates);

        var rootMatches = index.MatchRouteDetailedToArray("/orders");
        var itemMatches = index.MatchRouteDetailedToArray("/orders/123");

        ShouldEqual(rootMatches.Length, 1);
        ShouldEqual(itemMatches.Length, 1);
        ShouldEqual(rootMatches[0].PatternId, "orders-optional-id");
        ShouldEqual(itemMatches[0].PatternId, "orders-optional-id");
        ShouldEqual(itemMatches[0].Captures[0], new PatternCapture<string>("id", "123", 1));
    }
}

public sealed class RouteIdentityTests
{
    [Test]
    public void AddRouteFlowsPatternIdentityToDetailedMatches()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .AddRoute("/orders/{id}", "handler", patternId: "orders-by-id")
            .Build();

        var match = index.MatchDetailedToArray(["orders", "123"]).Single();

        ShouldEqual(match.PatternId, "orders-by-id");
        ShouldEqual(match.RegistrationOrder, 0);
        ShouldEqual(match.Captures[0], new PatternCapture<string>("id", "123", 1));
    }
}
