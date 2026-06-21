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
}
