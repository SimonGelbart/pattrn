namespace Pattrn;

internal readonly struct BuilderRegistrationMetadata
{
    internal const int UnassignedRegistrationOrder = -1;

    internal static readonly BuilderRegistrationMetadata Exact = new([], PatternMatchKind.Exact, 0, null, UnassignedRegistrationOrder);

    internal BuilderRegistrationMetadata(
        CaptureDescriptor[] captures,
        PatternMatchKind kind,
        int score,
        string? patternId = null,
        int registrationOrder = UnassignedRegistrationOrder)
    {
        Captures = captures;
        Kind = kind;
        Score = score;
        PatternId = patternId;
        RegistrationOrder = registrationOrder;
    }

    internal CaptureDescriptor[] Captures { get; }

    internal PatternMatchKind Kind { get; }

    internal int Score { get; }

    internal string? PatternId { get; }

    internal int RegistrationOrder { get; }

    internal BuilderRegistrationMetadata WithRegistrationOrder(int registrationOrder)
    {
        return new BuilderRegistrationMetadata(Captures, Kind, Score, PatternId, registrationOrder);
    }
}
