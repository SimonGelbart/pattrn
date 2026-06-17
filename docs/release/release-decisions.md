# Release decisions before beta

This document records the product/API decisions made through the alpha.31 performance guardrails and speed triage update.

## Core remains generic

The core package remains a generic segmented-pattern index. It may expose generic concepts such as literals, parameters, wildcard segments, terminal catch-alls, captures, specificity, diagnostics, and duplicate registration policies. It must not parse route strings, normalize URLs, enforce route constraints, validate authorization rules, or depend on application-framework concepts.

If a feature becomes domain-specific, it should move to a companion package such as `Pattrn.Routing`, `Pattrn.Strings`, or a future integration package.

## Everything remains preview during alpha

All packages are still alpha/preview. API names and shapes may change before beta. The stable-candidate labels in `API_STABILIZATION.md` mean “likely to keep”, not “frozen”.

The routing package is explicitly preview. It is useful for validating the package split, but it should not force route semantics into the core.

## Specificity contract

The stable contract is the broad deterministic ordering:

```text
literal > parameter > wildcard > catch-all
registration order is the final tie-breaker for equivalent specificity
```

The numeric `Specificity` value is informational and should not be persisted or compared across versions.

## Detailed match APIs

Detailed match results expose accepted-match metadata: value, optional `PatternId`, deterministic `RegistrationOrder`, match kind, specificity, and captures. Domain metadata still belongs in `TValue`.

The core exposes two detailed-match layers:

```csharp
int MatchDetailed(
    ReadOnlySpan<TSegment> path,
    Span<PatternMatch<TValue>> matches,
    Span<PatternCapture<TSegment>> captures,
    out int capturesWritten);

PatternMatchResult<TSegment, TValue>[] MatchDetailed(ReadOnlySpan<TSegment> path);
PatternMatchResult<TSegment, TValue>[] MatchDetailedToArray(ReadOnlySpan<TSegment> path);
```

The span-based API is the hot accepted-match metadata path. The array-returning APIs are convenience APIs and allocate result/capture objects.

## Explanation APIs

`Explain(...)` is the diagnostics-oriented surface. It may allocate, copy the input path, return accepted detailed matches, report upper-bound metadata, and optionally collect rejected-candidate hints. Rejected-candidate hints are off by default and remain preview.

Hot matching should stay on `Match`, `TryMatch`, `MatchToArray`, or span-based `MatchDetailed`.

## No partial `Try*` results

`TryMatch` and `TryMatchDetailed` do not publish partial results. When they return `false`:

```text
written / matchesWritten / capturesWritten == 0
caller-provided destination spans are not written by the method
callers must not inspect output spans for partial data
```

`Match` and `MatchDetailed` continue to throw `ArgumentException` when the provided destination span is too small.

## Duplicate behavior split

Duplicate handling intentionally has separate builder-time and match-time concepts:

```text
DuplicatePatternRegistrationBehavior: builder-time structural duplicate policy
DuplicateValueMatchMode / MatchOptions: match-time duplicate value emission policy
Diagnostics: advisory structural duplicate/overlap reporting
```

Alpha.14 renamed and documented these concepts. The split is acceptable for beta if the final API snapshot review approves the names. Avoid adding more duplicate-related behavior before beta.

## Primary pattern model

`PatternSegment<TSegment>` / `AddPattern(...)` is the primary core registration model. Alpha.23 added tokenless builder creation so explicit pattern-segment callers do not need to provide a wildcard token. Tokenized wildcard registration remains opt-in convenience for callers that intentionally reserve a wildcard segment value.

## Pattern segment naming

The preferred public factories are:

```csharp
PatternSegment<TSegment>.Literal(value)
PatternSegment<TSegment>.Parameter(name)
PatternSegment<TSegment>.Wildcard()
PatternSegment<TSegment>.CatchAll()
PatternSegment<TSegment>.CatchAll(name)
```

The older `Any()`, `Wildcard` property, and `CatchAllWildcard` property were removed before beta to reduce naming ambiguity.

## Catch-all captures

The core keeps catch-all captures segmented:

```text
path = a
path = b
path = c.txt
```

Route-specific joined catch-all values such as `"a/b/c.txt"` belong in `Pattrn.Routing` or a future routing convenience layer, not in the core package.


## Alpha.26 normalization boundary

The core stays an already-segmented matcher. Segment comparer selection is a core builder concern, but string splitting, trimming, empty-segment policy, and custom string normalization belong in `Pattrn.Strings` through `StringNormalizationOptions`.

Do not add URL decoding, filesystem path rules, glob syntax, route optionality, route constraints, or route constraint validation to the core normalization boundary.

## Deferred features

Do not add these before the next matching-contract decision:

```text
route constraint validation
broader optional route semantics beyond suffix expansion
regex constraints
OpenAPI integration
ASP.NET Core integration
metadata-aware filtering
async matching APIs
source generators
analyzers
```

## Benchmarks

Full publishable benchmark results are still required before stable release. The benchmark suite and scripts exist, but official results should be generated later on a stable machine or CI runner.

## Alpha.16 documentation gate

Alpha.16 added migration notes, package README mapping, and a beta checklist. It did not change matcher semantics. The next beta decision should be based on API snapshot review and a full benchmark report.

## Alpha.17 benchmark follow-up

The first benchmark-driven follow-up is an internal optimization to detailed-match duplicate-value deduplication. The package still needs a new full benchmark run before beta to confirm the expected improvement.


## Alpha.20 beta-readiness decisions

- Keep route constraint validation out of the core; route-layer validation is available in the routing companion as of alpha.29.
- Keep routing preview for beta.
- Keep diagnostics preview for beta.
- Keep `Specificity` numeric values informational.
- Keep exact package boundaries: core remains generic; route syntax stays in `Pattrn.Routing`.

## Alpha.21 architecture-hardening decisions

- Do not jump directly to beta from alpha.20.
- Complete a focused core construction simplification next.
- Make `PatternSegment<TSegment>` / `AddPattern(...)` documentation-first.
- Add tokenless builder creation before beta.
- Retain tokenized wildcard registration as convenience unless a later review finds a simpler safe migration.
- Do not add matcher features in this hardening phase.


## Alpha.23 builder decision

The default core builder is tokenless. `Add(...)` on a tokenless builder registers literal-only segments. Wildcards, named parameters, and catch-alls should be registered with `AddPattern(...)`. Builders created with a wildcard segment token retain tokenized `Add(...)` behavior as a convenience layer, not the core mental model.

## Decision: String ergonomics stay convenience-oriented

`3.0.0-alpha.27` adds `StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` as convenience facades over the generic string-segment builder and index. These facades store `StringNormalizationOptions` once and reduce repeated options arguments, but they do not introduce URL, filesystem, route-template, glob, or framework semantics.

Advanced callers can still use `CoreBuilder`, `CoreIndex`, or the direct separated/dotted extension methods when they need low-level control.

## Decision: Route-template metadata stays above the core

`3.0.0-alpha.28` added structured route-template metadata in `Pattrn.Routing`. `3.0.0-alpha.29` added optional route-layer constraint validation over the captures from structural matches. `3.0.0-alpha.30` adds `RouteTemplateExpansion` metadata for generated optional/defaulted structural registrations. `3.0.0-alpha.31` does not change route semantics; it commits the alpha.30 benchmark baseline and protects exact-only core matching fast paths. Structural route templates still compile into generic `PatternSegment<string>` registrations before entering the core matcher, and constraint failures remain routing validation failures rather than core diagnostics.

The core does not evaluate route constraints, apply route defaults, URL-decode values, join catch-all captures, or implement framework route precedence.
