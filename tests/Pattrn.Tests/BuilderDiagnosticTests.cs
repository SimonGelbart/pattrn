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

    [Test]
    public void DiagnosticsReferenceDocumentsEveryBuilderDiagnosticKindWithSeverityAndStability()
    {
        var diagnosticKindNames = Enum.GetNames<PatternDiagnosticKind>();
        var tableRows = ReadDiagnosticsReference()
            .Where(line => diagnosticKindNames.Any(kind => line.StartsWith($"| `{kind}`", StringComparison.Ordinal)))
            .Select(line => line.Split('|', StringSplitOptions.TrimEntries))
            .ToDictionary(cells => cells[1].Trim('`'), cells => new { NumericValue = cells[2], Severity = cells[3], Stability = cells[4] }, StringComparer.Ordinal);

        var expected = new Dictionary<PatternDiagnosticKind, (int NumericValue, PatternDiagnosticSeverity Severity, string Stability)>
        {
            [PatternDiagnosticKind.DuplicatePattern] = (0, PatternDiagnosticSeverity.Warning, "Stable"),
            [PatternDiagnosticKind.AmbiguousParameterNames] = (1, PatternDiagnosticSeverity.Warning, "Stable"),
            [PatternDiagnosticKind.OverlappingWildcard] = (2, PatternDiagnosticSeverity.Info, "Stable kind/severity; overlap metadata needs follow-up"),
            [PatternDiagnosticKind.OverlappingCatchAll] = (3, PatternDiagnosticSeverity.Info, "Stable kind/severity; overlap metadata needs follow-up")
        };

        ShouldEqual(tableRows.Count, Enum.GetValues<PatternDiagnosticKind>().Length);

        foreach (var kind in Enum.GetValues<PatternDiagnosticKind>())
        {
            ShouldBeTrue(tableRows.TryGetValue(kind.ToString(), out var row), $"Missing diagnostics reference row for {kind}.");
            ShouldEqual(row!.NumericValue, ((int)kind).ToString(System.Globalization.CultureInfo.InvariantCulture));
            ShouldEqual(row.Severity, expected[kind].Severity.ToString());
            ShouldEqual(row.Stability, expected[kind].Stability);
            ShouldEqual((int)kind, expected[kind].NumericValue);
        }
    }

    [Test]
    public void ExistingDiagnosticKindsEmitDocumentedDefaultSeverities()
    {
        var diagnostics = new[]
        {
            CreateDiagnostic(PatternDiagnosticKind.DuplicatePattern),
            CreateDiagnostic(PatternDiagnosticKind.AmbiguousParameterNames),
            CreateDiagnostic(PatternDiagnosticKind.OverlappingWildcard),
            CreateDiagnostic(PatternDiagnosticKind.OverlappingCatchAll)
        };

        var severities = diagnostics.ToDictionary(diagnostic => diagnostic.Kind, diagnostic => diagnostic.Severity);

        ShouldEqual(severities[PatternDiagnosticKind.DuplicatePattern], PatternDiagnosticSeverity.Warning);
        ShouldEqual(severities[PatternDiagnosticKind.AmbiguousParameterNames], PatternDiagnosticSeverity.Warning);
        ShouldEqual(severities[PatternDiagnosticKind.OverlappingWildcard], PatternDiagnosticSeverity.Info);
        ShouldEqual(severities[PatternDiagnosticKind.OverlappingCatchAll], PatternDiagnosticSeverity.Info);
    }

    private static PatternDiagnostic<string> CreateDiagnostic(PatternDiagnosticKind kind)
    {
        var builder = PattrnIndex<string, string>.Builder("*");

        switch (kind)
        {
            case PatternDiagnosticKind.DuplicatePattern:
                builder.Add(["orders", "new"], "a").Add(["orders", "new"], "b");
                break;
            case PatternDiagnosticKind.AmbiguousParameterNames:
                builder
                    .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")], "a")
                    .AddPattern([PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")], "b");
                break;
            case PatternDiagnosticKind.OverlappingWildcard:
                builder.Add(["orders", "*"], "wildcard").Add(["orders", "new"], "literal");
                break;
            case PatternDiagnosticKind.OverlappingCatchAll:
                builder
                    .AddPattern([PatternSegment<string>.Literal("files"), PatternSegment<string>.CatchAll("path")], "catch-all")
                    .Add(["files", "readme"], "literal");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown diagnostic kind.");
        }

        return builder.GetDiagnostics().Single(diagnostic => diagnostic.Kind == kind);
    }

    private static string[] ReadDiagnosticsReference()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, "docs", "reference", "diagnostics.md");
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find docs/reference/diagnostics.md.");
    }
}
