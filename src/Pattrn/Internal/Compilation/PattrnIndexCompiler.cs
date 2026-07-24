using System.Collections.Immutable;

namespace Pattrn.Internal.Compilation;

/// <summary>Contains the single canonical registration validation and compilation path.</summary>
internal static class PattrnIndexCompiler<TSegment, TValue>
    where TSegment : notnull
{
    internal static PattrnCompileResult<TSegment, TValue> CompileWithDiagnostics(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        PattrnCompileOptions? options,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer,
        IEqualityComparer<TValue>? valueComparer)
    {
        ArgumentNullException.ThrowIfNull(registrations);
        options ??= PattrnCompileOptions.Default;
        if (!Enum.IsDefined(options.DuplicatePatternPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Unknown duplicate pattern policy.");
        }

        var segmentEquality = segmentComparer ?? EqualityComparer<TSegment>.Default;
        var valueEquality = valueComparer ?? EqualityComparer<TValue>.Default;
        var snapshot = registrations.ToArray();
        var diagnostics = new List<PattrnDiagnostic>();
        var ids = new HashSet<RegistrationId>();
        var patterns = new List<ImmutableArray<PatternSegment<TSegment>>>();

        for (var registrationIndex = 0; registrationIndex < snapshot.Length; registrationIndex++)
        {
            var registration = snapshot[registrationIndex] ?? throw new ArgumentException("Registrations cannot contain null entries.", nameof(registrations));
            if (registration.Id.Value == Guid.Empty)
            {
                diagnostics.Add(new("PTRN1001", PattrnDiagnosticSeverity.Error, "Registration identity must not be empty.", registration.Id));
            }
            else if (!ids.Add(registration.Id))
            {
                diagnostics.Add(new("PTRN1002", PattrnDiagnosticSeverity.Error, "Registration identity is duplicated.", registration.Id));
            }

            var duplicateIndex = patterns.FindIndex(pattern => SamePattern(pattern, registration.Pattern, segmentEquality));
            if (duplicateIndex >= 0 && options.DuplicatePatternPolicy != DuplicatePatternPolicy.Allow)
            {
                diagnostics.Add(new(
                    "PTRN1003",
                    options.DuplicatePatternPolicy == DuplicatePatternPolicy.Warn ? PattrnDiagnosticSeverity.Warning : PattrnDiagnosticSeverity.Error,
                    "The canonical pattern is duplicated.",
                    registration.Id));
            }
            patterns.Add(registration.Pattern);

            var captureNames = new HashSet<string>(StringComparer.Ordinal);
            for (var segmentIndex = 0; segmentIndex < registration.Pattern.Length; segmentIndex++)
            {
                var segment = registration.Pattern[segmentIndex];
                if (segment.IsCatchAll && segmentIndex != registration.Pattern.Length - 1)
                {
                    diagnostics.Add(new("PTRN1004", PattrnDiagnosticSeverity.Error, "Catch-all segments must be terminal.", registration.Id, segmentIndex));
                }

                if (segment.ParameterName is not null && !captureNames.Add(segment.ParameterName))
                {
                    diagnostics.Add(new("PTRN1005", PattrnDiagnosticSeverity.Error, "Capture names must be unique within a pattern.", registration.Id, segmentIndex));
                }
            }
        }

        PattrnIndex<TSegment, TValue>? index = null;
        if (!diagnostics.Any(d => d.Severity == PattrnDiagnosticSeverity.Error)
            && !(options.TreatWarningsAsErrors && diagnostics.Any(d => d.Severity == PattrnDiagnosticSeverity.Warning)))
        {
            index = new PattrnIndex<TSegment, TValue>(
                ImmutableIndexStorageBuilder<TSegment, TValue>.FromRegistrations(
                    snapshot,
                    segmentEquality,
                    valueEquality,
                    deduplicateValues: matchOptions.DeduplicateValues),
                CountDistinctPatterns(snapshot, segmentEquality),
                snapshot.Length,
                matchOptions,
                segmentEquality,
                valueEquality);
        }

        var finalReport = new PattrnDiagnosticReport(diagnostics);
        if (finalReport.HasWarnings && options.TreatWarningsAsErrors)
        {
            index = null;
        }

        return new PattrnCompileResult<TSegment, TValue>(index, finalReport);
    }

    private static int CountDistinctPatterns(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        IEqualityComparer<TSegment> comparer)
    {
        var patterns = new List<ImmutableArray<PatternSegment<TSegment>>>();
        foreach (var registration in registrations)
        {
            if (!patterns.Any(pattern => SamePattern(pattern, registration.Pattern, comparer)))
            {
                patterns.Add(registration.Pattern);
            }
        }

        return patterns.Count;
    }

    private static bool SamePattern(
        ImmutableArray<PatternSegment<TSegment>> left,
        ImmutableArray<PatternSegment<TSegment>> right,
        IEqualityComparer<TSegment> comparer)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            left[i].Deconstruct(out var leftKind, out var leftLiteral, out var leftName);
            right[i].Deconstruct(out var rightKind, out var rightLiteral, out var rightName);
            if (leftKind != rightKind
                || (leftKind == PatternSegmentKind.Literal && !comparer.Equals(leftLiteral, rightLiteral))
                || !string.Equals(leftName, rightName, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
