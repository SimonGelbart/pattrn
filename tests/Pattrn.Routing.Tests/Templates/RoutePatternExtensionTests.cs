using TUnit.Assertions.Enums;

namespace Pattrn.Routing.Tests.Templates;

public sealed class RoutePatternExtensionTests
{
    [Test]
    public async Task AddRouteRegistersRouteTemplateWithoutRouteSyntaxInCore()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddRoute("/orders/{id}", "order-handler");

        var index = builder.Build();

        await Assert.That(index.MatchRouteToArray("/orders/123")).IsEquivalentTo(["order-handler"], CollectionOrdering.Matching);
        await Assert.That(index.MatchRouteToArray("/customers/123")).IsEquivalentTo(Array.Empty<string>(), CollectionOrdering.Matching);
    }

    [Test]
    public async Task MatchRouteDetailedReturnsNamedParameterCaptures()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/customers/{customerId}/orders/{orderId}", "handler")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/customers/42/orders/99");

        await Assert.That(matches.Length).IsEqualTo(1);
        await Assert.That(matches[0].Value).IsEqualTo("handler");
        await Assert.That(matches[0].Captures.Length).IsEqualTo(2);
        await Assert.That(matches[0].Captures[0]).IsEqualTo(new PatternCapture<string>("customerId", "42", 1));
        await Assert.That(matches[0].Captures[1]).IsEqualTo(new PatternCapture<string>("orderId", "99", 3));
    }

    [Test]
    public async Task MatchRouteDetailedReturnsCatchAllCapturesAsSegments()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddRoute("/files/{*path}", "file-handler")
            .Build();

        var matches = index.MatchRouteDetailedToArray("/files/a/b/c.txt");

        await Assert.That(matches.Length).IsEqualTo(1);
        await Assert.That(matches[0].Value).IsEqualTo("file-handler");
        await Assert.That(matches[0].Captures.Length).IsEqualTo(1);
        await Assert.That(matches[0].Captures[0]).IsEqualTo(new PatternCapture<string>("path", ["a", "b", "c.txt"], 1));
    }

    [Test]
    public async Task ContainsAndRemoveRouteUseRouteTemplateSemantics()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.AddRoute("/orders/{id}", "handler");

        await Assert.That(builder.ContainsRoute("/orders/{id}")).IsTrue().Because("Expected route registration to exist.");
        await Assert.That(builder.RemoveRoute("/orders/{id}", "handler")).IsTrue().Because("Expected route registration to be removed.");
        await Assert.That(builder.ContainsRoute("/orders/{id}")).IsFalse().Because("Expected route registration to be gone.");
    }

    [Test]
    public async Task RemoveAllRouteRemovesEveryRegistrationForTemplate()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        builder.AddRoute("/orders/{id}", "a");
        builder.AddRoute("/orders/{id}", "b");

        await Assert.That(builder.RemoveAllRoute("/orders/{id}")).IsEqualTo(2);
        await Assert.That(builder.RegistrationCount).IsEqualTo(0);
    }

    [Test]
    public async Task MatchRouteCanWriteToDestinationSpan()
    {
        var index = PattrnIndex<string, int>
            .Builder("*")
            .AddRoute("/orders/{id}", 1)
            .Build();

        Span<int> destination = stackalloc int[1];
        var written = index.MatchRoute("/orders/123", destination);
        var matchedValue = destination[0];

        await Assert.That(written).IsEqualTo(1);
        await Assert.That(matchedValue).IsEqualTo(1);
    }
}

