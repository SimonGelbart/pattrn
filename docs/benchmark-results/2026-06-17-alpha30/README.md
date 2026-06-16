# Benchmark results: 3.0.0-alpha.30 full run

Date: 2026-06-17

This report commits the uploaded BenchmarkDotNet result set for the `3.0.0-alpha.30` line and makes it the first performance baseline after the roadmap refresh and routing expansion hardening pass.

Environment reported by BenchmarkDotNet:

```text
BenchmarkDotNet v0.15.8
OS: Linux
CPU: AMD Ryzen 9 PRO 8945HS w/ Radeon 780M Graphics, 1 CPU, 16 logical / 8 physical cores
.NET SDK: 10.0.301
Runtime: .NET 10.0.9, X64 RyuJIT x86-64-v4
GC: Concurrent Workstation
Executed benchmarks: 125
```

Raw BenchmarkDotNet CSV, GitHub Markdown, and HTML reports are committed under:

```text
docs/benchmark-results/2026-06-17-alpha30/raw/
```

Comparison CSVs are committed under:

```text
docs/benchmark-results/2026-06-17-alpha30/comparison/
```

## Executive summary

`3.0.0-alpha.30` preserves the main product thesis: the compiled trie/index remains much faster than naive scanning, and the core span-based hot paths remain allocation-free. However, the comparison against the previous committed baselines is mixed and should influence the next roadmap step.

The important conclusions are:

- core `Trie_MatchToSpan` remains allocation-free in all measured core read-path scenarios;
- most core hot-path rows are stable or improved;
- `ExactOnlyWideFanOut / Trie_MatchToSpan` regressed against alpha.17 and must be treated as a speed red flag;
- duplicate-heavy detailed matching regressed and remains the main non-hot-path materialization concern;
- route-template parsing is materially slower because alpha.28-alpha.30 preserve richer route metadata;
- pre-split route matching remains allocation-free and is still the routing hot path;
- build-time parameter-heavy construction regressed and should be investigated before beta.

## Baselines used for comparison

| Area | Previous baseline | alpha.30 comparison file |
| --- | --- | --- |
| Core read path | `2026-06-15-alpha17` | `comparison/core-alpha17-to-alpha30.csv` |
| Builder/diagnostics | `2026-06-15-alpha17` | `comparison/builder-alpha17-to-alpha30.csv` |
| Routing | `2026-06-15-alpha19-routing` | `comparison/routing-alpha19-to-alpha30.csv` |

## Core read-path highlights

| Scenario | Method | Previous | alpha.30 | Direction |
| --- | --- | ---: | ---: | --- |
| ExactOnlySparseDeep | `Trie_MatchToSpan` | 31.229 ns | 31.301 ns | stable |
| ExactOnlyWideFanOut | `Trie_MatchToSpan` | 15.139 ns | 27.034 ns | red flag |
| WildcardDense | `Trie_MatchToSpan` | 87.996 ns | 72.919 ns | improved |
| PrefixWildcard | `Trie_MatchToSpan` | 100.316 ns | 92.178 ns | improved |
| NoMatch | `Trie_MatchToSpan` | 7.803 ns | 7.640 ns | stable/improved |
| ParameterCaptures | `Trie_MatchToSpan` | 34.298 ns | 35.138 ns | stable |
| CatchAllTerminal | `Trie_MatchToSpan` | 36.417 ns | 36.319 ns | stable |

Allocation stayed at `0 B` for the core span hot path rows.

## Red flags to triage

| Area | Observation | alpha.31 decision |
| --- | --- | --- |
| Exact wide fan-out span matching | `15.139 ns -> 27.034 ns` | protect with an exact-only direct span fast path |
| Duplicate-heavy detailed preserve-duplicates | `625.639 ns -> 1,122.453 ns` | keep out of the hot-path promise; investigate after core span speed is protected |
| Duplicate-heavy detailed deduplicate | `1.596 us -> 1.771 us` | monitor; avoid optimizing before ranking/specificity decisions unless it worsens |
| Route parsing | roughly `2x` slower | document as expected cost of richer route metadata; parsing is not the matching hot path |
| BuildLargeParameters | `1.829 ms -> 3.497 ms` | investigate before beta; build time is secondary to read-path speed |

## Routing highlights

Pre-split route matching remains the routing hot path:

| Scenario | Method | Previous | alpha.30 | Allocation |
| --- | --- | ---: | ---: | ---: |
| ParseSimple | `RouteIndex_MatchPreSplitToSpan` | 36.640 ns | 38.521 ns | 0 B |
| ParseParameters | `RouteIndex_MatchPreSplitToSpan` | 38.698 ns | 39.471 ns | 0 B |
| ParseCatchAll | `RouteIndex_MatchPreSplitToSpan` | 22.223 ns | 22.104 ns | 0 B |
| MatchDetailed | `RouteIndex_MatchPreSplitToSpan` | 36.948 ns | 35.588 ns | 0 B |

Raw-string route helpers remain convenience APIs. They split string paths and are not the primary performance surface.

## Speed policy established by alpha.31

Future increments should treat speed as a release gate:

1. core span hot paths must remain allocation-free;
2. unexplained hot-path regression above roughly `5-10%` should block feature work;
3. pre-split route matching must stay allocation-free;
4. diagnostics, parsing, and array materialization may allocate, but their costs must be documented and isolated;
5. benchmark comparisons should be updated after performance-sensitive code changes.

## Product conclusion

The library remains aligned with the high-performance indexing goal, but alpha.30 proves that performance needs to be managed explicitly. Alpha.31 therefore shifts the roadmap from feature expansion to speed guardrails and hot-path triage before specificity/ranking customization.
