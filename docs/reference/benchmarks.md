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
| Builder / validation | Build-time behavior, diagnostics, validation, and large-pattern construction. |
| String helpers | String splitting, normalization, and convenience facade costs. |
| Unclassified | Rows that do not match classification rules. These rows are reported and require follow-up instead of being dropped. |

The grouped summary includes guardrail statuses for protected hot paths:

| Status | Meaning |
|---|---|
| `Pass` | The relevant rows were found and match the expectation. |
| `Review` | Relevant rows were found, but the result needs human review. |
| `Unknown` | The report generator could not confidently find enough evidence. |

`Unknown` is intentionally preferred when evidence is missing or ambiguous. Treat unclassified rows and `Unknown` guardrails as follow-up items before using a run to support a performance-sensitive decision.

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
