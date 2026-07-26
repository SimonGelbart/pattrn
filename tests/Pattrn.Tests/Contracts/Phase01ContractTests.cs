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
        var exactBuffer = new PatternMatch<string>[index.GetMatchCountUpperBound(["files"])];
        var exactSucceeded = index.TryMatch(["files"], exactBuffer, out var exactWritten);
        var prefixBuffer = new PatternMatch<string>[index.GetPrefixMatchCountUpperBound(["files"])];
        var prefixSucceeded = index.TryMatchPrefix(["files"], prefixBuffer, out var prefixWritten);
        var enumeratedBuffer = new PatternMatch<string>[index.GetEnumeratePrefixMatchCountUpperBound(["files"])];
        var enumeratedSucceeded = index.TryEnumeratePrefixMatches(["files"], enumeratedBuffer, out var enumeratedWritten);
        var detailedBuffer = new PatternMatchDetailedSlice<string>[index.GetMatchCountUpperBound(["files"])];
        var captureBuffer = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(["files"])];
        var detailedSucceeded = index.TryMatchDetailed(
            ["files"],
            detailedBuffer,
            captureBuffer,
            out var detailedWritten,
            out _);

        await Assert.That(exactValues).IsEquivalentTo(["exact", "catch-all"], CollectionOrdering.Matching);
        await Assert.That(valueOnlyValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(detailedValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(bestPrefixValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(enumeratedPrefixValues).IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(exactSucceeded).IsTrue();
        await Assert.That(exactBuffer[..exactWritten].Select(match => match.Value))
            .IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(prefixSucceeded).IsTrue();
        await Assert.That(prefixBuffer[..prefixWritten].Select(match => match.Value))
            .IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(enumeratedSucceeded).IsTrue();
        await Assert.That(enumeratedBuffer[..enumeratedWritten].Select(match => match.Value))
            .IsEquivalentTo(exactValues, CollectionOrdering.Matching);
        await Assert.That(detailedSucceeded).IsTrue();
        await Assert.That(detailedBuffer[..detailedWritten].Select(match => match.Value))
            .IsEquivalentTo(exactValues, CollectionOrdering.Matching);

        var rootIndex = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create([PatternSegment<string>.CatchAll("path")], "root-catch-all"),
            PattrnRegistration<string, string>.Create([], "root-exact")
        ]);

        await Assert.That(rootIndex.MatchValuesToArray([])).IsEquivalentTo(["root-exact", "root-catch-all"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task ZeroLengthNamedCatchAllDetailedCapturesPreserveTheEmptyRange()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")],
                "catch-all"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("files")],
                "exact")
        ]);
        var path = new[] { "files" };

        var owning = index.MatchDetailedToArray(path);
        await Assert.That(owning.Select(match => match.Value))
            .IsEquivalentTo(["exact", "catch-all"], CollectionOrdering.Matching);
        var owningCapture = owning[1].GetCapture("path");
        await Assert.That(owningCapture.Name).IsEqualTo("path");
        await Assert.That(owningCapture.StartSegmentIndex).IsEqualTo(1);
        await Assert.That(owningCapture.Values.Length).IsEqualTo(0);

        var matches = new PatternMatchDetailedSlice<string>[index.GetMatchCountUpperBound(path)];
        var captures = new PatternCaptureSlice<string>[index.GetCaptureCountUpperBound(path)];
        var succeeded = index.TryMatchDetailed(path, matches, captures, out var matchesWritten, out var capturesWritten);

        await Assert.That(succeeded).IsTrue();
        await Assert.That(matches[..matchesWritten].Select(match => match.Value))
            .IsEquivalentTo(["exact", "catch-all"], CollectionOrdering.Matching);
        await Assert.That(capturesWritten).IsEqualTo(1);
        var bufferedCapture = captures[matches[1].CaptureStart];
        await Assert.That(bufferedCapture.Name).IsEqualTo("path");
        await Assert.That(bufferedCapture.StartSegmentIndex).IsEqualTo(1);
        await Assert.That(bufferedCapture.SegmentCount).IsEqualTo(0);
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
    public async Task PrefixAndDetailedInsufficientBuffersRemainUnmodifiedAndUpperBoundsAreExact()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api")],
                "api"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Parameter("id")],
                "parameter"),
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Wildcard()],
                "wildcard")
        ]);
        var exactPath = new[] { "api", "orders" };
        var prefixPath = new[] { "api", "orders", "tail" };

        await Assert.That(index.GetMatchCountUpperBound(exactPath)).IsEqualTo(2);
        await Assert.That(index.GetCaptureCountUpperBound(exactPath)).IsEqualTo(1);
        await Assert.That(index.GetPrefixMatchCountUpperBound(prefixPath)).IsEqualTo(2);
        await Assert.That(index.GetPrefixCaptureCountUpperBound(prefixPath)).IsEqualTo(1);
        await Assert.That(index.GetEnumeratePrefixMatchCountUpperBound(prefixPath)).IsEqualTo(3);
        await Assert.That(index.GetEnumeratePrefixCaptureCountUpperBound(prefixPath)).IsEqualTo(1);
        await Assert.That(index.MatchDetailedToArray(exactPath).Sum(match => match.Captures.Length)).IsEqualTo(1);
        await Assert.That(index.MatchPrefixDetailedToArray(prefixPath).Sum(match => match.Captures.Length)).IsEqualTo(1);
        await Assert.That(index.EnumeratePrefixDetailedToArray(prefixPath).Sum(match => match.Captures.Length)).IsEqualTo(1);

        var prefixValues = new[] { "sentinel" };
        var prefixMatches = new[] { default(PatternMatch<string>) };
        await Assert.That(index.TryMatchPrefixValues(prefixPath, prefixValues, out var prefixValuesWritten)).IsFalse();
        await Assert.That(prefixValuesWritten).IsEqualTo(0);
        await Assert.That(prefixValues[0]).IsEqualTo("sentinel");
        await Assert.That(index.TryMatchPrefix(prefixPath, prefixMatches, out var prefixMatchesWritten)).IsFalse();
        await Assert.That(prefixMatchesWritten).IsEqualTo(0);
        await Assert.That(prefixMatches[0]).IsEqualTo(default(PatternMatch<string>));

        var enumeratedValues = new[] { "sentinel-1", "sentinel-2" };
        var enumeratedMatches = new[] { default(PatternMatch<string>), default(PatternMatch<string>) };
        await Assert.That(index.TryEnumeratePrefixValues(prefixPath, enumeratedValues, out var enumeratedValuesWritten)).IsFalse();
        await Assert.That(enumeratedValuesWritten).IsEqualTo(0);
        await Assert.That(enumeratedValues).IsEquivalentTo(["sentinel-1", "sentinel-2"], CollectionOrdering.Matching);
        await Assert.That(index.TryEnumeratePrefixMatches(prefixPath, enumeratedMatches, out var enumeratedMatchesWritten)).IsFalse();
        await Assert.That(enumeratedMatchesWritten).IsEqualTo(0);
        await Assert.That(enumeratedMatches)
            .IsEquivalentTo([default(PatternMatch<string>), default(PatternMatch<string>)], CollectionOrdering.Matching);

        var exactDetailedMatches = new[] { default(PatternMatchDetailedSlice<string>) };
        var exactDetailedCaptures = new[] { default(PatternCaptureSlice<string>) };
        await Assert.That(index.TryMatchDetailed(
                exactPath,
                exactDetailedMatches,
                exactDetailedCaptures,
                out var exactMatchesWritten,
                out var exactCapturesWritten))
            .IsFalse();
        await Assert.That(exactMatchesWritten).IsEqualTo(0);
        await Assert.That(exactCapturesWritten).IsEqualTo(0);
        await Assert.That(exactDetailedMatches[0]).IsEqualTo(default(PatternMatchDetailedSlice<string>));
        await Assert.That(exactDetailedCaptures[0]).IsEqualTo(default(PatternCaptureSlice<string>));

        var prefixDetailedMatches = new[] { default(PatternMatchDetailedSlice<string>) };
        var prefixDetailedCaptures = Array.Empty<PatternCaptureSlice<string>>();
        await Assert.That(index.TryMatchPrefixDetailed(
                prefixPath,
                prefixDetailedMatches,
                prefixDetailedCaptures,
                out var prefixDetailedMatchesWritten,
                out var prefixDetailedCapturesWritten))
            .IsFalse();
        await Assert.That(prefixDetailedMatchesWritten).IsEqualTo(0);
        await Assert.That(prefixDetailedCapturesWritten).IsEqualTo(0);
        await Assert.That(prefixDetailedMatches[0]).IsEqualTo(default(PatternMatchDetailedSlice<string>));

        var enumeratedDetailedMatches = new[]
        {
            default(PatternMatchDetailedSlice<string>),
            default(PatternMatchDetailedSlice<string>)
        };
        var enumeratedDetailedCaptures = Array.Empty<PatternCaptureSlice<string>>();
        await Assert.That(index.TryEnumeratePrefixDetailed(
                prefixPath,
                enumeratedDetailedMatches,
                enumeratedDetailedCaptures,
                out var enumeratedDetailedMatchesWritten,
                out var enumeratedDetailedCapturesWritten))
            .IsFalse();
        await Assert.That(enumeratedDetailedMatchesWritten).IsEqualTo(0);
        await Assert.That(enumeratedDetailedCapturesWritten).IsEqualTo(0);
        await Assert.That(enumeratedDetailedMatches)
            .IsEquivalentTo(
                [default(PatternMatchDetailedSlice<string>), default(PatternMatchDetailedSlice<string>)],
                CollectionOrdering.Matching);
    }

    [Test]
    public async Task NoCandidateDetailedCallsLeaveBuffersUntouchedAndDoNotAllocateWhenWarmed()
    {
        var index = PattrnIndex<string, string>.Compile([
            PattrnRegistration<string, string>.Create(
                [PatternSegment<string>.Literal("known"), PatternSegment<string>.Parameter("id")],
                "handler")
        ]);
        var path = new[] { "missing" };
        var matches = new[] { default(PatternMatchDetailedSlice<string>) };
        var captures = new[] { default(PatternCaptureSlice<string>) };

        await Assert.That(index.TryMatchDetailed(path, matches, captures, out var exactMatchesWritten, out var exactCapturesWritten))
            .IsTrue();
        await Assert.That(exactMatchesWritten).IsEqualTo(0);
        await Assert.That(exactCapturesWritten).IsEqualTo(0);
        await Assert.That(matches[0]).IsEqualTo(default(PatternMatchDetailedSlice<string>));
        await Assert.That(captures[0]).IsEqualTo(default(PatternCaptureSlice<string>));

        await Assert.That(index.TryMatchPrefixDetailed(path, matches, captures, out var prefixMatchesWritten, out var prefixCapturesWritten))
            .IsTrue();
        await Assert.That(prefixMatchesWritten).IsEqualTo(0);
        await Assert.That(prefixCapturesWritten).IsEqualTo(0);
        await Assert.That(matches[0]).IsEqualTo(default(PatternMatchDetailedSlice<string>));
        await Assert.That(captures[0]).IsEqualTo(default(PatternCaptureSlice<string>));

        await Assert.That(index.TryEnumeratePrefixDetailed(path, matches, captures, out var enumeratedMatchesWritten, out var enumeratedCapturesWritten))
            .IsTrue();
        await Assert.That(enumeratedMatchesWritten).IsEqualTo(0);
        await Assert.That(enumeratedCapturesWritten).IsEqualTo(0);
        await Assert.That(matches[0]).IsEqualTo(default(PatternMatchDetailedSlice<string>));
        await Assert.That(captures[0]).IsEqualTo(default(PatternCaptureSlice<string>));

        _ = index.TryMatchDetailed(path, matches, captures, out _, out _);
        _ = index.TryMatchPrefixDetailed(path, matches, captures, out _, out _);
        _ = index.TryEnumeratePrefixDetailed(path, matches, captures, out _, out _);

        await Assert.That(AllocatedBytes(() => _ = index.TryMatchDetailed(path, matches, captures, out _, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryMatchPrefixDetailed(path, matches, captures, out _, out _))).IsEqualTo(0);
        await Assert.That(AllocatedBytes(() => _ = index.TryEnumeratePrefixDetailed(path, matches, captures, out _, out _))).IsEqualTo(0);
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
