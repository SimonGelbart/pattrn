namespace Pattrn;

internal ref struct ResultMatchWriter<TValue>
{
    private readonly Span<PatternMatch<TValue>> _destination;
    private readonly bool _deduplicateValues;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _throwOnInsufficientCapacity;
    private int _count;
    private bool _succeeded;

    internal ResultMatchWriter(
        Span<PatternMatch<TValue>> destination,
        bool deduplicateValues,
        IEqualityComparer<TValue> valueComparer,
        bool throwOnInsufficientCapacity)
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

    internal void Add(
        ReadOnlySpan<TValue> values,
        ReadOnlySpan<CompiledValueDetail> details,
        int consumedSegmentCount)
    {
        if (!_succeeded)
        {
            return;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (_deduplicateValues && Contains(values[i]))
            {
                continue;
            }

            if (_count >= _destination.Length)
            {
                Fail();
                return;
            }

            _destination[_count++] = new PatternMatch<TValue>(
                values[i],
                details[i].RegistrationId,
                consumedSegmentCount);
        }
    }

    internal void Add(TValue value, CompiledValueDetail detail, int consumedSegmentCount)
    {
        if (!_succeeded)
        {
            return;
        }

        if (_deduplicateValues && Contains(value))
        {
            return;
        }

        if (_count >= _destination.Length)
        {
            Fail();
            return;
        }

        _destination[_count++] = new PatternMatch<TValue>(value, detail.RegistrationId, consumedSegmentCount);
    }

    private readonly bool Contains(TValue value)
    {
        for (var i = 0; i < _count; i++)
        {
            if (_valueComparer.Equals(_destination[i].Value, value))
            {
                return true;
            }
        }

        return false;
    }

    private void Fail()
    {
        if (_throwOnInsufficientCapacity)
        {
            throw new ArgumentException("The destination span is too small to hold all matched results.", "destination");
        }

        _succeeded = false;
    }
}
