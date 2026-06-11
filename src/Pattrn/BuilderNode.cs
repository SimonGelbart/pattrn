namespace Pattrn;

internal sealed class BuilderNode<TSegment, TValue>
    where TSegment : notnull
{
    private readonly IEqualityComparer<TSegment> _segmentComparer;
    private Dictionary<TSegment, BuilderNode<TSegment, TValue>>? _children;

    internal BuilderNode(IEqualityComparer<TSegment> segmentComparer)
    {
        _segmentComparer = segmentComparer;
    }

    internal Dictionary<TSegment, BuilderNode<TSegment, TValue>>? Children => _children;

    internal int ChildCount => _children?.Count ?? 0;

    internal BuilderNode<TSegment, TValue>? WildcardChild { get; set; }

    internal BuilderNode<TSegment, TValue>? CatchAllChild { get; set; }

    internal List<TValue>? Values { get; set; }

    internal List<BuilderRegistrationMetadata>? Metadata { get; set; }

    internal bool IsEmpty => ChildCount == 0 && WildcardChild is null && CatchAllChild is null && (Values is null || Values.Count == 0);

    internal BuilderNode<TSegment, TValue> GetOrAddChild(TSegment segment)
    {
        _children ??= new Dictionary<TSegment, BuilderNode<TSegment, TValue>>(_segmentComparer);

        if (!_children.TryGetValue(segment, out var child))
        {
            child = new BuilderNode<TSegment, TValue>(_segmentComparer);
            _children.Add(segment, child);
        }

        return child;
    }

    internal bool TryGetChild(TSegment segment, out BuilderNode<TSegment, TValue>? child)
    {
        if (_children is null)
        {
            child = null;
            return false;
        }

        return _children.TryGetValue(segment, out child);
    }

    internal BuilderNode<TSegment, TValue>? GetChildOrDefault(TSegment segment)
    {
        return _children is not null && _children.TryGetValue(segment, out var child)
            ? child
            : null;
    }

    internal bool RemoveChild(TSegment segment)
    {
        if (_children is null || !_children.Remove(segment))
        {
            return false;
        }

        if (_children.Count == 0)
        {
            _children = null;
        }

        return true;
    }

    internal void Clear()
    {
        _children = null;
        WildcardChild = null;
        CatchAllChild = null;
        Values = null;
        Metadata = null;
    }
}
