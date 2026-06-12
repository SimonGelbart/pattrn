using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class DetailedMatchTests
{
    [Test]
    public void DetailedMatchCapturesNamedParameters()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern(
            [
                PatternSegment<string>.Literal("customers"),
                PatternSegment<string>.Parameter("customerId"),
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Parameter("orderId")
            ],
            "handler");

        var index = builder.Build();
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(["customers", "42", "orders", "99"] )];
        var captures = new PatternCapture<string>[index.GetCaptureCountUpperBound(["customers", "42", "orders", "99"] )];

        var matchCount = index.MatchDetailed(["customers", "42", "orders", "99"], matches, captures, out var captureCount);

        ShouldEqual(matchCount, 1);
        ShouldEqual(captureCount, 2);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.Parameter);
        ShouldEqual(matches[0].CaptureStart, 0);
        ShouldEqual(matches[0].CaptureCount, 2);
        ShouldEqual(captures[0], new PatternCapture<string>("customerId", "42", 1));
        ShouldEqual(captures[1], new PatternCapture<string>("orderId", "99", 3));
    }

    [Test]
    public void DetailedMatchPreservesLowLevelValueSemantics()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["orders", "new"], "literal");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "parameter");
        builder.Add(["orders", "*"], "wildcard");

        var index = builder.Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchToArray(["orders", "new"]);
        var details = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSetEqual(values, details.Select(match => match.Value));
        ShouldBeTrue(details.Any(match => match.Kind == PatternMatchKind.Exact && match.Value == "literal"), "Detailed results should include the literal match.");
        ShouldBeTrue(details.Any(match => match.Kind == PatternMatchKind.Parameter && match.Value == "parameter"), "Detailed results should include the parameter match.");
        ShouldBeTrue(details.Any(match => match.Kind == PatternMatchKind.Wildcard && match.Value == "wildcard"), "Detailed results should include the wildcard match.");
    }

    [Test]
    public void TryMatchDetailedReturnsFalseWhenMatchDestinationIsTooSmall()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler");
        var index = builder.Build();

        var matches = Array.Empty<PatternMatch<string>>();
        var captures = new PatternCapture<string>[1];

        var succeeded = index.TryMatchDetailed(["orders", "123"], matches, captures, out var matchesWritten, out var capturesWritten);

        ShouldBeFalse(succeeded, "TryMatchDetailed should fail when the match span is too small.");
        ShouldEqual(matchesWritten, 0);
        ShouldEqual(capturesWritten, 0);
    }

    [Test]
    public void TryMatchDetailedReturnsFalseWhenCaptureDestinationIsTooSmall()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler");
        var index = builder.Build();

        var matches = new PatternMatch<string>[1];
        var captures = Array.Empty<PatternCapture<string>>();

        var succeeded = index.TryMatchDetailed(["orders", "123"], matches, captures, out var matchesWritten, out var capturesWritten);

        ShouldBeFalse(succeeded, "TryMatchDetailed should fail when the capture span is too small.");
        ShouldEqual(matchesWritten, 0);
        ShouldEqual(capturesWritten, 0);
    }


    [Test]
    public void TryMatchDetailedDoesNotWriteDestinationsWhenMatchDestinationIsTooSmall()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["orders", "123"], "literal");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "parameter");
        var index = builder.Build(MatchOptions.PreserveDuplicates);

        var matches = new[] { new PatternMatch<string>("sentinel", PatternMatchKind.Exact, 100, 0, 0) };
        var captures = new[] { new PatternCapture<string>("sentinel", "value", 0) };

        var succeeded = index.TryMatchDetailed(["orders", "123"], matches, captures, out var matchesWritten, out var capturesWritten);

        ShouldBeFalse(succeeded, "TryMatchDetailed should fail when the match span is too small.");
        ShouldEqual(matchesWritten, 0);
        ShouldEqual(capturesWritten, 0);
        ShouldEqual(matches[0].Value, "sentinel");
        ShouldEqual(captures[0], new PatternCapture<string>("sentinel", "value", 0));
    }

    [Test]
    public void TryMatchDetailedDoesNotWriteDestinationsWhenCaptureDestinationIsTooSmall()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler");
        var index = builder.Build();

        var matches = new[] { new PatternMatch<string>("sentinel", PatternMatchKind.Exact, 100, 0, 0) };
        var captures = new[] { new PatternCapture<string>("sentinel", "value", 0) };

        var succeeded = index.TryMatchDetailed(["orders", "123"], matches, [], out var matchesWritten, out var capturesWritten);

        ShouldBeFalse(succeeded, "TryMatchDetailed should fail when the capture span is too small.");
        ShouldEqual(matchesWritten, 0);
        ShouldEqual(capturesWritten, 0);
        ShouldEqual(matches[0].Value, "sentinel");
        ShouldEqual(captures[0], new PatternCapture<string>("sentinel", "value", 0));
    }

    [Test]
    public void MatchDetailedConvenienceMethodReturnsDetailedResults()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler");
        var index = builder.Build();

        var matches = index.MatchDetailed(["orders", "123"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("id", "123", 1));
    }

    [Test]
    public void DetailedArrayResultCopiesCapturesPerMatch()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler");
        var index = builder.Build();

        var matches = index.MatchDetailedToArray(["orders", "123"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Captures.Count, 1);
        ShouldEqual(matches[0].Captures[0], new PatternCapture<string>("id", "123", 1));
    }

    [Test]
    public void PrefixDetailedMatchesCaptureOnlyForMatchedPrefix()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "order");
        builder.AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id"), PatternSegment<string>.Literal("items")], "items");
        var index = builder.Build(MatchOptions.Prefix);

        var matches = index.MatchDetailedToArray(["orders", "123", "items", "extra"]);

        ShouldSetEqual(matches.Select(match => match.Value), ["order", "items"]);
        var order = matches.Single(match => match.Value == "order");
        var items = matches.Single(match => match.Value == "items");
        ShouldEqual(order.Captures.Count, 1);
        ShouldEqual(order.Captures[0], new PatternCapture<string>("id", "123", 1));
        ShouldEqual(items.Captures.Count, 1);
        ShouldEqual(items.Captures[0], new PatternCapture<string>("id", "123", 1));
    }
}
