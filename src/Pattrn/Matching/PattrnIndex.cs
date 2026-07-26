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
        => PattrnIndexCompiler<TSegment, TValue>.CompileWithDiagnostics(
            registrations,
            options,
            matchOptions,
            segmentComparer,
            valueComparer);

    private readonly CompiledIndex<TSegment, TValue> _storage;
    private readonly CaptureDescriptor[] _captureDescriptors;
    private readonly IEqualityComparer<TSegment> _segmentComparer;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _deduplicateValues;

    internal PattrnIndex(
        CompiledIndex<TSegment, TValue> index,
        int patternCount,
        int registrationCount,
        MatchOptions options,
        IEqualityComparer<TSegment> segmentComparer,
        IEqualityComparer<TValue> valueComparer)
    {
        _storage = index;
        _captureDescriptors = index.CaptureDescriptors;
        _segmentComparer = segmentComparer;
        _valueComparer = valueComparer;
        PatternCount = patternCount;
        RegistrationCount = registrationCount;
        MatchCountUpperBound = registrationCount;
        Options = options;
        _deduplicateValues = options.DeduplicateValues;
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
        => CreateTraversal().CountCandidates(path, CandidateTraversalKind.Exact);

    /// <summary>
    /// Gets a path-specific upper bound for the number of prefix matching values that matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a destination span used with <see cref="TryMatchPrefixValues(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>.</returns>
    /// <remarks>
    /// This method traverses only the prefix branches that can match <paramref name="path"/>. When deduplication is enabled, the returned value can be larger than the final emitted value count because overlapping patterns may reach the same value.
    /// </remarks>
    public int GetPrefixMatchCountUpperBound(ReadOnlySpan<TSegment> path)
        => CreateTraversal().CountBestPrefixCandidates(path);

    /// <summary>Attempts exact matching into a caller-provided result span.</summary>
    public bool TryMatch(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, CandidateTraversalKind.Exact, bestPrefix: false, destination, out written);
    }

    /// <summary>Returns all exact matches as owning result values.</summary>
    public PatternMatch<TValue>[] MatchToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.Exact,
            bestPrefix: false);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates.Candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    /// <summary>Attempts exact value-only matching.</summary>
    public bool TryMatchValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWritePathValues(path, CandidateTraversalKind.Exact, bestPrefix: false, destination, out written);

    /// <summary>Returns exact values without constructing result descriptors.</summary>
    public TValue[] MatchValuesToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.Exact,
            bestPrefix: false);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        var selected = candidates.Candidates;
        for (var i = 0; i < selected.Length; i++)
        {
            writer.AddValue(selected[i].Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts best-prefix value-only matching.</summary>
    public bool TryMatchPrefixValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWritePathValues(path, CandidateTraversalKind.AllPrefix, bestPrefix: true, destination, out written);

    /// <summary>Returns values from the deepest accepted prefix level.</summary>
    public TValue[] MatchPrefixValuesToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: true);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        var selected = candidates.Candidates;
        for (var i = 0; i < selected.Length; i++)
        {
            writer.AddValue(selected[i].Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts all-prefix value-only enumeration.</summary>
    public bool TryEnumeratePrefixValues(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
        => TryWritePathValues(path, CandidateTraversalKind.AllPrefix, bestPrefix: false, destination, out written);

    /// <summary>Returns values from every accepted prefix level.</summary>
    public TValue[] EnumeratePrefixValuesToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: false);
        var values = new TValue[candidates.Count];
        var writer = new SpanMatchWriter<TValue>(values, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        var selected = candidates.Candidates;
        for (var i = 0; i < selected.Length; i++)
        {
            writer.AddValue(selected[i].Value);
        }

        return writer.Count == values.Length ? values : values[..writer.Count];
    }

    /// <summary>Attempts best-prefix matching into a caller-provided result span.</summary>
    public bool TryMatchPrefix(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: true, destination, out written);
    }

    /// <summary>Returns only the deepest accepted prefix matches.</summary>
    public PatternMatch<TValue>[] MatchPrefixToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: true);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates.Candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    /// <summary>Gets an upper bound for all-prefix enumeration result matches.</summary>
    public int GetEnumeratePrefixMatchCountUpperBound(ReadOnlySpan<TSegment> path)
        => CreateTraversal().CountCandidates(path, CandidateTraversalKind.AllPrefix);

    /// <summary>Attempts all-prefix enumeration into a caller-provided result span.</summary>
    public bool TryEnumeratePrefixMatches(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> destination, out int written)
    {
        return TryWritePathCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: false, destination, out written);
    }

    /// <summary>Returns every accepted prefix match, from the root outward.</summary>
    public PatternMatch<TValue>[] EnumeratePrefixMatchesToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: false);
        var results = new PatternMatch<TValue>[candidates.Count];
        var writer = new ResultMatchWriter<TValue>(results, _deduplicateValues, _valueComparer, throwOnInsufficientCapacity: true);
        WriteCandidates(candidates.Candidates, ref writer);
        return writer.Count == results.Length ? results : results[..writer.Count];
    }

    private int GetDetailedMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CreateTraversal().CountCandidates(path, CandidateTraversalKind.Exact);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="MatchDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CreateTraversal().CountCaptures(path, CandidateTraversalKind.Exact);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that best-prefix detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="MatchPrefixDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetPrefixCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CreateTraversal().CountBestPrefixCaptures(path);
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that all-prefix detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="EnumeratePrefixDetailed(ReadOnlySpan{TSegment}, Span{PatternMatchDetailedSlice{TValue}}, Span{PatternCaptureSlice{TSegment}}, out int)"/>.</returns>
    public int GetEnumeratePrefixCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        return CreateTraversal().CountCaptures(path, CandidateTraversalKind.AllPrefix);
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
        return TryWritePathDetailedCandidates(path, CandidateTraversalKind.Exact, bestPrefix: false, matches, captures, out matchesWritten, out capturesWritten);
    }

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a newly allocated array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    public PatternMatchDetailed<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path)
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.Exact,
            bestPrefix: false);
        return MaterializeDetailed(candidates.Candidates, candidates.CaptureCountUpperBound, path);
    }

    /// <summary>Attempts best-prefix detailed matching into caller-provided buffers.</summary>
    public bool TryMatchPrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        => TryWritePathDetailedCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: true, matches, captures, out matchesWritten, out capturesWritten);

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
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: true);
        return MaterializeDetailed(candidates.Candidates, candidates.CaptureCountUpperBound, path);
    }

    /// <summary>Attempts all-prefix detailed enumeration into caller-provided buffers.</summary>
    public bool TryEnumeratePrefixDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
        => TryWritePathDetailedCandidates(path, CandidateTraversalKind.AllPrefix, bestPrefix: false, matches, captures, out matchesWritten, out capturesWritten);

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
    {
        using var candidates = CreateTraversal().TraverseCandidates(
            path,
            CandidateTraversalKind.AllPrefix,
            bestPrefix: false);
        return MaterializeDetailed(candidates.Candidates, candidates.CaptureCountUpperBound, path);
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
            ? ExplanationCollector<TSegment, TValue>.Collect(_storage, _segmentComparer, path)
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

    private MatchTraversal<TSegment, TValue> CreateTraversal()
        => new(_storage, _segmentComparer);

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
        CandidateTraversalKind kind,
        bool bestPrefix,
        Span<PatternMatch<TValue>> destination,
        out int written)
    {
        using var candidates = CreateTraversal().TraverseCandidates(path, kind, bestPrefix);
        return TryWriteCandidates(candidates.Candidates, destination, out written);
    }

    private bool TryWritePathValues(
        ReadOnlySpan<TSegment> path,
        CandidateTraversalKind kind,
        bool bestPrefix,
        Span<TValue> destination,
        out int written)
    {
        using var candidates = CreateTraversal().TraverseCandidates(path, kind, bestPrefix);
        return TryWriteValueCandidates(candidates.Candidates, destination, out written);
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

    private bool TryWriteDetailedCandidates(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        int captureUpperBound,
        ReadOnlySpan<TSegment> path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        if (candidates.IsEmpty)
        {
            matchesWritten = 0;
            capturesWritten = 0;
            return true;
        }

        var rentedMatches = ArrayPool<PatternMatchDetailedSlice<TValue>>.Shared.Rent(Math.Max(1, candidates.Length));
        var rentedCaptures = ArrayPool<PatternCaptureSlice<TSegment>>.Shared.Rent(Math.Max(1, captureUpperBound));
        try
        {
            var temporaryMatches = rentedMatches.AsSpan(0, candidates.Length);
            var temporaryCaptures = rentedCaptures.AsSpan(0, captureUpperBound);
            var writer = new DetailedMatchWriter<TSegment, TValue>(
                temporaryMatches,
                temporaryCaptures,
                path,
                _deduplicateValues,
                _valueComparer,
                throwOnInsufficientCapacity: false);

            for (var candidateIndex = 0; candidateIndex < candidates.Length; candidateIndex++)
            {
                var candidate = candidates[candidateIndex];
                writer.Add(
                    candidate.Value,
                    candidate.Detail,
                    _captureDescriptors,
                    candidate.ConsumedSegmentCount);
                if (!writer.Succeeded)
                {
                    break;
                }
            }

            if (!writer.Succeeded || writer.MatchCount > matches.Length || writer.CaptureCount > captures.Length)
            {
                matchesWritten = 0;
                capturesWritten = 0;
                return false;
            }

            temporaryMatches[..writer.MatchCount].CopyTo(matches);
            temporaryCaptures[..writer.CaptureCount].CopyTo(captures);
            matchesWritten = writer.MatchCount;
            capturesWritten = writer.CaptureCount;
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
        CandidateTraversalKind kind,
        bool bestPrefix,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        using var candidates = CreateTraversal().TraverseCandidates(path, kind, bestPrefix);
        return TryWriteDetailedCandidates(
            candidates.Candidates,
            candidates.CaptureCountUpperBound,
            path,
            matches,
            captures,
            out matchesWritten,
            out capturesWritten);
    }

    private PatternMatchDetailed<TSegment, TValue>[] MaterializeDetailed(
        ReadOnlySpan<MatchCandidate<TValue>> candidates,
        int captureUpperBound,
        ReadOnlySpan<TSegment> path)
    {
        if (candidates.IsEmpty)
        {
            return [];
        }

        var matches = new PatternMatchDetailedSlice<TValue>[candidates.Length];
        var captures = new PatternCaptureSlice<TSegment>[captureUpperBound];
        if (!TryWriteDetailedCandidates(
            candidates,
            captureUpperBound,
            path,
            matches,
            captures,
            out var matchCount,
            out var captureCount))
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

}
