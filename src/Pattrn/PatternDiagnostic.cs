namespace Pattrn;

/// <summary>
/// Describes a generic diagnostic discovered in a builder's registered pattern set.
/// </summary>
/// <typeparam name="TSegment">The segment type used by registered patterns.</typeparam>
/// <remarks>
/// Diagnostics are advisory. They do not change matching behavior and are intentionally domain-neutral.
/// Route-, authorization-, filesystem-, or topic-specific validation should live in companion packages.
/// </remarks>
public sealed class PatternDiagnostic<TSegment>
    where TSegment : notnull
{
    internal PatternDiagnostic(
        PatternDiagnosticKind kind,
        PatternDiagnosticSeverity severity,
        IReadOnlyList<PatternSegment<TSegment>> pattern,
        string message,
        int registrationCount)
    {
        Kind = kind;
        Severity = severity;
        Pattern = pattern;
        Message = message;
        RegistrationCount = registrationCount;
    }

    /// <summary>
    /// Gets the diagnostic kind.
    /// </summary>
    public PatternDiagnosticKind Kind { get; }

    /// <summary>
    /// Gets the diagnostic severity.
    /// </summary>
    public PatternDiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the structural pattern associated with the diagnostic.
    /// </summary>
    public IReadOnlyList<PatternSegment<TSegment>> Pattern { get; }

    /// <summary>
    /// Gets a human-readable diagnostic message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the number of registrations involved when the diagnostic is registration-count based; otherwise, zero.
    /// </summary>
    public int RegistrationCount { get; }
}
