using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class BuilderDiagnosticTests
{
    [Test]
    public void CompileWithDiagnosticsReportsStableDuplicatePatternWarning()
    {
        var first = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a");
        var second = PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            [first, second],
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn });

        ShouldBeFalse(result.Report.HasErrors, "Warnings should not prevent compilation.");
        ShouldBeTrue(result.Report.HasWarnings, "Expected a duplicate-pattern warning.");
        var diagnostic = result.Report.Diagnostics.Single();
        ShouldEqual(diagnostic.Code, "PTRN1003");
        ShouldEqual(diagnostic.Severity, PattrnDiagnosticSeverity.Warning);
        ShouldEqual(diagnostic.RegistrationId, second.Id);
        ShouldEqual(diagnostic.PatternSegmentIndex, null);
    }

    [Test]
    public void DuplicatePatternRejectsWithoutProducingAnIndex()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b")
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(registrations);

        ShouldBeTrue(result.Report.HasErrors, "The default duplicate policy should reject duplicates.");
        ShouldBeFalse(result.TryGetIndex(out _), "Rejected compilation must not expose a partial index.");
        ShouldEqual(result.Report.Diagnostics.Single().Code, "PTRN1003");
    }

    [Test]
    public void StructuralDiagnosticsCarryRegistrationAndSegmentAttribution()
    {
        var registration = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.CatchAll("path"), PatternSegment<string>.Literal("tail")],
            "invalid");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics([registration], new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        var diagnostic = result.Report.Diagnostics.Single();
        ShouldEqual(diagnostic.Code, "PTRN1004");
        ShouldEqual(diagnostic.Severity, PattrnDiagnosticSeverity.Error);
        ShouldEqual(diagnostic.RegistrationId, registration.Id);
        ShouldEqual(diagnostic.PatternSegmentIndex, 0);
    }

    [Test]
    public void TreatWarningsAsErrorsSuppressesIndexButKeepsWarningSeverity()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "a"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "b")
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn, TreatWarningsAsErrors = true });

        ShouldBeTrue(result.Report.HasWarnings, "The report retains warning severity.");
        ShouldBeFalse(result.Report.HasErrors, "Warning escalation must not rewrite diagnostic severity.");
        ShouldBeFalse(result.TryGetIndex(out _), "Escalated warnings must suppress the index.");
    }
}
