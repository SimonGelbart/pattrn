# API overview

The generic package accepts segmented paths and keeps registration, compilation,
and matching contracts separate.

## Canonical registrations

```csharp
var registration = PattrnRegistration<string, string>.Create(
    [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
    "order-handler",
    "orders-by-id");

var result = PattrnIndex<string, string>.CompileWithDiagnostics([registration]);
if (result.TryGetIndex(out var index))
{
    // Publish the complete immutable index.
}
```

Builders are single-writer mutable sources of canonical registrations. Use
`Add(PattrnRegistration<,>)` or convenience `Add`/`AddPattern` methods; the
canonical overload returns a `RegistrationId`. Use `Replace` and `Remove` by ID,
`Clear`, and `ToRegistrations` to inspect or mutate the source list. Existing
compiled indexes are immutable snapshots.

## Compilation and diagnostics

`PattrnIndex.Compile` is the normal static compiler. `CompileWithDiagnostics`
returns an immutable `PattrnDiagnosticReport`; `BuildWithDiagnostics` delegates
to the same compiler. `PattrnCompileOptions` contains only
`DuplicatePatternPolicy` and `TreatWarningsAsErrors`.

Structural diagnostics use stable `PTRN1001`–`PTRN1005` codes. Duplicate IDs are
always errors. A failed compilation never exposes a partial index.

## Match families

Exact result records:

```csharp
PatternMatch<TValue>[] matches = index.MatchToArray(path);
index.TryMatch(path, destination, out var written);
```

Exact value-only methods are explicit:

```csharp
TValue[] values = index.MatchValuesToArray(path);
index.TryMatchValues(path, valueDestination, out var valueCount);
```

`PatternMatch<TValue>` contains `Value`, `RegistrationId`, and
`ConsumedSegmentCount`. Result ordering is specificity descending, then
registration order ascending.

Best-prefix methods return only the deepest matching prefix:

```csharp
var best = index.MatchPrefixToArray(path);
var bestValues = index.MatchPrefixValuesToArray(path);
```

All-prefix enumeration is shallowest to deepest, with the same ordering within
each depth:

```csharp
var all = index.EnumeratePrefixMatchesToArray(path);
var allValues = index.EnumeratePrefixValuesToArray(path);
```

Use the corresponding `Try...` methods and separate upper bounds when supplying
caller-owned spans. `MatchOptions` does not select prefix behavior; it contains
only `DuplicateValueMatchMode` and its `DeduplicateValues` convenience property.

## Detailed matching

`MatchDetailedToArray` returns owning `PatternMatchDetailed<TSegment, TValue>`
records with immutable captures. Caller-buffer detailed methods return
`PatternMatchDetailedSlice<TValue>` plus a capture span. Use `TryGetCapture` or
`GetCapture` for ordinal capture-name lookup.

## Companion packages

`Pattrn.Strings` normalizes string paths and delegates to the core segmented
index. `Pattrn.Routing` is preview and translates route templates into explicit
pattern segments. `Pattrn.DependencyInjection` only owns registration and
publication of immutable indexes. None of these packages changes generic-core
result ordering or diagnostics semantics.

See [matching semantics](matching-semantics.md), [diagnostics](diagnostics.md),
and [duplicate behavior](duplicate-behavior.md) for the stable contracts.
