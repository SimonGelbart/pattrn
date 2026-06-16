# Benchmark results: 3.0.0-alpha.17 full run

Date: 2026-06-15

This report commits the full BenchmarkDotNet result set for the `3.0.0-alpha.17` line after the detailed duplicate deduplication optimization.

Environment reported by BenchmarkDotNet:

```text
BenchmarkDotNet v0.15.8
OS: Linux Pop!_OS 24.04 LTS
CPU: AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical / 8 physical cores
.NET SDK: 10.0.301
Runtime: .NET 10.0.9, X64 RyuJIT x86-64-v4
GC: Concurrent Workstation
Global run time: about 34 minutes
Executed benchmarks: 101
```

BenchmarkDotNet CSV and Markdown reports are committed under:

```text
docs/benchmark-results/2026-06-15-alpha17/raw/
```

## Executive summary

The alpha.17 result is mostly in line with the theory and confirms that the alpha.17 optimization worked.

- Core segmented span APIs remain allocation-free in measured read-path scenarios.
- Adaptive exact-child lookup remains strong: `ExactOnlyWideFanOut` resolves through the span API in `15.139 ns` with `0 B` allocation.
- Parameter and catch-all detailed matching remain cheap and allocation-free through span APIs.
- The alpha.17 ordered-block fast path substantially improves the detailed duplicate-heavy deduplication outlier.
- Duplicate-heavy detailed deduplication is still the main core outlier, but it is no longer a multi-microsecond failure.
- Routing helpers still allocate because they split string paths before calling the core. That is expected for the preview convenience layer.

## Alpha.16 to alpha.17 comparison

Key rows from the optimization target and nearby scenarios:

| Scenario | Method | Alpha.16 | Alpha.17 | Alpha.17 / Alpha.16 | Alloc |
| --- | --- | --- | --- | --- | --- |
| DuplicateHeavyDeduplicate | Trie_MatchDetailedToSpans | 5.486 µs | 1.596 µs | 0.29x | 0 B |
| DuplicateHeavyDeduplicate | Trie_MatchDetailedToArray | 6.981 µs | 2.689 µs | 0.39x | 9800 B |
| DuplicateHeavyDeduplicate | Trie_MatchToSpan | 168.931 ns | 175.425 ns | 1.04x | 0 B |
| DuplicateHeavyPreserveDuplicates | Trie_MatchDetailedToSpans | 616.283 ns | 625.639 ns | 1.02x | 0 B |
| DuplicateHeavyPreserveDuplicates | Trie_MatchDetailedToArray | 4.061 µs | 4.379 µs | 1.08x | 23624 B |
| DuplicateHeavyPreserveDuplicates | Trie_MatchToSpan | 55.513 ns | 59.225 ns | 1.07x | 0 B |
| ExactOnlyWideFanOut | Trie_MatchDetailedToSpans | 20.476 ns | 19.125 ns | 0.93x | 0 B |
| ExactOnlyWideFanOut | Trie_MatchDetailedToArray | 68.614 ns | 70.384 ns | 1.03x | 168 B |
| ExactOnlyWideFanOut | Trie_MatchToSpan | 16.816 ns | 15.139 ns | 0.90x | 0 B |
| ParameterCaptures | Trie_MatchDetailedToSpans | 47.418 ns | 49.150 ns | 1.04x | 0 B |
| ParameterCaptures | Trie_MatchDetailedToArray | 147.390 ns | 148.239 ns | 1.01x | 216 B |
| ParameterCaptures | Trie_MatchToSpan | 35.152 ns | 34.298 ns | 0.98x | 0 B |
| CatchAllTerminal | Trie_MatchDetailedToSpans | 53.072 ns | 53.335 ns | 1.00x | 0 B |
| CatchAllTerminal | Trie_MatchDetailedToArray | 155.784 ns | 162.646 ns | 1.04x | 312 B |
| CatchAllTerminal | Trie_MatchToSpan | 34.736 ns | 36.417 ns | 1.05x | 0 B |
| NoMatch | Trie_MatchDetailedToSpans | 8.229 ns | 10.069 ns | 1.22x | 0 B |
| NoMatch | Trie_MatchDetailedToArray | 15.742 ns | 15.950 ns | 1.01x | 0 B |
| NoMatch | Trie_MatchToSpan | 7.605 ns | 7.803 ns | 1.03x | 0 B |

The important result is:

```text
DuplicateHeavyDeduplicate / Trie_MatchDetailedToSpans
alpha.16: 5.486 µs
alpha.17: 1.596 µs
improvement: about 3.4x faster
```

`Trie_MatchDetailedToArray` also improves from `6.981 µs` to `2.689 µs`, while allocation remains unchanged because the array API still materializes detailed result objects and capture arrays.

## Core read-path summary

Baseline is the benchmark's naive scan implementation for the same scenario. Speedup values compare `NaiveScan_MatchToArray` to the listed trie method.

| Scenario | Naive scan | Trie span | Span speedup | Detailed spans | Detailed speedup | Span alloc | Array alloc |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ExactOnlySparseDeep | 3.792 µs | 31.229 ns | 121.4x | 36.767 ns | 103.1x | 0 B | 144 B |
| ExactOnlyWideFanOut | 16.962 µs | 15.139 ns | 1120.4x | 19.125 ns | 886.9x | 0 B | 144 B |
| WildcardSparse | 1.216 µs | 68.339 ns | 17.8x | 88.098 ns | 13.8x | 0 B | 224 B |
| WildcardDense | 31.062 µs | 87.996 ns | 353.0x | 94.154 ns | 329.9x | 0 B | 224 B |
| PrefixExactOnly | 30.876 µs | 31.734 ns | 973.0x | 42.722 ns | 722.7x | 0 B | 176 B |
| PrefixWildcard | 32.172 µs | 100.316 ns | 320.7x | 123.944 ns | 259.6x | 0 B | 288 B |
| DuplicateHeavyDeduplicate | 1.539 µs | 175.425 ns | 8.8x | 1.596 µs | 1.0x | 0 B | 1936 B |
| DuplicateHeavyPreserveDuplicates | 1.251 µs | 59.225 ns | 21.1x | 625.639 ns | 2.0x | 0 B | 2992 B |
| NoMatch | 23.190 µs | 7.803 ns | 2972.0x | 10.069 ns | 2303.1x | 0 B | 48 B |
| ParameterCaptures | 6.953 µs | 34.298 ns | 202.7x | 49.150 ns | 141.5x | 0 B | 144 B |
| CatchAllTerminal | 7.281 µs | 36.417 ns | 199.9x | 53.335 ns | 136.5x | 0 B | 144 B |

## Routing package summary

Routing helpers are intentionally outside the core package. They parse/split strings into generic segments before calling the core index.

| Scenario | Parse | Parse alloc | Split path | Split alloc | Match span | Match span alloc | Detailed spans | Detailed array |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ParseSimple | 33.950 ns | 184 B | 45.180 ns | 200 B | 85.980 ns | 200 B | 127.410 ns | 206.390 ns |
| ParseParameters | 107.090 ns | 432 B | 46.560 ns | 200 B | 97.120 ns | 200 B | 107.720 ns | 205.880 ns |
| ParseCatchAll | 47.740 ns | 216 B | 38.460 ns | 168 B | 76.040 ns | 168 B | 102.920 ns | 160.390 ns |
| MatchDetailed | 97.890 ns | 432 B | 46.050 ns | 200 B | 86.120 ns | 200 B | 116.110 ns | 273.400 ns |

## Builder and diagnostics summary

Builder benchmarks measure construction and advisory diagnostics, not read-path matching.

| Scenario | Build | Build alloc | Diagnostics | Diagnostics alloc | Build with validation | Validation alloc |
| --- | --- | --- | --- | --- | --- | --- |
| BuildLargeExact | 4.064 ms | 5.45 MB | 2.305 ms | 3.43 MB | 4.700 ms | 6.23 MB |
| BuildLargeParameters | 1.829 ms | 4.98 MB | 2.581 ms | 3.46 MB | 3.345 ms | 6.14 MB |
| GetDiagnosticsClean | 4.199 ms | 5.45 MB | 2.419 ms | 3.43 MB | 4.691 ms | 6.23 MB |
| GetDi(...)guous [23] | 1.384 ms | 2.79 MB | 1.545 ms | 2.77 MB | 1.906 ms | 4.08 MB |
| ValidateOnBuild | 1.410 ms | 2.79 MB | 1.517 ms | 2.77 MB | 1.865 ms | 4.08 MB |

## Surprising or important findings

### 1. The alpha.17 optimization worked

The main alpha.16 outlier improved materially:

```text
DuplicateHeavyDeduplicate / Trie_MatchDetailedToSpans
5.486 µs -> 1.596 µs
```

This is inline with the theory. The previous detailed writer was paying repeated duplicate checks over equivalent ordered value blocks. The ordered-block fast path avoids much of that repeated work.

### 2. Detailed duplicate-heavy deduplication is still the core pain point

Even after the improvement, `DuplicateHeavyDeduplicate / Trie_MatchDetailedToSpans` is `1.596 µs`, while value-only `Trie_MatchToSpan` is `175.425 ns` and the naive scan baseline is `1.539 µs`. This means detailed deduplication is now much better, but still not beta-quality if the product wants to claim detailed matching is always faster than naive matching.

The likely remaining cost is not trie traversal. It is detailed-value deduplication plus capture bookkeeping across many structurally different matches that collapse to the same values.

### 3. Wide fan-out remains excellent

`ExactOnlyWideFanOut / Trie_MatchToSpan` is `15.139 ns`, slightly better than the alpha.16 result. This continues to validate the adaptive exact-child lookup design.

### 4. Tiny no-match changes should not drive work

Some no-match helper rows moved by a few nanoseconds. Because the absolute numbers are around `8 ns` to `16 ns`, these differences are not meaningful enough to drive code changes. The product remains excellent at failed lookups.

### 5. Routing allocations remain the clearest companion-package issue

`RouteIndex_MatchRouteToSpan` still allocates `168 B` to `200 B` per call because route helpers split strings. This is expected and should be documented as convenience behavior. A future routing increment can add pre-tokenized or caller-buffered APIs.

## Improvement backlog from this run

Recommended next implementation work, in priority order:

1. Continue optimizing detailed duplicate-heavy deduplication only if detailed matching is part of the beta performance promise.
   - Consider a thresholded value-index strategy inside `DetailedMatchWriter<TSegment, TValue>`.
   - Keep the current linear/ordered-block path for small outputs.
   - Avoid adding allocations to the normal detailed span path unless the duplicate-heavy threshold is crossed and the tradeoff is measured.

2. Add a targeted benchmark mode for expensive full-suite alternatives.
   - Full suite took about 34 minutes.
   - Full runs should happen after important refactors, before beta/release, and before publishing performance claims.
   - Smaller changes should use focused BenchmarkDotNet filters.

3. Reduce routing-helper allocation in the preview routing package.
   - Keep `MatchRoute(string, ...)` as convenience.
   - Add a low-allocation route path API only if it stays clearly in `Pattrn.Routing`.

4. Keep array APIs documented as convenience APIs.
   - Span APIs are the hot path.
   - Array/detailed-array APIs allocate by design.

## Benchmark cadence recommendation

Full BenchmarkDotNet runs are expensive enough that they should not be required after every small cleanup.

Use this cadence:

- **Full run** after important engine/writer/build refactors, before beta/RC/stable, and before changing README performance claims.
- **Focused run** after a targeted optimization, for example `*DuplicateHeavy*` or `*RoutingBenchmarks*`.
- **Smoke/dry run** for benchmark project build/discovery after documentation or benchmark-suite edits.
- **Normal test suite** for regular API/docs/behavior changes.

A good practical rule:

```text
Tests always.
Focused benchmark after performance-sensitive code changes.
Full benchmark only after important refactoring or before release decisions.
```

## Product conclusion

The core matcher is still performing like a read-optimized indexed matcher. Alpha.17 fixed the largest outlier enough to validate the chosen optimization direction. The remaining performance work should be selective, not broad: detailed duplicate-heavy matching and routing-helper allocations are the only clear optimization targets from this run.
