using System.Buffers;
using System.Diagnostics;

namespace Pattrn.Routing;

/// <summary>
/// Parses route-like string templates into the generic segmented pattern model used by <see cref="PattrnIndex{TSegment, TValue}"/>.
/// </summary>
/// <remarks>
/// <para>
/// This type is intentionally framework-neutral. It understands literals, named parameters, terminal named catch-alls, preserved constraint tokens,
/// defaults, and optional suffix parameters. It does not implement ASP.NET Core routing, URL decoding, or constraint validation.
/// </para>
/// <para>
/// Supported template examples include <c>/orders/{id}</c>, <c>/orders/{id:int}</c>, <c>/files/{*path}</c>, and <c>/orders/{id?}</c>.
/// </para>
/// </remarks>
public static class RoutePattern
{
    private const char Separator = '/';

    /// <summary>
    /// Parses a route-like pattern template into generic string pattern segments.
    /// </summary>
    /// <param name="pattern">The pattern to parse. Leading and trailing slashes are ignored.</param>
    /// <returns>The parsed pattern segments. Optional/defaulted parameters are included; use <see cref="Expand"/> to create omitted-suffix variants.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the pattern contains empty segments, invalid parameter syntax, or unsupported route syntax.</exception>
    public static PatternSegment<string>[] Parse(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return ParseTemplate(pattern).Compile();
    }

    /// <summary>
    /// Parses a framework-neutral route template and returns its structured metadata.
    /// </summary>
    /// <param name="pattern">The route template to parse. Leading and trailing slashes are ignored.</param>
    /// <returns>The parsed route-template model.</returns>
    public static RouteTemplate ParseTemplate(string pattern) => RouteTemplate.Parse(pattern);

    /// <summary>
    /// Attempts to parse a framework-neutral route template without throwing for syntax diagnostics.
    /// </summary>
    /// <param name="pattern">The route template to parse. Leading and trailing slashes are ignored.</param>
    /// <param name="template">The parsed route template when parsing succeeds; otherwise, <see langword="null"/>.</param>
    /// <param name="diagnostics">The diagnostics produced by parsing and validation.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParseTemplate(string pattern, out RouteTemplate? template, out RouteTemplateDiagnostic[] diagnostics) => RouteTemplate.TryParse(pattern, out template, out diagnostics);

    /// <summary>
    /// Expands a route-like pattern template into generic string pattern segments.
    /// </summary>
    /// <param name="pattern">The route template to expand. Leading and trailing slashes are ignored.</param>
    /// <returns>One or more generic core patterns. Optional/defaulted suffix parameters produce shorter variants before longer variants.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the pattern contains empty segments, invalid parameter syntax, or unsupported route syntax.</exception>
    public static PatternSegment<string>[][] Expand(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return ParseTemplate(pattern).Expand();
    }

    /// <summary>
    /// Expands a route-like pattern template into generic string pattern segments with expansion metadata.
    /// </summary>
    /// <param name="pattern">The route template to expand. Leading and trailing slashes are ignored.</param>
    /// <returns>One or more route-template expansions. Optional/defaulted suffix parameters produce shorter variants before longer variants.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the pattern contains empty segments, invalid parameter syntax, or unsupported route syntax.</exception>
    public static RouteTemplateExpansion[] ExpandDetailed(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return ParseTemplate(pattern).ExpandDetailed();
    }

    /// <summary>
    /// Gets the number of segments in a runtime route path.
    /// </summary>
    /// <param name="path">The runtime route path. Leading and trailing slashes are ignored.</param>
    /// <returns>The number of path segments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the path contains empty interior segments.</exception>
    public static int GetPathSegmentCount(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return CountSegments(path, nameof(path), out _, out _);
    }

    /// <summary>
    /// Splits a runtime route path into string segments suitable for matching against an index built from route patterns.
    /// </summary>
    /// <param name="path">The runtime route path. Leading and trailing slashes are ignored.</param>
    /// <returns>The route path segments.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the path contains empty interior segments.</exception>
    public static string[] SplitPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return Split(path, nameof(path));
    }

    /// <summary>
    /// Splits a runtime route path into a caller-provided destination span.
    /// </summary>
    /// <param name="path">The runtime route path. Leading and trailing slashes are ignored.</param>
    /// <param name="destination">The destination span that receives the route path segments.</param>
    /// <returns>The number of segments written to <paramref name="destination"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the path contains empty interior segments or <paramref name="destination"/> is too small.</exception>
    /// <remarks>
    /// This overload avoids allocating the segment array used by <see cref="SplitPath(string)"/>. It still creates string instances for each segment because the routing package
    /// converts route paths into the generic string-segment model before calling the core matcher.
    /// </remarks>
    public static int SplitPath(string path, Span<string> destination)
    {
        ArgumentNullException.ThrowIfNull(path);
        return SplitInto(path, nameof(path), destination, throwWhenTooSmall: true);
    }

    /// <summary>
    /// Tries to split a runtime route path into a caller-provided destination span.
    /// </summary>
    /// <param name="path">The runtime route path. Leading and trailing slashes are ignored.</param>
    /// <param name="destination">The destination span that receives the route path segments.</param>
    /// <param name="written">The number of segments written when the split succeeds; otherwise <c>0</c>.</param>
    /// <returns><see langword="true"/> when the destination span was large enough; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the path contains empty interior segments.</exception>
    /// <remarks>
    /// On failure, no segments are written to <paramref name="destination"/>.
    /// </remarks>
    public static bool TrySplitPath(string path, Span<string> destination, out int written)
    {
        ArgumentNullException.ThrowIfNull(path);

        var count = CountSegments(path, nameof(path), out _, out _);
        if (destination.Length < count)
        {
            written = 0;
            return false;
        }

        written = SplitInto(path, nameof(path), destination, throwWhenTooSmall: false);
        return true;
    }

    internal static string[] SplitTemplateSegments(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return Split(pattern, nameof(pattern));
    }

    internal static string[] RentSplitPath(string path, out int count)
    {
        ArgumentNullException.ThrowIfNull(path);

        count = CountSegments(path, nameof(path), out _, out _);
        if (count == 0)
        {
            return [];
        }

        var rented = ArrayPool<string>.Shared.Rent(count);
        var written = SplitInto(path, nameof(path), rented.AsSpan(0, count), throwWhenTooSmall: false);
        Debug.Assert(written == count, "The rented route path segment buffer must be large enough.");
        return rented;
    }

    internal static void ReturnSplitPath(string[] segments, int count)
    {
        if (segments.Length == 0)
        {
            return;
        }

        Array.Clear(segments, 0, count);
        ArrayPool<string>.Shared.Return(segments);
    }

    private static string[] Split(string value, string paramName)
    {
        var segmentCount = CountSegments(value, paramName, out var start, out var end);
        if (segmentCount == 0)
        {
            return [];
        }

        var segments = new string[segmentCount];
        WriteSegments(value, start, end, segments);
        return segments;
    }

    private static int SplitInto(string value, string paramName, Span<string> destination, bool throwWhenTooSmall)
    {
        var segmentCount = CountSegments(value, paramName, out var start, out var end);
        if (destination.Length < segmentCount)
        {
            if (throwWhenTooSmall)
            {
                throw new ArgumentException("The destination span is too small for the route path segments.", nameof(destination));
            }

            return 0;
        }

        if (segmentCount == 0)
        {
            return 0;
        }

        WriteSegments(value, start, end, destination);
        return segmentCount;
    }

    private static int CountSegments(string value, string paramName, out int start, out int end)
    {
        if (value.Length > 1 && value[0] == Separator && value[1] == Separator)
        {
            throw new ArgumentException("Route patterns and paths cannot contain repeated separators.", paramName);
        }

        if (value.Length > 1 && value[^1] == Separator && value[^2] == Separator)
        {
            throw new ArgumentException("Route patterns and paths cannot contain repeated separators.", paramName);
        }

        start = 0;
        end = value.Length;

        if (end > 0 && value[start] == Separator)
        {
            start++;
        }

        if (end > start && value[end - 1] == Separator)
        {
            end--;
        }

        if (end == start)
        {
            return 0;
        }

        var segmentCount = 1;
        for (var i = start; i < end; i++)
        {
            if (value[i] != Separator)
            {
                continue;
            }

            if (i == start || i == end - 1 || value[i - 1] == Separator)
            {
                throw new ArgumentException("Route patterns and paths cannot contain empty interior segments.", paramName);
            }

            segmentCount++;
        }

        return segmentCount;
    }

    private static void WriteSegments(string value, int start, int end, Span<string> destination)
    {
        var segmentStart = start;
        var segmentIndex = 0;
        for (var i = start; i <= end; i++)
        {
            if (i != end && value[i] != Separator)
            {
                continue;
            }

            destination[segmentIndex++] = value[segmentStart..i];
            segmentStart = i + 1;
        }
    }
}
