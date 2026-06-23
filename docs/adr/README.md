# Architecture Decision Records

This directory contains Pattrn Architecture Decision Records (ADRs).

ADRs capture durable product, architecture, package-boundary, compatibility, and workflow decisions. Accepted ADRs are binding guidance for future changes unless a newer ADR explicitly supersedes or amends them.

## Read order

Start with these ADRs when changing public behavior, package boundaries, docs, or validation policy:

1. [ADR 0001: Core remains segmented and domain-neutral](0001-core-remains-segmented-and-domain-neutral.md)
2. [ADR 0003: Explicit pattern segments are the primary core model](0003-explicit-pattern-segments-are-primary.md)
3. [ADR 0004: Strings, DI, and Routing are companion packages](0004-companion-packages.md)
4. [ADR 0012: Simplify pre-beta versioning and roadmap milestones](0012-simplify-pre-beta-versioning-and-roadmap-milestones.md)
5. [ADR 0013: Use fixed ranking with consumer-side sorting](0013-use-fixed-ranking-with-consumer-side-sorting.md)

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
