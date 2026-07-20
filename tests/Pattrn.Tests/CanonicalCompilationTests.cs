using TUnit.Assertions.Enums;

namespace Pattrn.Tests;

public sealed class CanonicalCompilationTests
{
    [Test]
    public async Task RegistrationSnapshotsPatternAndGeneratesIdentity()
    {
        var pattern = new List<PatternSegment<string>> { PatternSegment<string>.Literal("a") };
        var registration = PattrnRegistration<string, string>.Create(pattern, "value", "description");
        pattern[0] = PatternSegment<string>.Literal("changed");

        await Assert.That(registration.Id.Value != Guid.Empty).IsTrue().Because("Expected a generated identity.");
        await Assert.That(registration.Pattern[0].LiteralValue).IsEqualTo("a");
        await Assert.That(registration.Name).IsEqualTo("description");
    }

    [Test]
    public async Task DuplicatePatternsProduceStableDiagnosticsAndNoIndex()
    {
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "first");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("a")], "second");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics([first, second]);

        await Assert.That(result.Succeeded).IsFalse().Because("Expected duplicate patterns to reject compilation.");
        await Assert.That(result.Report.Diagnostics.Length).IsEqualTo(1);
        await Assert.That(result.Report.Diagnostics[0].Code).IsEqualTo("PTRN1003");
        await Assert.That(result.Report.Diagnostics[0].RegistrationId).IsEqualTo(second.Id);
    }

    [Test]
    public async Task BuilderCanonicalMutationsPreserveOrderAndCompile()
    {
        var builder = PattrnIndex<string, string>.Builder();
        var first = builder.Add((IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("a")], "first");
        var second = builder.Add((IReadOnlyList<PatternSegment<string>>)[PatternSegment<string>.Literal("b")], "second");

        await Assert.That(builder.ToRegistrations().Select(r => r.Id)).IsEquivalentTo([first, second], CollectionOrdering.Matching);
        await Assert.That(builder.Remove(first)).IsTrue().Because("Expected identity removal to succeed.");
        await Assert.That(builder.Build().MatchValuesToArray(["b"])).IsEquivalentTo(["second"], CollectionOrdering.Matching);
    }
}
