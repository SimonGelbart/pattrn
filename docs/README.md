# Documentation

Pattrn documentation is organized around the current pre-beta product state. Start here when you want current user-facing docs; historical alpha-line notes are kept separately so they do not obscure the active documentation path.

Start with the [roadmap](roadmap.md) for product direction, current status, and the next increment.

## Current product docs

- [Root README](../README.md) for the short product overview and first examples.
- [Roadmap](roadmap.md) for current product direction.
- [Project profile](reference/project-profile.md) for durable project facts, package family, and stability posture.
- [Known limitations](reference/limitations.md) for current boundaries and deferred behavior.

## Package and usage docs

These pages are still being reconciled from the older getting-started folder into the durable documentation structure. Treat them as current package-oriented guidance unless a page says it is historical.

- [Examples](getting-started/examples.md)
- [String helpers](getting-started/strings.md)
- [Routing companion package](getting-started/routing.md)
- [Dependency injection](getting-started/dependency-injection.md)

## How-to guides

- [Select a best match](how-to/select-best-match.md)

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

New durable documentation should follow the Diataxis folders: `tutorials/`, `how-to/`, `reference/`, `explanation/`, and `adr/`. Existing alpha-era folders are retained while their content is reconciled, archived, or migrated.

## Architecture decisions

- [ADR index](adr/README.md)

ADRs are architecture decision records. They preserve decision history and should not be used as a general documentation bucket.

## Historical and transitional material

The following areas contain alpha-line history, maintainer notes, local benchmark evidence, or transitional documentation. They remain available for context, but they are not the primary current user path.

### Design history

- [Architecture](design/architecture.md)
- [State-of-the-art architecture review](design/state-of-the-art-architecture-review.md)

### Maintainer and legacy local workflow notes

- [Benchmarks](development/benchmarks.md)
- [Building offline](development/building-offline.md)
- [Packaging](development/packaging.md)
- [Testing](development/testing.md)
- [Package README mapping](development/package-readmes.md)

### Release history and planning notes

- [Package maturity](release/package-maturity.md)
- [API stabilization](release/api-stabilization.md)
- [API freeze policy](release/api-freeze.md)
- [Beta checklist](release/beta-checklist.md)
- [Beta readiness review](release/beta-readiness-review.md)
- [Release checklist](release/release-checklist.md)
- [Release decisions](release/release-decisions.md)
- [Alpha migration notes](release/migration-alpha.md)

### Historical benchmark results

Local committed benchmark reports are historical evidence, not the long-term benchmark source of truth. Benchmark reporting is expected to move to CI-owned artifacts and workflow summaries.

- [Historical full benchmark baseline](benchmark-results/latest.md)
- [Historical routing benchmark baseline](benchmark-results/routing-latest.md)
- [Performance guardrails](benchmark-results/performance-guardrails.md)
- [Committed benchmark result runs](benchmark-results/)

## Archive

Historical notes and pre-release migration documents live under [archive](archive/).
