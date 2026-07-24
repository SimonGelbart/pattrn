namespace Pattrn.Matching;

/// <summary>Describes one accepted match.</summary>
/// <typeparam name="TValue">The registered value type.</typeparam>
public readonly record struct PatternMatch<TValue>(
    TValue Value,
    RegistrationId RegistrationId,
    int ConsumedSegmentCount);
