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
}
