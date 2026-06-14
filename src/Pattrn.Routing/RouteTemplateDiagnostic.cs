namespace Pattrn.Routing;

/// <summary>
/// Describes one route-template parse or compilation diagnostic.
/// </summary>
/// <remarks>
/// Diagnostics are framework-neutral and are intended for tooling and configuration validation. They do not imply ASP.NET Core routing compatibility.
/// </remarks>
public sealed class RouteTemplateDiagnostic
{
    internal RouteTemplateDiagnostic(string code, string message, int segmentIndex = -1)
    {
        Code = code;
        Message = message;
        SegmentIndex = segmentIndex;
    }

    /// <summary>
    /// Gets the stable diagnostic code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the zero-based route segment index related to the diagnostic, or <c>-1</c> when the diagnostic applies to the template as a whole.
    /// </summary>
    public int SegmentIndex { get; }
}
