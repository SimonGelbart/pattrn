using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class StarterKitSemanticsTests
{
    [Test]
    public void MarketLevelSubscriptionRequiresExplicitPrefixModeToMatchInstrumentPath()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "market-subscriber");
        builder.Add(["NASDAQ", "MSFT"], "instrument-subscriber");

        var exactLengthIndex = builder.Build();
        var prefixIndex = builder.Build(MatchOptions.Prefix);

        ShouldSequenceEqual(exactLengthIndex.MatchToArray(["NASDAQ", "MSFT"]), ["instrument-subscriber"]);
        ShouldSetEqual(prefixIndex.MatchToArray(["NASDAQ", "MSFT"]), ["market-subscriber", "instrument-subscriber"]);
    }

    [Test]
    public void WildcardMarketSubscriptionMatchesAnyMarketForOneInstrumentSegment()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["*", "MSFT"], "any-market-msft");
        builder.Add(["NASDAQ", "MSFT"], "nasdaq-msft");

        var index = builder.Build();

        ShouldSetEqual(index.MatchToArray(["NASDAQ", "MSFT"]), ["any-market-msft", "nasdaq-msft"]);
        ShouldSequenceEqual(index.MatchToArray(["NYSE", "MSFT"]), ["any-market-msft"]);
        ShouldEqual(index.MatchToArray(["NYSE", "AAPL"]).Length, 0);
    }

    [Test]
    public void WildcardInstrumentSubscriptionMatchesAnyInstrumentWithinExactMarket()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ", "*"], "nasdaq-any-instrument");
        builder.Add(["NYSE", "*"], "nyse-any-instrument");

        var index = builder.Build();

        ShouldSequenceEqual(index.MatchToArray(["NASDAQ", "MSFT"]), ["nasdaq-any-instrument"]);
        ShouldSequenceEqual(index.MatchToArray(["NYSE", "IBM"]), ["nyse-any-instrument"]);
        ShouldEqual(index.MatchToArray(["EURONEXT", "AIR"]).Length, 0);
    }

    [Test]
    public void OverlappingStarterKitSubscriptionsReturnAllSubscribedClients()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "client-market");
        builder.Add(["NASDAQ", "*"], "client-any-nasdaq-symbol");
        builder.Add(["*", "MSFT"], "client-any-market-msft");
        builder.Add(["NASDAQ", "MSFT"], "client-exact");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSetEqual(
            index.MatchToArray(["NASDAQ", "MSFT"]),
            ["client-market", "client-any-nasdaq-symbol", "client-any-market-msft", "client-exact"]);
    }

    [Test]
    public void PatternLongerThanInputPathNeverMatchesEvenInPrefixMode()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ", "MSFT", "QUOTE"], "too-specific");
        builder.Add(["NASDAQ", "*"], "two-segments");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldEqual(index.MatchToArray(["NASDAQ"]).Length, 0);
        ShouldSequenceEqual(index.MatchToArray(["NASDAQ", "MSFT"]), ["two-segments"]);
    }

    [Test]
    public void SingleSegmentWildcardDoesNotMatchZeroSegments()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ", "*"], "requires-symbol");

        var exactLengthIndex = builder.Build();
        var prefixIndex = builder.Build(MatchOptions.Prefix);

        ShouldEqual(exactLengthIndex.MatchToArray(["NASDAQ"]).Length, 0);
        ShouldEqual(prefixIndex.MatchToArray(["NASDAQ"]).Length, 0);
    }

    [Test]
    public void SingleSegmentWildcardCanBecomeAPrefixOnlyAfterConsumingOneSegment()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ", "*"], "nasdaq-any-symbol-prefix");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSequenceEqual(index.MatchToArray(["NASDAQ", "MSFT", "QUOTE"]), ["nasdaq-any-symbol-prefix"]);
    }

    [Test]
    public void EmptyPatternMatchesOnlyEmptyInputUnlessPrefixModeIsEnabled()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add([], "root-subscription");

        var exactLengthIndex = builder.Build();
        var prefixIndex = builder.Build(MatchOptions.Prefix);

        ShouldSequenceEqual(exactLengthIndex.MatchToArray([]), ["root-subscription"]);
        ShouldEqual(exactLengthIndex.MatchToArray(["NASDAQ"]).Length, 0);
        ShouldSequenceEqual(prefixIndex.MatchToArray(["NASDAQ", "MSFT"]), ["root-subscription"]);
    }

    [Test]
    public void DeduplicationAppliesAcrossPrefixExactAndWildcardMatches()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "same-client");
        builder.Add(["NASDAQ", "*"], "same-client");
        builder.Add(["NASDAQ", "MSFT"], "same-client");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSequenceEqual(index.MatchToArray(["NASDAQ", "MSFT"]), ["same-client"]);
    }

    [Test]
    public void DeduplicationCanBeDisabledAcrossPrefixExactAndWildcardMatches()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "same-client");
        builder.Add(["NASDAQ", "*"], "same-client");
        builder.Add(["NASDAQ", "MSFT"], "same-client");

        var index = builder.Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        ShouldSequenceEqual(
            index.MatchToArray(["NASDAQ", "MSFT"]).Order(),
            ["same-client", "same-client", "same-client"]);
    }

    [Test]
    public void StarterKitStyleExampleSupportsPrefixSubscriptions()
    {
        var builder = PattrnIndex<string, string>.Builder("*");
        builder.Add(["NASDAQ"], "market-subscriber");
        builder.Add(["NASDAQ", "*"], "any-nasdaq-symbol");
        builder.Add(["NASDAQ", "MSFT"], "exact-msft");

        var index = builder.Build(MatchOptions.Prefix);

        ShouldSetEqual(
            index.MatchToArray(["NASDAQ", "MSFT"]),
            ["market-subscriber", "any-nasdaq-symbol", "exact-msft"]);
    }
}
