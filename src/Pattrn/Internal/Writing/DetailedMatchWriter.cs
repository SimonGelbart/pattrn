namespace Pattrn.Internal.Writing;

internal ref struct DetailedMatchWriter<TSegment, TValue>
    where TSegment : notnull
{
    private readonly Span<PatternMatchDetailedSlice<TValue>> _matches;
    private readonly Span<PatternCaptureSlice<TSegment>> _captures;
    private readonly ReadOnlySpan<TSegment> _path;
    private readonly bool _deduplicateValues;
    private readonly IEqualityComparer<TValue> _valueComparer;
    private readonly bool _throwOnInsufficientCapacity;
    private int _matchCount;
    private int _captureCount;
    private bool _succeeded;

    internal DetailedMatchWriter(
        Span<PatternMatchDetailedSlice<TValue>> matches,
        Span<PatternCaptureSlice<TSegment>> captures,
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
        TValue value,
        CompiledValueDetail detail,
        ReadOnlySpan<CaptureDescriptor> captureDescriptors,
        int consumedSegmentCount)
    {
        if (!_succeeded)
        {
            return;
        }

        if (_deduplicateValues && Contains(value))
        {
            return;
        }

        var actualCaptureCount = detail.CaptureCount;
        if (_matchCount >= _matches.Length || _captureCount + actualCaptureCount > _captures.Length)
        {
            Fail();
            return;
        }

        var captureStart = _captureCount;
        WriteCaptures(detail, captureDescriptors);

        _matches[_matchCount] = new PatternMatchDetailedSlice<TValue>(
            value,
            detail.RegistrationId,
            consumedSegmentCount,
            captureStart,
            actualCaptureCount);
        _matchCount++;
    }

    private void WriteCaptures(CompiledValueDetail detail, ReadOnlySpan<CaptureDescriptor> captureDescriptors)
    {
        for (var i = 0; i < detail.CaptureCount; i++)
        {
            ref readonly var descriptor = ref captureDescriptors[detail.FirstCapture + i];
            if (descriptor.IsCatchAll)
            {
                _captures[_captureCount] = new PatternCaptureSlice<TSegment>(
                    descriptor.Name,
                    descriptor.SegmentIndex,
                    Math.Max(0, _path.Length - descriptor.SegmentIndex));
                _captureCount++;
                continue;
            }

            _captures[_captureCount] = new PatternCaptureSlice<TSegment>(
                descriptor.Name,
                descriptor.SegmentIndex,
                1);
            _captureCount++;
        }
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
