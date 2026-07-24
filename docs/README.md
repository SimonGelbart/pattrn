# Documentation

These docs describe the current code and public API. Historical experiments and unused planning records belong in Git history, not in the normal reading path.

## Start here (new users)

1. [Build your first Pattrn index](tutorials/first-index.md)
2. [Core package docs](packages/pattrn.md)
3. [Core API](reference/api.md) and [Matching semantics](reference/matching-semantics.md)
4. [Select a best match](how-to/select-best-match.md)
5. [Known limitations](reference/limitations.md)

## Current product docs

- [Root README](../README.md) for the short product overview and first example.
- [Validation](reference/validation.md) for CI-first validation policy.
- [Trimming and Native AOT compatibility](reference/aot-trimming.md) for stable-package support posture and local harness commands.
- [Benchmarks](reference/benchmarks.md) for evidence policy and benchmark workflow expectations.
- [Namespace migration](reference/namespace-migration.md) for the pre-beta namespace changes.
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
- Reference: [api](reference/api.md), [namespace migration](reference/namespace-migration.md), [matching semantics](reference/matching-semantics.md), [trimming and Native AOT compatibility](reference/aot-trimming.md), [duplicate behavior](reference/duplicate-behavior.md), [diagnostics](reference/diagnostics.md)

## Maintainer guidance

- [`../AGENTS.md`](../AGENTS.md)
- [Validation](reference/validation.md)
- [Git workflow](reference/git-workflow.md)
- [Documentation site](reference/documentation-site.md)

Current benchmark proof should come from CI workflow artifacts and summaries referenced by [benchmarks reference](reference/benchmarks.md). Future work is tracked outside the product documentation.
