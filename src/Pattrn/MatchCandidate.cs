namespace Pattrn;

internal readonly record struct MatchCandidate<TValue>(
    TValue Value,
    CompiledValueDetail Detail,
    int ConsumedSegmentCount,
    bool IsZeroLengthCatchAll);
