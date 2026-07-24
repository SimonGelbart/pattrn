namespace Pattrn.Routing.Integration;

/// <summary>
/// Matching helpers for route-like runtime paths.
/// </summary>
/// <remarks>
/// These helpers convert route paths into string segments before calling the generic core APIs. The string-based helpers rent the temporary segment array internally,
/// but they still create string instances for path segments. Hot callers can split once with <see cref="RoutePattern.SplitPath(string, Span{string})"/> and call the core span APIs directly.
/// </remarks>
public static class RoutePattrnIndexExtensions
{
    /// <summary>
    /// Gets a route-path-specific upper bound for value-only matches.
    /// </summary>
    public static int GetRouteMatchCountUpperBound<TValue>(
        this PattrnIndex<string, TValue> index,
        string path)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.GetMatchCountUpperBound(segments.AsSpan(0, count));
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Gets a route-path-specific upper bound for detailed-match captures.
    /// </summary>
    public static int GetRouteCaptureCountUpperBound<TValue>(
        this PattrnIndex<string, TValue> index,
        string path)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.GetCaptureCountUpperBound(segments.AsSpan(0, count));
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Matches a route-like runtime path and writes values into a destination span.
    /// </summary>
    public static int MatchRoute<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            if (index.TryMatchValues(segments.AsSpan(0, count), destination, out var written))
            {
                return written;
            }

            throw new ArgumentException("The destination span is too small to hold all matched values.", nameof(destination));
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Tries to match a route-like runtime path and writes values into a destination span.
    /// </summary>
    public static bool TryMatchRoute<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<TValue> destination,
        out int written)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.TryMatchValues(segments.AsSpan(0, count), destination, out written);
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Matches a route-like runtime path and returns values in a new array.
    /// </summary>
    public static TValue[] MatchRouteToArray<TValue>(
        this PattrnIndex<string, TValue> index,
        string path)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.MatchValuesToArray(segments.AsSpan(0, count));
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Matches a route-like runtime path and writes detailed match metadata into destination spans.
    /// </summary>
    public static int MatchRouteDetailed<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<string>> captures,
        out int capturesWritten)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.MatchDetailed(segments.AsSpan(0, count), matches, captures, out capturesWritten);
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Tries to match a route-like runtime path and writes detailed match metadata into destination spans.
    /// </summary>
    public static bool TryMatchRouteDetailed<TValue>(
        this PattrnIndex<string, TValue> index,
        string path,
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<string>> captures,
        out int matchesWritten,
        out int capturesWritten)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.TryMatchDetailed(segments.AsSpan(0, count), matches, captures, out matchesWritten, out capturesWritten);
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }

    /// <summary>
    /// Matches a route-like runtime path and returns detailed matches in a new array.
    /// </summary>
    public static PatternMatchDetailed<string, TValue>[] MatchRouteDetailedToArray<TValue>(
        this PattrnIndex<string, TValue> index,
        string path)
    {
        ArgumentNullException.ThrowIfNull(index);
        var segments = RoutePattern.RentSplitPath(path, out var count);
        try
        {
            return index.MatchDetailedToArray(segments.AsSpan(0, count));
        }
        finally
        {
            RoutePattern.ReturnSplitPath(segments, count);
        }
    }
}
