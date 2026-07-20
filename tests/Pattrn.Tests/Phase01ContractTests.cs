using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class Phase01ContractTests
{
    [Test]
    public void BuilderUsesCanonicalRegistrationsAndPreservesOrderOnReplace()
    {
        var builder = PattrnIndex<string, string>.Builder();
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first", "one");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("b")], "second", "two");
        builder.Add(first);
        builder.Add(second);

        var replacement = new PattrnRegistration<string, string>(first.Id, [PatternSegment<string>.Literal("c")], "updated", "one");
        ShouldBeTrue(builder.Replace(replacement), "Expected replacement by registration ID.");
        ShouldSequenceEqual(builder.ToRegistrations().Select(registration => registration.Id), [first.Id, second.Id]);
        ShouldEqual(builder.ToRegistrations()[0].Value, "updated");
        ShouldBeTrue(builder.Remove(second.Id), "Expected removal by registration ID.");
        ShouldEqual(builder.RegistrationCount, 1);
    }

    [Test]
    public void BuilderContainsAndRemovalUseConfiguredSegmentComparer()
    {
        var builder = PattrnIndex<string, string>.Builder(segmentComparer: StringComparer.OrdinalIgnoreCase);
        var id = builder.Add(
            (IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("MARKET")],
            "handler");

        ShouldBeTrue(builder.ContainsPattern([PatternSegment<string>.Literal("market")]), "Expected comparer-aware containment.");
        ShouldBeTrue(builder.Remove(id), "Expected identity removal.");
        ShouldBeFalse(builder.ContainsPattern([PatternSegment<string>.Literal("market")]), "Expected registration to be gone.");
    }

    [Test]
    public void ExactResultsExposeValueIdentityAndConsumedDepth()
    {
        var exact = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Literal("new")], "exact");
        var wildcard = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "wildcard");
        var index = PattrnIndex<string, string>.Compile([wildcard, exact]);

        var matches = index.MatchToArray(["orders", "new"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["exact", "wildcard"]);
        ShouldEqual(matches[0].RegistrationId, exact.Id);
        ShouldEqual(matches[0].ConsumedSegmentCount, 2);
    }

    [Test]
    public void ExactResultAndValueFamiliesShareRankedOrdering()
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

        ShouldSequenceEqual(resultValues, ["wildcard-literal", "literal-catch-all"]);
        ShouldSequenceEqual(valueValues, resultValues);
        ShouldSequenceEqual(detailedValues, resultValues);
    }

    [Test]
    public void BestPrefixAndPrefixEnumerationAreSeparateContracts()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api")], "api"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders")], "orders")
        ]);

        ShouldSequenceEqual(index.MatchPrefixToArray(["api", "orders", "new"]).Select(match => match.Value), ["orders"]);
        ShouldSequenceEqual(index.EnumeratePrefixMatchesToArray(["api", "orders", "new"]).Select(match => match.Value), ["api", "orders"]);
    }

    [Test]
    public void ExplicitValueApiPreservesConfiguredDuplicates()
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

        ShouldBeTrue(index.TryMatchValues(["orders"], destination, out var written), "Expected value matching to succeed.");
        ShouldSequenceEqual(destination[..written], ["handler", "handler"]);
    }

    [Test]
    public void DetailedResultsOwnNamedCaptures()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler")
        ]);

        var match = index.MatchDetailedToArray(["orders", "42"]).Single();

        ShouldEqual(match.Value, "handler");
        ShouldEqual(match.ConsumedSegmentCount, 2);
        ShouldBeTrue(match.TryGetCapture("id", out var capture), "Expected named capture.");
        ShouldEqual(capture.Value, "42");
        ShouldEqual(match.GetCapture("id").Name, "id");
    }

    [Test]
    public void CallerBuffersDoNotReceivePartialResults()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "second")
        ],
        new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });
        var values = new[] { "sentinel" };
        var results = new[] { default(PatternMatch<string>) };

        ShouldBeFalse(index.TryMatchValues(["a"], values, out var valuesWritten), "Expected an insufficient value buffer.");
        ShouldEqual(valuesWritten, 0);
        ShouldEqual(values[0], "sentinel");
        ShouldBeFalse(index.TryMatch(["a"], results, out var resultsWritten), "Expected an insufficient result buffer.");
        ShouldEqual(resultsWritten, 0);
        ShouldEqual(results[0], default(PatternMatch<string>));
    }
}
