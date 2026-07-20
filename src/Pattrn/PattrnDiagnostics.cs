using System.Collections.Immutable;
#pragma warning disable CS1591

namespace Pattrn;

public enum PattrnDiagnosticSeverity { Error, Warning }

public sealed record PattrnDiagnostic(
    string Code,
    PattrnDiagnosticSeverity Severity,
    string Message,
    RegistrationId? RegistrationId = null,
    int? PatternSegmentIndex = null);

public sealed class PattrnDiagnosticReport
{
    public PattrnDiagnosticReport(IEnumerable<PattrnDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        Diagnostics = diagnostics.ToImmutableArray();
    }

    public ImmutableArray<PattrnDiagnostic> Diagnostics { get; }
    public bool HasErrors => Diagnostics.Any(d => d.Severity == PattrnDiagnosticSeverity.Error);
    public bool HasWarnings => Diagnostics.Any(d => d.Severity == PattrnDiagnosticSeverity.Warning);
}

public sealed class PattrnCompilationException : Exception
{
    public PattrnCompilationException(PattrnDiagnosticReport report)
        : base("Pattrn compilation failed.")
    {
        ArgumentNullException.ThrowIfNull(report);
        Report = report;
    }

    public PattrnDiagnosticReport Report { get; }
}

public enum DuplicatePatternPolicy { Allow, Warn, Reject }

public sealed record PattrnCompileOptions
{
    public static PattrnCompileOptions Default { get; } = new();
    public DuplicatePatternPolicy DuplicatePatternPolicy { get; init; } = DuplicatePatternPolicy.Reject;
    public bool TreatWarningsAsErrors { get; init; }
}

public sealed class PattrnCompileResult<TSegment, TValue>
    where TSegment : notnull
{
    internal PattrnCompileResult(PattrnIndex<TSegment, TValue>? index, PattrnDiagnosticReport report)
    {
        Index = index;
        Report = report;
    }

    private PattrnIndex<TSegment, TValue>? Index { get; }
    public bool Succeeded => Index is not null && !Report.HasErrors;
    public PattrnDiagnosticReport Report { get; }

    public bool TryGetIndex(out PattrnIndex<TSegment, TValue>? index)
    {
        index = Succeeded ? Index : null;
        return index is not null;
    }
}
