# Architecture Decision Records

This directory contains Pattrn Architecture Decision Records (ADRs).

ADRs capture durable product, architecture, package-boundary, compatibility, and workflow decisions. Accepted ADRs are binding guidance unless a newer ADR explicitly supersedes or amends them.

## ADR index (canonical)

| ADR | Title | Status | Topic labels | Public relevance |
|---|---|---|---|---|
| [0001](0001-core-remains-segmented-and-domain-neutral.md) | Core remains segmented and domain-neutral | Accepted | core-boundary, architecture | Public |
| [0002](0002-target-dotnet-10-only.md) | Target .NET 10 only for the current product cycle | Accepted | platform, compatibility | Public |
| [0003](0003-explicit-pattern-segments-are-primary.md) | Explicit pattern segments are the primary core model | Accepted | api-shape, pattern-model | Public |
| [0004](0004-companion-packages.md) | Strings, DI, and Routing are companion packages | Accepted | package-boundary, architecture | Public |
| [0005](0005-routing-remains-preview.md) | Routing remains preview and outside the initial stable scope | Accepted | routing, preview-policy | Public |
| [0006](0006-globbing-is-post-1-0.md) | Globbing is deferred to a post-1.0 companion package | Accepted | roadmap, deferral | Public |
| [0007](0007-matching-apis-remain-synchronous.md) | Matching APIs remain synchronous | Accepted | api-shape, performance | Public |
| [0008](0008-aspnetcore-behavior-stays-outside-core.md) | ASP.NET Core behavior stays outside the core | Accepted | framework-boundary, architecture | Public |
| [0009](0009-compiled-index-internals-are-private.md) | Compiled index internals are private and not serialized | Accepted | encapsulation, compatibility | Public |
| [0010](0010-performance-guardrails-are-release-gates.md) | Performance guardrails are release gates | Accepted | performance, release-policy | Public |
| [0011](0011-diagnostics-are-optional-not-hot-path.md) | Diagnostics are optional and not hot-path behavior | Accepted | diagnostics, performance | Public |
| [0012](0012-simplify-pre-beta-versioning-and-roadmap-milestones.md) | Simplify pre-beta versioning and roadmap milestones | Accepted | versioning, roadmap | Public |
| [0013](0013-use-fixed-ranking-with-consumer-side-sorting.md) | Use fixed ranking with consumer-side sorting | Accepted | ranking, matching-semantics | Public |
| [0000 template](0000-adr-template.md) | ADR template | Proposed | process, authoring | Maintainer-only authoring aid |

## Task-based read order

### Changing public matching behavior or APIs

1. [0001](0001-core-remains-segmented-and-domain-neutral.md)
2. [0003](0003-explicit-pattern-segments-are-primary.md)
3. [0007](0007-matching-apis-remain-synchronous.md)
4. [0013](0013-use-fixed-ranking-with-consumer-side-sorting.md)

### Changing package boundaries or package docs

1. [0004](0004-companion-packages.md)
2. [0005](0005-routing-remains-preview.md)
3. [0006](0006-globbing-is-post-1-0.md)
4. [0008](0008-aspnetcore-behavior-stays-outside-core.md)

### Changing roadmap, release framing, or version policy

1. [0012](0012-simplify-pre-beta-versioning-and-roadmap-milestones.md)
2. [0010](0010-performance-guardrails-are-release-gates.md)

### Changing diagnostics, encapsulation, or compatibility framing

1. [0011](0011-diagnostics-are-optional-not-hot-path.md)
2. [0009](0009-compiled-index-internals-are-private.md)
3. [0002](0002-target-dotnet-10-only.md)

## Status values

Use one of:

```text
Proposed
Accepted
Superseded
Deprecated
```

## Change rules

Do not rewrite a historical ADR to hide a change in direction. If the project intentionally changes direction, add a new ADR that explains what changed, why the old decision no longer holds, and what migration or compatibility consequences follow.

## Historical sources

These ADRs summarize and stabilize decisions that were previously spread across README sections, release docs, benchmark notes, the living roadmap, and historical design reviews such as:

- `docs/archive/pre-beta/design/architecture.md`
- `docs/archive/pre-beta/design/state-of-the-art-architecture-review.md`
- `docs/archive/`
- `docs/archive/pre-beta/release/`
- `docs/benchmark-results/`
