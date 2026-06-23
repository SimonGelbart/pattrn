# Documentation

Pattrn documentation is organized around the current pre-beta product state. Start here when you want current user-facing docs; historical pre-beta notes are kept separately so they do not obscure the active documentation path.

Start with the [roadmap](roadmap.md) for product direction, current status, and the next increment.

## Current product docs

- [Root README](../README.md) for the short product overview and first examples.
- [Roadmap](roadmap.md) for current product direction.
- [Project profile](reference/project-profile.md) for durable project facts, package family, and stability posture.
- [Known limitations](reference/limitations.md) for current boundaries and deferred behavior.

## Tutorials

- [Build your first Pattrn index](tutorials/first-index.md)

## Package and usage docs

Package README sources live under `docs/packages/` so package-scoped NuGet guidance is separate from older getting-started material.

- [Core package](packages/pattrn.md)
- [String helpers](packages/pattrn-strings.md)
- [Routing companion package](packages/pattrn-routing.md)
- [Dependency injection](packages/pattrn-dependency-injection.md)
- [Examples](tutorials/examples.md)

## How-to guides

- [Select a best match](how-to/select-best-match.md)

## Explanation

- [Package boundaries](explanation/package-boundaries.md)

## Reference

- [Core API](reference/api.md)
- [Generic pattern segments](reference/pattern-segments.md)
- [Matching semantics](reference/matching-semantics.md)
- [Ranking and specificity](reference/ranking-specificity.md)
- [Compatibility semantics](reference/compatibility-semantics.md)
- [Duplicate behavior](reference/duplicate-behavior.md)
- [Diagnostics](reference/diagnostics.md)
- [Known limitations](reference/limitations.md)

## Maintainer guidance

For assisted or maintainer work, start with:

- [`../AGENTS.md`](../AGENTS.md)
- [Project profile](reference/project-profile.md)
- [Repository layout](reference/repository-layout.md)
- [Git workflow](reference/git-workflow.md)
- [Validation](reference/validation.md)
- [Documentation standards](reference/documentation-standards.md)
- [Architecture decisions](adr/README.md)

New durable documentation should follow the Diataxis folders: `tutorials/`, `how-to/`, `reference/`, `explanation/`, and `adr/`. Older pre-beta folders have been moved under `docs/archive/pre-beta/` while their content is reconciled or migrated.

## Architecture decisions

- [ADR index](adr/README.md)

ADRs are architecture decision records. They preserve decision history and should not be used as a general documentation bucket.

## Historical and transitional material

The following areas contain pre-beta history, maintainer notes, local benchmark evidence, or transitional documentation. They remain available for context, but they are not the primary current user path.

### Design history

- [Architecture](archive/pre-beta/design/architecture.md)
- [State-of-the-art architecture review](archive/pre-beta/design/state-of-the-art-architecture-review.md)

### Maintainer and legacy local workflow notes

- [Benchmarks](reference/benchmarks.md)
- [Building offline](reference/building-offline.md)
- [Packaging](reference/packaging.md)
- [Testing](reference/testing.md)
- [Package README mapping](reference/package-readmes.md)

### Release history and planning notes

- [Package maturity](archive/pre-beta/release/package-maturity.md)
- [API stabilization](archive/pre-beta/release/api-stabilization.md)
- [API freeze policy](archive/pre-beta/release/api-freeze.md)
- [Beta checklist](archive/pre-beta/release/beta-checklist.md)
- [Beta readiness review](archive/pre-beta/release/beta-readiness-review.md)
- [Release checklist](archive/pre-beta/release/release-checklist.md)
- [Release decisions](archive/pre-beta/release/release-decisions.md)
- [Historical migration notes](archive/pre-beta/release/migration-alpha.md)

### Historical benchmark results

Local committed benchmark reports are historical evidence, not the benchmark source of truth. Current benchmark evidence should come from CI-owned workflow artifacts and summaries.

- [Historical full benchmark baseline](benchmark-results/latest.md)
- [Historical routing benchmark baseline](benchmark-results/routing-latest.md)
- [Performance guardrails](benchmark-results/performance-guardrails.md)
- [Committed benchmark result runs](benchmark-results/)

## Archive

Historical notes and pre-release migration documents live under the [archive index](archive/README.md).
