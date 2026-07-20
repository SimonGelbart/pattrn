using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class RankingSpecificityContractTests
{
    [Test]
    public void ValueAndDetailedResultsUseTheSameGenericSpecificityOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll("tail")], "catch-all")
            .Add(["orders", "*"], "wildcard")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "parameter")
            .Add(["orders", "new"], "literal")
            .Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchPrefixToArray(["orders", "new"]);
        var detailed = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSequenceEqual(values, ["literal", "parameter", "wildcard", "catch-all"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(
            detailed.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Parameter, PatternMatchKind.Wildcard, PatternMatchKind.CatchAll]);
    }

    [Test]
    public void ParameterOutranksAnonymousWildcardOnTheSameStructuralBranch()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "wildcard")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "parameter")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["orders", "42"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["parameter", "wildcard"]);
        ShouldEqual(matches[0].Kind, PatternMatchKind.Parameter);
        ShouldEqual(matches[1].Kind, PatternMatchKind.Wildcard);
    }

    [Test]
    public void EqualSpecificityPreservesRegistrationOrderWhenDuplicatesArePreserved()
    {
        var index = PattrnIndex<string, string>
            .Builder()
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .Add(["orders", "new"], "first", patternId: "first")
            .Add(["orders", "new"], "second", patternId: "second")
            .Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchPrefixToArray(["orders", "new"]);
        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSequenceEqual(values, ["first", "second"]);
        ShouldSequenceEqual(matches.Select(match => match.Value), ["first", "second"]);
    }

    [Test]
    public void ValueAndDetailedResultsPreserveRegistrationOrderForEqualSpecificityWildcards()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .Add(["orders", "*"], "first", patternId: "first-wildcard")
            .Add(["orders", "*"], "second", patternId: "second-wildcard")
            .Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchPrefixToArray(["orders", "42"]);
        var matches = index.MatchDetailedToArray(["orders", "42"]);

        ShouldSequenceEqual(values, ["first", "second"]);
        ShouldSequenceEqual(matches.Select(match => match.Value), values);
    }

    [Test]
    public void ValueAndDetailedResultsPreserveRegistrationOrderForEqualSpecificityParameterPatterns()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "first",
                patternId: "first-parameter")
            .AddPattern(
                [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("orderId")],
                "second",
                patternId: "second-parameter")
            .Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchPrefixToArray(["orders", "42"]);
        var matches = index.MatchDetailedToArray(["orders", "42"]);

        ShouldSequenceEqual(values, ["first", "second"]);
        ShouldSequenceEqual(matches.Select(match => match.Value), values);
        ShouldSequenceEqual(matches[0].Captures, [new PatternCapture<string>("id", "42", 1)]);
        ShouldSequenceEqual(matches[1].Captures, [new PatternCapture<string>("orderId", "42", 1)]);
    }

    [Test]
    public void DeduplicationKeepsTheFirstValueInDeterministicRankOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "handler", patternId: "wildcard-handler")
            .Add(["orders", "new"], "handler", patternId: "literal-handler")
            .Build();

        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.Exact);
    }

    [Test]
    public void PrefixModeEmitsPrefixRegistrationsBeforeDeeperRegistrations()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["api"], "api-prefix")
            .Add(["api", "orders"], "orders-prefix")
            .Add(["api", "orders", "new"], "orders-new")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var values = index.MatchPrefixToArray(["api", "orders", "new"]);
        var detailed = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(values, ["api-prefix", "orders-prefix", "orders-new"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
    }

    [Test]
    public void PrefixModeIsTraversalOrderedRatherThanGloballySpecificitySorted()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["api"], "api-prefix", patternId: "api-prefix")
            .Add(["api", "orders", "new"], "orders-new-literal", patternId: "orders-new-literal")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var values = index.MatchPrefixToArray(["api", "orders", "new"]);
        var detailed = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(values, ["api-prefix", "orders-new-literal"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(detailed.Select(match => match.Kind), [PatternMatchKind.Exact, PatternMatchKind.Exact]);
    }

    [Test]
    public void PrefixModeStillUsesSpecificityWithinTheSameDepth()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["api", "orders", "*"], "wildcard")
            .AddPattern(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "parameter")
            .Add(["api", "orders", "new"], "literal")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var matches = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["literal", "parameter", "wildcard"]);
    }

    [Test]
    public void PrefixModeUsesSpecificityWithinTheSameDepthIncludingCatchAll()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll("tail")],
                "catch-all")
            .Add(["api", "orders", "*"], "wildcard")
            .AddPattern(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "parameter")
            .Add(["api", "orders", "new"], "literal")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var values = index.MatchPrefixToArray(["api", "orders", "new"]);
        var detailed = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(values, ["literal", "parameter", "wildcard", "catch-all"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(
            detailed.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Parameter, PatternMatchKind.Wildcard, PatternMatchKind.CatchAll]);
        ShouldSequenceEqual(detailed[3].Captures, [new PatternCapture<string>("tail", "new", 2)]);
    }

    [Test]
    public void PrefixModeEmitsPrefixNodeBeforeSpecificDeeperBranchMatches()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["api", "orders"], "orders-prefix")
            .AddPattern(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.CatchAll("tail")],
                "catch-all")
            .Add(["api", "orders", "*"], "wildcard")
            .AddPattern(
                [PatternSegment<string>.Literal("api"), PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
                "parameter")
            .Add(["api", "orders", "new"], "literal")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var values = index.MatchPrefixToArray(["api", "orders", "new"]);
        var detailed = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(values, ["orders-prefix", "literal", "parameter", "wildcard", "catch-all"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(
            detailed.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Exact, PatternMatchKind.Parameter, PatternMatchKind.Wildcard, PatternMatchKind.CatchAll]);
    }

    [Test]
    public void PrefixModeAllowsTerminalCatchAllWithoutChangingTraversalOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern(
                [PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")],
                "files-catch-all")
            .Add(["files", "public"], "files-public")
            .Add(["files", "public", "images"], "files-public-images")
            .Build(new MatchOptions(PrefixMatchMode.IncludePrefixPatterns, DuplicateValueMatchMode.PreserveDuplicates));

        var values = index.MatchPrefixToArray(["files", "public", "images", "logo.png"]);
        var detailed = index.MatchDetailedToArray(["files", "public", "images", "logo.png"]);

        ShouldSequenceEqual(values, ["files-public", "files-public-images", "files-catch-all"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(
            detailed.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Exact, PatternMatchKind.CatchAll]);
        ShouldSequenceEqual(
            detailed[2].Captures,
            [
                new PatternCapture<string>("path", ["public", "images", "logo.png"], 1)
            ]);
    }

    [Test]
    public void CatchAllWithEmptyRemainderIsEmittedAfterExactRegistrationAtTheSameNode()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "files-catch-all")
            .Add(["files"], "files-root")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["files"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["files-root", "files-catch-all"]);
        ShouldEqual(matches[0].Kind, PatternMatchKind.Exact);
        ShouldEqual(matches[1].Kind, PatternMatchKind.CatchAll);
        ShouldEqual(matches[1].Captures.Length, 1);
        ShouldEqual(matches[1].Captures[0], new PatternCapture<string>("path", [], 1));
    }

    [Test]
    public void EqualCatchAllSpecificityPreservesRegistrationOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("firstPath")], "first")
            .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("secondPath")], "second")
            .Build(MatchOptions.PreserveDuplicates);

        var values = index.MatchPrefixToArray(["files", "a", "b.txt"]);
        var matches = index.MatchDetailedToArray(["files", "a", "b.txt"]);

        ShouldSequenceEqual(values, ["first", "second"]);
        ShouldSequenceEqual(matches.Select(match => match.Value), ["first", "second"]);
        ShouldSequenceEqual(
            matches[0].Captures,
            [new PatternCapture<string>("firstPath", ["a", "b.txt"], 1)]);
        ShouldSequenceEqual(
            matches[1].Captures,
            [new PatternCapture<string>("secondPath", ["a", "b.txt"], 1)]);
    }
}
