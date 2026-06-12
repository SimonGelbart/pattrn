using static Pattrn.Tests.TestAssertions;

namespace Pattrn.Tests;

public sealed class BuilderDiagnosticTests
{
    [Test]
    public void GetDiagnosticsReturnsEmptyArrayWhenBuilderHasNoDiagnostics()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["orders", "new"], "new-order");

        var diagnostics = builder.GetDiagnostics();

        ShouldEqual(diagnostics.Length, 0);
    }

    [Test]
    public void GetDiagnosticsReportsDuplicateStructuralPattern()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["orders", "new"], "a")
            .Add(["orders", "new"], "b");

        var diagnostics = builder.GetDiagnostics();
        var duplicate = diagnostics.Single(diagnostic => diagnostic.Kind == PatternDiagnosticKind.DuplicatePattern);

        ShouldEqual(duplicate.Severity, PatternDiagnosticSeverity.Warning);
        ShouldEqual(duplicate.RegistrationCount, 2);
        ShouldSequenceEqual(duplicate.Pattern.Select(segment => segment.ToString()), ["orders", "new"]);
    }

    [Test]
    public void GetDiagnosticsReportsAmbiguousParameterNamesForEquivalentPatterns()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .AddPattern(
                [
                    PatternSegment<string>.Literal("orders"),
                    PatternSegment<string>.Parameter("id")
                ],
                "a")
            .AddPattern(
                [
                    PatternSegment<string>.Literal("orders"),
                    PatternSegment<string>.Parameter("name")
                ],
                "b");

        var diagnostics = builder.GetDiagnostics();
        var ambiguous = diagnostics.Single(diagnostic => diagnostic.Kind == PatternDiagnosticKind.AmbiguousParameterNames);

        ShouldEqual(ambiguous.Severity, PatternDiagnosticSeverity.Warning);
        ShouldSequenceEqual(ambiguous.Pattern.Select(segment => segment.ToString()), ["orders", "{id}"]);
    }

    [Test]
    public void GetDiagnosticsReportsWildcardOverlapWithLiteralBranch()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["orders", "*"], "wildcard")
            .Add(["orders", "new"], "literal");

        var diagnostics = builder.GetDiagnostics();
        var overlap = diagnostics.Single(diagnostic => diagnostic.Kind == PatternDiagnosticKind.OverlappingWildcard);

        ShouldEqual(overlap.Severity, PatternDiagnosticSeverity.Info);
        ShouldSequenceEqual(overlap.Pattern.Select(segment => segment.ToString()), ["orders", "*"]);
    }

    [Test]
    public void GetDiagnosticsReportsCatchAllOverlapWithMoreSpecificBranch()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .AddPattern(
                [
                    PatternSegment<string>.Literal("files"),
                    PatternSegment<string>.CatchAll("path")
                ],
                "catch-all")
            .Add(["files", "readme"], "literal");

        var diagnostics = builder.GetDiagnostics();
        var overlap = diagnostics.Single(diagnostic => diagnostic.Kind == PatternDiagnosticKind.OverlappingCatchAll);

        ShouldEqual(overlap.Severity, PatternDiagnosticSeverity.Info);
        ShouldSequenceEqual(overlap.Pattern.Select(segment => segment.ToString()), ["files", "{*path}"]);
    }

    [Test]
    public void DiagnosticsDoNotChangeMatchingSemantics()
    {
        var builder = PattrnIndex<string, string>.Builder("*")
            .Add(["orders", "*"], "wildcard")
            .Add(["orders", "new"], "literal");

        var diagnostics = builder.GetDiagnostics();
        var index = builder.Build();

        ShouldBeTrue(diagnostics.Length > 0, "Expected diagnostics for the overlapping wildcard registration.");
        ShouldSequenceEqual(index.MatchToArray(["orders", "new"]), ["literal", "wildcard"]);
    }
}
