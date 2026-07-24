using TUnit.Assertions.Enums;

namespace Pattrn.Tests.Contracts;

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
    public async Task DeduplicatedValueBufferCapacityIsMeasuredAfterDeduplication()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "handler"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Wildcard()], "handler")
        ]);
        var destination = new[] { "sentinel" };

        await Assert.That(index.TryMatchValues(["orders"], destination, out var written)).IsTrue().Because("The duplicate value should fit after deduplication.");
        await Assert.That(written).IsEqualTo(1);
        await Assert.That(destination[0]).IsEqualTo("handler");
    }

    [Test]
    public async Task ZeroLengthCatchAllDoesNotOutrankAnExactRegistration()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "catch-all"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("files")], "exact")
        ]);

        var exactValues = index.MatchToArray(["files"]).Select(match => match.Value).ToArray();
        var valueOnlyValues = index.MatchValuesToArray(["files"]);
        var detailedValues = index.MatchDetailedToArray(["files"]).Select(match => match.Value).ToArray();
        var bestPrefixValues = index.MatchPrefixToArray(["files"]).Select(match => match.Value).ToArray();
        var enumeratedPrefixValues = index.EnumeratePrefixMatchesToArray(["files"]).Select(match => match.Value).ToArray();

        await Assert.That(exactValues).IsEquivalentTo(["exact", "catch-all"], CollectionOrdering.Matching);
        await Assert.That(valueOnlyValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(detailedValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(bestPrefixValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(enumeratedPrefixValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);

        var rootIndex = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.CatchAll("path")], "root-catch-all"),
            PattrnRegistration<string, string>.Create([], "root-exact")
        ]);

        await Assert.That(rootIndex.MatchValuesToArray([])).IsEquivalentTo(["root-exact", "root-catch-all"], CollectionOrdering.Matching);
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

    [Test]
    public async Task WarmedCallerBufferAndUpperBoundPathsDoNotAllocate()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api")], "api"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders")], "orders"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("id")], "parameter"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("api"), PatternSegment<string>.CatchAll("rest")], "catch-all")
        ]);
        var path = new[] { "api", "orders" };
        var matches = new PatternMatch<string>[index.GetMatchCountUpperBound(path)];
        var prefixMatches = new PatternMatch<string>[index.GetPrefixMatchCountUpperBound(path)];
        var enumeratedMatches = new PatternMatch<string>[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var values = new string[index.GetMatchCountUpperBound(path)];
        var prefixValues = new string[index.GetPrefixMatchCountUpperBound(path)];
        var enumeratedValues = new string[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var detailedMatches = new PatternMatchDetailedSlice<string>[index.GetMatchCountUpperBound(path)];
        var detailedCaptures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];
        var prefixDetailedMatches = new PatternMatchDetailedSlice<string>[index.GetPrefixMatchCountUpperBound(path)];
        var prefixDetailedCaptures = new PatternCaptureSlice<string>[index.GetPrefixCaptureCountUpperBound(path)];
        var enumeratedDetailedMatches = new PatternMatchDetailedSlice<string>[index.GetEnumeratePrefixMatchCountUpperBound(path)];
        var enumeratedDetailedCaptures = new PatternCaptureSlice<string>[index.GetEnumeratePrefixCaptureCountUpperBound(path)];

        _ = index.TryMatch(path, matches, out _);
        _ = index.TryMatchValues(path, values, out _);
        _ = index.TryMatchPrefix(path, prefixMatches, out _);
        _ = index.TryMatchPrefixValues(path, prefixValues, out _);
        _ = index.TryEnumeratePrefixMatches(path, enumeratedMatches, out _);
        _ = index.TryEnumeratePrefixValues(path, enumeratedValues, out _);
        _ = index.TryMatchDetailed(path, detailedMatches, detailedCaptures, out _, out _);
        _ = index.TryMatchPrefixDetailed(path, prefixDetailedMatches, prefixDetailedCaptures, out _, out _);
        _ = index.TryEnumeratePrefixDetailed(path, enumeratedDetailedMatches, enumeratedDetailedCaptures, out _, out _);
        _ = index.GetMatchCountUpperBound(path);
        _ = index.GetPrefixMatchCountUpperBound(path);
        _ = index.GetEnumeratePrefixMatchCountUpperBound(path);
        _ = index.GetCaptureCountUpperBound(path);
        _ = index.GetPrefixCaptureCountUpperBound(path);
        _ = index.GetEnumeratePrefixCaptureCountUpperBound(path);

        await Assert.That(AllocatedBytes(() => _ = index.TryMatch(path, matches, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchValues(path, values, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchPrefix(path, prefixMatches, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchPrefixValues(path, prefixValues, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryEnumeratePrefixMatches(path, enumeratedMatches, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryEnumeratePrefixValues(path, enumeratedValues, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchDetailed(path, detailedMatches, detailedCaptures, out _, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchPrefixDetailed(path, prefixDetailedMatches, prefixDetailedCaptures, out _, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryEnumeratePrefixDetailed(path, enumeratedDetailedMatches, enumeratedDetailedCaptures, out _, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetMatchCountUpperBound(path))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetPrefixMatchCountUpperBound(path))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetEnumeratePrefixMatchCountUpperBound(path))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetCaptureCountUpperBound(path))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetPrefixCaptureCountUpperBound(path))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.GetEnumeratePrefixCaptureCountUpperBound(path))).IsEqualTo(0);
    }

    private static long AllocatedBytes(Action operation)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        operation();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}

