# Benchmark results: 3.0.0-alpha.16 full run

Date: 2026-06-15

This report commits the first full BenchmarkDotNet result set for the `3.0.0-alpha.16` pre-beta line.

Environment reported by BenchmarkDotNet:

```text
BenchmarkDotNet v0.15.8
OS: Linux Pop!_OS 24.04 LTS
CPU: AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical / 8 physical cores
.NET SDK: 10.0.301
Runtime: .NET 10.0.9, X64 RyuJIT x86-64-v4
GC: Concurrent Workstation
```

BenchmarkDotNet CSV and Markdown reports are committed under:

```text
docs/benchmark-results/2026-06-15-alpha16/raw/
```

## Executive summary

The result is mostly in line with the product theory:

- the packed index read path is very fast for exact, prefix, parameter, catch-all, wildcard, and failed matches;
- the span-based core APIs are allocation-free in measured read-path scenarios;
- adaptive exact-child lookup appears to be working: wide fan-out lookup is among the fastest scenarios;
- routing helpers are convenient but allocate because they split string paths on every call;
- builder diagnostics and validation are acceptable build-time costs, but not cheap;
- detailed duplicate-heavy matching is the clear outlier and should drive the next optimization increment.

## Core read-path summary

Baseline is the benchmark's naive scan implementation for the same scenario. Speedup values compare `NaiveScan_MatchToArray` to the listed trie method.

| Scenario | Naive scan | Trie span | Span speedup | Detailed spans | Detailed speedup | Span alloc | Array alloc |
| --- | --- | --- | --- | --- | --- | --- | --- |
| ExactOnlySparseDeep | 3.821 µs | 31.686 ns | 120.6x | 34.106 ns | 112.0x | 0 B | 144 B |
| ExactOnlyWideFanOut | 17.360 µs | 16.816 ns | 1032.4x | 20.476 ns | 847.8x | 0 B | 144 B |
| WildcardSparse | 1.219 µs | 74.538 ns | 16.4x | 86.170 ns | 14.1x | 0 B | 224 B |
| WildcardDense | 30.176 µs | 75.947 ns | 397.3x | 89.538 ns | 337.0x | 0 B | 224 B |
| PrefixExactOnly | 31.321 µs | 31.507 ns | 994.1x | 34.507 ns | 907.7x | 0 B | 176 B |
| PrefixWildcard | 32.941 µs | 98.421 ns | 334.7x | 125.139 ns | 263.2x | 0 B | 288 B |
| DuplicateHeavyDeduplicate | 1.538 µs | 168.931 ns | 9.1x | 5.486 µs | 0.3x | 0 B | 1.89 KB |
| DuplicateHeavyPreserveDuplicates | 1.285 µs | 55.513 ns | 23.1x | 616.283 ns | 2.1x | 0 B | 2.92 KB |
| NoMatch | 21.959 µs | 7.605 ns | 2887.5x | 8.229 ns | 2668.5x | 0 B | 48 B |
| ParameterCaptures | 7.112 µs | 35.152 ns | 202.3x | 47.418 ns | 150.0x | 0 B | 144 B |
| CatchAllTerminal | 7.129 µs | 34.736 ns | 205.2x | 53.072 ns | 134.3x | 0 B | 144 B |

## Routing package summary

Routing helpers are intentionally outside the core package. They parse/split strings into generic segments before calling the core index.

| Scenario | Parse | Parse alloc | Split path | Split alloc | Match span | Match span alloc | Detailed spans | Detailed array | Detailed array alloc |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ParseSimple | 33.010 ns | 184 B | 44.240 ns | 200 B | 84.770 ns | 200 B | 104.370 ns | 204.560 ns | 464 B |
| ParseParameters | 101.410 ns | 432 B | 47.800 ns | 200 B | 87.310 ns | 200 B | 116.250 ns | 285.660 ns | 464 B |
| ParseCatchAll | 47.860 ns | 216 B | 42.190 ns | 168 B | 65.730 ns | 168 B | 98.710 ns | 158.450 ns | 480 B |
| MatchDetailed | 96.410 ns | 432 B | 44.930 ns | 200 B | 88.270 ns | 200 B | 105.130 ns | 206.010 ns | 464 B |

## Builder and diagnostics summary

Builder benchmarks measure construction and advisory diagnostics, not read-path matching.

| Scenario | Build | Build alloc | Diagnostics | Diagnostics alloc | Build with validation | Validation alloc |
| --- | --- | --- | --- | --- | --- | --- |
| BuildLargeExact | 4.121 ms | 5.45 MB | 2.407 ms | 3.43 MB | 4.905 ms | 6.23 MB |
| BuildLargeParameters | 1.872 ms | 4.98 MB | 2.655 ms | 3.46 MB | 3.471 ms | 6.14 MB |
| GetDiagnosticsClean | 4.299 ms | 5.45 MB | 2.418 ms | 3.43 MB | 4.910 ms | 6.23 MB |
| GetDiagnosticsAmbiguous | 1.499 ms | 2.79 MB | 1.599 ms | 2.77 MB | 1.903 ms | 4.08 MB |
| ValidateOnBuild | 1.476 ms | 2.79 MB | 1.514 ms | 2.77 MB | 1.874 ms | 4.08 MB |

## Surprising or important findings

### 1. Wide fan-out is excellent

`ExactOnlyWideFanOut` resolves to the span API in `16.816 ns` with `0 B` allocation. This supports the alpha.3 adaptive exact-child lookup decision. This is better than the original concern that packed contiguous children could regress wide nodes.

### 2. No-match is extremely cheap

`NoMatch` resolves through the span API in `7.605 ns` with `0 B` allocation. This is exactly what an indexed matcher should do: fail early without scanning unrelated registrations.

### 3. Parameter and catch-all mechanics are not expensive in the core

`ParameterCaptures` detailed span matching is `47.418 ns` with `0 B` allocation. `CatchAllTerminal` detailed span matching is `53.072 ns` with `0 B` allocation. This supports the decision to keep generic parameter/catch-all mechanics in the core while keeping route syntax separate.

### 4. Routing convenience helpers allocate by design

`RouteIndex_MatchRouteToSpan` allocates `168 B` to `200 B` per call because route helpers split the string path before calling the core. This is acceptable for a preview convenience package, but it should not be marketed as the hot path. Hot callers should pre-segment paths or use a future low-allocation route path API.

### 5. Detailed duplicate-heavy deduplication is the outlier

`DuplicateHeavyDeduplicate` has good value-only span performance at `168.931 ns`, but detailed span matching is `5.486 µs`. This is slower than the naive scan baseline for that scenario. The likely cause is detailed-match value deduplication using linear duplicate checks across detailed matches, combined with many structurally different matches producing the same values.

`DuplicateHeavyPreserveDuplicates` is much better for detailed spans at `616.283 ns`, which reinforces that the slow path is deduplication, not traversal alone.

### 6. Array convenience APIs are useful but visibly allocate

The core span APIs generally allocate `0 B`. Array APIs allocate as expected. Detailed array results allocate more because they materialize match-result objects/arrays and capture arrays. This is acceptable, but documentation should continue to position span APIs as the hot path.

## Improvement backlog from this run

Recommended next implementation work, in priority order:

1. Optimize detailed-match deduplication for duplicate-heavy scenarios.
   - Add a thresholded dedup strategy to `DetailedMatchWriter<TSegment, TValue>`.
   - Reuse the idea already present in value writers/accumulators: linear search for small outputs, hash-based lookup for larger outputs.
   - Carefully decide whether the span API may allocate in this rare case or whether a pooled/internal temporary set is required.

2. Add a low-allocation routing path API.
   - Keep the current `MatchRoute(string, ...)` helpers as convenience APIs.
   - Add an API that lets callers pre-split or stack-split paths before matching.
   - Candidate shape: `RoutePattern.SplitPath(ReadOnlySpan<char>, Span<Range>)`, a `RoutePath` value type, or explicit `ReadOnlySpan<string>` guidance.

3. Reduce `MatchDetailedToArray` allocation pressure.
   - Consider a result shape that shares a single capture backing array across returned results.
   - Or document current behavior as intentionally convenient rather than hot-path optimized.

4. Reduce validation/diagnostic build overhead if it becomes relevant.
   - `BuildWithValidation` currently pays extra allocation/time because diagnostics are computed before building.
   - This is not a read-path issue, so it should come after duplicate detailed matching and routing allocations.

5. Add regression benchmarks around these improvement targets.
   - Especially `DuplicateHeavyDeduplicate` detailed span/array benchmarks and route matching allocation benchmarks.

## Product conclusion

The core matcher is performing like a read-optimized indexed matcher. The most important product claim is now supportable with these caveats:

- core span matching is very fast and allocation-free in the measured scenarios;
- convenience APIs and routing helpers allocate;
- duplicate-heavy detailed deduplication needs optimization before beta-quality performance claims around detailed matching.

Do not use broad claims such as "blazing fast". Prefer precise claims tied to this report.
