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

Do not treat local benchmark output or old committed benchmark reports as current product proof. Keep old reports only as historical evidence.

## Benchmark groups

The current workflow groups existing benchmark classes as follows:

| Group | Existing benchmark classes | Purpose |
|---|---|---|
| Core matching | `PattrnIndexBenchmarks` | Generic segmented matching, detailed matches, captures, duplicates, and naive baseline comparison. |
| Routing preview | `RoutingBenchmarks` | Route-template parsing, path splitting, route helper matching, and route detailed matching. |
| Builder and diagnostics | `BuilderBenchmarks` | Build-time behavior, large pattern sets, diagnostics, and validation overhead. |
| String helpers | Existing string-helper tests for now. Add focused benchmarks only if a future performance question needs them. | Separated string ergonomics and normalization scenarios. |

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
- `MatchToArray`;
- `MatchDetailed` into caller-provided match/capture spans;
- `MatchDetailedToArray`;
- naive scan baseline.

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

## Historical reports

Committed benchmark reports under `docs/benchmark-results/` are historical local evidence. They can explain past decisions, but they should not be treated as current proof for new README performance claims.

- [Historical full benchmark baseline](../benchmark-results/latest.md)
- [Historical routing benchmark baseline](../benchmark-results/routing-latest.md)
- [Performance guardrails](../benchmark-results/performance-guardrails.md)
