# API Stabilization Checkpoint

> Historical note: this file preserves earlier alpha-line release notes. Current roadmap and versioning policy live in `docs/roadmap.md` and `docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md`.


This document records the public API status through `3.0.0-alpha.31` before moving toward beta.

The goal is not to freeze everything immediately. The alpha.22 consolidated roadmap explicitly allows breaking alpha changes while the library is unused/pre-beta. Alpha.23 used that freedom to make tokenless builders the default, alpha.24 completed the stable matching-contract metadata, alpha.25 separated hot matching from diagnostics-oriented explanation matching, alpha.26 established the generic/string normalization boundary, alpha.27 added ergonomic string-path facades on top of that boundary, alpha.28 expanded framework-neutral route-template metadata and optional/defaulted suffix expansion, alpha.29 added optional route constraint validation above structural matching, alpha.30 refreshed the roadmap while hardening optional/defaulted route expansion metadata, and alpha.31 made speed a release gate while protecting exact-only matching fast paths. The goal is to make the maturity level of each surface explicit so future changes are intentional.

## Product boundary

The core package remains a generic segmented-pattern index. It should not parse route strings, normalize URLs, interpret OpenAPI paths, enforce authorization rules, or own application-framework concepts.

Higher-level syntax and framework behavior belong in companion packages.

## Maturity levels

| Level | Meaning |
|---|---|
| Stable candidate | Intended to remain unless benchmark or usability evidence shows a problem before beta. |
| Preview | Usable, but naming/shape may still change before beta. |
| Experimental | Included to validate direction; breaking changes are expected. |
| Internal | Not public API. Can change freely. |

## Core package: `Pattrn`

| Surface | Status | Notes |
|---|---|---|
| `PattrnIndex<TSegment, TValue>` | Stable candidate | Immutable read model is the core product. |
| `IPattrnIndex<TSegment, TValue>` | Stable candidate | Segmented span-first API should remain small. `Explain(...)` is diagnostics-oriented and intentionally separate from hot matching. |
| `PattrnIndexBuilder<TSegment, TValue>` | Stable candidate | Alpha.23 added tokenless builder creation and made explicit generic pattern registration the primary model. Alpha.24 added optional `patternId` registration metadata. Tokenized wildcard registration remains opt-in convenience. |
| Value-only `Match` / `TryMatch` / `MatchToArray` | Stable candidate | Hot API. Keep allocation-conscious. |
| `MatchOptions`, `PrefixMatchMode`, `DuplicateValueMatchMode` | Preview / stable candidate | Alpha.14 renamed the duplicate value mode for clarity. Keep the semantics; approve the final name before beta. |
| `PatternSegment<TSegment>` / `PatternSegmentKind` | Stable candidate | Alpha.23 makes tokenless `Builder()` plus `AddPattern(...)` the primary core construction path. Alpha.13 standardized factories on `Literal`, `Parameter`, `Wildcard()`, and `CatchAll(...)`. |
| Detailed match APIs | Stable candidate | Low-allocation accepted-match design is right. Alpha.24 exposes pattern identity, registration order, kind, specificity, and captures. Alpha.25 keeps rejected-candidate diagnostics in `Explain(...)`, not in the hot detailed span API. |
| `PatternMatch<TValue>` | Stable candidate | Alpha.24 added `PatternId` and `RegistrationOrder`. The broad ordering is compatibility-covered; exact numeric weights remain implementation detail. |
| `PatternMatchResult<TSegment, TValue>` | Stable candidate | Convenience allocation type now mirrors accepted-match metadata: value, optional pattern identity, registration order, kind, specificity, and captures. |
| `PatternCapture<TSegment>` | Preview | Generic capture values are correct. Catch-all multi-capture semantics are now compatibility-covered. |
| Catch-all support | Preview | Generic terminal catch-all is correct. Keep route string joining out of core. |
| Diagnostics | Preview | Advisory builder diagnostics and explanation rejected-candidate hints are useful. Keep names, codes, reasons, and severities preview until explicitly frozen. |
| Duplicate registration behavior | Preview | Alpha.14 renamed this to `DuplicatePatternRegistrationBehavior` and documented the split from `DuplicateValueMatchMode`. Final approval is still needed before beta. |

## Strings package: `Pattrn.Strings`

| Surface | Status | Notes |
|---|---|---|
| String helper extension methods | Stable candidate | These remain convenience helpers over segmented core APIs. |
| `StringNormalizationOptions` | Preview / stable candidate | Added in alpha.26 to make separator, case-sensitivity, empty-segment, trimming, and custom segment normalization explicit in the string companion package. |
| `StringPattrnIndexBuilder<TValue>` / `StringPattrnIndex<TValue>` | Preview / stable candidate | Added in alpha.27 to reduce option repetition for string registration and matching while preserving the generic core boundary. |

## Dependency injection package: `Pattrn.DependencyInjection`

| Surface | Status | Notes |
|---|---|---|
| Fluent registration API | Stable candidate | Keeps DI ergonomic without driving core design. Alpha.23 makes DI registrations tokenless by default; `UseWildcard(...)` opts into tokenized convenience behavior. |
| `IPattrnProvider<TSegment, TValue>` | Preview | Retained as a named-index convenience over keyed services. Review once more before beta. |
| Registration sources | Preview | Keep constructor-injected source pattern. Context exposes Add/AddPattern instead of IServiceProvider or the mutable builder. |

## Routing package: `Pattrn.Routing`

| Surface | Status | Notes |
|---|---|---|
| Entire package | Experimental / preview | It is intentionally a companion package. It should not force route semantics into the core. |
| `RoutePattern.Parse` / `ParseTemplate` / `ExpandDetailed` | Preview | Supports literals, named parameters, terminal named catch-alls, preserved constraints/defaults/optional metadata, and expansion metadata. |
| `RoutePattern.SplitPath` | Preview | Allocation is expected. More normalization rules should be explicit options, not defaults. |
| Route builder/match extensions | Preview | Useful, but should remain thin translations to the generic core. Parsed-template overloads reduce reparsing when callers need metadata. |

## Beta blockers

Before beta:

1. Complete the next implementation increment: specificity and ranking customization.
2. Run and commit a real full benchmark report on a stable machine after any matcher/compiler refactor and before beta/RC.
3. Review DI provider and registration-source maturity once more, but alpha.15 reduced the context surface and kept the provider as preview convenience.
4. Keep duplicate behavior names from alpha.14 unless a final API snapshot review finds a clearer name.
5. Keep routing preview for the first stable core release unless strong usage evidence says otherwise.
6. Keep diagnostics preview unless names and severities are explicitly frozen.
7. Keep expanding compatibility tests when route optional segments, constraints, ranking hooks, or diagnostics are introduced in companion packages.

## Non-goals before beta

Do not add these before the stabilization work is done:

- ASP.NET Core integration;
- OpenAPI integration;
- broader optional route semantics beyond suffix expansion;
- framework-specific route precedence;
- metadata-aware filtering;
- source generators;
- analyzers;
- async matching APIs.

## Alpha.15 DI review outcome

- `PattrnOptions<TSegment, TValue>` was made internal because it was an implementation detail of fluent registration.
- `PattrnRegistrationContext<TSegment, TValue>.Builder` was removed from the public DI surface.
- `PattrnRegistrationContext<TSegment, TValue>.AddPattern(...)` was added so registration sources can contribute generic pattern-segment registrations without builder access.
- `IPattrnProvider<TSegment, TValue>` remains public preview as an ergonomic named-index resolver. Direct keyed-service resolution is also documented.


## Alpha.16 final documentation sweep outcome

- Added `docs/release/beta-checklist.md` to make remaining beta gates explicit.
- Added `docs/release/migration-alpha.md` to capture alpha.13 through alpha.15 breaking changes.
- Added `docs/development/package-readmes.md` to document which README is packed by each package.
- Rechecked preview/stable-candidate labels without adding matcher behavior.
- Full benchmark results remain the main external beta blocker.

## Alpha.17 note

`3.0.0-alpha.17` does not change the public API. It only optimizes detailed-match duplicate-value deduplication internally and adds compatibility coverage.

## Alpha.18 note

`3.0.0-alpha.18` adds preview routing-package APIs for caller-buffer route path splitting. The addition is intentionally outside the generic core and is meant to reduce route-helper allocation pressure without introducing route-specific behavior into `Pattrn`.


## Alpha.19 note

`3.0.0-alpha.19` cleaned the source artifact and reorganized documentation. It also incorporated the focused routing benchmark report. No matcher behavior or public API shape changed.

## Alpha.20 beta-readiness outcome

`3.0.0-alpha.20` is a final alpha review pass. It confirms the package maturity posture, updates the beta checklist, and adds a beta-readiness review document. No matcher behavior or public API shape changed.


## Alpha.21 architecture-hardening outcome

`3.0.0-alpha.21` is a documentation/design checkpoint. It confirms that `PatternSegment<TSegment>` and `AddPattern(...)` should become the primary core mental model before beta, while tokenized wildcard registration should remain available as a convenience surface for callers that intentionally reserve a wildcard segment value.

No matcher behavior, route syntax, diagnostics behavior, or public API shape changed in alpha.21. The next implementation increment should implement the focused API simplification by adding tokenless builder creation and updating samples/docs accordingly.


## Alpha.22 roadmap-consolidation outcome

`3.0.0-alpha.22` adds `docs/roadmap.md` as the first-class roadmap and records the alpha compatibility posture: breaking changes are acceptable while the library is unused/pre-beta. It does not change matcher behavior or public API shape. The tokenless-builder/API cleanup moves to the next implementation increment.


## Alpha.23 core API reorientation outcome

`3.0.0-alpha.23` adds tokenless builder creation through `PattrnIndex<TSegment, TValue>.Builder()` and `PattrnIndexBuilder<TSegment, TValue>.Create()`. On tokenless builders, `Add(...)` registers literal-only patterns. Wildcard, parameter, and catch-all behavior is explicit through `AddPattern(...)`. Tokenized wildcard behavior remains available through builder factories that accept a wildcard segment.

DI registrations now default to tokenless builders. `UseWildcard(...)` remains available when callers intentionally want tokenized `Add(...)` convenience. Core matching semantics are unchanged.

## Alpha.24 stable matching contract outcome

`3.0.0-alpha.24` adds optional caller-provided `patternId` metadata to registrations and exposes `PatternId` and `RegistrationOrder` on detailed match descriptors/results. Domain metadata still belongs in `TValue`; the core only carries stable registration identity/order metadata for diagnostics, logging, and deterministic references. Value-only matching semantics are unchanged.

## Alpha.25 fast/explainability outcome

`3.0.0-alpha.25` added `Explain(...)` as a diagnostics-oriented match surface. `3.0.0-alpha.26` added explicit string normalization options without moving string parsing into the core. `3.0.0-alpha.27` added string-path builder/index facades that store those options once for registration and matching. `3.0.0-alpha.28` added framework-neutral route-template metadata and optional/defaulted suffix expansion. `3.0.0-alpha.29` added optional route-layer constraint validation. `3.0.0-alpha.30` added route expansion metadata through `RouteTemplateExpansion`. `3.0.0-alpha.31` committed the alpha.30 benchmark baseline and added exact-only direct fast paths. The hot value-only APIs remain `Match`, `TryMatch`, and `MatchToArray`; detailed span matching remains the low-allocation accepted-match metadata API. Rejected-candidate diagnostics are opt-in through `PatternExplanationOptions` and remain preview. The next API stabilization gap is specificity/ranking customization and route-template naming/maturity decisions.
