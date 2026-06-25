# Documentation

Pattrn documentation is organized around the current pre-beta product state. Use the newcomer path first; maintainer material is intentionally lower on this page.

## Start here (new users)

1. [Roadmap](roadmap.md) — current direction and milestone status.
2. [Build your first Pattrn index](tutorials/first-index.md) — first success path.
3. [Core package docs](packages/pattrn.md) — install + package selection table (canonical owner).
4. [Core API](reference/api.md) and [Generic pattern segments](reference/pattern-segments.md) — stable contracts.
5. [Select a best match](how-to/select-best-match.md) — practical ranking workflow.
6. [Known limitations](reference/limitations.md) — boundaries and deferred behavior.

## Current product docs

- [Root README](../README.md) for the short product overview and first example.
- [Project profile](reference/project-profile.md) for durable package/status facts.
- [Validation](reference/validation.md) for CI-first validation policy.
- [Benchmarks](reference/benchmarks.md) for evidence policy and benchmark workflow expectations.
- [Architecture decisions](adr/README.md) for accepted decision history.

## Packages

- [Pattrn](packages/pattrn.md)
- [Pattrn.Strings](packages/pattrn-strings.md)
- [Pattrn.DependencyInjection](packages/pattrn-dependency-injection.md)
- [Pattrn.Routing](packages/pattrn-routing.md)

## Tutorials, how-to, explanation, and reference

- Tutorials: [first index](tutorials/first-index.md), [examples](tutorials/examples.md)
- How-to: [select a best match](how-to/select-best-match.md)
- Explanation: [package boundaries](explanation/package-boundaries.md)
- Reference: [api](reference/api.md), [matching semantics](reference/matching-semantics.md), [ranking and specificity](reference/ranking-specificity.md), [duplicate behavior](reference/duplicate-behavior.md), [diagnostics](reference/diagnostics.md)

## Maintainer guidance

- [`../AGENTS.md`](../AGENTS.md)
- [Project profile](reference/project-profile.md)
- [Validation](reference/validation.md)
- [Repository layout](reference/repository-layout.md)
- [Git workflow](reference/git-workflow.md)
- [Documentation standards](reference/documentation-standards.md)
- [Documentation site](reference/documentation-site.md)
- [AI documentation management](reference/ai-documentation-management.md)
- [Building offline](reference/building-offline.md)

## Historical material

Pattrn is pre-beta. Alpha-era migration notes, local benchmark reports, release-planning notes, and design drafts are not kept in the current documentation path. Use Git history for old context. Durable product decisions are captured as ADRs.

Current benchmark proof should come from CI workflow artifacts and summaries referenced by [benchmarks reference](reference/benchmarks.md).
