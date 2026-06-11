namespace Pattrn;

internal ref struct DetailedMatchWriter<TSegment, TValue>
    where TSegment : notnull
{
    private const int OrderedBlockDedupThreshold = 8;

    private readonly Span<PatternMatch<TValue>> _matches;
    private readonly Span<PatternCapture<TSegment>> _captures;
    private readonly ReadOnlySpan<TSegment> _path;
    private readonly bool _deduplicateValues;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _throwOnInsufficientCapacity;
    private int _matchCount;
    private int _captureCount;
    private bool _succeeded;

    internal DetailedMatchWriter(
        Span<PatternMatch<TValue>> matches,
        Span<PatternCapture<TSegment>> captures,
        ReadOnlySpan<TSegment> path,
        bool deduplicateValues,
        IEqualityComparer<TValue> valueComparer,
        bool throwOnInsufficientCapacity = true)
    {
        _matches = matches;
        _captures = captures;
        _path = path;
        _deduplicateValues = deduplicateValues;
        _valueComparer = valueComparer;
        _throwOnInsufficientCapacity = throwOnInsufficientCapacity;
        _matchCount = 0;
        _captureCount = 0;
        _succeeded = true;
    }

    internal readonly int MatchCount => _matchCount;

    internal readonly int CaptureCount => _captureCount;

    internal readonly bool Succeeded => _succeeded;

    internal void Add(
        ReadOnlySpan<TValue> values,
        ReadOnlySpan<CompiledValueDetail> details,
        ReadOnlySpan<CaptureDescriptor> captureDescriptors)
    {
        if (values.IsEmpty || !_succeeded)
        {
            return;
        }

        if (_deduplicateValues && IsAlreadyWrittenOrderedBlock(values))
        {
            return;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (_deduplicateValues && Contains(values[i]))
            {
                continue;
            }

            ref readonly var detail = ref details[i];
            var actualCaptureCount = GetActualCaptureCount(detail, captureDescriptors);
            if (_matchCount >= _matches.Length || _captureCount + actualCaptureCount > _captures.Length)
            {
                Fail();
                return;
            }

            var captureStart = _captureCount;
            WriteCaptures(detail, captureDescriptors);

            _matches[_matchCount] = new PatternMatch<TValue>(
                values[i],
                detail.Kind,
                detail.Score,
                captureStart,
                actualCaptureCount,
                detail.PatternId,
                detail.RegistrationOrder);
            _matchCount++;
        }
    }

    private readonly int GetActualCaptureCount(CompiledValueDetail detail, ReadOnlySpan<CaptureDescriptor> captureDescriptors)
    {
        var count = 0;
        for (var i = 0; i < detail.CaptureCount; i++)
        {
            ref readonly var descriptor = ref captureDescriptors[detail.FirstCapture + i];
            count += descriptor.IsCatchAll ? Math.Max(0, _path.Length - descriptor.SegmentIndex) : 1;
        }

        return count;
    }

    private void WriteCaptures(CompiledValueDetail detail, ReadOnlySpan<CaptureDescriptor> captureDescriptors)
    {
        for (var i = 0; i < detail.CaptureCount; i++)
        {
            ref readonly var descriptor = ref captureDescriptors[detail.FirstCapture + i];
            if (descriptor.IsCatchAll)
            {
                for (var segmentIndex = descriptor.SegmentIndex; segmentIndex < _path.Length; segmentIndex++)
                {
                    _captures[_captureCount] = new PatternCapture<TSegment>(
                        descriptor.Name,
                        _path[segmentIndex],
                        segmentIndex);
                    _captureCount++;
                }

                continue;
            }

            _captures[_captureCount] = new PatternCapture<TSegment>(
                descriptor.Name,
                _path[descriptor.SegmentIndex],
                descriptor.SegmentIndex);
            _captureCount++;
        }
    }

    private readonly bool IsAlreadyWrittenOrderedBlock(ReadOnlySpan<TValue> values)
    {
        if (values.Length < OrderedBlockDedupThreshold || _matchCount < values.Length)
        {
            return false;
        }

        for (var i = 0; i < values.Length; i++)
        {
            if (!_valueComparer.Equals(_matches[i].Value, values[i]))
            {
                return false;
            }
        }

        return true;
    }

    private readonly bool Contains(TValue value)
    {
        for (var i = 0; i < _matchCount; i++)
        {
            if (_valueComparer.Equals(_matches[i].Value, value))
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
            throw new ArgumentException("The destination match or capture span is too small for the match result.");
        }

        _succeeded = false;
    }
}
