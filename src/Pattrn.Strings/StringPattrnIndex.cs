namespace Pattrn;

/// <summary>
/// Provides ergonomic string-path matching over an immutable string-segment index.
/// </summary>
/// <typeparam name="TValue">The stored value type.</typeparam>
/// <remarks>
/// This wrapper stores the same <see cref="StringNormalizationOptions"/> used by its builder so callers do not need to
/// pass options into every string match operation. Use <see cref="CoreIndex"/> for already-segmented hot paths.
/// </remarks>
public sealed class StringPattrnIndex<TValue>
{
    /// <summary>
    /// Initializes a new string-path wrapper for an existing generic string-segment index.
    /// </summary>
    /// <param name="coreIndex">The underlying immutable string-segment index.</param>
    /// <param name="options">The string normalization options used by this wrapper.</param>
    public StringPattrnIndex(
        PattrnIndex<string, TValue> coreIndex,
        StringNormalizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(coreIndex);
        ArgumentNullException.ThrowIfNull(options);

        CoreIndex = coreIndex;
        Options = options;
    }

    /// <summary>
    /// Gets the underlying generic string-segment index.
    /// </summary>
    public PattrnIndex<string, TValue> CoreIndex { get; }

    /// <summary>
    /// Gets the string normalization options applied before matching string paths.
    /// </summary>
    public StringNormalizationOptions Options { get; }

    /// <summary>
    /// Gets the number of distinct patterns compiled into the index.
    /// </summary>
    public int PatternCount => CoreIndex.PatternCount;

    /// <summary>
    /// Gets the number of pattern/value registrations compiled into the index.
    /// </summary>
    public int RegistrationCount => CoreIndex.RegistrationCount;

    /// <summary>
    /// Gets the maximum number of values that a single match operation can emit.
    /// </summary>
    public int MatchCountUpperBound => CoreIndex.MatchCountUpperBound;

    /// <summary>
    /// Gets the matching options used by the underlying index.
    /// </summary>
    public MatchOptions MatchOptions => CoreIndex.Options;

    /// <summary>
    /// Gets a path-specific upper bound for a normalized string path.
    /// </summary>
    public int GetMatchCountUpperBound(string path)
    {
        return CoreIndex.GetMatchCountUpperBound(Options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Gets a path-specific capture upper bound for a normalized string path.
    /// </summary>
    public int GetCaptureCountUpperBound(string path)
    {
        return CoreIndex.GetCaptureCountUpperBound(Options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Matches a normalized string path and writes matching values into the caller-provided destination span.
    /// </summary>
    public int MatchValues(string path, Span<TValue> destination)
    {
        if (CoreIndex.TryMatchValues(Options.Split(path, nameof(path)), destination, out var written))
        {
            return written;
        }

        throw new ArgumentException("The destination span is too small to hold all matched values.", nameof(destination));
    }

    /// <summary>
    /// Attempts to match a normalized string path and write values into the caller-provided destination span.
    /// </summary>
    public bool TryMatchValues(string path, Span<TValue> destination, out int written)
    {
        return CoreIndex.TryMatchValues(Options.Split(path, nameof(path)), destination, out written);
    }

    /// <summary>Attempts to match a path and write exact result records.</summary>
    public bool TryMatch(string path, Span<PatternMatch<TValue>> destination, out int written)
        => CoreIndex.TryMatch(Options.Split(path, nameof(path)), destination, out written);

    /// <summary>
    /// Matches a normalized string path and returns matching values as a new array.
    /// </summary>
    public PatternMatch<TValue>[] MatchToArray(string path)
    {
        return CoreIndex.MatchToArray(Options.Split(path, nameof(path)));
    }

    /// <summary>Matches a normalized string path and returns value-only results.</summary>
    public TValue[] MatchValuesToArray(string path)
    {
        return CoreIndex.MatchValuesToArray(Options.Split(path, nameof(path)));
    }

    /// <summary>Attempts to write the deepest matching prefix result records.</summary>
    public bool TryMatchPrefix(string path, Span<PatternMatch<TValue>> destination, out int written)
        => CoreIndex.TryMatchPrefix(Options.Split(path, nameof(path)), destination, out written);

    /// <summary>Attempts to write values from the deepest matching prefix.</summary>
    public bool TryMatchPrefixValues(string path, Span<TValue> destination, out int written)
        => CoreIndex.TryMatchPrefixValues(Options.Split(path, nameof(path)), destination, out written);

    /// <summary>Returns result records from the deepest matching prefix.</summary>
    public PatternMatch<TValue>[] MatchPrefixToArray(string path)
        => CoreIndex.MatchPrefixToArray(Options.Split(path, nameof(path)));

    /// <summary>Returns values from the deepest matching prefix.</summary>
    public TValue[] MatchPrefixValuesToArray(string path)
        => CoreIndex.MatchPrefixValuesToArray(Options.Split(path, nameof(path)));

    /// <summary>Attempts to enumerate all matching prefixes from shallowest to deepest.</summary>
    public bool TryEnumeratePrefixMatches(string path, Span<PatternMatch<TValue>> destination, out int written)
        => CoreIndex.TryEnumeratePrefixMatches(Options.Split(path, nameof(path)), destination, out written);

    /// <summary>Attempts to enumerate values from all matching prefixes.</summary>
    public bool TryEnumeratePrefixValues(string path, Span<TValue> destination, out int written)
        => CoreIndex.TryEnumeratePrefixValues(Options.Split(path, nameof(path)), destination, out written);

    /// <summary>Returns result records from all matching prefixes.</summary>
    public PatternMatch<TValue>[] EnumeratePrefixMatchesToArray(string path)
        => CoreIndex.EnumeratePrefixMatchesToArray(Options.Split(path, nameof(path)));

    /// <summary>Returns values from all matching prefixes.</summary>
    public TValue[] EnumeratePrefixValuesToArray(string path)
        => CoreIndex.EnumeratePrefixValuesToArray(Options.Split(path, nameof(path)));

    /// <summary>
    /// Matches a normalized string path and writes detailed matches and captures into caller-provided destination spans.
    /// </summary>
    public int MatchDetailed(
        string path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<string>> captures,
        out int capturesWritten)
    {
        return CoreIndex.MatchDetailed(Options.Split(path, nameof(path)), matches, captures, out capturesWritten);
    }

    /// <summary>
    /// Attempts to match a normalized string path and write detailed matches and captures into caller-provided destination spans.
    /// </summary>
    public bool TryMatchDetailed(
        string path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<string>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        return CoreIndex.TryMatchDetailed(
            Options.Split(path, nameof(path)),
            matches,
            captures,
            out matchesWritten,
            out capturesWritten);
    }

    /// <summary>
    /// Matches a normalized string path and returns detailed matches as a new array.
    /// </summary>
    public PatternMatchDetailed<string, TValue>[] MatchDetailedToArray(string path)
    {
        return CoreIndex.MatchDetailedToArray(Options.Split(path, nameof(path)));
    }

    /// <summary>
    /// Explains matching behavior for a normalized string path using diagnostics-oriented results.
    /// </summary>
    public PatternMatchExplanation<string, TValue> Explain(
        string path,
        PatternExplanationOptions options = default)
    {
        return CoreIndex.Explain(Options.Split(path, nameof(path)), options);
    }
}
