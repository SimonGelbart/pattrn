using System.Collections.Immutable;

namespace Pattrn.Internal.Compilation;

/// <summary>Single-writer storage for the builder's ordered canonical registrations.</summary>
internal sealed class BuilderRegistrationState<TSegment, TValue>
    where TSegment : notnull
{
    private readonly List<PattrnRegistration<TSegment, TValue>> _registrations = [];

    internal int Count => _registrations.Count;

    internal void Add(PattrnRegistration<TSegment, TValue> registration)
    {
        _registrations.Add(registration);
    }

    internal bool Replace(PattrnRegistration<TSegment, TValue> registration)
    {
        var index = _registrations.FindIndex(existing => existing.Id == registration.Id);
        if (index < 0)
        {
            return false;
        }

        _registrations[index] = registration;
        return true;
    }

    internal bool ContainsId(RegistrationId id) => _registrations.Any(existing => existing.Id == id);

    internal bool Remove(RegistrationId id)
    {
        var index = _registrations.FindIndex(existing => existing.Id == id);
        if (index < 0)
        {
            return false;
        }

        _registrations.RemoveAt(index);
        return true;
    }

    internal void Clear() => _registrations.Clear();

    internal ImmutableArray<PattrnRegistration<TSegment, TValue>> Snapshot() => [.. _registrations];

    internal bool ContainsPattern(
        ReadOnlySpan<PatternSegment<TSegment>> pattern,
        IEqualityComparer<TSegment> comparer)
    {
        var requested = pattern.ToArray().ToImmutableArray();
        return _registrations.Any(registration => SamePattern(registration.Pattern, requested, comparer));
    }

    private static bool SamePattern(
        ImmutableArray<PatternSegment<TSegment>> left,
        ImmutableArray<PatternSegment<TSegment>> right,
        IEqualityComparer<TSegment> comparer)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        for (var i = 0; i < left.Length; i++)
        {
            left[i].Deconstruct(out var leftKind, out var leftLiteral, out var leftName);
            right[i].Deconstruct(out var rightKind, out var rightLiteral, out var rightName);
            if (leftKind != rightKind
                || (leftKind == PatternSegmentKind.Literal && !comparer.Equals(leftLiteral, rightLiteral))
                || !string.Equals(leftName, rightName, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}
