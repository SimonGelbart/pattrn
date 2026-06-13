# Architecture

`Pattrn` is built around a clear separation between mutation and matching.

The library targets .NET 10 only.


## Package layers

The v3 track separates core matching, string convenience APIs, and framework integration:

```text
Pattrn
  Dependency-free core package. Contains the builder, immutable index, `IPattrnIndex<TSegment, TValue>`, generic `PatternSegment<TSegment>` model, and options.

Pattrn.Strings
  Optional separated and dotted string helpers plus explicit string normalization options built on top of the segmented core APIs.

Pattrn.DependencyInjection
  Companion DI package. Contains `IServiceCollection` registration extensions and depends on `Microsoft.Extensions.DependencyInjection.Abstractions`.
```

This keeps hot-path consumers from taking string-helper or DI dependencies while making application composition idiomatic for Microsoft.Extensions.DependencyInjection users.

## Builder representation

`PattrnIndexBuilder<TSegment, TValue>` is mutable and intentionally not thread-safe. It keeps mutation simple while avoiding unnecessary heap use in leaf-heavy indexes:

- exact segment children are stored in lazily allocated dictionaries using the configured segment comparer;
- leaf nodes do not allocate child dictionaries;
- the single-segment wildcard child is stored separately from exact children;
- generic `PatternSegment<TSegment>` registrations map literals to exact children, wildcard/parameter segments to the wildcard child, and terminal catch-all segments to a separate catch-all child;
- values and registration-level metadata are appended to the terminal node for each distinct pattern.

The recommended registration style for the core package is fluent and explicit:

```csharp
var index = PattrnIndex<string, string>
    .Builder()
    .AddPattern(
        [
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Literal("NASDAQ"),
            PatternSegment<string>.Wildcard()
        ],
        "client-a")
    .Build();
```

The default alpha.23 builder is tokenless. Tokenized convenience registration remains available through factories that accept a wildcard token, but explicit `AddPattern(...)` users no longer need to reserve any segment value.

The direct factory remains available:

```csharp
var builder = PattrnIndexBuilder<string, string>.Create();
var tokenizedBuilder = PattrnIndexBuilder<string, string>.Create("*");
```

## Compiled representation

`Build()` snapshots the builder into a packed immutable representation. Compiled indexes are safe for concurrent readers and do not mutate while matching. Compilation uses iterative traversal rather than recursive node compilation, so very deep registered patterns do not consume call-stack depth during build.

The runtime representation is array-based:

- `CompiledNode[]` stores node metadata and integer ranges;
- `CompiledChild<TSegment>[]` stores exact child entries contiguously;
- wide nodes optionally use packed open-addressed lookup slots for exact-child lookup;
- values are stored in one mode-specific contiguous `TValue[]`;
- registration metadata is stored in parallel `CompiledValueDetail[]` and capture descriptor ranges;
- deduplicating indexes compile each node to a locally deduplicated value range;
- duplicate-preserving indexes compile each node to its raw registration value range;
- wildcard and catch-all children are stored as integer node indexes.

The read path traverses only plausible branches:

1. the exact child for the current segment, when present;
2. the wildcard child, when present;
3. the terminal catch-all child, when present.

This keeps matching proportional to path depth and relevant wildcard branching instead of scanning all registered patterns. The packed representation avoids per-node object graphs in the immutable read path and makes traversal use integer node indexes rather than object references.

When a compiled index contains no wildcard branches, the matcher selects an exact-only fast path. Exact-only matching descends through one exact child per input segment and does not consider wildcard alternatives. Prefix-enabled exact-only matching walks the same linear path while collecting prefix node values. Small child ranges use linear scan; larger fan-out nodes use a per-node packed lookup table built with the configured segment comparer. This keeps narrow trie nodes compact while avoiding wide-node scans in dictionary-like workloads.

Wildcard/catch-all-capable matching uses an iterative traversal stack instead of recursive calls. The common traversal stack is backed by `stackalloc`; it rents from `ArrayPool<T>` only when wildcard branching exceeds the stack buffer. The traversal preserves the same exact-child-before-wildcard semantics while avoiding recursion depth as an input-path concern.

## Matching semantics

Default matching is exact-length:

```text
pattern: NASDAQ
path:    NASDAQ.MSFT
result:  no match
```

Starter-kit/reference-compatible prefix matching is available explicitly:

```csharp
var index = builder.Build(MatchOptions.Prefix);
```

With prefix matching enabled:

```text
pattern: NASDAQ
path:    NASDAQ.MSFT
result:  match
```

The legacy wildcard token is a single-segment wildcard, not a recursive glob. Generic terminal catch-all semantics are available through `PatternSegment<TSegment>.CatchAll()` and remain independent of string route syntax. See [matching semantics](../reference/matching-semantics.md) for the complete behavior contract. Result ordering is intentionally not part of the public contract so the internal traversal and child-map strategy can evolve.

## Deduplication

Deduplication is enabled by default, including when a value is reached through overlapping exact and wildcard patterns. It uses the configured value comparer.

Compiled nodes store a single value range. When `MatchOptions.DeduplicateValues` is enabled, the compiler writes locally deduplicated per-node values; when deduplication is disabled, it writes raw registration values. This avoids carrying both raw and locally deduplicated value blocks in every compiled index. Detailed match metadata is stored in parallel with the compiled value range so the value-only hot path can continue reading contiguous `TValue` spans without constructing capture objects.

`Explain(...)` composes the detailed matching layer with diagnostics-oriented allocation. It copies the input path, returns accepted detailed matches, and only collects rejected-candidate hints when requested through `PatternExplanationOptions`. This keeps explanation separate from the hot value-only path.

The match accumulator also has an ordered-block fast path. Overlapping wildcard branches often produce the same ordered value block repeatedly, especially in subscription-routing scenarios. The fast path skips the whole block when it has already been emitted, avoiding repeated per-value linear duplicate checks while keeping the span overload allocation-free.

## Builder lifecycle

The builder owns mutation-only concerns such as `AddRange`, `Contains`, `RemoveAll`, `Clear`, and advisory diagnostics. Removing the final value from a pattern prunes empty branches so long-lived builders do not keep dead trie paths around unnecessarily.

`GetDiagnostics()` walks the mutable builder trie and reports generic pattern-set concerns such as duplicate structural patterns, ambiguous parameter names, wildcard/literal overlap, and catch-all overlap. Diagnostics do not affect matching and are not route-specific policy. Higher-level packages can decide whether to treat them as warnings or errors.

## String helpers and normalization

Dotted and generic separated-path helpers are convenience APIs. They allocate because they split strings into segment arrays. Prefer the span-based APIs for hot paths.

The core package does not parse or normalize strings. `Pattrn.Strings` owns that boundary through `StringNormalizationOptions`, which can configure separators, case-sensitivity for created builders, empty-segment handling, whitespace trimming, and custom segment normalization. The char-based helpers keep the conservative default behavior: reject empty strings/segments and preserve segments exactly.

`StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` are ergonomic facades over the generic string-segment builder and index. They store the normalization options once and delegate to the same core matching engine. They do not introduce route-template parsing, URL decoding, filesystem normalization, glob semantics, or framework behavior.

Route, URL, filesystem, glob, and application-specific semantics should be implemented above this layer rather than in the generic core or generic string helper layer.

## Dependency injection lifecycle

The DI package builds the mutable builder once when the singleton index is created and registers the compiled immutable index as a singleton. Both the concrete `PattrnIndex<TSegment, TValue>` and `IPattrnIndex<TSegment, TValue>` resolve to the same instance for default indexes. Named indexes are registered as keyed singletons and can be resolved through `IPattrnProvider<TSegment, TValue>` or keyed DI APIs.

## Benchmark project

`benchmarks/Pattrn.Benchmarks` keeps a performance baseline for the starter-kit-inspired cases. It compares the compiled trie matcher with a deliberately naive scan across exact, wildcard, prefix, and duplicate-heavy scenarios. See [benchmarks](../development/benchmarks.md).

## Builder policies

The builder remains mutable and permissive by default. Duplicate structural patterns append values unless callers opt into `DuplicatePatternRegistrationBehavior.Throw`, `Replace`, or `Ignore`. This is a registration-time policy and does not change compiled-index matching semantics.

Build validation is also opt-in. `ValidateOnBuild(...)` evaluates the generic diagnostics produced by `GetDiagnostics()` and can reject builds by severity or caller-supplied predicate. This keeps domain-specific strictness in applications or companion packages while keeping the core package generic.
