using System.Buffers;

namespace Pattrn;

/// <summary>
/// Immutable index that matches segmented input paths against previously registered path patterns.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// Instances are created by <see cref="PattrnIndexBuilder{TSegment, TValue}.Build"/> and are safe for concurrent readers.
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
public sealed class PattrnIndex<TSegment, TValue> : IPattrnIndex<TSegment, TValue>
    where TSegment : notnull
{
    private readonly CompiledNode[] _nodes;
    private readonly CompiledChild<TSegment>[] _children;
    private readonly int[] _childLookupSlots;
    private readonly TValue[] _values;
    private readonly CompiledValueDetail[] _valueDetails;
    private readonly CaptureDescriptor[] _captureDescriptors;
    private readonly IEqualityComparer<TSegment> _segmentComparer;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _includePrefixMatches;
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
        _includePrefixMatches = options.IncludePrefixMatches;
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
    /// <returns>A safe upper bound for a destination span used with <see cref="Match(ReadOnlySpan{TSegment}, Span{TValue})"/> or <see cref="TryMatch(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>.</returns>
    /// <remarks>
    /// This method traverses only the branches that can match <paramref name="path"/>. When deduplication is enabled, the returned value can be larger than the final emitted value count because overlapping patterns may reach the same value.
    /// </remarks>
    public int GetMatchCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        if (!_hasWildcardBranches)
        {
            return _includePrefixMatches
                ? CountPrefixExactOnly(path)
                : CountExactOnly(path);
        }

        return _includePrefixMatches
            ? CountPrefix(path)
            : CountExact(path);
    }

    /// <summary>
    /// Matches the specified segmented path and writes matching values into the caller-provided destination span.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="destination">The destination span that receives matching values.</param>
    /// <returns>The number of values written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is too small.</exception>
    public int Match(ReadOnlySpan<TSegment> path, Span<TValue> destination)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            return MatchExactOnlyDirect(path, destination);
        }

        var writer = new SpanMatchWriter<TValue>(destination, _deduplicateValues, _valueComparer);
        CollectValues(path, ref writer);
        return writer.Count;
    }

    /// <summary>
    /// Attempts to match the specified segmented path and write matching values into the caller-provided destination span.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="destination">The destination span that receives matching values when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="written">When this method returns <see langword="true"/>, contains the number of values written to <paramref name="destination"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <returns><see langword="true"/> when <paramref name="destination"/> was large enough; otherwise, <see langword="false"/>.</returns>
    public bool TryMatch(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            return TryMatchExactOnlyDirect(path, destination, out written);
        }

        var upperBound = GetMatchCountUpperBound(path);
        if (upperBound == 0)
        {
            written = 0;
            return true;
        }

        if (upperBound <= destination.Length)
        {
            var writer = new SpanMatchWriter<TValue>(
                destination,
                _deduplicateValues,
                _valueComparer,
                throwOnInsufficientCapacity: false);

            CollectValues(path, ref writer);
            if (!writer.Succeeded)
            {
                written = 0;
                return false;
            }

            written = writer.Count;
            return true;
        }

        var rentedValues = ArrayPool<TValue>.Shared.Rent(upperBound);
        try
        {
            var temporary = rentedValues.AsSpan(0, upperBound);
            var writer = new SpanMatchWriter<TValue>(
                temporary,
                _deduplicateValues,
                _valueComparer,
                throwOnInsufficientCapacity: false);

            CollectValues(path, ref writer);
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
            ArrayPool<TValue>.Shared.Return(rentedValues, clearArray: true);
        }
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

    private int MatchExactOnlyDirect(ReadOnlySpan<TSegment> path, Span<TValue> destination)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex == CompiledNode.NoNode)
        {
            return 0;
        }

        var values = GetValues(nodeIndex);
        if (values.Length > destination.Length)
        {
            throw new ArgumentException("The destination span is too small to hold all matched values.", nameof(destination));
        }

        values.CopyTo(destination);
        return values.Length;
    }

    private int MatchDetailedExactOnlyDirect(ReadOnlySpan<TSegment> path, Span<PatternMatch<TValue>> matches)
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
            matches[i] = new PatternMatch<TValue>(
                values[i],
                detail.Kind,
                detail.Score,
                captureStart: 0,
                captureCount: 0,
                detail.PatternId,
                detail.RegistrationOrder);
        }

        return values.Length;
    }

    private void CollectValues(ReadOnlySpan<TSegment> path, ref SpanMatchWriter<TValue> writer)
    {
        if (!_hasWildcardBranches)
        {
            if (_includePrefixMatches)
            {
                CollectPrefixExactOnly(path, ref writer);
            }
            else
            {
                CollectExactOnly(path, ref writer);
            }
        }
        else if (_includePrefixMatches)
        {
            CollectPrefix(path, ref writer);
        }
        else
        {
            CollectExact(path, ref writer);
        }
    }

    /// <summary>
    /// Matches the specified segmented path and returns matching values as a new array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all matching values.</returns>
    public TValue[] MatchToArray(ReadOnlySpan<TSegment> path)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            var nodeIndex = TryDescendExactOnly(path);
            return nodeIndex == CompiledNode.NoNode ? [] : GetValues(nodeIndex).ToArray();
        }

        var accumulator = new MatchAccumulator<TValue>(_deduplicateValues, _valueComparer);

        if (!_hasWildcardBranches)
        {
            if (_includePrefixMatches)
            {
                CollectPrefixExactOnly(path, accumulator);
            }
            else
            {
                CollectExactOnly(path, accumulator);
            }
        }
        else if (_includePrefixMatches)
        {
            CollectPrefix(path, accumulator);
        }
        else
        {
            CollectExact(path, accumulator);
        }

        return accumulator.ToArray();
    }

    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a capture destination span used with <see cref="MatchDetailed(ReadOnlySpan{TSegment}, Span{PatternMatch{TValue}}, Span{PatternCapture{TSegment}}, out int)"/>.</returns>
    public int GetCaptureCountUpperBound(ReadOnlySpan<TSegment> path)
    {
        if (!_hasWildcardBranches)
        {
            return 0;
        }

        return _includePrefixMatches
            ? CountPrefixCaptures(path)
            : CountExactCaptures(path);
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
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int capturesWritten)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            capturesWritten = 0;
            return MatchDetailedExactOnlyDirect(path, matches);
        }

        var writer = new DetailedMatchWriter<TSegment, TValue>(
            matches,
            captures,
            path,
            _deduplicateValues,
            _valueComparer);

        CollectDetailedMatches(path, ref writer);
        capturesWritten = writer.CaptureCount;
        return writer.MatchCount;
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
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            capturesWritten = 0;
            return TryMatchDetailedExactOnlyDirect(path, matches, out matchesWritten);
        }

        var matchUpperBound = GetMatchCountUpperBound(path);
        var captureUpperBound = GetCaptureCountUpperBound(path);

        if (matchUpperBound == 0)
        {
            matchesWritten = 0;
            capturesWritten = 0;
            return true;
        }

        if (matchUpperBound <= matches.Length && captureUpperBound <= captures.Length)
        {
            var writer = new DetailedMatchWriter<TSegment, TValue>(
                matches,
                captures,
                path,
                _deduplicateValues,
                _valueComparer,
                throwOnInsufficientCapacity: false);

            CollectDetailedMatches(path, ref writer);
            if (!writer.Succeeded)
            {
                matchesWritten = 0;
                capturesWritten = 0;
                return false;
            }

            matchesWritten = writer.MatchCount;
            capturesWritten = writer.CaptureCount;
            return true;
        }

        var rentedMatches = ArrayPool<PatternMatch<TValue>>.Shared.Rent(matchUpperBound);
        var rentedCaptures = ArrayPool<PatternCapture<TSegment>>.Shared.Rent(Math.Max(1, captureUpperBound));
        try
        {
            var temporaryMatches = rentedMatches.AsSpan(0, matchUpperBound);
            var temporaryCaptures = rentedCaptures.AsSpan(0, captureUpperBound);
            var writer = new DetailedMatchWriter<TSegment, TValue>(
                temporaryMatches,
                temporaryCaptures,
                path,
                _deduplicateValues,
                _valueComparer,
                throwOnInsufficientCapacity: false);

            CollectDetailedMatches(path, ref writer);
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
            ArrayPool<PatternMatch<TValue>>.Shared.Return(rentedMatches, clearArray: true);
            ArrayPool<PatternCapture<TSegment>>.Shared.Return(rentedCaptures, clearArray: true);
        }
    }

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a newly allocated array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    public PatternMatchResult<TSegment, TValue>[] MatchDetailed(ReadOnlySpan<TSegment> path) => MatchDetailedToArray(path);

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a newly allocated array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    public PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path)
    {
        if (!_hasWildcardBranches && !_includePrefixMatches)
        {
            return MatchDetailedExactOnlyToArray(path);
        }

        var matchUpperBound = GetMatchCountUpperBound(path);
        var captureUpperBound = GetCaptureCountUpperBound(path);

        if (matchUpperBound == 0)
        {
            return [];
        }

        var matches = new PatternMatch<TValue>[matchUpperBound];
        var captures = new PatternCapture<TSegment>[captureUpperBound];
        var matchCount = MatchDetailed(path, matches, captures, out var captureCount);
        _ = captureCount;

        if (matchCount == 0)
        {
            return [];
        }

        var results = new PatternMatchResult<TSegment, TValue>[matchCount];
        for (var i = 0; i < matchCount; i++)
        {
            var match = matches[i];
            var matchCaptures = new PatternCapture<TSegment>[match.CaptureCount];
            if (match.CaptureCount > 0)
            {
                captures.AsSpan(match.CaptureStart, match.CaptureCount).CopyTo(matchCaptures);
            }

            results[i] = new PatternMatchResult<TSegment, TValue>(
                match.Value,
                match.PatternId,
                match.RegistrationOrder,
                match.Kind,
                match.Specificity,
                matchCaptures);
        }

        return results;
    }

    private bool TryMatchDetailedExactOnlyDirect(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatch<TValue>> matches,
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
            matches[i] = new PatternMatch<TValue>(
                values[i],
                detail.Kind,
                detail.Score,
                captureStart: 0,
                captureCount: 0,
                detail.PatternId,
                detail.RegistrationOrder);
        }

        matchesWritten = values.Length;
        return true;
    }

    private PatternMatchResult<TSegment, TValue>[] MatchDetailedExactOnlyToArray(ReadOnlySpan<TSegment> path)
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
        var results = new PatternMatchResult<TSegment, TValue>[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            ref readonly var detail = ref details[i];
            results[i] = new PatternMatchResult<TSegment, TValue>(
                values[i],
                detail.PatternId,
                detail.RegistrationOrder,
                detail.Kind,
                detail.Score,
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
    /// This method intentionally composes detailed matching and optional diagnostics. Use <see cref="Match(ReadOnlySpan{TSegment}, Span{TValue})"/>,
    /// <see cref="TryMatch(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>, or <see cref="MatchToArray(ReadOnlySpan{TSegment})"/> for hot paths.
    /// </remarks>
    public PatternMatchExplanation<TSegment, TValue> Explain(
        ReadOnlySpan<TSegment> path,
        PatternExplanationOptions options = default)
    {
        var pathCopy = path.ToArray();
        var matchUpperBound = GetMatchCountUpperBound(path);
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
                    "No literal branch matched this input segment."));
                return;
            }

            nodeIndex = childNodeIndex;
        }

        if (GetValues(nodeIndex).IsEmpty)
        {
            rejectedCandidates.Add(new PatternRejectedCandidate(
                path.Length,
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
            for (var j = 0; j < detail.CaptureCount; j++)
            {
                ref readonly var descriptor = ref _captureDescriptors[detail.FirstCapture + j];
                count += descriptor.IsCatchAll ? Math.Max(0, pathLength - descriptor.SegmentIndex) : 1;
            }
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

    private void AddDetailedValuesIncludingTerminalCatchAll(int nodeIndex, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        AddDetailedValues(nodeIndex, ref writer);
        if (!writer.Succeeded)
        {
            return;
        }

        ref readonly var node = ref _nodes[nodeIndex];
        if (node.CatchAllChild != CompiledNode.NoNode)
        {
            AddDetailedValues(node.CatchAllChild, ref writer);
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

    private void CollectDetailedMatches(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        if (!_hasWildcardBranches)
        {
            if (_includePrefixMatches)
            {
                CollectPrefixDetailedExactOnly(path, ref writer);
            }
            else
            {
                CollectDetailedExactOnly(path, ref writer);
            }
        }
        else if (_includePrefixMatches)
        {
            CollectPrefixDetailed(path, ref writer);
        }
        else
        {
            CollectDetailedWildcard(path, ref writer);
        }
    }

    private void AddDetailedValues(int nodeIndex, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        writer.Add(GetValues(nodeIndex), GetValueDetails(nodeIndex), _captureDescriptors);
    }

    private void CollectDetailedExactOnly(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        var nodeIndex = TryDescendExactOnly(path);
        if (nodeIndex != CompiledNode.NoNode)
        {
            AddDetailedValues(nodeIndex, ref writer);
        }
    }

    private void CollectPrefixDetailedExactOnly(ReadOnlySpan<TSegment> path, ref DetailedMatchWriter<TSegment, TValue> writer)
    {
        var nodeIndex = 0;
        AddDetailedValues(nodeIndex, ref writer);
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
            AddDetailedValues(nodeIndex, ref writer);
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
                    AddDetailedValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer);
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
                    AddDetailedValuesIncludingTerminalCatchAll(frame.NodeIndex, ref writer);
                    if (!writer.Succeeded)
                    {
                        return;
                    }

                    continue;
                }

                AddDetailedValues(frame.NodeIndex, ref writer);
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
