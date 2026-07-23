namespace Pattrn;

internal readonly struct BuilderRegistrationMetadata
{
    internal static readonly BuilderRegistrationMetadata Exact = new([], 0, -1, default, null, -1);
    internal BuilderRegistrationMetadata(
        CaptureDescriptor[] captures,
        int score,
        int registrationOrder,
        RegistrationId registrationId,
        string? name,
        int terminalCatchAllSegmentIndex)
    {
        Captures = captures;
        Score = score;
        RegistrationOrder = registrationOrder;
        RegistrationId = registrationId;
        Name = name;
        TerminalCatchAllSegmentIndex = terminalCatchAllSegmentIndex;
    }

    internal CaptureDescriptor[] Captures { get; }
    internal int Score { get; }
    internal int RegistrationOrder { get; }
    internal RegistrationId RegistrationId { get; }
    internal string? Name { get; }
    internal int TerminalCatchAllSegmentIndex { get; }
}
