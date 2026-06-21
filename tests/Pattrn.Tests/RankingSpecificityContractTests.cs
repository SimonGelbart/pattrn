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

        var values = index.MatchToArray(["orders", "new"]);
        var detailed = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSequenceEqual(values, ["literal", "parameter", "wildcard", "catch-all"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(
            detailed.Select(match => match.Kind),
            [PatternMatchKind.Exact, PatternMatchKind.Parameter, PatternMatchKind.Wildcard, PatternMatchKind.CatchAll]);
        ShouldBeTrue(detailed[0].Specificity > detailed[1].Specificity, "Literal should outrank parameter.");
        ShouldBeTrue(detailed[1].Specificity > detailed[2].Specificity, "Parameter should outrank wildcard.");
        ShouldBeTrue(detailed[2].Specificity > detailed[3].Specificity, "Wildcard should outrank catch-all.");
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
        ShouldBeTrue(matches[0].Specificity > matches[1].Specificity, "A named parameter carries capture metadata and should outrank an anonymous wildcard.");
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

        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["first", "second"]);
        ShouldEqual(matches[0].Specificity, matches[1].Specificity);
        ShouldEqual(matches[0].RegistrationOrder, 0);
        ShouldEqual(matches[1].RegistrationOrder, 1);
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
        ShouldEqual(matches[0].PatternId, "literal-handler");
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

        var values = index.MatchToArray(["api", "orders", "new"]);
        var detailed = index.MatchDetailedToArray(["api", "orders", "new"]);

        ShouldSequenceEqual(values, ["api-prefix", "orders-prefix", "orders-new"]);
        ShouldSequenceEqual(detailed.Select(match => match.Value), values);
        ShouldSequenceEqual(detailed.Select(match => match.RegistrationOrder), [0, 1, 2]);
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
        ShouldBeTrue(matches[0].Specificity > matches[1].Specificity, "Literal should outrank parameter at the same depth.");
        ShouldBeTrue(matches[1].Specificity > matches[2].Specificity, "Parameter should outrank wildcard at the same depth.");
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
        ShouldEqual(matches[1].Captures.Count, 0);
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

        var matches = index.MatchDetailedToArray(["files", "a", "b.txt"]);

        ShouldSequenceEqual(matches.Select(match => match.Value), ["first", "second"]);
        ShouldEqual(matches[0].Specificity, matches[1].Specificity);
        ShouldEqual(matches[0].RegistrationOrder, 0);
        ShouldEqual(matches[1].RegistrationOrder, 1);
        ShouldSequenceEqual(
            matches[0].Captures,
            [new PatternCapture<string>("firstPath", "a", 1), new PatternCapture<string>("firstPath", "b.txt", 2)]);
        ShouldSequenceEqual(
            matches[1].Captures,
            [new PatternCapture<string>("secondPath", "a", 1), new PatternCapture<string>("secondPath", "b.txt", 2)]);
    }
}
