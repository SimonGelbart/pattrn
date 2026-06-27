using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class DuplicatePatternRegistrationBehaviorTests
{
    [Test]
    public void DefaultBehaviorAppendsDuplicateStructuralPattern()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["first", "second"]);
        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 2);
    }

    [Test]
    public void ThrowBehaviorRejectsDuplicateStructuralPattern()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw)
            .Add(["orders", "new"], "first");

        ShouldThrow<InvalidOperationException>(() => builder.Add(["orders", "new"], "second"));
        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
    }

    [Test]
    public void ThrowRejectsStructurallyDuplicateParameterPattern()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "first");

        ShouldThrow<InvalidOperationException>(() => builder.AddPattern(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")],
            "second"));
        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
    }

    [Test]
    public void ReplaceBehaviorReplacesDuplicateStructuralPattern()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second");

        var index = builder.Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["second"]);
    }

    [Test]
    public void IgnoreBehaviorKeepsExistingDuplicateStructuralPattern()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Ignore)
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second");

        var index = builder.Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldEqual(builder.PatternCount, 1);
        ShouldEqual(builder.RegistrationCount, 1);
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["first"]);
    }

    [Test]
    public void DuplicatePatternRegistrationBehaviorAppliesToStructurallyEquivalentParameterPatterns()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "first")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "second");

        var result = builder.Build().MatchDetailedToArray(["orders", "123"]);

        ShouldEqual(result.Length, 1);
        ShouldEqual(result[0].Value, "second");
        ShouldEqual(result[0].Captures[0].Name, "name");
    }

    [Test]
    public void AppendPreservesStructurallyDuplicateParameterMetadataWhenDuplicatesArePreserved()
    {
        var preserving = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("orderId")], "handler")
            .Build(MatchOptions.PreserveDuplicates);

        var deduplicating = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "handler")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("orderId")], "handler")
            .Build();

        ShouldEqual(preserving.PatternCount, 1);
        ShouldEqual(preserving.RegistrationCount, 2);
        ShouldSequenceEqual(preserving.MatchToArray(["orders", "42"]), ["handler", "handler"]);
        ShouldSequenceEqual(deduplicating.MatchToArray(["orders", "42"]), ["handler"]);

        var matches = preserving.MatchDetailedToArray(["orders", "42"]);

        ShouldEqual(matches.Length, 2);
        ShouldSequenceEqual(matches.Select(match => match.RegistrationOrder), [0, 1]);
        ShouldSequenceEqual(matches[0].Captures, [new PatternCapture<string>("id", "42", 1)]);
        ShouldSequenceEqual(matches[1].Captures, [new PatternCapture<string>("orderId", "42", 1)]);
    }

    [Test]
    public void ReplaceKeepsLatestCaptureMetadataForStructurallyDuplicateParameterPattern()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "first")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "second")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["orders", "42"]);

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 1);
        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "second");
        ShouldSequenceEqual(matches[0].Captures, [new PatternCapture<string>("name", "42", 1)]);
    }

    [Test]
    public void IgnoreKeepsFirstCaptureMetadataForStructurallyDuplicateParameterPattern()
    {
        var index = PattrnIndex<string, string>
            .Builder("*")
            .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Ignore)
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "first")
            .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "second")
            .Build(MatchOptions.PreserveDuplicates);

        var matches = index.MatchDetailedToArray(["orders", "42"]);

        ShouldEqual(index.PatternCount, 1);
        ShouldEqual(index.RegistrationCount, 1);
        ShouldEqual(matches.Length, 1);
        ShouldEqual(matches[0].Value, "first");
        ShouldSequenceEqual(matches[0].Captures, [new PatternCapture<string>("id", "42", 1)]);
    }

    [Test]
    public void ValidateOnBuildRejectsWarningDiagnosticsByDefault()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .ValidateOnBuild();

        ShouldThrow<InvalidOperationException>(() => builder.Build());
    }

    [Test]
    public void ValidateOnBuildCanRejectInfoDiagnostics()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "wildcard")
            .Add(["orders", "new"], "literal")
            .ValidateOnBuild(PatternDiagnosticSeverity.Info);

        ShouldThrow<InvalidOperationException>(() => builder.Build());
    }

    [Test]
    public void ValidateOnBuildCanUseCustomPredicate()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "*"], "wildcard")
            .Add(["orders", "new"], "literal")
            .ValidateOnBuild(diagnostic => diagnostic.Kind == PatternDiagnosticKind.OverlappingWildcard);

        ShouldThrow<InvalidOperationException>(() => builder.Build());
    }

    [Test]
    public void DisableBuildValidationAllowsPreviouslyRejectedDiagnostics()
    {
        var builder = PattrnIndex<string, string>
            .Builder("*")
            .Add(["orders", "new"], "first")
            .Add(["orders", "new"], "second")
            .ValidateOnBuild()
            .DisableBuildValidation();

        var index = builder.Build(new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));

        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["first", "second"]);
    }
}
