using System.Collections.Immutable;

namespace Pattrn;

/// <summary>Describes an accepted match and its owning named captures.</summary>
public readonly record struct PatternMatchDetailed<TSegment, TValue>(
    TValue Value,
    RegistrationId RegistrationId,
    int ConsumedSegmentCount,
    ImmutableArray<PatternCapture<TSegment>> Captures)
    where TSegment : notnull
{
    /// <summary>Returns a capture by its ordinal, case-sensitive name.</summary>
    public bool TryGetCapture(string name, out PatternCapture<TSegment> capture)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (Captures.IsDefault)
        {
            capture = default;
            return false;
        }

        foreach (var candidate in Captures)
        {
            if (string.Equals(candidate.Name, name, StringComparison.Ordinal))
            {
                capture = candidate;
                return true;
            }
        }

        capture = default;
        return false;
    }

    /// <summary>Returns a capture by its ordinal, case-sensitive name.</summary>
    /// <exception cref="KeyNotFoundException">The named capture does not exist.</exception>
    public PatternCapture<TSegment> GetCapture(string name)
    {
        if (TryGetCapture(name, out var capture))
        {
            return capture;
        }

        throw new KeyNotFoundException($"The capture '{name}' was not found.");
    }
}

/// <summary>Describes a detailed caller-buffer match and its contiguous capture range.</summary>
public readonly record struct PatternMatchDetailedSlice<TValue>(
    TValue Value,
    RegistrationId RegistrationId,
    int ConsumedSegmentCount,
    int CaptureStart,
    int CaptureCount);
