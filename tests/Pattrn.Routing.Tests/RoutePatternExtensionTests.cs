using static Pattrn.Routing.Tests.TestAssertions;

namespace Pattrn.Routing.Tests;

public sealed class RoutePatternExtensionTests
{
    [Test]
    public void AddRouteRegistersRouteTemplateWithoutRouteSyntaxInCore()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddRoute("/orders/{id}", "order-handler");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchRouteToArray("/orders/123"), ["order-handler"]);
        ShouldSequenceEqual(index.MatchRouteToArray("/customers/123"), []);
    }

    [Test]
    public void MatchRouteDetailedReturnsNamedParameterCaptures()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/customers/{customerId}/orders/{orderId}", "handler")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/customers/42/orders/99");

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Captures.Count, 2);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("customerId", "42", 1));
        ShouldEqual(matches[0].Captures[1], new PatternCapture<string>("orderId", "99", 3));
    }

    [Test]
    public void MatchRouteDetailedReturnsCatchAllCapturesAsSegments()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/files/{*path}", "file-handler")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/files/a/b/c.txt");

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "file-handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.CatchAll);
        ShouldEqual(matches[0].Captures.Count, 3);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("path", "a", 1));
        ShouldEqual(matches[0].Captures[1], new PatternCapture<string>("path", "b", 2));
        ShouldEqual(matches[0].Captures[2], new PatternCapture<string>("path", "c.txt", 3));
    }

    [Test]
    public void ContainsAndRemoveRouteUseRouteTemplateSemantics()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.AddRoute("/orders/{id}", "handler");

        ShouldBeTrue(builder.ContainsRoute("/orders/{id}"), "Expected route registration to exist.");
        ShouldBeTrue(builder.RemoveRoute("/orders/{id}", "handler"), "Expected route registration to be removed.");
        ShouldBeFalse(builder.ContainsRoute("/orders/{id}"), "Expected route registration to be gone.");
    }

    [Test]
    public void RemoveAllRouteRemovesEveryRegistrationForTemplate()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.AddRoute("/orders/{id}", "a");
        builder.AddRoute("/orders/{id}", "b");

        ShouldEqual(builder.RemoveAllRoute("/orders/{id}"), 2);
        ShouldEqual(builder.RegistrationCount, 0);
    }

    [Test]
    public void MatchRouteCanWriteToDestinationSpan()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddRoute("/orders/{id}", 1)
            .Build();

        Span<int> destination = stackalloc int[1];
        var written = index.MatchRoute("/orders/123", destination);

        ShouldEqual(written, 1);
        ShouldEqual(destination[0], 1);
    }
}
