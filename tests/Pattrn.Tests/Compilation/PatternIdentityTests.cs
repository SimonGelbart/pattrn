using Pattrn.Diagnostics;
using Pattrn.Patterns;
using Pattrn.Registrations;

namespace Pattrn.Tests.Compilation;

public sealed class PatternIdentityTests
{
    [Test]
    public async Task StructurallyEqualPatternsShareIdentityAndPatternCount()
    {
        var first = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()],
            "first");
        var second = PattrnRegistration<string, string>.Create(
            [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()],
            "second");

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            [first, second],
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        await Assert.That(result.TryGetIndex(out var index)).IsTrue();
        await Assert.That(index!.PatternCount).IsEqualTo(1);
        await Assert.That(result.Report.Diagnostics).IsEmpty();
    }

    [Test]
    public async Task EqualLengthKindLiteralAndNameDifferencesRemainDistinct()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "literal"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("payments")], "other-literal"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Wildcard()], "wildcard"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Parameter("value")], "parameter"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Parameter("other")], "other-parameter"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.CatchAll("value")], "catch-all"),
        };

        var index = PattrnIndex<string, string>.Compile(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        await Assert.That(index.PatternCount).IsEqualTo(registrations.Length);
    }

    [Test]
    public async Task DifferentLengthsRemainDistinct()
    {
        var index = PattrnIndex<string, string>.Compile(
            [
                PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "short"),
                PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()], "long")
            ],
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        await Assert.That(index.PatternCount).IsEqualTo(2);
    }

    [Test]
    public async Task ConfiguredLiteralComparerDefinesCanonicalIdentity()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("Orders")], "upper"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "lower"),
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn },
            StringComparer.OrdinalIgnoreCase);

        await Assert.That(result.TryGetIndex(out var index)).IsTrue();
        await Assert.That(index!.PatternCount).IsEqualTo(1);
        await Assert.That(result.Report.Diagnostics.Length).IsEqualTo(1);
        await Assert.That(result.Report.Diagnostics[0].Code).IsEqualTo("PTRN1003");
        await Assert.That(result.Report.Diagnostics[0].RegistrationId).IsEqualTo(registrations[1].Id);
    }

    [Test]
    public async Task DuplicatePolicyControlsOnlyPatternDiagnostics()
    {
        var registrations = new[]
        {
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "first"),
            PattrnRegistration<string, string>.Create([PatternSegment<string>.Literal("orders")], "second"),
        };

        var rejected = PattrnIndex<string, string>.CompileWithDiagnostics(registrations);
        var warned = PattrnIndex<string, string>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn });
        var allowed = PattrnIndex<string, string>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow });

        await Assert.That(rejected.Report.Diagnostics.Single().Severity).IsEqualTo(PattrnDiagnosticSeverity.Error);
        await Assert.That(warned.Report.Diagnostics.Single().Severity).IsEqualTo(PattrnDiagnosticSeverity.Warning);
        await Assert.That(allowed.Report.Diagnostics).IsEmpty();
        await Assert.That(allowed.TryGetIndex(out var index)).IsTrue();
        await Assert.That(index!.PatternCount).IsEqualTo(1);
    }

    [Test]
    public async Task DuplicateRegistrationIdentityIsStillReportedForDifferentPatterns()
    {
        var id = RegistrationId.New();
        var registrations = new[]
        {
            new PattrnRegistration<string, string>(
                id,
                [PatternSegment<string>.Literal("orders")],
                "first"),
            new PattrnRegistration<string, string>(
                id,
                [PatternSegment<string>.Literal("payments")],
                "second"),
        };

        var result = PattrnIndex<string, string>.CompileWithDiagnostics(registrations);

        await Assert.That(result.Report.Diagnostics.Length).IsEqualTo(1);
        await Assert.That(result.Report.Diagnostics[0].Code).IsEqualTo("PTRN1002");
        await Assert.That(result.Report.Diagnostics[0].RegistrationId).IsEqualTo(id);
    }

    [Test]
    public async Task BuilderPatternCountUsesTheConfiguredComparer()
    {
        var builder = PattrnIndex<string, string>.Builder(StringComparer.OrdinalIgnoreCase);
        builder.AddPattern([PatternSegment<string>.Literal("Orders")], "upper");
        builder.AddPattern([PatternSegment<string>.Literal("orders")], "lower");
        builder.AddPattern([PatternSegment<string>.Parameter("value")], "parameter");

        await Assert.That(builder.PatternCount).IsEqualTo(2);
        await Assert.That(builder.Build(new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow }).PatternCount).IsEqualTo(2);
    }
}
