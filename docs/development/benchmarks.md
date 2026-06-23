# Benchmarks


> Transitional note: this page still documents legacy local benchmark workflows and historical alpha-line benchmark reports. Benchmark reporting is expected to move to CI-owned artifacts and workflow summaries in a dedicated benchmark PR.

> Historical note: alpha-version benchmark references are retained as benchmark-history labels, not as current roadmap milestones.


The benchmark project is part of the product contract. It exists to keep performance claims measurable and to protect the read path from regressions as the matcher becomes more expressive.

Run the reproducible benchmark script with the provided SDK and offline NuGet bundle:

```bash
./eng/benchmark.sh
```

By default, the script runs a real out-of-process BenchmarkDotNet smoke run that completes in constrained sandboxes and verifies the SDK/NuGet/BenchmarkDotNet pipeline.

For a broader dry run across all benchmark classes:

```bash
BENCHMARK_MODE=dry ./eng/benchmark.sh
```

For a short comparative run across all benchmark classes:

```bash
BENCHMARK_MODE=short ./eng/benchmark.sh
```

For a full default BenchmarkDotNet run suitable for a publishable report:

```bash
BENCHMARK_MODE=full ./eng/benchmark.sh
```

You can also pass BenchmarkDotNet arguments directly:

```bash
./eng/benchmark.sh --filter '*ExactOnlyWideFanOut*' --job short
./eng/benchmark.sh --filter '*RoutingBenchmarks*' --job short
./eng/benchmark.sh --filter '*BuilderBenchmarks*' --job dry
```

The script writes the latest generated markdown report to `BenchmarkDotNet.Artifacts/results`. Only copy a result into `docs/benchmark-results/` when it should become committed product evidence.

Raw BenchmarkDotNet output remains under:

```text
BenchmarkDotNet.Artifacts/results
```

## Committed reports

The current full baseline is committed at:

```text
docs/benchmark-results/2026-06-17-alpha30/README.md
```

The latest benchmark pointers are:

```text
docs/benchmark-results/latest.md
docs/benchmark-results/routing-latest.md
docs/benchmark-results/performance-guardrails.md
```

Alpha.30 is now the first official post-roadmap-refresh performance baseline. It preserves the core span hot-path allocation profile, but it also records red flags in exact wide fan-out matching, duplicate-heavy detailed materialization, route parsing, and parameter-heavy build time. Alpha.31 responds by making speed a roadmap gate and by protecting exact-only span/detailed matching with direct fast paths.

The previous baselines remain available for historical comparison:

```text
docs/benchmark-results/2026-06-15-alpha17/README.md
docs/benchmark-results/2026-06-15-alpha19-routing/README.md
```

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

## Reporting rules

Do not add a performance claim to the README unless a committed benchmark report supports it.

When publishing a benchmark report, include:

- SDK and runtime versions;
- BenchmarkDotNet version;
- host CPU/OS;
- benchmark command;
- exact package/source version;
- whether the result is smoke, dry, short, or full/default;
- allocation data.

Smoke and dry runs prove the benchmark pipeline works. They are not publishable product-performance evidence.

## Alpha.17 optimization follow-up

`3.0.0-alpha.17` implements the first optimization target from the alpha.16 benchmark analysis: detailed-match duplicate-heavy deduplication. It adds an ordered-block fast path to the detailed writer, and the committed alpha.17 full run confirms the targeted outlier improved substantially.

## Alpha.18 routing allocation follow-up

`3.0.0-alpha.18` addresses the next benchmark finding in the preview routing package. String-based route helpers now rent the temporary `string[]` used to call the generic core matcher, and `RoutePattern.SplitPath(string, Span<string>)` lets hot callers provide their own segment buffer.

This does not make raw route-string matching allocation-free: each route segment is still materialized as a `string` because the core index is generic over `string` segments. Truly allocation-free matching for raw route strings would require a separate span-of-char-aware routing index, which is intentionally deferred unless benchmarks and users prove it is worth the added package complexity.


## Alpha.31 speed policy

`3.0.0-alpha.31` makes speed a first-class release gate. Future feature increments should not be considered complete until the benchmark comparison says the core span hot paths remain allocation-free and any latency regressions are explained. Use focused benchmark filters after performance-sensitive code changes, and reserve full BenchmarkDotNet runs for important refactors and beta/release decisions.
