# Documentation

Pattrn documentation is organized by audience and lifecycle. Keep this directory small, predictable, and product-facing; archive historical alpha notes instead of leaving one-off files at the top level.

Start with the [roadmap](roadmap.md) for product direction, current status, and the next increment.

## Repository guidance

For assisted or maintainer work, start with:

- [`../AGENTS.md`](../AGENTS.md)
- [Project profile](reference/project-profile.md)
- [Repository layout](reference/repository-layout.md)
- [Git workflow](reference/git-workflow.md)
- [Validation](reference/validation.md)
- [Documentation standards](reference/documentation-standards.md)
- [Architecture decisions](adr/README.md)

New durable documentation should follow the Diataxis folders: `tutorials/`, `how-to/`, `reference/`, `explanation/`, and `adr/`. Existing alpha-era folders are retained while their content is reconciled.

## Getting started

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

## Design

- [Architecture](design/architecture.md)
- [State-of-the-art architecture review](design/state-of-the-art-architecture-review.md)

## Development

- [Benchmarks](development/benchmarks.md)
- [Building offline](development/building-offline.md)
- [Packaging](development/packaging.md)
- [Testing](development/testing.md)
- [Package README mapping](development/package-readmes.md)

## Release management

- [Package maturity](release/package-maturity.md)
- [API stabilization](release/api-stabilization.md)
- [API freeze policy](release/api-freeze.md)
- [Beta checklist](release/beta-checklist.md)
- [Beta readiness review](release/beta-readiness-review.md)
- [Release checklist](release/release-checklist.md)
- [Release decisions](release/release-decisions.md)
- [Alpha migration notes](release/migration-alpha.md)

## Benchmark reports

- [Latest full benchmark report](benchmark-results/latest.md)
- [Latest routing benchmark report](benchmark-results/routing-latest.md)
- [Performance guardrails](benchmark-results/performance-guardrails.md)
- [Committed benchmark result runs](benchmark-results/)

## Archive

Historical notes and pre-release migration documents live under [archive](archive/).
