using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class BuilderPolishTests
{
    [Test]
    public void AddReturnsSameBuilderForFluentRegistration()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        var returned = builder
            .Add(["market", "NASDAQ", "MSFT"], "exact")
            .Add(["market", "NASDAQ", "*"], "wildcard");

        ShouldBeTrue(ReferenceEquals(builder, returned), "Fluent Add should return the same mutable builder instance.");
        ShouldSetEqual(builder.Build().MatchToArray(["market", "NASDAQ", "MSFT"]), ["exact", "wildcard"]);
    }

    [Test]
    public void AddRangeRegistersMultiplePatternsAndReturnsSameBuilder()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        var registrations = new (IEnumerable<string> Pattern, string Value)[]
        {
            (new[] { "market", "NASDAQ", "MSFT" }, "exact"),
            (new[] { "market", "NASDAQ", "*" }, "wildcard"),
        };

        var returned = builder.AddRange(registrations);

        ShouldBeTrue(ReferenceEquals(builder, returned), "Fluent AddRange should return the same mutable builder instance.");
        ShouldEqual(builder.PatternCount, 2);
        ShouldEqual(builder.RegistrationCount, 2);
        ShouldSetEqual(builder.Build().MatchToArray(["market", "NASDAQ", "MSFT"]), ["exact", "wildcard"]);
    }

    [Test]
    public void ContainsReportsOnlyRegisteredPatterns()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["market", "NASDAQ", "*"], "wildcard");

        ShouldBeTrue(builder.Contains(["market", "NASDAQ", "*"]), "Expected the wildcard pattern to exist.");
        ShouldBeFalse(builder.Contains(["market", "NASDAQ", "MSFT"]), "A matching path is not the same as a registered pattern.");
        ShouldBeFalse(builder.Contains(["market", "NASDAQ"]), "A prefix is not the same as a registered pattern.");
    }

    [Test]
    public void ContainsSupportsMemoryAndEnumerableConvenienceOverloads()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        string[] pattern = ["market", "NASDAQ", "MSFT"];

        builder.Add(pattern.AsMemory(), "client");

        ShouldBeTrue(builder.Contains(pattern.AsMemory()), "Expected memory overload to find the pattern.");
        ShouldBeTrue(builder.Contains((IEnumerable<string>)pattern), "Expected enumerable overload to find the pattern.");
    }

    [Test]
    public void RemoveAllRemovesEveryRegistrationForPattern()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["market", "NASDAQ"], "client-1")
            .Add(["market", "NASDAQ"], "client-2")
            .Add(["market", "NYSE"], "client-3");

        var removed = builder.RemoveAll(["market", "NASDAQ"]);

        ShouldEqual(removed, 2);
        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
        ShouldBeFalse(builder.Contains(["market", "NASDAQ"]), "Expected all values for the NASDAQ pattern to be removed.");
        ShouldSequenceEqual(builder.Build().MatchToArray(["market", "NYSE"]), ["client-3"]);
    }

    [Test]
    public void RemoveAllSupportsMemoryAndEnumerableConvenienceOverloads()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        string[] firstPattern = ["market", "NASDAQ"];
        string[] secondPattern = ["market", "NYSE"];

        builder
            .Add(firstPattern, "client-1")
            .Add(secondPattern, "client-2");

        ShouldEqual(builder.RemoveAll(firstPattern.AsMemory()), 1);
        ShouldEqual(builder.RemoveAll((IEnumerable<string>)secondPattern), 1);
        ShouldEqual(builder.PatternCount, 0);
        ShouldEqual(builder.RegistrationCount, 0);
    }

    [Test]
    public void RemovePrunesEmptyBranchesWithoutRemovingSiblings()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["market", "NASDAQ", "MSFT"], "remove-me")
            .Add(["market", "NYSE", "IBM"], "keep-me");

        ShouldBeTrue(builder.Remove(["market", "NASDAQ", "MSFT"], "remove-me"), "Expected registration to be removed.");

        ShouldBeFalse(builder.Contains(["market", "NASDAQ", "MSFT"]), "Expected removed branch to be absent.");
        ShouldBeTrue(builder.Contains(["market", "NYSE", "IBM"]), "Expected sibling branch to remain.");
        ShouldSequenceEqual(builder.Build().MatchToArray(["market", "NYSE", "IBM"]), ["keep-me"]);
    }

    [Test]
    public void RemoveAllPrunesWildcardBranchesWithoutRemovingExactSiblings()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["market", "*", "MSFT"], "remove-me")
            .Add(["market", "NASDAQ", "MSFT"], "keep-me");

        ShouldEqual(builder.RemoveAll(["market", "*", "MSFT"]), 1);

        ShouldBeFalse(builder.Contains(["market", "*", "MSFT"]), "Expected wildcard branch to be removed.");
        ShouldBeTrue(builder.Contains(["market", "NASDAQ", "MSFT"]), "Expected exact sibling branch to remain.");
        ShouldSequenceEqual(builder.Build().MatchToArray(["market", "NASDAQ", "MSFT"]), ["keep-me"]);
    }

    [Test]
    public void ClearRemovesEverythingAndReturnsSameBuilder()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["market", "NASDAQ"], "client-1")
            .Add(["market", "NYSE"], "client-2");

        var returned = builder.Clear();

        ShouldBeTrue(ReferenceEquals(builder, returned), "Fluent Clear should return the same mutable builder instance.");
        ShouldEqual(builder.PatternCount, 0);
        ShouldEqual(builder.RegistrationCount, 0);
        ShouldEqual(builder.Build().MatchToArray(["market", "NASDAQ"]).Length, 0);
    }
}

public sealed class TokenlessBuilderTests
{
    [Test]
    public void StaticBuilderFactoryCreatesTokenlessBuilder()
    {
        var builder = PattrnIndex<string, string>.Builder();

        ShouldBeFalse(builder.UsesWildcardSegmentToken, "The default builder factory should not reserve a wildcard token.");

        builder.AddPattern(
            [
                PatternSegment<string>.Literal("market"),
                PatternSegment<string>.Literal("NASDAQ"),
                PatternSegment<string>.Parameter("symbol")
            ],
            "client-a");

        var detailed = builder.Build().MatchDetailedToArray(["market", "NASDAQ", "MSFT"]);

        ShouldEqual(detailed.Length, 1);
        ShouldEqual(detailed[0].Value, "client-a");
        ShouldEqual(detailed[0].Captures.Count, 1);
        ShouldEqual(detailed[0].Captures[0].Name, "symbol");
        ShouldEqual(detailed[0].Captures[0].Value, "MSFT");
    }

    [Test]
    public void TokenlessAddRegistersLiteralOnlySegments()
    {
        var builder = PattrnIndex<string, string>.Builder();

        builder.Add(["market", "*"], "literal-star");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "*"]), ["literal-star"]);
        ShouldEqual(index.MatchToArray(["market", "MSFT"]).Length, 0);
    }

    [Test]
    public void TokenlessBuilderUsesExplicitPatternSegmentsForWildcardBehavior()
    {
        var builder = PattrnIndexBuilder<string, string>.Create();

        builder.AddPattern(
            [PatternSegment<string>.Literal("market"), PatternSegment<string>.Wildcard()],
            "wildcard");
        builder.AddPattern(
            [PatternSegment<string>.Literal("market"), PatternSegment<string>.Literal("*")],
            "literal-star");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["market", "MSFT"]), ["wildcard"]);
        ShouldSetEqual(index.MatchToArray(["market", "*"]), ["literal-star", "wildcard"]);
    }

    [Test]
    public void TokenizedBuilderStillSupportsReservedWildcardSegmentConvenience()
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        ShouldBeTrue(builder.UsesWildcardSegmentToken, "Tokenized factory should keep reserved wildcard-token behavior.");

        builder.Add(["market", "*"], "wildcard");

        ShouldSequenceEqual(builder.Build().MatchToArray(["market", "MSFT"]), ["wildcard"]);
    }
}
