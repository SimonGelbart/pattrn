# Matching semantics

The authoritative semantic contract now lives at [../semantics.md](../semantics.md).
This reference page remains as a compatibility link for existing documentation
URLs.

Pattrn matches segmented paths against an ordered immutable set of canonical
registrations. Literal segments match through the configured segment comparer;
wildcards and parameters consume one segment; terminal catch-alls consume zero
or more remaining segments.

## Registration and ordering

Registration order is deterministic and is preserved by the builder, compiler,
and result APIs. Exact and best-prefix results are ordered by specificity
descending, then registration order ascending for true ties. Specificity is
fixed internal behavior: literals outrank parameters, parameters outrank
wildcards, and wildcards outrank catch-alls. A zero-length terminal catch-all is
lower-ranked than a non-catch-all registration at the same consumed depth. The
public result records do not expose ranking metadata.

```csharp
var id = builder.AddPattern(
    [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
    "order-handler",
    "orders-by-id");
```

`PattrnRegistration` is the canonical source and snapshots its pattern. Its
`RegistrationId`, `Value`, and optional `Name` are retained through compilation.

## Exact matching

Exact APIs require the pattern to consume the complete input path:

```csharp
PatternMatch<TValue>[] results = index.MatchToArray(path);
TValue[] values = index.MatchValuesToArray(path);
index.TryMatch(path, resultDestination, out var resultCount);
index.TryMatchValues(path, valueDestination, out var valueCount);
```

`PatternMatch<TValue>` contains `Value`, `RegistrationId`, and
`ConsumedSegmentCount`. Value-only methods are explicit `...Values` methods.
Duplicate values are deduplicated by default, retaining the first value accepted
by the deterministic order, or retained when the index was compiled with
`MatchOptions.PreserveDuplicates`. Normal, value-only, owning detailed, and
caller-buffer detailed projections preserve the same relative ordering.

## Best-prefix matching

Best-prefix APIs select only registrations at the deepest successfully consumed
prefix depth. Within that depth they use the normal specificity and registration
order rules:

```csharp
var best = index.MatchPrefixToArray(path);
var bestValues = index.MatchPrefixValuesToArray(path);
```

`GetPrefixMatchCountUpperBound` is the bound for best-prefix results.

## Prefix enumeration

Enumeration returns registrations from shallowest matching prefix depth to
deepest. Within each depth, specificity and registration order apply. Depth is
resolved before specificity, so deeper accepted prefixes are selected only by
best-prefix matching, not moved ahead of ancestors during enumeration:

```csharp
var all = index.EnumeratePrefixMatchesToArray(path);
var allValues = index.EnumeratePrefixValuesToArray(path);
```

Use `TryEnumeratePrefixMatches` or `TryEnumeratePrefixValues` for caller-owned
buffers. Its separate bound is `GetEnumeratePrefixMatchCountUpperBound`.

No prefix behavior is stored in `MatchOptions`.

## Detailed results and captures

`MatchDetailedToArray` returns owning `PatternMatchDetailed<TSegment, TValue>`
records. Each record contains its value, registration identity, consumed depth,
and an immutable capture array. `TryGetCapture` and `GetCapture` use ordinal name
lookup. Caller-buffer detailed methods return
`PatternMatchDetailedSlice<TValue>` records plus a caller-owned capture span.
Use `GetPrefixCaptureCountUpperBound` for best-prefix detailed matching and
`GetEnumeratePrefixCaptureCountUpperBound` for all-prefix detailed enumeration
when sizing that capture span. These bounds are conservative when duplicate
values are deduplicated because they are calculated from the selected
candidate registrations before result filtering.

Named parameters and named catch-alls are captured; unnamed wildcards and
catch-alls are not. A terminal catch-all capture owns all remaining segments and
may contain zero segments.

## Empty and invalid patterns

The core permits empty patterns, which match only empty paths in exact mode and
every path in prefix queries. Non-terminal catch-alls and duplicate capture names
are compilation errors. String, routing, and dependency-injection packages keep
their domain-specific parsing and normalization outside the generic core.
