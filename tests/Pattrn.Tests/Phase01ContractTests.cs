using TUnit.Assertions.Enums;

namespace Pattrn.Tests;

public sealed class Phase01ContractTests
{
    [Test]
    public async Task BuilderUsesCanonicalRegistrationsAndPreservesOrderOnReplace()
    {
        var builder = PattrnIndex<string, string>.Builder();
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first", "one");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("b")], "second", "two");
        builder.Add(first);
        builder.Add(second);

        var replacement = new PattrnRegistration<string, string>(first.Id, [PatternSegment<string>.Literal("c")], "updated", "one");
        await Assert.That(builder.Replace(replacement)).IsTrue().Because("Expected replacement by registration ID.");
        await Assert.That(builder.ToRegistrations().Select(registration => registration.Id)).IsEquivalentTo([first.Id, second.Id], CollectionOrdering.Matching);
        await Assert.That(builder.ToRegistrations()[0].Value).IsEqualTo("updated");
        await Assert.That(builder.Remove(second.Id)).IsTrue().Because("Expected removal by registration ID.");
        await Assert.That(builder.RegistrationCount).IsEqualTo(1);
    }

    [Test]
    public async Task BuilderContainsAndRemovalUseConfiguredSegmentComparer()
    {
        var builder = PattrnIndex<string, string>.Builder(segmentComparer: StringComparer.OrdinalIgnoreCase);
        var id = builder.Add(
            (IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("MARKET")],
            "handler");

        await Assert.That(builder.ContainsPattern([PatternSegment<string>.Literal("market")])).IsTrue().Because("Expected comparer-aware containment.");
        await Assert.That(builder.Remove(id)).IsTrue().Because("Expected identity removal.");
        await Assert.That(builder.ContainsPattern([PatternSegment<string>.Literal("market")])).IsFalse().Because("Expected registration to be gone.");
    }

    [Test]
    public async Task ExactResultsExposeValueIdentityAndConsumedDepth()
    {
        var exact = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Literal("new")], "exact");
        var wildcard = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "wildcard");
        var index = PattrnIndex<string, string>.Compile([wildcard, exact]);

        var matches = index.MatchToArray(["orders", "new"]);

        await Assert.That(matches.Select(match => match.Value)).IsEquivalentTo(["exact", "wildcard"], CollectionOrdering.Matching);
        await Assert.That(matches[0].RegistrationId).IsEqualTo(exact.Id);
        await Assert.That(matches[0].ConsumedSegmentCount).IsEqualTo(2);
    }

    [Test]
    public async Task ExactResultAndValueFamiliesShareRankedOrdering()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll()], "literal-catch-all"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Wildcard(), PatternSegment<string>.Literal("new")], "wildcard-literal")
        ]);

        var resultValues = index.MatchToArray(["orders", "new"]).Select(match => match.Value).ToArray();
        var valueValues = index.MatchValuesToArray(["orders", "new"]);
        var detailedValues = index.MatchDetailedToArray(["orders", "new"]).Select(match => match.Value).ToArray();

        await Assert.That(resultValues).IsEquivalentTo(["wildcard-literal", "literal-catch-all"], CollectionOrdering.Matching);
        await Assert.That(valueValues).IsEquivalentTo(resultValues, CollectionOrdering.Matching);
        await Assert.That(detailedValues).IsEquivalentTo(resultValues, CollectionOrdering.Matching);
    }

    [Test]
    public async Task BestPrefixAndPrefixEnumerationAreSeparateContracts()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api")], "api"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders")], "orders")
        ]);

        await Assert.That(index.MatchPrefixToArray(["api", "orders", "new"]).Select(match => match.Value)).IsEquivalentTo(["orders"], CollectionOrdering.Matching);
        await Assert.That(index.EnumeratePrefixMatchesToArray(["api", "orders", "new"]).Select(match => match.Value)).IsEquivalentTo(["api", "orders"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ExplicitValueApiPreservesConfiguredDuplicates()
    {
        var builder = PattrnIndex<string, string>.Builder();
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "handler"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "handler")
        };
        var index = PattrnIndex<string, string>.Compile(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow },
            MatchOptions.PreserveDuplicates);
        var destination = new string[2];

        await Assert.That(index.TryMatchValues(["orders"], destination, out var written)).IsTrue().Because("Expected value matching to succeed.");
        await Assert.That(destination[..written]).IsEquivalentTo(["handler", "handler"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task DetailedResultsOwnNamedCaptures()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler")
        ]);

        var match = index.MatchDetailedToArray(["orders", "42"]).Single();

        await Assert.That(match.Value).IsEqualTo("handler");
        await Assert.That(match.ConsumedSegmentCount).IsEqualTo(2);
        await Assert.That(match.TryGetCapture("id", out var capture)).IsTrue().Because("Expected named capture.");
        await Assert.That(capture.Value).IsEqualTo("42");
        await Assert.That(match.GetCapture("id").Name).IsEqualTo("id");
    }

    [Test]
    public async Task CallerBuffersDoNotReceivePartialResults()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "second")
        ],
        new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });
        var values = new[] { "sentinel" };
        var results = new[] { default(PatternMatch<string>) };

        await Assert.That(index.TryMatchValues(["a"], values, out var valuesWritten)).IsFalse().Because("Expected an insufficient value buffer.");
        await Assert.That(valuesWritten).IsEqualTo(0);
        await Assert.That(values[0]).IsEqualTo("sentinel");
        await Assert.That(index.TryMatch(["a"], results, out var resultsWritten)).IsFalse().Because("Expected an insufficient result buffer.");
        await Assert.That(resultsWritten).IsEqualTo(0);
        await Assert.That(results[0]).IsEqualTo(default(PatternMatch<string>));
    }
}
