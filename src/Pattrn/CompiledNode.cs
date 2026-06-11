namespace Pattrn;

internal readonly struct CompiledNode
{
    internal const int NoNode = -1;
    internal const int NoLookup = -1;

    internal CompiledNode(
        int firstChild,
        int childCount,
        int wildcardChild,
        int catchAllChild,
        int firstValue,
        int valueCount,
        int firstLookupSlot,
        int lookupMask)
    {
        FirstChild = firstChild;
        ChildCount = childCount;
        WildcardChild = wildcardChild;
        CatchAllChild = catchAllChild;
        FirstValue = firstValue;
        ValueCount = valueCount;
        FirstLookupSlot = firstLookupSlot;
        LookupMask = lookupMask;
    }

    internal int FirstChild { get; }

    internal int ChildCount { get; }

    internal int WildcardChild { get; }

    internal int CatchAllChild { get; }

    internal int FirstValue { get; }

    internal int ValueCount { get; }

    internal int FirstLookupSlot { get; }

    internal int LookupMask { get; }

    internal bool HasLookup => FirstLookupSlot != NoLookup;
}
