# Performance guardrails

Speed is a product constraint for `Pattrn`, not a secondary quality attribute.

The core package exists to provide a read-optimized segmented-pattern index. New features should not silently degrade the value-only read path.

## Protected hot paths

The following surfaces are performance-protected:

| Surface | Expected behavior |
| --- | --- |
| `PattrnIndex<TSegment, TValue>.Match(ReadOnlySpan<TSegment>, Span<TValue>)` | allocation-free for caller-provided spans |
| `TryMatch(ReadOnlySpan<TSegment>, Span<TValue>, out int)` | allocation-free when the destination is large enough |
| `GetMatchCountUpperBound(ReadOnlySpan<TSegment>)` | traversal-only; no allocation |
| pre-split route matching | allocation-free once route segments are already available |

## Non-hot paths

The following surfaces are useful but not part of the strict hot-path promise:

| Surface | Expected behavior |
| --- | --- |
| `MatchToArray(...)` | allocates by design |
| `MatchDetailedToArray(...)` | allocates by design |
| `Explain(...)` with rejected candidates | diagnostics-oriented and allowed to allocate |
| route-template parsing | allowed to allocate because it preserves metadata/diagnostics |
| string convenience matching | may allocate for splitting/normalization |
| build-time validation | may allocate; should stay bounded and measured |

## Regression gates

Before beta, treat the following as blockers unless explicitly accepted in documentation:

- core span hot-path allocation above `0 B`;
- unexplained core hot-path latency regression above roughly `5-10%`;
- pre-split route span matching allocations;
- convenience APIs becoming faster by moving hidden cost into the core matcher;
- diagnostics/explainability work changing the value-only match path.

## Benchmark cadence

Use this cadence:

```text
Tests always.
Focused benchmark after performance-sensitive code changes.
Full benchmark after important engine/writer/build refactors and before beta/release claims.
```

The first official post-roadmap-refresh baseline is:

```text
docs/benchmark-results/2026-06-17-alpha30/
```
