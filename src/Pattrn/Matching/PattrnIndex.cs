using System.Buffers;
using System.Collections.Immutable;

namespace Pattrn.Matching;

/// <summary>
/// Immutable index that matches segmented input paths against previously registered path patterns.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// Instances are created by <see cref="PattrnIndexBuilder{TSegment, TValue}.Build(MatchOptions)"/> and are safe for concurrent readers.
/// </remarks>
/// <example>
/// <code>
/// var builder = PattrnIndex&lt;string, string&gt;.Builder();
/// builder.AddPattern(
///     [PatternSegment.Literal("market"), PatternSegment.Literal("NASDAQ"), PatternSegment.Parameter("symbol")],
///     "client-a");
/// var index = builder.Build();
/// var matches = index.MatchToArray(["market", "NASDAQ", "MSFT"]);
/// </code>
/// </example>
public sealed class PattrnIndex<TSegment, TValue>
    where TSegment : notnull
{
    /// <summary>Compiles the canonical registrations into an immutable index.</summary>
    public static PattrnIndex<TSegment, TValue> Compile(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        PattrnCompileOptions? options = null,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        var result = CompileWithDiagnostics(registrations, options, segmentComparer, valueComparer);
        if (!result.TryGetIndex(out var index))
        {
            throw new PattrnCompilationException(result.Report);
        }

        return index!;
    }

    /// <summary>Compiles registrations with default compile policy and explicit duplicate-value behavior.</summary>
    public static PattrnIndex<TSegment, TValue> Compile(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
        => Compile(registrations, PattrnCompileOptions.Default, matchOptions, segmentComparer, valueComparer);

    /// <summary>Compiles registrations with explicit duplicate-value behavior.</summary>
    public static PattrnIndex<TSegment, TValue> Compile(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        PattrnCompileOptions? compileOptions,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        var result = CompileWithDiagnostics(registrations, compileOptions, matchOptions, segmentComparer, valueComparer);
        if (!result.TryGetIndex(out var index))
        {
            throw new PattrnCompilationException(result.Report);
        }

        return index;
    }

    /// <summary>Compiles canonical registrations and returns stable structural diagnostics.</summary>
    public static PattrnCompileResult<TSegment, TValue> CompileWithDiagnostics(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        PattrnCompileOptions? options = null,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return CompileWithDiagnostics(registrations, options, MatchOptions.Default, segmentComparer, valueComparer);
    }

    /// <summary>Compiles registrations with default compile policy and explicit duplicate-value behavior.</summary>
    public static PattrnCompileResult<TSegment, TValue> CompileWithDiagnostics(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
        => CompileWithDiagnostics(registrations, PattrnCompileOptions.Default, matchOptions, segmentComparer, valueComparer);

    /// <summary>Compiles registrations with explicit duplicate-value behavior and stable diagnostics.</summary>
    public static PattrnCompileResult<TSegment, TValue> CompileWithDiagnostics(
        IReadOnlyList<PattrnRegistration<TSegment, TValue>> registrations,
        PattrnCompileOptions? options,
        MatchOptions matchOptions,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
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
                CompiledIndex<TSegment, TValue>.FromRegistrations(
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
        if (left.Length != right.Length) return false;
        for (var i = 0; i < left.Length; i++)
        {
            left[i].Deconstruct(out var leftKind, out var leftLiteral, out var leftName);
            right[i].Deconstruct(out var rightKind, out var rightLiteral, out var rightName);
            if (leftKind != rightKind || (leftKind == PatternSegmentKind.Literal && !comparer.Equals(leftLiteral, rightLiteral)) ||
                !string.Equals(leftName, rightName, StringComparison.Ordinal)) return false;
        }
        return true;
    }

    private readonly CompiledNode[] _nodes;
    private readonly CompiledChild<TSegment>[] _children;
    private readonly int[] _childLookupSlots;
    private readonly TValue[] _values;
    private readonly CompiledValueDetail[] _valueDetails;
    private readonly CaptureDescriptor[] _captureDescriptors;
    private readonly IEqualityComparer<TSegment> _segmentComparer;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _deduplicateValues;
    private readonly bool _hasWildcardBranches;

    internal PattrnIndex(
        CompiledIndex<TSegment, TValue> index,
        int patternCount,
        int registrationCount,
        MatchOptions options,
        IEqualityComparer<TSegment> segmentComparer,
        IEqualityComparer<TValue> valueComparer)
    {
        _nodes = index.Nodes;
        _children = index.Children;
        _childLookupSlots = index.ChildLookupSlots;
        _values = index.Values;
        _valueDetails = index.ValueDetails;
        _captureDescriptors = index.CaptureDescriptors;
        _segmentComparer = segmentComparer;
        _valueComparer = valueComparer;
        PatternCount = patternCount;
        RegistrationCount = registrationCount;
        MatchCountUpperBound = registrationCount;
        Options = options;
        _deduplicateValues = options.DeduplicateValues;
        _hasWildcardBranches = index.HasWildcardBranches;
    }

    /// <summary>
    /// Gets the number of distinct patterns compiled into the index.
    /// </summary>
    public int PatternCount { get; }

    /// <summary>
    /// Gets the number of pattern/value registrations compiled into the index.
    /// </summary>
    public int RegistrationCount { get; }

    /// <summary>
    /// Gets the maximum number of values that a single match operation can emit. This is a safe upper bound for caller-provided match destination spans.
    /// </summary>
    public int MatchCountUpperBound { get; }

    /// <summary>
    /// Gets the matching options used by this index.
    /// </summary>
    public MatchOptions Options { get; }

    /// <summary>
    /// Creates a mutable tokenless builder for <see cref="PattrnIndex{TSegment, TValue}"/>.
    /// </summary>
    /// <param name="segmentComparer">The comparer used for exact segment lookup.</param>
    /// <param name="valueComparer">The comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable builder.</returns>
    /// <remarks>
    /// Prefer this overload for new core usage. Use <c>AddPattern(...)</c> to register wildcard, parameter, and catch-all segments explicitly.
    /// </remarks>
    public static PattrnIndexBuilder<TSegment, TValue> Builder(
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return PattrnIndexBuilder<TSegment, TValue>.Create(segmentComparer, valueComparer);
    }

    /// <summary>
    /// Creates a mutable builder that treats a reserved segment value as a single-segment wildcard in tokenized registrations.
    /// </summary>
    /// <param name="wildcardSegment">The segment value that represents a single-segment wildcard in tokenized registered patterns.</param>
    /// <param name="segmentComparer">The comparer used for exact segment lookup and wildcard detection.</param>
    /// <param name="valueComparer">The comparer used for removal and optional match-result deduplication.</param>
    /// <returns>A new mutable builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="wildcardSegment"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This overload is retained as a convenience for callers that intentionally reserve a wildcard token.
    /// Prefer <see cref="Builder(IEqualityComparer{TSegment}, IEqualityComparer{TValue})"/> plus explicit <see cref="PatternSegment{TSegment}"/> registrations for new core usage.
    /// </remarks>
    public static PattrnIndexBuilder<TSegment, TValue> Builder(
        TSegment wildcardSegment,
        IEqualityComparer<TSegment>? segmentComparer = null,
        IEqualityComparer<TValue>? valueComparer = null)
    {
        return PattrnIndexBuilder<TSegment, TValue>.Create(wildcardSegment, segmentComparer, valueComparer);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of values that matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a destination span used with <see cref="TryMatchValues(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>.</returns>
    /// <remarks>
    /// This method traverses only the branches that can match <paramref name="path"/>. When deduplication is enabled, the returned value can be larger than the final emitted value count because overlapping patterns may reach the same value.
    /// </remarks>
    public int GetMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return !_hasWildcardBranches
            ? CountExactOnly(path)
            : CountExact(path);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of prefix matching values that matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a destination span used with <see cref="TryMatchPrefixValues(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>.</returns>
    /// <remarks>
    /// This method traverses only the prefix branches that can match <paramref name="path"/>. When deduplication is enabled, the returned value can be larger than the final emitted value count because overlapping patterns may reach the same value.
    /// </remarks>
    public int GetPrefixMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountBestPrefixCandidates(path);
    }

    /// <summary>Attempts exact matching into a caller-provided result span.</summary>
    public bool TryMatch(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, prefix: false, bestPrefix: false, destination, out written);
    }

    /// <summary>Returns all exact matches as owning result values.</summary>
    public PatternMatch<TValue>[] MatchToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: false);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    /// <summary>Attempts exact value-only matching.</summary>
    public bool TryMatchValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWriteExactValueCandidates(path, destination, out written);

    /// <summary>Returns exact values without constructing result descriptors.</summary>
    public TValue[] MatchValuesToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: false);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        foreach (var candidate in candidates)
        {
            writer.AddValue(candidate.Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts best-prefix value-only matching.</summary>
    public bool TryMatchPrefixValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWritePathValues(path, prefix: true, bestPrefix: true, destination, out written);

    /// <summary>Returns values from the deepest accepted prefix level.</summary>
    public TValue[] MatchPrefixValuesToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = SelectBestPrefixCandidates(path);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        foreach (var candidate in candidates)
        {
            writer.AddValue(candidate.Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts all-prefix value-only enumeration.</summary>
    public bool TryEnumeratePrefixValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWritePathValues(path, prefix: true, bestPrefix: false, destination, out written);

    /// <summary>Returns values from every accepted prefix level.</summary>
    public TValue[] EnumeratePrefixValuesToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: true);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        foreach (var candidate in candidates)
        {
            writer.AddValue(candidate.Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts best-prefix matching into a caller-provided result span.</summary>
    public bool TryMatchPrefix(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, prefix: true, bestPrefix: true, destination, out written);
    }

    /// <summary>Returns only the deepest accepted prefix matches.</summary>
    public PatternMatch<TValue>[] MatchPrefixToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = SelectBestPrefixCandidates(path);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    /// <summary>Gets an upper bound for all-prefix enumeration result matches.</summary>
    public int GetEnumeratePrefixMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountPrefix(path);
    }

    /// <summary>Attempts all-prefix enumeration into a caller-provided result span.</summary>
    public bool TryEnumeratePrefixMatches(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, prefix: true, bestPrefix: false, destination, out written);
    }

    /// <summary>Returns every accepted prefix match, from the root outward.</summary>
    public PatternMatch<TValue>[] EnumeratePrefixMatchesToArray(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: true);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    private bool TryMatchExactOnlyDirect(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex == CompiledNode.NoNode)
        {
            written = 0;
            return true;
        }

        var values = GetValues(nodeIndex);
        if (values.Length > destination.Length)
        {
            written = 0;
            return false;
        }

        values.CopyTo(destination);
        written = values.Length;
        return true;
    }

    private int MatchDetailedExactOnlyDirect(ReadOnlySpan<TSegment> path, Span<PatternMatchDetailedSlice<TValue>> matches)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex == CompiledNode.NoNode)
        {
            return 0;
        }

        var values = GetValues(nodeIndex);
        if (values.Length > matches.Length)
        {
            throw new ArgumentException("The destination match or capture span is too small for the match result.");
        }

        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            ref readonly var detail = ref details[i];
            matches[i] = new PatternMatchDetailedSlice<TValue>(
                values[i],
                detail.RegistrationId,
                path.Length,
                CaptureStart: 0,
                CaptureCount: 0);
        }

        return values.Length;
    }

    private void CollectPrefixValues(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        if (!_hasWildcardBranches)
        {
            CollectPrefixExactOnly(path, ref writer);
        }
        else
        {
            CollectPrefix(path, ref writer);
        }
    }

    private int GetDetailedMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountExact(path);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="MatchDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountExactCaptures(path);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that best-prefix detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="MatchPrefixDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetPrefixCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountBestPrefixCaptures(path);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that all-prefix detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="EnumeratePrefixDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetEnumeratePrefixCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CountPrefixCaptures(path);
    }

    /// <summary>
    /// Matches the specified segmented path and writes detailed matches and named captures into caller-provided destination spans.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="matches">The destination span that receives detailed match descriptors.</param>
    /// <param name="captures">The destination span that receives named captures shared by the written match descriptors.</param>
    /// <param name="capturesWritten">The number of captures written to <paramref name="captures"/>.</param>
    /// <returns>The number of matches written to <paramref name="matches"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="matches"/> or <paramref name="captures"/> is too small.</exception>
    public int MatchDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int capturesWritten)
    {
        if (!TryMatchDetailed(path, matches, captures, out var matchesWritten, out capturesWritten))
        {
            throw new ArgumentException("The destination match or capture span is too small for the detailed result.");
        }

        return matchesWritten;
    }

    /// <summary>
    /// Attempts to match the specified segmented path and write detailed matches and named captures into caller-provided destination spans.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="matches">The destination span that receives detailed match descriptors when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="captures">The destination span that receives named captures when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="matchesWritten">When this method returns <see langword="true"/>, contains the number of matches written to <paramref name="matches"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <param name="capturesWritten">When this method returns <see langword="true"/>, contains the number of captures written to <paramref name="captures"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <returns><see langword="true"/> when both destination spans were large enough; otherwise, <see langword="false"/>.</returns>
    public bool TryMatchDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        return TryWritePathDetailedCandidates(path, prefix: false, bestPrefix: false, matches, captures, out matchesWritten, out capturesWritten);
    }

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a newly allocated array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    public PatternMatchDetailed<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path)
    {
        return MaterializeDetailed(CollectCandidates(path, prefix: false), path);
    }

    /// <summary>Attempts best-prefix detailed matching into caller-provided buffers.</summary>
    public bool TryMatchPrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        => TryWritePathDetailedCandidates(path, prefix: true, bestPrefix: true, matches, captures, out matchesWritten, out capturesWritten);

    /// <summary>Writes best-prefix detailed matches into caller-provided buffers.</summary>
    public int MatchPrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int capturesWritten)
    {
        if (!TryMatchPrefixDetailed(path, matches, captures, out var matchesWritten, out capturesWritten))
        {
            throw new ArgumentException("The destination match or capture span is too small for the detailed result.");
        }

        return matchesWritten;
    }

    /// <summary>Returns owning best-prefix detailed matches.</summary>
    public PatternMatchDetailed<TSegment, TValue>[] MatchPrefixDetailedToArray(ReadOnlySpan<TSegment> path)
        => MaterializeDetailed(SelectBestPrefixCandidates(path), path);

    /// <summary>Attempts all-prefix detailed enumeration into caller-provided buffers.</summary>
    public bool TryEnumeratePrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        => TryWritePathDetailedCandidates(path, prefix: true, bestPrefix: false, matches, captures, out matchesWritten, out capturesWritten);

    /// <summary>Writes all-prefix detailed matches into caller-provided buffers.</summary>
    public int EnumeratePrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int capturesWritten)
    {
        if (!TryEnumeratePrefixDetailed(path, matches, captures, out var matchesWritten, out capturesWritten))
        {
            throw new ArgumentException("The destination match or capture span is too small for the detailed result.");
        }

        return matchesWritten;
    }

    /// <summary>Returns owning all-prefix detailed matches.</summary>
    public PatternMatchDetailed<TSegment, TValue>[] EnumeratePrefixDetailedToArray(ReadOnlySpan<TSegment> path)
        => MaterializeDetailed(CollectCandidates(path, prefix: true), path);

    private bool TryMatchDetailedExactOnlyDirect(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        out int matchesWritten)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex == CompiledNode.NoNode)
        {
            matchesWritten = 0;
            return true;
        }

        var values = GetValues(nodeIndex);
        if (values.Length > matches.Length)
        {
            matchesWritten = 0;
            return false;
        }

        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            ref readonly var detail = ref details[i];
            matches[i] = new PatternMatchDetailedSlice<TValue>(
                values[i],
                detail.RegistrationId,
                path.Length,
                CaptureStart: 0,
                CaptureCount: 0);
        }

        matchesWritten = values.Length;
        return true;
    }

    private PatternMatchDetailed<TSegment, TValue>[] MatchDetailedExactOnlyToArray(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex == CompiledNode.NoNode)
        {
            return [];
        }

        var values = GetValues(nodeIndex);
        if (values.IsEmpty)
        {
            return [];
        }

        var details = GetValueDetails(nodeIndex);
        var results = new PatternMatchDetailed<TSegment, TValue>[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            ref readonly var detail = ref details[i];
            results[i] = new PatternMatchDetailed<TSegment, TValue>(
                values[i],
                detail.RegistrationId,
                path.Length,
                []);
        }

        return results;
    }

    /// <summary>
    /// Explains matching behavior for the specified segmented path using diagnostics-oriented allocation-friendly results.
    /// </summary>
    /// <param name="path">The segmented input path to explain.</param>
    /// <param name="options">Options controlling optional diagnostic work.</param>
    /// <returns>An explanation containing accepted detailed matches and optional rejected-candidate diagnostics.</returns>
    /// <remarks>
    /// This method intentionally composes detailed matching and optional diagnostics. Use <see cref="TryMatchValues(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>, or <see cref="MatchToArray(ReadOnlySpan{TSegment})"/> for hot paths.
    /// </remarks>
    public PatternMatchExplanation<TSegment, TValue> Explain(
        ReadOnlySpan<TSegment> path,
        PatternExplanationOptions options = default)
    {
        var pathCopy = path.ToArray();
        var matchUpperBound = GetDetailedMatchCountUpperBound(path);
        var captureUpperBound = GetCaptureCountUpperBound(path);
        var matches = MatchDetailedToArray(path);
        var rejectedCandidates = options.IncludeRejectedCandidates
            ? CollectRejectedCandidates(path)
            : [];

        return new PatternMatchExplanation<TSegment, TValue>(
            pathCopy,
            matches,
            rejectedCandidates,
            Options,
            options,
            matchUpperBound,
            captureUpperBound);
    }

    private PatternRejectedCandidate[] CollectRejectedCandidates(ReadOnlySpan<TSegment> path)
    {
        var rejectedCandidates = new List<PatternRejectedCandidate>();

        if (!_hasWildcardBranches)
        {
            CollectExactOnlyRejections(path, rejectedCandidates);
            return rejectedCandidates.ToArray();
        }

        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                ref readonly var node = ref _nodes[frame.NodeIndex];

                if (frame.Depth == path.Length)
                {
                    if (GetValueCountIncludingTerminalCatchAll(frame.NodeIndex) == 0)
                    {
                        rejectedCandidates.Add(new PatternRejectedCandidate(
                            frame.Depth,
                            PatternRejectedCandidateReasonKind.PathTooShort,
                            "The input ended before this branch reached a terminal registration."));
                    }

                    continue;
                }

                var hadCandidate = false;

                if (node.CatchAllChild != CompiledNode.NoNode)
                {
                    hadCandidate = true;
                    stack.Push(new TraversalFrame(node.CatchAllChild, path.Length));
                }

                if (node.WildcardChild != CompiledNode.NoNode)
                {
                    hadCandidate = true;
                    stack.Push(new TraversalFrame(node.WildcardChild, frame.Depth + 1));
                }

                if (TryGetExactChild(frame.NodeIndex, path[frame.Depth], out var exactChildNodeIndex))
                {
                    hadCandidate = true;
                    stack.Push(new TraversalFrame(exactChildNodeIndex, frame.Depth + 1));
                }

                if (!hadCandidate)
                {
                    rejectedCandidates.Add(new PatternRejectedCandidate(
                        frame.Depth,
                        PatternRejectedCandidateReasonKind.BranchNotMatched,
                        "No literal, wildcard, or catch-all branch matched this input segment."));
                }
            }
        }
        finally
        {
            stack.Dispose();
        }

        return rejectedCandidates.ToArray();
    }

    private void CollectExactOnlyRejections(
        ReadOnlySpan<TSegment> path,
        List<PatternRejectedCandidate> rejectedCandidates)
    {
        var nodeIndex = 0;

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                rejectedCandidates.Add(new PatternRejectedCandidate(
                    depth,
                    PatternRejectedCandidateReasonKind.LiteralMismatch,
                    "No literal branch matched this input segment."));
                return;
            }

            nodeIndex = childNodeIndex;
        }

        if (GetValues(nodeIndex).IsEmpty)
        {
            rejectedCandidates.Add(new PatternRejectedCandidate(
                path.Length,
                PatternRejectedCandidateReasonKind.PathTooShort,
                "The input ended at a compiled node that has no terminal registration."));
        }
    }

    private ReadOnlySpan<TValue> GetValues(int nodeIndex)
    {
        ref readonly var node = ref _nodes[nodeIndex];
        return _values.AsSpan(node.FirstValue, node.ValueCount);
    }

    private ReadOnlySpan<CompiledValueDetail> GetValueDetails(int nodeIndex)
    {
        ref readonly var node = ref _nodes[nodeIndex];
        return _valueDetails.AsSpan(node.FirstValue, node.ValueCount);
    }

    private int GetCaptureCount(int nodeIndex, int pathLength)
    {
        var count = 0;
        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < details.Length; i++)
        {
            ref readonly var detail = ref details[i];
            count += detail.CaptureCount;
        }

        return count;
    }

    private int GetValueCountIncludingTerminalCatchAll(int nodeIndex)
    {
        var count = GetValues(nodeIndex).Length;
        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetValues(node.CatchAllChild).Length;
        }

        return count;
    }

    private int GetCaptureCountIncludingTerminalCatchAll(int nodeIndex, int pathLength)
    {
        var count = GetCaptureCount(nodeIndex, pathLength);
        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            count += GetCaptureCount(node.CatchAllChild, pathLength);
        }

        return count;
    }

    private void AddValuesIncludingTerminalCatchAll(int nodeIndex, MatchAccumulator<TValue> accumulator)
    {
        accumulator.Add(GetValues(nodeIndex), _deduplicateValues);
        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            accumulator.Add(GetValues(node.CatchAllChild), _deduplicateValues);
        }
    }

    private void AddValuesIncludingTerminalCatchAll(int nodeIndex, ref SpanMatchWriter<TValue> writer)
    {
        writer.Add(GetValues(nodeIndex), _deduplicateValues);
        if (!writer.Succeeded)
        {
            return;
        }

        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            writer.Add(GetValues(node.CatchAllChild), _deduplicateValues);
        }
    }

    private void AddDetailedValuesIncludingTerminalCatchAll(
        int nodeIndex,
        ref DetailedMatchWriter<TSegment, TValue> writer,
        int consumedSegmentCount)
    {
        AddDetailedValues(nodeIndex, ref writer, consumedSegmentCount);
        if (!writer.Succeeded)
        {
            return;
        }

        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            AddDetailedValues(node.CatchAllChild, ref writer, consumedSegmentCount);
        }
    }

    private bool TryGetExactChild(int nodeIndex, TSegment segment, out int childNodeIndex)
    {
        ref readonly var node = ref _nodes[nodeIndex];

        if (node.HasLookup)
        {
            return TryGetExactChildFromLookup(in node, segment, out childNodeIndex);
        }

        return TryGetExactChildByLinearScan(in node, segment, out childNodeIndex);
    }

    private bool TryGetExactChildFromLookup(in CompiledNode node, TSegment segment, out int childNodeIndex)
    {
        var slotOffset = _segmentComparer.GetHashCode(segment) & node.LookupMask;

        for (var probeCount = 0; probeCount <= node.LookupMask; probeCount++)
        {
            var childIndex = _childLookupSlots[node.FirstLookupSlot + slotOffset];
            if (childIndex == CompiledNode.NoNode)
            {
                break;
            }

            ref readonly var child = ref _children[childIndex];
            if (_segmentComparer.Equals(child.Segment, segment))
            {
                childNodeIndex = child.NodeIndex;
                return true;
            }

            slotOffset = (slotOffset + 1) & node.LookupMask;
        }

        childNodeIndex = CompiledNode.NoNode;
        return false;
    }

    private bool TryGetExactChildByLinearScan(in CompiledNode node, TSegment segment, out int childNodeIndex)
    {
        var end = node.FirstChild + node.ChildCount;

        for (var i = node.FirstChild; i < end; i++)
        {
            ref readonly var child = ref _children[i];
            if (!_segmentComparer.Equals(child.Segment, segment))
            {
                continue;
            }

            childNodeIndex = child.NodeIndex;
            return true;
        }

        childNodeIndex = CompiledNode.NoNode;
        return false;
    }

    private int TryDescendExactOnly(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = 0;

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return CompiledNode.NoNode;
            }

            nodeIndex = childNodeIndex;
        }

        return nodeIndex;
    }

    private int CountExactOnlyCaptures(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = TryDescendExactOnly(path);
        return nodeIndex == CompiledNode.NoNode ? 0 : GetCaptureCount(nodeIndex, path.Length);
    }

    private int CountPrefixExactOnlyCaptures(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = 0;
        var count = GetCaptureCount(nodeIndex, path.Length);

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return count;
            }

            nodeIndex = childNodeIndex;
            count += GetCaptureCount(nodeIndex, path.Length);
        }

        return count;
    }

    private int CountExactOnly(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = TryDescendExactOnly(path);
        return nodeIndex == CompiledNode.NoNode ? 0 : GetValues(nodeIndex).Length;
    }

    private int CountPrefixExactOnly(ReadOnlySpan<TSegment> path)
    {
        var nodeIndex = 0;
        var count = GetValues(nodeIndex).Length;

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return count;
            }

            nodeIndex = childNodeIndex;
            count += GetValues(nodeIndex).Length;
        }

        return count;
    }

    private void CollectExactOnly(ReadOnlySpan<TSegment> path, MatchAccumulator<TValue> accumulator)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex != CompiledNode.NoNode)
        {
            accumulator.Add(GetValues(nodeIndex), _deduplicateValues);
        }
    }

    private void CollectPrefixExactOnly(ReadOnlySpan<TSegment> path, MatchAccumulator<TValue> accumulator)
    {
        var nodeIndex = 0;
        accumulator.Add(GetValues(nodeIndex), _deduplicateValues);

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return;
            }

            nodeIndex = childNodeIndex;
            accumulator.Add(GetValues(nodeIndex), _deduplicateValues);
        }
    }

    private void CollectExactOnly(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex != CompiledNode.NoNode)
        {
            writer.Add(GetValues(nodeIndex), _deduplicateValues);
        }
    }

    private void CollectPrefixExactOnly(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        var nodeIndex = 0;
        writer.Add(GetValues(nodeIndex), _deduplicateValues);
        if (!writer.Succeeded)
        {
            return;
        }

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return;
            }

            nodeIndex = childNodeIndex;
            writer.Add(GetValues(nodeIndex), _deduplicateValues);
            if (!writer.Succeeded)
            {
                return;
            }
        }
    }

    private int CountExactCaptures(ReadOnlySpan<TSegment> path)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    count += GetCaptureCountIncludingTerminalCatchAll(frame.NodeIndex, path.Length);
                    continue;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    private int CountPrefixCaptures(ReadOnlySpan<TSegment> path)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    count += GetCaptureCountIncludingTerminalCatchAll(frame.NodeIndex, path.Length);
                    continue;
                }

                count += GetCaptureCount(frame.NodeIndex, path.Length);
                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    private void AddDetailedValues(int nodeIndex, ref DetailedMatchWriter<TSegment, TValue> writer, int consumedSegmentCount)
    {
        writer.Add(GetValues(nodeIndex), GetValueDetails(nodeIndex), _captureDescriptors, consumedSegmentCount);
    }

    private void CollectDetailedExactOnly(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex != CompiledNode.NoNode)
        {
            AddDetailedValues(nodeIndex, ref writer, path.Length);
        }
    }

    private void CollectPrefixDetailedExactOnly(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        var nodeIndex = 0;
        AddDetailedValues(nodeIndex, ref writer, 0);
        if (!writer.Succeeded)
        {
            return;
        }

        for (var depth = 0; depth < path.Length; depth++)
        {
            if (!TryGetExactChild(nodeIndex, path[depth], out var childNodeIndex))
            {
                return;
            }

            nodeIndex = childNodeIndex;
            AddDetailedValues(nodeIndex, ref writer, depth + 1);
            if (!writer.Succeeded)
            {
                return;
            }
        }
    }

    private void CollectDetailedWildcard(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    AddDetailedValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer, path.Length);
                    if (!writer.Succeeded)
                    {
                        return;
                    }

                    continue;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private void CollectPrefixDetailed(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    AddDetailedValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer, path.Length);
                    if (!writer.Succeeded)
                    {
                        return;
                    }

                    continue;
                }

                AddDetailedValues(frame.NodeIndex, ref writer, frame.Depth);
                if (!writer.Succeeded)
                {
                    return;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private int CountExact(ReadOnlySpan<TSegment> path)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    count += GetValueCountIncludingTerminalCatchAll(frame.NodeIndex);
                    continue;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    private int CountPrefix(ReadOnlySpan<TSegment> path)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    count += GetValueCountIncludingTerminalCatchAll(frame.NodeIndex);
                    continue;
                }

                count += GetValues(frame.NodeIndex).Length;
                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    private int CountBestPrefixCandidates(ReadOnlySpan<TSegment> path)
    {
        var capacity = CountPrefix(path);
        if (capacity == 0)
        {
            return 0;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix: true, candidates);
            SortCandidates(candidates[..count], prefix: true);
            return SelectDeepestPrefixCandidates(candidates[..count]).Length;
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private int CountBestPrefixCaptures(ReadOnlySpan<TSegment> path)
    {
        var capacity = CountPrefix(path);
        if (capacity == 0)
        {
            return 0;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix: true, candidates);
            SortCandidates(candidates[..count], prefix: true);
            var selected = SelectDeepestPrefixCandidates(candidates[..count]);
            var captureCount = 0;
            for (var i = 0; i < selected.Length; i++)
            {
                captureCount += selected[i].Detail.CaptureCount;
            }

            return captureCount;
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private static ReadOnlySpan<MatchCandidate<TValue>> SelectDeepestPrefixCandidates(
        Span<MatchCandidate<TValue>> candidates)
    {
        if (candidates.IsEmpty)
        {
            return ReadOnlySpan<MatchCandidate<TValue>>.Empty;
        }

        var deepestDepth = candidates[^1].ConsumedSegmentCount;
        var first = candidates.Length - 1;
        while (first > 0 && candidates[first - 1].ConsumedSegmentCount == deepestDepth)
        {
            first--;
        }

        return candidates[first..];
    }

    private void CollectExact(ReadOnlySpan<TSegment> path, MatchAccumulator<TValue> accumulator)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    AddValuesIncludingTerminalCatchAll(frame.NodeIndex, accumulator);
                    continue;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private void CollectPrefix(ReadOnlySpan<TSegment> path, MatchAccumulator<TValue> accumulator)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    AddValuesIncludingTerminalCatchAll(frame.NodeIndex, accumulator);
                    continue;
                }

                accumulator.Add(GetValues(frame.NodeIndex), _deduplicateValues);
                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private void CollectExact(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();

                if (frame.Depth == path.Length)
                {
                    AddValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer);
                    if (!writer.Succeeded)
                    {
                        return;
                    }

                    continue;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private void CollectPrefix(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    AddValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer);
                    if (!writer.Succeeded)
                    {
                        return;
                    }

                    continue;
                }

                writer.Add(GetValues(frame.NodeIndex), _deduplicateValues);
                if (!writer.Succeeded)
                {
                    return;
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }
    }

    private void PushMatchingChildren(ref TraversalStack stack, int nodeIndex, TSegment segment, int nextDepth, int terminalDepth)
    {
        ref readonly var node = ref _nodes[nodeIndex];

        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            stack.Push(new TraversalFrame(node.CatchAllChild, terminalDepth));
        }

        if (node.WildcardChild != CompiledNode.NoNode)
        {
            stack.Push(new TraversalFrame(node.WildcardChild, nextDepth));
        }

        if (TryGetExactChild(nodeIndex, segment, out var exactChildNodeIndex))
        {
            stack.Push(new TraversalFrame(exactChildNodeIndex, nextDepth));
        }
    }

    private List<MatchCandidate<TValue>> SelectBestPrefixCandidates(ReadOnlySpan<TSegment> path)
    {
        var candidates = CollectCandidates(path, prefix: true);
        if (candidates.Count == 0)
        {
            return candidates;
        }

        var depth = candidates.Max(candidate => candidate.ConsumedSegmentCount);
        candidates.RemoveAll(candidate => candidate.ConsumedSegmentCount != depth);
        return candidates;
    }

    private List<MatchCandidate<TValue>> CollectCandidates(ReadOnlySpan<TSegment> path, bool prefix)
    {
        var candidates = new List<MatchCandidate<TValue>>();
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    AddCandidates(frame.NodeIndex, frame.Depth, path.Length, candidates);
                    ref readonly var node = ref _nodes[frame.NodeIndex];
                    if (node.CatchAllChild != CompiledNode.NoNode)
                    {
                        AddCandidates(node.CatchAllChild, frame.Depth, path.Length, candidates);
                    }

                    continue;
                }

                if (prefix)
                {
                    AddCandidates(frame.NodeIndex, frame.Depth, path.Length, candidates);
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        candidates.Sort((left, right) => CompareCandidates(left, right, prefix));

        return candidates;
    }

    private void AddCandidates(int nodeIndex, int depth, int pathLength, List<MatchCandidate<TValue>> candidates)
    {
        var values = GetValues(nodeIndex);
        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            candidates.Add(new MatchCandidate<TValue>(
                values[i],
                details[i],
                depth,
                details[i].TerminalCatchAllSegmentIndex == pathLength));
        }
    }

    private bool TryWriteCandidates(
        List<MatchCandidate<TValue>> candidates,
        Span<PatternMatch<TValue>> destination,
        out int written)
    {
        if (candidates.Count == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<PatternMatch<TValue>>.Shared.Rent(candidates.Count);
        try
        {
            var temporary = rented.AsSpan(0, candidates.Count);
            var writer = new ResultMatchWriter<TValue>(temporary, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: false);
            WriteCandidates(candidates, ref writer);
            if (!writer.Succeeded || writer.Count > destination.Length)
            {
                written = 0;
                return false;
            }

            temporary[..writer.Count].CopyTo(destination);
            written = writer.Count;
            return true;
        }
        finally
        {
            ArrayPool<PatternMatch<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWriteCandidates(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        Span<PatternMatch<TValue>> destination,
        out int written)
    {
        if (candidates.IsEmpty)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<PatternMatch<TValue>>.Shared.Rent(candidates.Length);
        try
        {
            var temporary = rented.AsSpan(0, candidates.Length);
            var writer = new ResultMatchWriter<TValue>(temporary, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: false);
            WriteCandidates(candidates, ref writer);
            if (!writer.Succeeded || writer.Count > destination.Length)
            {
                written = 0;
                return false;
            }

            temporary[..writer.Count].CopyTo(destination);
            written = writer.Count;
            return true;
        }
        finally
        {
            ArrayPool<PatternMatch<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWriteValueCandidates(
        List<MatchCandidate<TValue>> candidates,
        Span<TValue> destination,
        out int written)
    {
        if (candidates.Count == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<TValue>.Shared.Rent(candidates.Count);
        try
        {
            var temporary = rented.AsSpan(0, candidates.Count);
            var writer = new SpanMatchWriter<TValue>(temporary, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: false);
            foreach (var candidate in candidates)
            {
                writer.AddValue(candidate.Value);
                if (!writer.Succeeded)
                {
                    written = 0;
                    return false;
                }
            }

            if (writer.Count > destination.Length)
            {
                written = 0;
                return false;
            }

            temporary[..writer.Count].CopyTo(destination);
            written = writer.Count;
            return true;
        }
        finally
        {
            ArrayPool<TValue>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWriteValueCandidates(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        Span<TValue> destination,
        out int written)
    {
        if (candidates.IsEmpty)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<TValue>.Shared.Rent(candidates.Length);
        try
        {
            var temporary = rented.AsSpan(0, candidates.Length);
            var writer = new SpanMatchWriter<TValue>(temporary, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: false);
            for (var i = 0; i < candidates.Length; i++)
            {
                writer.AddValue(candidates[i].Value);
                if (!writer.Succeeded)
                {
                    written = 0;
                    return false;
                }
            }

            if (writer.Count > destination.Length)
            {
                written = 0;
                return false;
            }

            temporary[..writer.Count].CopyTo(destination);
            written = writer.Count;
            return true;
        }
        finally
        {
            ArrayPool<TValue>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWritePathCandidates(
        ReadOnlySpan<TSegment> path,
        bool prefix,
        bool bestPrefix,
        Span<PatternMatch<TValue>> destination,
        out int written)
    {
        var capacity = prefix ? CountPrefix(path) : CountExact(path);
        if (capacity == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix, candidates);
            SortCandidates(candidates[..count], prefix);
            var selected = bestPrefix ? SelectDeepestPrefixCandidates(candidates[..count]) : candidates[..count];
            return TryWriteCandidates(selected, destination, out written);
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWritePathValues(
        ReadOnlySpan<TSegment> path,
        bool prefix,
        bool bestPrefix,
        Span<TValue> destination,
        out int written)
    {
        var capacity = prefix ? CountPrefix(path) : CountExact(path);
        if (capacity == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix, candidates);
            SortCandidates(candidates[..count], prefix);
            var selected = bestPrefix ? SelectDeepestPrefixCandidates(candidates[..count]) : candidates[..count];
            return TryWriteValueCandidates(selected, destination, out written);
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool TryWriteExactValueCandidates(
        ReadOnlySpan<TSegment> path,
        Span<TValue> destination,
        out int written)
    {
        var capacity = CountExact(path);
        if (capacity == 0)
        {
            written = 0;
            return true;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix: false, candidates);
            SortCandidates(candidates[..count], prefix: false);
            return TryWriteValueCandidates(candidates[..count], destination, out written);
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private static void WriteCandidates(
        List<MatchCandidate<TValue>> candidates,
        ref ResultMatchWriter<TValue> writer)
    {
        foreach (var candidate in candidates)
        {
            writer.Add(candidate.Value, candidate.Detail, candidate.ConsumedSegmentCount);
            if (!writer.Succeeded)
            {
                return;
            }
        }
    }

    private static void WriteCandidates(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        ref ResultMatchWriter<TValue> writer)
    {
        for (var i = 0; i < candidates.Length; i++)
        {
            var candidate = candidates[i];
            writer.Add(candidate.Value, candidate.Detail, candidate.ConsumedSegmentCount);
            if (!writer.Succeeded)
            {
                return;
            }
        }
    }

    private int CollectCandidatesInto(
        ReadOnlySpan<TSegment> path,
        bool prefix,
        Span<MatchCandidate<TValue>> destination)
    {
        var count = 0;
        Span<TraversalFrame> initialFrames = stackalloc TraversalFrame[64];
        var stack = new TraversalStack(initialFrames);
        stack.Push(new TraversalFrame(0, 0));

        try
        {
            while (!stack.IsEmpty)
            {
                var frame = stack.Pop();
                if (frame.Depth == path.Length)
                {
                    count = AddCandidates(frame.NodeIndex, frame.Depth, path.Length, destination, count);
                    ref readonly var node = ref _nodes[frame.NodeIndex];
                    if (node.CatchAllChild != CompiledNode.NoNode)
                    {
                        count = AddCandidates(node.CatchAllChild, frame.Depth, path.Length, destination, count);
                    }

                    continue;
                }

                if (prefix)
                {
                    count = AddCandidates(frame.NodeIndex, frame.Depth, path.Length, destination, count);
                }

                PushMatchingChildren(ref stack, frame.NodeIndex, path[frame.Depth], frame.Depth + 1, path.Length);
            }
        }
        finally
        {
            stack.Dispose();
        }

        return count;
    }

    private int AddCandidates(
        int nodeIndex,
        int depth,
        int pathLength,
        Span<MatchCandidate<TValue>> destination,
        int count)
    {
        var values = GetValues(nodeIndex);
        var details = GetValueDetails(nodeIndex);
        for (var i = 0; i < values.Length; i++)
        {
            destination[count++] = new MatchCandidate<TValue>(
                values[i],
                details[i],
                depth,
                details[i].TerminalCatchAllSegmentIndex == pathLength);
        }

        return count;
    }

    private static void SortCandidates(Span<MatchCandidate<TValue>> candidates, bool prefix)
    {
        for (var i = 1; i < candidates.Length; i++)
        {
            var candidate = candidates[i];
            var j = i - 1;
            while (j >= 0 && CompareCandidates(candidate, candidates[j], prefix) < 0)
            {
                candidates[j + 1] = candidates[j];
                j--;
            }

            candidates[j + 1] = candidate;
        }
    }

    private static int CompareCandidates(
        MatchCandidate<TValue> left,
        MatchCandidate<TValue> right,
        bool prefix)
    {
        if (prefix)
        {
            var depth = left.ConsumedSegmentCount.CompareTo(right.ConsumedSegmentCount);
            if (depth != 0)
            {
                return depth;
            }
        }

        if (left.IsZeroLengthCatchAll != right.IsZeroLengthCatchAll)
        {
            return left.IsZeroLengthCatchAll ? 1 : -1;
        }

        var score = right.Detail.Score.CompareTo(left.Detail.Score);
        return score != 0 ? score : left.Detail.RegistrationOrder.CompareTo(right.Detail.RegistrationOrder);
    }

    private bool TryWriteDetailedCandidates(
        List<MatchCandidate<TValue>> candidates,
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        var captureUpperBound = 0;
        foreach (var candidate in candidates)
        {
            captureUpperBound += candidate.Detail.CaptureCount;
        }

        var rentedMatches = ArrayPool<PatternMatchDetailedSlice<TValue>>.Shared.Rent(Math.Max(1, candidates.Count));
        var rentedCaptures = ArrayPool<PatternCaptureSlice<TSegment>>.Shared.Rent(Math.Max(1, captureUpperBound));
        try
        {
            var temporaryMatches = rentedMatches.AsSpan(0, candidates.Count);
            var temporaryCaptures = rentedCaptures.AsSpan(0, captureUpperBound);
            var matchCount = 0;
            var captureCount = 0;

            foreach (var candidate in candidates)
            {
                if (_deduplicateValues && ContainsValue(temporaryMatches[..matchCount], candidate.Value))
                {
                    continue;
                }

                var detail = candidate.Detail;
                var captureStart = captureCount;
                for (var i = 0; i < detail.CaptureCount; i++)
                {
                    var descriptor = _captureDescriptors[detail.FirstCapture + i];
                    var segmentCount = descriptor.IsCatchAll
                        ? Math.Max(0, path.Length - descriptor.SegmentIndex)
                        : 1;
                    temporaryCaptures[captureCount++] = new PatternCaptureSlice<TSegment>(
                        descriptor.Name,
                        descriptor.SegmentIndex,
                        segmentCount);
                }

                temporaryMatches[matchCount++] = new PatternMatchDetailedSlice<TValue>(
                    candidate.Value,
                    detail.RegistrationId,
                    candidate.ConsumedSegmentCount,
                    captureStart,
                    detail.CaptureCount);
            }

            if (matchCount > matches.Length || captureCount > captures.Length)
            {
                matchesWritten = 0;
                capturesWritten = 0;
                return false;
            }

            temporaryMatches[..matchCount].CopyTo(matches);
            temporaryCaptures[..captureCount].CopyTo(captures);
            matchesWritten = matchCount;
            capturesWritten = captureCount;
            return true;
        }
        finally
        {
            ArrayPool<PatternMatchDetailedSlice<TValue>>.Shared.Return(rentedMatches, clearArray: true);
            ArrayPool<PatternCaptureSlice<TSegment>>.Shared.Return(rentedCaptures, clearArray: true);
        }
    }

    private bool TryWriteDetailedCandidates(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        var captureUpperBound = 0;
        for (var i = 0; i < candidates.Length; i++)
        {
            captureUpperBound += candidates[i].Detail.CaptureCount;
        }

        var rentedMatches = ArrayPool<PatternMatchDetailedSlice<TValue>>.Shared.Rent(Math.Max(1, candidates.Length));
        var rentedCaptures = ArrayPool<PatternCaptureSlice<TSegment>>.Shared.Rent(Math.Max(1, captureUpperBound));
        try
        {
            var temporaryMatches = rentedMatches.AsSpan(0, candidates.Length);
            var temporaryCaptures = rentedCaptures.AsSpan(0, captureUpperBound);
            var matchCount = 0;
            var captureCount = 0;

            for (var candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
            {
                var candidate = candidates[candidateIndex];
                if (_deduplicateValues && ContainsValue(temporaryMatches[..matchCount], candidate.Value))
                {
                    continue;
                }

                var detail = candidate.Detail;
                var captureStart = captureCount;
                for (var i = 0; i < detail.CaptureCount; i++)
                {
                    var descriptor = _captureDescriptors[detail.FirstCapture + i];
                    var segmentCount = descriptor.IsCatchAll
                        ? Math.Max(0, path.Length - descriptor.SegmentIndex)
                        : 1;
                    temporaryCaptures[captureCount++] = new PatternCaptureSlice<TSegment>(
                        descriptor.Name,
                        descriptor.SegmentIndex,
                        segmentCount);
                }

                temporaryMatches[matchCount++] = new PatternMatchDetailedSlice<TValue>(
                    candidate.Value,
                    detail.RegistrationId,
                    candidate.ConsumedSegmentCount,
                    captureStart,
                    detail.CaptureCount);
            }

            if (matchCount > matches.Length || captureCount > captures.Length)
            {
                matchesWritten = 0;
                capturesWritten = 0;
                return false;
            }

            temporaryMatches[..matchCount].CopyTo(matches);
            temporaryCaptures[..captureCount].CopyTo(captures);
            matchesWritten = matchCount;
            capturesWritten = captureCount;
            return true;
        }
        finally
        {
            ArrayPool<PatternMatchDetailedSlice<TValue>>.Shared.Return(rentedMatches, clearArray: true);
            ArrayPool<PatternCaptureSlice<TSegment>>.Shared.Return(rentedCaptures, clearArray: true);
        }
    }

    private bool TryWritePathDetailedCandidates(
        ReadOnlySpan<TSegment> path,
        bool prefix,
        bool bestPrefix,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        var capacity = prefix ? CountPrefix(path) : CountExact(path);
        if (capacity == 0)
        {
            matchesWritten = 0;
            capturesWritten = 0;
            return true;
        }

        var rented = ArrayPool<MatchCandidate<TValue>>.Shared.Rent(capacity);
        try
        {
            var candidates = rented.AsSpan(0, capacity);
            var count = CollectCandidatesInto(path, prefix, candidates);
            SortCandidates(candidates[..count], prefix);
            var selected = bestPrefix ? SelectDeepestPrefixCandidates(candidates[..count]) : candidates[..count];
            return TryWriteDetailedCandidates(selected, path, matches, captures, out matchesWritten, out capturesWritten);
        }
        finally
        {
            ArrayPool<MatchCandidate<TValue>>.Shared.Return(rented, clearArray: true);
        }
    }

    private bool ContainsValue(ReadOnlySpan<PatternMatchDetailedSlice<TValue>> matches, TValue value)
    {
        for (var i = 0; i < matches.Length; i++)
        {
            if (_valueComparer.Equals(matches[i].Value, value))
            {
                return true;
            }
        }

        return false;
    }

    private PatternMatchDetailed<TSegment, TValue>[] MaterializeDetailed(
        List<MatchCandidate<TValue>> candidates,
        ReadOnlySpan<TSegment> path)
    {
        if (candidates.Count == 0)
        {
            return [];
        }

        var matches = new PatternMatchDetailedSlice<TValue>[candidates.Count];
        var captureUpperBound = candidates.Sum(candidate => candidate.Detail.CaptureCount);
        var captures = new PatternCaptureSlice<TSegment>[captureUpperBound];
        if (!TryWriteDetailedCandidates(candidates, path, matches, captures, out var matchCount, out var captureCount))
        {
            throw new InvalidOperationException("The internal detailed result buffer was unexpectedly too small.");
        }

        var results = new PatternMatchDetailed<TSegment, TValue>[matchCount];
        for (var i = 0; i < matchCount; i++)
        {
            var match = matches[i];
            var owningCaptures = new PatternCapture<TSegment>[match.CaptureCount];
            for (var captureIndex = 0; captureIndex < match.CaptureCount; captureIndex++)
            {
                var capture = captures[match.CaptureStart + captureIndex];
                owningCaptures[captureIndex] = new PatternCapture<TSegment>(
                    capture.Name,
                    ImmutableArray.Create(path.Slice(capture.StartSegmentIndex, capture.SegmentCount)),
                    capture.StartSegmentIndex);
            }

            results[i] = new PatternMatchDetailed<TSegment, TValue>(
                match.Value,
                match.RegistrationId,
                match.ConsumedSegmentCount,
                [.. owningCaptures]);
        }

        _ = captureCount;
        return results;
    }

    private readonly struct TraversalFrame
    {
        internal TraversalFrame(int nodeIndex, int depth)
        {
            NodeIndex = nodeIndex;
            Depth = depth;
        }

        internal int NodeIndex { get; }

        internal int Depth { get; }
    }

    private ref struct TraversalStack
    {
        private Span<TraversalFrame> _items;
        private TraversalFrame[]? _rentedItems;
        private int _count;

        internal TraversalStack(Span<TraversalFrame> initialItems)
        {
            _items = initialItems;
            _rentedItems = null;
            _count = 0;
        }

        internal readonly bool IsEmpty => _count == 0;

        internal void Push(TraversalFrame frame)
        {
            if (_count == _items.Length)
            {
                Grow();
            }

            _items[_count] = frame;
            _count++;
        }

        internal TraversalFrame Pop()
        {
            _count--;
            return _items[_count];
        }

        internal void Dispose()
        {
            if (_rentedItems is not null)
            {
                ArrayPool<TraversalFrame>.Shared.Return(_rentedItems);
                _rentedItems = null;
            }

            _items = default;
            _count = 0;
        }

        private void Grow()
        {
            var newLength = _items.Length == 0 ? 4 : _items.Length * 2;
            var rentedItems = ArrayPool<TraversalFrame>.Shared.Rent(newLength);
            _items[.._count].CopyTo(rentedItems);

            if (_rentedItems is not null)
            {
                ArrayPool<TraversalFrame>.Shared.Return(_rentedItems);
            }

            _items = rentedItems;
            _rentedItems = rentedItems;
        }
    }
}
