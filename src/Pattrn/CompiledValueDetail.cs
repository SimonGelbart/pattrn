namespace Pattrn;

internal readonly struct CompiledValueDetail
{
    internal CompiledValueDetail(
        int firstCapture,
        int captureCount,
        PatternMatchKind kind,
        int score,
        string? patternId,
        int registrationOrder)
    {
        FirstCapture = firstCapture;
        CaptureCount = captureCount;
        Kind = kind;
        Score = score;
        PatternId = patternId;
        RegistrationOrder = registrationOrder;
    }

    internal int FirstCapture { get; }

    internal int CaptureCount { get; }

    internal PatternMatchKind Kind { get; }

    internal int Score { get; }

    internal string? PatternId { get; }

    internal int RegistrationOrder { get; }
}
