namespace Pattrn;

/// <summary>
/// Matches segmented input paths against an immutable set of registered path patterns.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns and incoming paths.</typeparam>
/// <typeparam name="TValue">The value type returned when a registered pattern matches.</typeparam>
/// <remarks>
/// This abstraction intentionally exposes only the hot segmented-path surface. Convenience overloads live as extension methods or in companion packages.
/// Implementations are expected to be safe for concurrent readers.
/// </remarks>
public interface IPattrnIndex<TSegment, TValue>
    where TSegment : notnull
{
    /// <summary>
    /// Gets the number of distinct patterns compiled into the index.
    /// </summary>
    int PatternCount { get; }

    /// <summary>
    /// Gets the number of pattern/value registrations compiled into the index.
    /// </summary>
    int RegistrationCount { get; }

    /// <summary>
    /// Gets the maximum number of values that a single match operation can emit. This is a safe upper bound for caller-provided match destination spans.
    /// </summary>
    int MatchCountUpperBound { get; }

    /// <summary>
    /// Gets the matching options used by this index.
    /// </summary>
    MatchOptions Options { get; }

    /// <summary>
    /// Gets a path-specific upper bound for the number of values that matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a destination span used with <see cref="Match(ReadOnlySpan{TSegment}, Span{TValue})"/> or <see cref="TryMatch(ReadOnlySpan{TSegment}, Span{TValue}, out int)"/>.</returns>
    int GetMatchCountUpperBound(ReadOnlySpan<TSegment> path);

    /// <summary>
    /// Matches the specified segmented path and writes matching values into the caller-provided destination span.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="destination">The destination span that receives matching values.</param>
    /// <returns>The number of values written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="destination"/> is too small.</exception>
    int Match(ReadOnlySpan<TSegment> path, Span<TValue> destination);

    /// <summary>
    /// Attempts to match the specified segmented path and write matching values into the caller-provided destination span.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="destination">The destination span that receives matching values when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="written">When this method returns <see langword="true"/>, contains the number of values written to <paramref name="destination"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <returns><see langword="true"/> when <paramref name="destination"/> was large enough; otherwise, <see langword="false"/>.</returns>
    bool TryMatch(ReadOnlySpan<TSegment> path, Span<TValue> destination, out int written);


    /// <summary>
    /// Gets a path-specific upper bound for the number of named captures that detailed matching this path can emit.
    /// </summary>
    /// <param name="path">The segmented input path to inspect.</param>
    /// <returns>A safe upper bound for a detailed capture destination span.</returns>
    int GetCaptureCountUpperBound(ReadOnlySpan<TSegment> path);

    /// <summary>
    /// Matches the specified segmented path and writes detailed matches and named captures into caller-provided destination spans.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="matches">The destination span that receives detailed match descriptors.</param>
    /// <param name="captures">The destination span that receives named captures shared by the written match descriptors.</param>
    /// <param name="capturesWritten">The number of captures written to <paramref name="captures"/>.</param>
    /// <returns>The number of matches written to <paramref name="matches"/>.</returns>
    int MatchDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int capturesWritten);

    /// <summary>
    /// Attempts to match the specified segmented path and write detailed matches and named captures into caller-provided destination spans.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <param name="matches">The destination span that receives detailed match descriptors when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="captures">The destination span that receives named captures when it is large enough. When this method returns <see langword="false"/>, this span is not written by the method.</param>
    /// <param name="matchesWritten">When this method returns <see langword="true"/>, contains the number of matches written to <paramref name="matches"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <param name="capturesWritten">When this method returns <see langword="true"/>, contains the number of captures written to <paramref name="captures"/>. When this method returns <see langword="false"/>, contains <c>0</c>.</param>
    /// <returns><see langword="true"/> when both destination spans were large enough; otherwise, <see langword="false"/>.</returns>
    bool TryMatchDetailed(
        ReadOnlySpan<TSegment> path,
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        out int matchesWritten,
        out int capturesWritten);

    /// <summary>
    /// Explains matching behavior for the specified segmented path using diagnostics-oriented allocation-friendly results.
    /// </summary>
    /// <param name="path">The segmented input path to explain.</param>
    /// <param name="options">Options controlling optional diagnostic work.</param>
    /// <returns>An explanation containing accepted detailed matches and optional rejected-candidate diagnostics.</returns>
    PatternMatchExplanation<TSegment, TValue> Explain(
        ReadOnlySpan<TSegment> path,
        PatternExplanationOptions options = default);

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a new array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    PatternMatchResult<TSegment, TValue>[] MatchDetailed(ReadOnlySpan<TSegment> path);

    /// <summary>
    /// Matches the specified segmented path and returns detailed matches as a new array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all detailed matches.</returns>
    PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path);

    /// <summary>
    /// Matches the specified segmented path and returns matching values as a new array.
    /// </summary>
    /// <param name="path">The segmented input path to match.</param>
    /// <returns>An array containing all matching values.</returns>
    TValue[] MatchToArray(ReadOnlySpan<TSegment> path);
}
