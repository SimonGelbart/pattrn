using TUnit.Assertions.Enums;

namespace Pattrn.Tests.Matching.Ordering;

public sealed class RankingContractTests
{
    [Test]
    public async Task ExactOrderingUsesSpecificityThenRegistrationOrderAcrossProjections()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("second")], "parameter-second"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.CatchAll()], "catch-all"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Wildcard()], "wildcard"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders")], "literal"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("first")], "parameter-first")
        };
        var index = PattrnIndex<string, string>.Compile(registrations);
        var expected = new[] { "literal", "parameter-second", "parameter-first", "wildcard", "catch-all" };

        await AssertExactProjectionOrder(index, ["api", "orders"], expected);
        await Assert.That(index.MatchToArray(["api", "orders"]).Select(match => match.RegistrationId))
            .IsEquivalentTo([registrations[3].Id, registrations[0].Id, registrations[4].Id, registrations[2].Id, registrations[1].Id], CollectionOrdering.Matching);
    }

    [Test]
    public async Task PrefixOrderingIncludesRootAndAllLevelsAcrossProjections()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([], "root"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api")], "api"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders")], "orders"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.Literal("new")], "new")
        ]);
        var path = new[] { "api", "orders", "new", "tail" };

        await AssertBestPrefixProjectionOrder(index, path, ["new"]);
        await AssertEnumeratedPrefixProjectionOrder(index, path, ["root", "api", "orders", "new"]);
    }

    [Test]
    public async Task DuplicatePreservingOrderingRetainsTrueTieRegistrationOrder()
    {
        var first = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("first")], "same");
        var second = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("second")], "same");
        var index = PattrnIndex<string, string>.Compile([first, second], MatchOptions.PreserveDuplicates);

        var matches = index.MatchToArray(["api", "orders"]);
        var detailed = index.MatchDetailedToArray(["api", "orders"]);

        await Assert.That(matches.Select(match => match.Value)).IsEquivalentTo(["same", "same"], CollectionOrdering.Matching);
        await Assert.That(matches.Select(match => match.RegistrationId)).IsEquivalentTo([first.Id, second.Id], CollectionOrdering.Matching);
        await Assert.That(detailed.Select(match => match.RegistrationId)).IsEquivalentTo([first.Id, second.Id], CollectionOrdering.Matching);
        await Assert.That(index.MatchValuesToArray(["api", "orders"]))
            .IsEquivalentTo(["same", "same"], CollectionOrdering.Matching);
    }

    private static async Task AssertExactProjectionOrder(
        PattrnIndex<string, string> index,
        string[] path,
        string[] expected)
    {
        await Assert.That(index.MatchToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
        await Assert.That(index.MatchValuesToArray(path))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var matchSucceeded = index.TryMatch(path, matches, out var matchesWritten);
        await Assert.That(matchSucceeded).IsTrue();
        await Assert.That(matches[..matchesWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var values = new string[index.GetMatchCountUpperBound(path)];
        var valuesSucceeded = index.TryMatchValues(path, values, out var valuesWritten);
        await Assert.That(valuesSucceeded).IsTrue();
        await Assert.That(values[..valuesWritten]).IsEquivalentTo(expected, CollectionOrdering.Matching);

        await Assert.That(index.MatchDetailedToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var detailedMatches = new PatternMatchDetailedSlice<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];
        var detailedSucceeded = index.TryMatchDetailed(path, detailedMatches, captures, out var detailedWritten, out _);
        await Assert.That(detailedSucceeded).IsTrue();
        await Assert.That(detailedMatches[..detailedWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
    }

    private static async Task AssertBestPrefixProjectionOrder(
        PattrnIndex<string, string> index,
        string[] path,
        string[] expected)
    {
        await Assert.That(index.MatchPrefixToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
        await Assert.That(index.MatchPrefixValuesToArray(path))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var matches = new PatternMatch<string>[index.GetPrefixMatchCountUpperBound(path)];
        var matchSucceeded = index.TryMatchPrefix(path, matches, out var matchesWritten);
        await Assert.That(matchSucceeded).IsTrue();
        await Assert.That(matches[..matchesWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var values = new string[index.GetPrefixMatchCountUpperBound(path)];
        var valuesSucceeded = index.TryMatchPrefixValues(path, values, out var valuesWritten);
        await Assert.That(valuesSucceeded).IsTrue();
        await Assert.That(values[..valuesWritten]).IsEquivalentTo(expected, CollectionOrdering.Matching);

        await Assert.That(index.MatchPrefixDetailedToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var detailedMatches = new PatternMatchDetailedSlice<string>[index.GetPrefixMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetPrefixCaptureCountUpperBound(path)];
        var detailedSucceeded = index.TryMatchPrefixDetailed(path, detailedMatches, captures, out var detailedWritten, out _);
        await Assert.That(detailedSucceeded).IsTrue();
        await Assert.That(detailedMatches[..detailedWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
    }

    private static async Task AssertEnumeratedPrefixProjectionOrder(
        PattrnIndex<string, string> index,
        string[] path,
        string[] expected)
    {
        await Assert.That(index.EnumeratePrefixMatchesToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
        await Assert.That(index.EnumeratePrefixValuesToArray(path))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var matches = new PatternMatch<string>[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var matchSucceeded = index.TryEnumeratePrefixMatches(path, matches, out var matchesWritten);
        await Assert.That(matchSucceeded).IsTrue();
        await Assert.That(matches[..matchesWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var values = new string[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var valuesSucceeded = index.TryEnumeratePrefixValues(path, values, out var valuesWritten);
        await Assert.That(valuesSucceeded).IsTrue();
        await Assert.That(values[..valuesWritten]).IsEquivalentTo(expected, CollectionOrdering.Matching);

        await Assert.That(index.EnumeratePrefixDetailedToArray(path).Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);

        var detailedMatches = new PatternMatchDetailedSlice<string>[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetEnumeratePrefixCaptureCountUpperBound(path)];
        var detailedSucceeded = index.TryEnumeratePrefixDetailed(path, detailedMatches, captures, out var detailedWritten, out _);
        await Assert.That(detailedSucceeded).IsTrue();
        await Assert.That(detailedMatches[..detailedWritten].Select(match => match.Value))
            .IsEquivalentTo(expected, CollectionOrdering.Matching);
    }
}

