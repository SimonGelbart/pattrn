using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class DuplicateBehaviorInteractionTests
{
    [Test]
    public void AppendStoresDuplicateRegistrationsButDefaultMatchModeDeduplicatesEqualValues()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .Add(["orders", "new"], "handler")
            .Add(["orders", "new"], "handler")
            .Build();

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 2);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["handler"]);
    }

    [Test]
    public void AppendStoresDuplicateRegistrationsAndPreserveDuplicatesEmitsEqualValuesRepeatedly()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .Add(["orders", "new"], "handler")
            .Add(["orders", "new"], "handler")
            .Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 2);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["handler", "handler"]);
    }

    [Test]
    public void ReplaceRemovesPreviousValuesAndPreviousCaptureMetadataForStructuralPattern()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "first")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "second")
            .Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 1);

        var matches = index.MatchDetailed(["orders", "123"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "second");
        ShouldSequenceEqual(matches[0].Captures, [new PatternCapture<string>("name", "123", 1)]);
    }

    [Test]
    public void DifferentMatchingPatternsWithSameValueAreDeduplicatedByDefault()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "new"], "handler")
            .Add(["orders", "*"], "handler")
            .Build();

        ShouldEqual(index.PatternCount, 2);
        ShouldEqual(index.RegistrationCount, 2);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["handler"]);
    }

    [Test]
    public void DifferentMatchingPatternsWithSameValueCanPreserveDuplicateEmissions()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "new"], "handler")
            .Add(["orders", "*"], "handler")
            .Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["handler", "handler"]);
    }

    [Test]
    public void DefaultDeduplicationKeepsFirstAcceptedDetailedMatchInRankOrder()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "handler", patternId: "wildcard-handler")
            .Add(["orders", "new"], "handler", patternId: "literal-handler")
            .Build();

        ShouldEqual(index.PatternCount, 2);
        ShouldEqual(index.RegistrationCount, 2);

        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "handler");
        ShouldEqual(matches[0].PatternId, "literal-handler");
        ShouldEqual(matches[0].Kind, PatternMatchKind.Exact);
    }

    [Test]
    public void PreserveDuplicatesReturnsEqualDetailedMatchesWithMetadata()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "handler", patternId: "wildcard-handler")
            .Add(["orders", "new"], "handler", patternId: "literal-handler")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["orders", "new"]);

        ShouldEqual(matches.Length, 2);
        ShouldSequenceEqual(matches.Select(match => match.Value), ["handler", "handler"]);
        ShouldSequenceEqual(matches.Select(match => match.PatternId), ["literal-handler", "wildcard-handler"]);
        ShouldSequenceEqual(matches.Select(match => match.Kind), [PatternMatchKind.Exact, PatternMatchKind.Wildcard]);
        ShouldBeTrue(
            matches[0].RegistrationOrder > matches[1].RegistrationOrder,
            "The literal registration was added after the wildcard registration, so registration metadata should remain visible even when specificity determines match order.");
    }

    [Test]
    public void DeduplicationUsesConfiguredValueComparer()
    {
        var deduplicating = PattrnIndex<string, string>
            .Builder("*", valueComparer: StringComparer.OrdinalIgnoreCase)
            .Add(["orders", "new"], "HANDLER")
            .Add(["orders", "*"], "handler")
            .Build();

        var preserving = PattrnIndex<string, string>
            .Builder("*", valueComparer: StringComparer.OrdinalIgnoreCase)
            .Add(["orders", "new"], "HANDLER")
            .Add(["orders", "*"], "handler")
            .Build(MatchOptions.PreserveDuplicates);

        ShouldSequenceEqual(deduplicating.MatchToArray(["orders", "new"]), ["HANDLER"]);
        ShouldSequenceEqual(preserving.MatchToArray(["orders", "new"]), ["HANDLER", "handler"]);
    }
}
