# ADR 0010: Performance guardrails are release gates

## Status

Accepted

## Context

Pattrn's value proposition depends on fast repeated reads against an immutable compiled index. Feature work that harms hot-path matching without explanation weakens the product.

## Historical context

The alpha performance work added committed benchmark baselines, guardrail documentation, and exact-only fast paths. Benchmark notes identify allocation and latency-sensitive paths that should remain protected.

## Decision

Performance guardrails are release gates for performance-sensitive changes.

Hot paths must not gain hidden allocations or unexplained latency regressions. Diagnostics and explanations should remain optional and outside default matching paths.

## Consequences

Feature increments may need focused benchmarks before they are considered complete. Code changes that look small can still require performance evidence.

Maintainers should avoid benchmark noise by using focused runs for local changes and full runs for major refactors or release decisions.

## Alternatives considered

- Treat benchmarks as occasional documentation only: rejected because performance is part of the product promise.
- Optimize everything before beta: rejected because only protected hot paths need strict guardrails now.

## Follow-up work

Refresh benchmark baselines before beta and document allocation expectations for protected paths.
