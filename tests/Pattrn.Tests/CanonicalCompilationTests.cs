using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class CanonicalCompilationTests
{
    [Test]
    public void RegistrationSnapshotsPatternAndGeneratesIdentity()
    {
        var pattern = new List<PatternSegment<string>> { PatternSegment<string>.Literal("a") };
        var registration = PattrnRegistration<string, string>.Create(pattern, "value", "description");
        pattern[0] = PatternSegment<string>.Literal("changed");

        ShouldBeTrue(registration.Id.Value != Guid.Empty, "Expected a generated identity.");
        ShouldEqual(registration.Pattern[0].LiteralValue, "a");
        ShouldEqual(registration.Name, "description");
    }

    [Test]
    public void DuplicatePatternsProduceStableDiagnosticsAndNoIndex()
    {
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "second");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics([first, second]);

        ShouldBeFalse(result.Succeeded, "Expected duplicate patterns to reject compilation.");
        ShouldEqual(result.Report.Diagnostics.Length, 1);
        ShouldEqual(result.Report.Diagnostics[0].Code, "PTRN1003");
        ShouldEqual(result.Report.Diagnostics[0].RegistrationId, second.Id);
    }

    [Test]
    public void BuilderCanonicalMutationsPreserveOrderAndCompile()
    {
        var builder = PattrnIndex<string, string>.Builder();
        var first = builder.Add((IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("a")], "first");
        var second = builder.Add((IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("b")], "second");

        ShouldSequenceEqual(builder.ToRegistrations().Select(r => r.Id), [first, second]);
        ShouldBeTrue(builder.Remove(first), "Expected identity removal to succeed.");
        ShouldSequenceEqual(builder.Build().MatchValuesToArray(["b"]), ["second"]);
    }
}
