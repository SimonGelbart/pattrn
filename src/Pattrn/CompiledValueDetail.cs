namespace Pattrn;

internal readonly struct CompiledValueDetail
{
    internal CompiledValueDetail(
        int firstCapture,
        int captureCount,
        int score,
        int registrationOrder,
        RegistrationId registrationId,
        string? name)
    {
        FirstCapture = firstCapture;
        CaptureCount = captureCount;
        Score = score;
        RegistrationOrder = registrationOrder;
        RegistrationId = registrationId;
        Name = name;
    }

    internal int FirstCapture { get; }
    internal int CaptureCount { get; }
    internal int Score { get; }
    internal int RegistrationOrder { get; }
    internal RegistrationId RegistrationId { get; }
    internal string? Name { get; }
}
