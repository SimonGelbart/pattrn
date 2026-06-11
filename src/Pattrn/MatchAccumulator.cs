namespace Pattrn;

internal sealed class MatchAccumulator<TValue>
{
    private const int OrderedBlockDedupThreshold = 8;
    private const int HashSetDedupThreshold = 32;

    private readonly bool _deduplicateValues;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private List<TValue>? _values;
    private HashSet<TValue>? _seen;

    internal MatchAccumulator(bool deduplicateValues, IEqualityComparer<TValue> valueComparer)
    {
        _deduplicateValues = deduplicateValues;
        _valueComparer = valueComparer;
    }

    internal void Add(ReadOnlySpan<TValue> values, bool valuesAreUnique)
    {
        if (values.IsEmpty)
        {
            return;
        }

        if (!_deduplicateValues)
        {
            AddUnchecked(values);
            return;
        }

        if (valuesAreUnique && (_values is null || _values.Count == 0))
        {
            AddUnchecked(values);
            EnsureHashSetWhenLarge();
            return;
        }

        if (_seen is null && IsAlreadyAddedOrderedBlock(values))
        {
            return;
        }

        foreach (var value in values)
        {
            Add(value);
        }
    }

    internal TValue[] ToArray()
    {
        return _values is null || _values.Count == 0 ? Array.Empty<TValue>() : [.. _values];
    }

    private void AddUnchecked(ReadOnlySpan<TValue> values)
    {
        var destination = GetValuesList(values.Length);
        foreach (var value in values)
        {
            destination.Add(value);
        }
    }

    private bool IsAlreadyAddedOrderedBlock(ReadOnlySpan<TValue> values)
    {
        if (_values is null || values.Length < OrderedBlockDedupThreshold || _values.Count < values.Length)
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (!_valueComparer.Equals(_values[i], values[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void Add(TValue value)
    {
        if (_seen is not null)
        {
            if (_seen.Add(value))
            {
                _values!.Add(value);
            }

            return;
        }

        if (_values is not null)
        {
            foreach (var existing in _values)
            {
                if (_valueComparer.Equals(existing, value))
                {
                    return;
                }
            }
        }

        GetValuesList(1).Add(value);
        EnsureHashSetWhenLarge();
    }

    private void EnsureHashSetWhenLarge()
    {
        if (!_deduplicateValues || _seen is not null || _values is null || _values.Count < HashSetDedupThreshold)
        {
            return;
        }

        _seen = new HashSet<TValue>(_values, _valueComparer);
    }

    private List<TValue> GetValuesList(int additionalCapacity)
    {
        if (_values is not null)
        {
            return _values;
        }

        _values = new List<TValue>(additionalCapacity);
        return _values;
    }
}
