# Benchmark results: 3.0.0-alpha.19 focused routing run

Date: 2026-06-15

This is a focused BenchmarkDotNet run for `Pattrn.Benchmarks.RoutingBenchmarks` after the alpha.18 routing allocation work and the alpha.19 artifact cleanup. It is not a full-suite replacement for the alpha.17 report.

Raw BenchmarkDotNet CSV and GitHub Markdown reports are committed under:

```text
docs/benchmark-results/2026-06-15-alpha19-routing/raw/
```

## Executive summary

The focused routing run confirms the expected tradeoff:

- the new caller-buffer and pre-split routes make the routing hot path more explicit;
- pre-split matching through the generic core remains allocation-free;
- `SplitPath(string, Span<string>)` removes the segment-array allocation but still allocates substrings;
- string convenience route helpers now allocate less than alpha.17, but they are slower in several small-path scenarios because pooling temporary arrays costs more CPU than allocating a tiny array;
- routing should remain preview until the product chooses whether convenience helpers optimize for lower allocation, lower latency, or a future span-aware routing index.

## Alpha.17 to alpha.19 focused comparison

Comparable convenience-helper rows from the alpha.17 full report and this focused alpha.19 routing run:

| Scenario | Method | Alpha.17 | Alpha.19 routing | Time ratio | Alpha.17 alloc | Alpha.19 alloc |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| ParseSimple | RouteIndex_MatchRouteToSpan | 85.98 ns | 112.460 ns | 1.31x | 200 B | 144 B |
| ParseSimple | RouteIndex_MatchRouteDetailedToSpans | 127.41 ns | 132.803 ns | 1.04x | 200 B | 144 B |
| ParseSimple | RouteIndex_MatchRouteDetailedToArray | 206.39 ns | 237.350 ns | 1.15x | 464 B | 408 B |
| ParseParameters | RouteIndex_MatchRouteToSpan | 97.12 ns | 116.910 ns | 1.20x | 200 B | 144 B |
| ParseParameters | RouteIndex_MatchRouteDetailedToSpans | 107.72 ns | 124.071 ns | 1.15x | 200 B | 144 B |
| ParseParameters | RouteIndex_MatchRouteDetailedToArray | 205.88 ns | 241.830 ns | 1.17x | 464 B | 408 B |
| ParseCatchAll | RouteIndex_MatchRouteToSpan | 76.04 ns | 82.656 ns | 1.09x | 168 B | 112 B |
| ParseCatchAll | RouteIndex_MatchRouteDetailedToSpans | 102.92 ns | 102.145 ns | 0.99x | 168 B | 112 B |
| ParseCatchAll | RouteIndex_MatchRouteDetailedToArray | 160.39 ns | 183.447 ns | 1.14x | 480 B | 424 B |
| MatchDetailed | RouteIndex_MatchRouteToSpan | 86.12 ns | 102.016 ns | 1.18x | 200 B | 144 B |
| MatchDetailed | RouteIndex_MatchRouteDetailedToSpans | 116.11 ns | 122.589 ns | 1.06x | 200 B | 144 B |
| MatchDetailed | RouteIndex_MatchRouteDetailedToArray | 273.40 ns | 277.493 ns | 1.01x | 464 B | 408 B |


The allocation reduction is real: string route helper span methods typically moved from `168 B`/`200 B` to `112 B`/`144 B`. The time cost is also real: small-path convenience helpers are often about `1.04x` to `1.31x` slower.

## New routing hot-path rows

| Scenario | SplitPath | Split alloc | SplitPath to caller span | Caller split alloc | MatchRoute | MatchRoute alloc | Caller-buffer match | Caller-buffer alloc | Pre-split core match | Pre-split alloc | Detailed route | Detailed alloc | Caller-buffer detailed | Caller-buffer detailed alloc | Pre-split detailed | Pre-split detailed alloc |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| ParseSimple | 48.749 ns | 200 B | 46.313 ns | 144 B | 112.460 ns | 144 B | 88.133 ns | 144 B | 36.640 ns | 0 B | 132.803 ns | 144 B | 120.779 ns | 144 B | 56.267 ns | 0 B |
| ParseParameters | 48.806 ns | 200 B | 47.660 ns | 144 B | 116.910 ns | 144 B | 88.416 ns | 144 B | 38.698 ns | 0 B | 124.071 ns | 144 B | 110.512 ns | 144 B | 62.290 ns | 0 B |
| ParseCatchAll | 40.589 ns | 168 B | 39.829 ns | 112 B | 82.656 ns | 112 B | 64.212 ns | 112 B | 22.223 ns | 0 B | 102.145 ns | 112 B | 84.024 ns | 112 B | 42.985 ns | 0 B |
| MatchDetailed | 45.094 ns | 200 B | 48.439 ns | 144 B | 102.016 ns | 144 B | 93.575 ns | 144 B | 36.948 ns | 0 B | 122.589 ns | 144 B | 110.119 ns | 144 B | 57.516 ns | 0 B |


## Interpretation

### 1. The alpha.18 allocation goal was partially achieved

The avoidable temporary `string[]` allocation is gone from the convenience helpers. The remaining allocations are substring segment allocations because the generic core index stores `string` segments. This is the expected limit of the current generic design.

### 2. The latency result is mixed

The lower-allocation convenience helpers are slower in several small-route scenarios. This is not surprising: `ArrayPool<string>.Shared.Rent/Return` has a fixed cost, and these paths only need two or three segments. For very small route paths, allocating a tiny array can be faster than renting one.

### 3. Pre-split matching is the real hot path

Rows such as `RouteIndex_MatchPreSplitToSpan` and `RouteIndex_MatchPreSplitDetailedToSpans` are allocation-free. This is the best current recommendation for hot routing callers: split once, reuse the segments, then call the core span APIs.

### 4. Caller-buffer split is useful, but not allocation-free

`RoutePattern.SplitPath(string, Span<string>)` avoids allocating the segment array, but it still creates strings for each path segment. It is useful for reducing pressure, but it is not a true zero-allocation route parser.

## Decision

Keep `Pattrn.Routing` preview. The current API is useful and honest, but the benchmark shows that a fully allocation-free raw-string route matcher would require a dedicated span-aware routing index or a larger route-token model. That should not be forced into the generic core.

## Recommended next action

Do not add route optional segments or constraints yet. The next product step should be a beta-readiness decision:

1. accept routing convenience helpers as preview convenience APIs;
2. document pre-split/core APIs as the hot path;
3. optionally consider reverting string convenience helpers to fresh-array allocation if latency matters more than allocation for the preview package;
4. defer true zero-allocation raw-string routing to a future dedicated package/design.
