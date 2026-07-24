namespace Pattrn.Tests.Builders;

public sealed class BuilderDiagnosticTests
{
    [Test]
    public async Task CompileWithDiagnosticsReportsStableDuplicatePatternWarning()
    {
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            [first, second],
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn });

        await Assert.That(result.Report.HasErrors).IsFalse().Because("Warnings should not prevent compilation.");
        await Assert.That(result.Report.HasWarnings).IsTrue().Because("Expected a duplicate-pattern warning.");
        var diagnostic = result.Report.Diagnostics.Single();
        await Assert.That(diagnostic.Code).IsEqualTo("PTRN1003");
        await Assert.That(diagnostic.Severity).IsEqualTo(PattrnDiagnosticSeverity.Warning);
        await Assert.That(diagnostic.RegistrationId).IsEqualTo(second.Id);
        await Assert.That(diagnostic.PatternSegmentIndex).IsNull();
    }

    [Test]
    public async Task DuplicatePatternRejectsWithoutProducingAnIndex()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b")
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(registrations);

        await Assert.That(result.Report.HasErrors).IsTrue().Because("The default duplicate policy should reject duplicates.");
        await Assert.That(result.TryGetIndex(out _)).IsFalse().Because("Rejected compilation must not expose a partial index.");
        await Assert.That(result.Report.Diagnostics.Single().Code).IsEqualTo("PTRN1003");
    }

    [Test]
    public async Task StructuralDiagnosticsCarryRegistrationAndSegmentAttribution()
    {
        var registration = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.CatchAll("path"), PatternSegment<string>.Literal("tail")],
            "invalid");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics([registration], new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        var diagnostic = result.Report.Diagnostics.Single();
        await Assert.That(diagnostic.Code).IsEqualTo("PTRN1004");
        await Assert.That(diagnostic.Severity).IsEqualTo(PattrnDiagnosticSeverity.Error);
        await Assert.That(diagnostic.RegistrationId).IsEqualTo(registration.Id);
        await Assert.That(diagnostic.PatternSegmentIndex).IsEqualTo(0);
    }

    [Test]
    public async Task TreatWarningsAsErrorsSuppressesIndexButKeepsWarningSeverity()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b")
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn, TreatWarningsAsErrors = true });

        await Assert.That(result.Report.HasWarnings).IsTrue().Because("The report retains warning severity.");
        await Assert.That(result.Report.HasErrors).IsFalse().Because("Warning escalation must not rewrite diagnostic severity.");
        await Assert.That(result.TryGetIndex(out _)).IsFalse().Because("Escalated warnings must suppress the index.");
    }
}

