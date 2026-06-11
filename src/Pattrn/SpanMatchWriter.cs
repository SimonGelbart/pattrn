namespace Pattrn;

internal ref struct SpanMatchWriter<TValue>
{
    private const int OrderedBlockDedupThreshold = 8;

    private readonly Span<TValue> _destination;
    private readonly bool _deduplicateValues;
    private readonly bool _throwOnInsufficientCapacity;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private int _count;
    private bool _succeeded;

    internal SpanMatchWriter(
        Span<TValue> destination,
        bool deduplicateValues,
        IEqualityComparer<TValue> valueComparer,
        bool throwOnInsufficientCapacity = true)
    {
        _destination = destination;
        _deduplicateValues = deduplicateValues;
        _valueComparer = valueComparer;
        _throwOnInsufficientCapacity = throwOnInsufficientCapacity;
        _count = 0;
        _succeeded = true;
    }

    internal readonly int Count => _count;

    internal readonly bool Succeeded => _succeeded;

    internal void Add(ReadOnlySpan<TValue> values, bool valuesAreUnique)
    {
        if (values.IsEmpty || !_succeeded)
        {
            return;
        }

        if (!_deduplicateValues)
        {
            AddUnchecked(values);
            return;
        }

        if (valuesAreUnique && _count == 0)
        {
            AddUnchecked(values);
            return;
        }

        if (IsAlreadyWrittenOrderedBlock(values))
        {
            return;
        }

        foreach (var value in values)
        {
            Add(value);
            if (!_succeeded)
            {
                return;
            }
        }
    }

    private void AddUnchecked(ReadOnlySpan<TValue> values)
    {
        if (_count + values.Length > _destination.Length)
        {
            MarkInsufficientCapacity();
            return;
        }

        values.CopyTo(_destination[_count..]);
        _count += values.Length;
    }

    private readonly bool IsAlreadyWrittenOrderedBlock(ReadOnlySpan<TValue> values)
    {
        if (values.Length < OrderedBlockDedupThreshold || _count < values.Length)
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (!_valueComparer.Equals(_destination[i], values[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void Add(TValue value)
    {
        if (_deduplicateValues)
        {
            for (var i = 0; i < _count; i++)
            {
                if (_valueComparer.Equals(_destination[i], value))
                {
                    return;
                }
            }
        }

        if (_count >= _destination.Length)
        {
            MarkInsufficientCapacity();
            return;
        }

        _destination[_count] = value;
        _count++;
    }

    private void MarkInsufficientCapacity()
    {
        if (_throwOnInsufficientCapacity)
        {
            throw new ArgumentException("The destination span is too small to hold all matched values.", "destination");
        }

        _succeeded = false;
    }
}
