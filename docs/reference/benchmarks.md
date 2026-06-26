# Benchmarks

Benchmark evidence should come from the benchmark workflow, not from locally committed machine output.

The benchmark project is part of the product contract. It exists to keep performance claims measurable and to protect the read path from regressions as the matcher becomes more expressive.

## Source of truth

The `Benchmarks` GitHub Actions workflow is the benchmark source of truth for current commits. Each run should produce:

- raw BenchmarkDotNet artifacts;
- workflow metadata;
- grouped benchmark summaries;
- a GitHub job summary;
- a downloadable `benchmark-results-<run-id>-<attempt>` artifact.

Do not treat local benchmark output or old committed benchmark reports as current product proof. Git history is sufficient for retired local reports.

## Grouped reports

The benchmark workflow generates grouped reports from the BenchmarkDotNet full compressed JSON produced by that workflow run. The grouped reports are derived evidence, not hand-maintained performance tables.

Each benchmark artifact should contain:

```text
benchmark-ci-results/
  metadata.json
  raw/
    BenchmarkDotNet.Artifacts/
      results/
        ...
  summaries/
    grouped-summary.md
    grouped-results.json
```

The workflow remains the source of truth for current benchmark evidence. Retired local reports must not be cited as proof for current performance claims.

Rows are classified deterministically into these groups:

| Group | Purpose |
|---|---|
| Core hot path | Value matching and protected read-path operations. |
| Detailed matching | Match metadata, captures, deduplication, and materialization. |
| Routing preview | Route parsing, route splitting, route helper matching, and pre-split route matching. |
| Builder / validation | Build-time behavior, builder diagnostics, validation, and large-pattern construction. |
| Diagnostics | Matcher explanation and opt-in rejected-candidate diagnostics. These rows are non-hot-path allocation evidence, not protected core hot-path proof. |
| String helpers | String splitting, normalization, and convenience facade costs. |
| Unclassified | Rows that do not match classification rules. These rows are reported and require follow-up instead of being dropped. |

The grouped summary includes guardrail statuses for protected hot paths:

| Status | Meaning |
|---|---|
| `Pass` | The relevant rows were found and match the expectation. |
| `Review` | Relevant rows were found, but the result needs human review. |
| `Unknown` | The report generator could not confidently find enough evidence. |

`Unknown` is intentionally preferred when evidence is missing or ambiguous. Treat unclassified rows and `Unknown` guardrails as follow-up items before using a run to support a performance-sensitive decision.


## Allocation smoke tests

Release-mode allocation smoke tests complement the benchmark workflow by checking protected hot paths with `GC.GetAllocatedBytesForCurrentThread()` after warmup. They are fast guardrails for ordinary validation and should stay small, deterministic, and allocation-focused.

Smoke tests are not publishable benchmark evidence and must not be used to strengthen README performance claims. BenchmarkDotNet artifacts from the `Benchmarks` workflow remain the source of performance evidence for current commits.

## Coverage matrix

This matrix maps product/performance claims to current smoke-test and BenchmarkDotNet coverage.

`Present` means the repository contains current test or benchmark coverage for that row.
`Missing` means the claim is intentionally not covered yet and should link to a follow-up issue when known.
`Unknown` means coverage could not be confirmed from the current docs, scripts, or benchmark project.
`Not protected` means the behavior is intentionally outside the protected allocation-sensitive hot path, even when it has functional tests or benchmarks.

Do not treat this matrix as benchmark-result evidence. Current performance proof still comes from workflow-produced benchmark artifacts for the commit under review.

| Area | Claim / behavior | Smoke-test coverage | BenchmarkDotNet coverage | Evidence source | Status | Follow-up |
|---|---|---|---|---|---|---|
| Core hot path | `Match(..., Span<TValue>)` for segmented paths writes into caller-provided buffers. | Present | Present | `tests/Pattrn.Tests/PerformanceSmokeTests.cs`; `PattrnIndexBenchmarks.Trie_MatchToSpan`; workflow `Core hot path` grouped rows | Covered |  |
| Core hot path | `TryMatch(...)` with a sufficient destination reports success without materializing result arrays. | Present | Present | `tests/Pattrn.Tests/PerformanceSmokeTests.cs`; `PattrnIndexBenchmarks.Trie_TryMatchToSpan_SufficientDestination`; workflow `Core hot path` grouped rows | Covered |  |
| Core hot path | `TryMatch(...)` with an insufficient destination reports failure without materializing result arrays. | Not protected | Present | `tests/Pattrn.Tests/TryMatchTests.cs`; `PattrnIndexTryMatchFailureBenchmarks.Trie_TryMatchToSpan_InsufficientDestination`; workflow `Core hot path` grouped rows | Covered |  |
| Core hot path | `GetMatchCountUpperBound(...)` provides caller-buffer sizing for segmented paths. | Present | Present | `tests/Pattrn.Tests/PerformanceSmokeTests.cs`; `PattrnIndexBenchmarks.Trie_GetMatchCountUpperBound`; workflow `Core hot path` grouped rows | Covered |  |
| Detailed matching | `MatchDetailed(...)` writes matches and captures into caller-provided buffers. | Present | Present | `tests/Pattrn.Tests/PerformanceSmokeTests.cs`; `PattrnIndexBenchmarks.Trie_MatchDetailedToSpans`; workflow `Detailed matching` grouped rows | Covered |  |
| Convenience API | `MatchToArray` and materializing core APIs allocate result arrays for convenience. | Not protected | Present | `PattrnIndexBenchmarks.Trie_MatchToArray`; `PattrnIndexBenchmarks.Trie_MatchDetailedToArray`; workflow `Detailed matching` grouped rows | Covered |  |
| String helpers | `Pattrn.Strings` splitting, normalization, and string convenience facade costs stay separate from core segmented span claims. | Not protected | Present | `StringHelperBenchmarks.String_MatchDottedToSpan`; `StringHelperBenchmarks.String_MatchSeparatedToSpan`; `StringHelperBenchmarks.String_MatchDottedToArray`; `StringHelperBenchmarks.String_MatchWithNormalizationOptions`; workflow `String helpers` grouped rows | Covered |  |
| Routing preview | Pre-split route matching uses the core segmented matcher after route paths are already split. | Unknown; functional route span tests exist, but no current allocation smoke test was found. | Present | `tests/Pattrn.Routing.Tests/RoutePatternExtensionTests.cs`; `RoutingBenchmarks.RouteIndex_MatchPreSplitToSpan`; workflow `Routing preview` grouped rows | Partial |  |
| Routing preview | Route parsing, route splitting, and route convenience matching remain preview and are not core hot-path proof. | Not protected | Present | `RoutingBenchmarks.RoutePattern_Parse`; `RoutingBenchmarks.RoutePattern_SplitPath`; `RoutingBenchmarks.RoutePattern_SplitPathToSpan`; `RoutingBenchmarks.RouteIndex_MatchRouteToSpan`; workflow `Routing preview` grouped rows | Covered |  |
| Builder / validation | Builder construction, diagnostic scanning, and opt-in build validation costs are build-time behavior. | Not protected | Present | `BuilderBenchmarks.Build`; `BuilderBenchmarks.GetDiagnostics`; `BuilderBenchmarks.BuildWithValidation`; workflow `Builder / validation` grouped rows | Covered |  |
| Diagnostics | Explanation and diagnostic paths stay outside default matching hot paths. | Not protected | Present | `BuilderBenchmarks.GetDiagnostics`; `ExplainBenchmarks.Explain_MatchingPath`; `ExplainBenchmarks.Explain_MatchingPathWithRejectedCandidates`; `ExplainBenchmarks.Explain_NoMatchWithRejectedCandidates`; `ExplainBenchmarks.Explain_CaptureHeavyPath`; workflow `Diagnostics` grouped rows | Covered |  |
| Benchmark pipeline | Grouped summary generation classifies BenchmarkDotNet rows into documented report categories and emits workflow artifacts. | N/A | N/A | `.github/workflows/benchmarks.yml`; `tools/benchmarks/summarize_benchmarks.py` | Covered | [#31](https://github.com/SimonGelbart/pattrn/issues/31) |
| Benchmark pipeline | Baseline comparison for candidate benchmark runs. | N/A | N/A | No baseline comparison step or script was found in the benchmark workflow. | Missing | [#32](https://github.com/SimonGelbart/pattrn/issues/32) |

## Benchmark groups

Current grouped reports classify existing benchmark rows by benchmark class and method names. Avoid renaming benchmark methods solely for report presentation because stable names make historical comparison easier.

## Benchmark classes

### `PattrnIndexBenchmarks`

Covers the generic core matcher:

- exact-only sparse/deep lookup;
- exact-only wide fan-out lookup;
- sparse wildcard traversal;
- dense wildcard traversal;
- prefix exact-only matching;
- prefix wildcard matching;
- duplicate-heavy matching with deduplication;
- duplicate-heavy matching preserving duplicates;
- no-match lookup;
- named-parameter captures;
- terminal catch-all captures.

Measured APIs include:

- `GetMatchCountUpperBound`;
- `Match` into caller-provided spans;
- `TryMatch` into caller-provided spans with sufficient and insufficient destinations;
- `MatchToArray`;
- `MatchDetailed` into caller-provided match/capture spans;
- `MatchDetailedToArray`;
- naive scan baseline.

### `StringHelperBenchmarks`

Covers the `Pattrn.Strings` convenience helper layer:

- dotted string matching into caller-provided value spans;
- separated string matching with non-dot separators;
- dotted `MatchToArray` materialization;
- explicit `StringNormalizationOptions` with trimming, ignored empty segments, and segment normalization.

These benchmarks intentionally measure string splitting, parsing, normalization, and convenience materialization costs. They are separate from the generic core's already-segmented span hot-path claims, and allocations in these rows do not weaken the protected core hot-path guardrails.

### `RoutingBenchmarks`

Covers the optional routing companion package only:

- route-template parsing;
- route-path splitting;
- route helper matching;
- route detailed matching.

These benchmarks intentionally include allocation-heavy string parsing/splitting work. They should not be mixed with claims about the generic core's segmented span APIs.

### `BuilderBenchmarks`

Covers build-time behavior:

- large exact-pattern builds;
- large parameter-pattern builds;
- diagnostic scanning on clean builders;
- diagnostic scanning on ambiguous builders;
- opt-in build validation overhead.

### `ExplainBenchmarks`

Covers matcher explanation and opt-in diagnostic paths:

- accepted-match explanation without rejected-candidate diagnostics;
- accepted-match explanation with rejected-candidate diagnostics;
- no-match explanation with rejected-candidate diagnostics;
- capture-heavy explanation through terminal catch-all captures.

These benchmarks intentionally measure diagnostics-oriented work performed by `Explain(...)`: copying the explained path, materializing detailed match results, and optionally collecting rejected candidates. They are separated into the `Diagnostics` group and must not be cited as protected core hot-path evidence or compared against `Match(..., Span<TValue>)` as a competing hot-path API.

## Local preflight

Local benchmark commands are optional maintainer preflight helpers. They are useful for smoke checks or local investigation, but they are not the current source of benchmark truth.

If a local run is still useful, run the benchmark project directly:

```bash
dotnet run \
  --project benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj \
  --configuration Release \
  -- \
  --filter "*" \
  --join \
  --exporters json \
  --exporters markdown \
  --strategy Throughput \
  --artifacts BenchmarkDotNet.Artifacts
```

## Reporting rules

Do not add or strengthen a performance claim in the README unless CI-owned benchmark artifacts support it.

When reviewing benchmark artifacts, record:

- workflow run ID;
- commit SHA;
- SDK and runtime versions;
- BenchmarkDotNet version;
- benchmark filter;
- benchmark strategy;
- whether the run was full or focused;
- allocation data when relevant.

Smoke and dry runs prove the benchmark pipeline works. They are not publishable product-performance evidence.
