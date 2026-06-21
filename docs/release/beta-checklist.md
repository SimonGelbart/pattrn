# Beta checklist

> Historical note: this file preserves earlier alpha-line release notes. Current roadmap and versioning policy live in `docs/roadmap.md` and `docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md`.


This checklist defines the remaining work before a `3.0.0-beta.1` package line.

`3.0.0-alpha.20` was the beta-readiness review. `3.0.0-alpha.21` added the architecture decision to make explicit `PatternSegment<TSegment>` registration primary. `3.0.0-alpha.22` consolidated the standalone roadmap into `docs/roadmap.md` and explicitly allowed breaking alpha changes because the library is not yet used by consumers. `3.0.0-alpha.23` implemented tokenless builder creation and explicit-segment-first API reorientation. `3.0.0-alpha.24` added pattern identity and registration-order metadata to the stable matching contract. `3.0.0-alpha.25` separated hot matching from diagnostics-oriented explanation matching. `3.0.0-alpha.26` established the generic/string normalization boundary. `3.0.0-alpha.27` added ergonomic string-path builder and index facades. `3.0.0-alpha.28` expanded framework-neutral route-template metadata and optional/defaulted suffix expansion, `3.0.0-alpha.29` added optional route constraint validation above structural matching, `3.0.0-alpha.30` refreshed the roadmap while hardening optional/defaulted route expansion metadata, and `3.0.0-alpha.31` added speed guardrails plus exact-only matching fast-path protection.

## Required before beta

- [x] Complete tokenless builder creation / `AddPattern(...)`-first API and documentation pass.
- [x] Complete pattern identity and stable match-result contract.
- [x] Complete the fast matching versus explainability separation increment.
- [x] Complete the generic normalization hooks increment.
- [x] Complete the string API ergonomics increment.
- [x] Complete optional/defaulted route-segment expansion hardening.
- [ ] Complete the next implementation increment: specificity and ranking customization.
- [x] Run a full BenchmarkDotNet report on a stable developer machine or CI runner.
- [x] Commit the generated report under `docs/benchmark-results/`.
- [x] Review the public API snapshot files one final time.
- [x] Decide whether each preview surface remains preview or moves to stable candidate.
- [x] Confirm package README files are accurate for each package.
- [x] Confirm no route-specific behavior leaked into the core package.
- [x] Confirm `TryMatch` and `TryMatchDetailed` no-partial-result semantics are documented and tested.
- [x] Confirm duplicate registration and duplicate value emission are documented as separate concepts.
- [x] Confirm diagnostics remain preview unless their names and severities are explicitly frozen.
- [x] Confirm routing remains preview for the first stable core release unless it receives dedicated stabilization work.

## Stable-candidate surfaces

These are expected to move toward beta unless benchmark or usability evidence proves otherwise:

- `PattrnIndex<TSegment, TValue>`
- `IPattrnIndex<TSegment, TValue>`
- value-only `Match`, `TryMatch`, and `MatchToArray`
- explicit `PatternSegment<TSegment>` builder registration APIs
- tokenless builder creation and explicit pattern-segment registration APIs
- tokenized segmented builder registration APIs, as opt-in convenience
- `MatchOptions`, `PrefixMatchMode`, and `DuplicateValueMatchMode`
- `Pattrn.Strings` helper methods
- fluent DI registration APIs

## Preview surfaces

These remain usable but not frozen:

- rejected-candidate explanation/diagnostic matching APIs
- capture APIs
- catch-all support
- diagnostics
- duplicate registration policy APIs
- DI provider and registration-source APIs
- the entire `Pattrn.Routing` package

## Deferred until after beta decision

Do not add these before the beta decision unless the roadmap is explicitly revised:

- broader optional route semantics beyond suffix expansion;
- framework-specific route precedence;
- ASP.NET Core integration;
- OpenAPI integration;
- metadata-aware filtering;
- source generators;
- analyzers;
- async matching APIs.

## Benchmark gate

The latest full report is committed at `docs/benchmark-results/2026-06-15-alpha17/README.md`. A stable or beta announcement may now cite precise measured results from that report, but should still avoid vague language such as "blazing fast". The report also identifies detailed duplicate-heavy matching and routing-helper allocations as improvement areas before stronger detailed/routing performance claims.


## Alpha.17 follow-up

- [x] Address first benchmark outlier: detailed-match duplicate-heavy deduplication ordered-block fast path.
- [x] Re-run full benchmarks and compare `DuplicateHeavyDeduplicate` detailed span/array results against alpha.16.


## Benchmark cadence decision

Full BenchmarkDotNet runs are not required after every small cleanup. Run the full suite after important engine/writer/build refactors, before beta/RC/stable releases, and before updating public performance claims. Use focused filters for targeted optimization work and smoke/dry runs for benchmark build verification.


## Alpha.18 follow-up

- [x] Reduce avoidable routing-helper segment-array allocations by pooling temporary buffers in string convenience helpers.
- [x] Add caller-buffer route path splitting APIs for hot routing-package callers.
- [x] Run a focused `RoutingBenchmarks` comparison before making stronger routing-helper allocation claims.


## Alpha.19 routing benchmark follow-up

- [x] Commit focused `RoutingBenchmarks` results under `docs/benchmark-results/2026-06-15-alpha19-routing/`.
- [x] Confirm alpha.18 reduced route-helper allocation from `168 B`/`200 B` to `112 B`/`144 B` in comparable span-helper rows.
- [x] Document the tradeoff: lower allocation came with slower small-path convenience-helper latency because temporary-array pooling has fixed overhead.
- [x] Keep `Pattrn.Routing` preview and document pre-split/core APIs as the current hot path.


## Alpha.20 beta-readiness outcome

- [x] Added `docs/release/beta-readiness-review.md`.
- [x] Confirmed the core, strings, and fluent DI surfaces are stable candidates for beta feedback.
- [x] Confirmed routing, diagnostics, DI provider, and registration-source extras remain preview.
- [x] Confirmed no matcher behavior or public API shape changed in alpha.20.
- [x] Confirmed alpha.20 was beta-ready before the later alpha.21 architecture reorientation.

## Alpha.21 architecture-hardening outcome

- [x] Added the state-of-the-art architecture review.
- [x] Decided `PatternSegment<TSegment>` / `AddPattern(...)` should be the primary core model.
- [x] Decided tokenless builder creation should happen before beta.
- [x] Confirmed no matcher behavior or public API shape changed in alpha.21.


## Alpha.22 roadmap-consolidation outcome

- [x] Added `docs/roadmap.md` as the first-class consolidated roadmap.
- [x] Recorded that backward compatibility is not a priority while the library is unused and pre-beta.
- [x] Reframed tokenless builder creation as the next implementation increment rather than this documentation consolidation pass.
- [x] Confirmed no matcher behavior or public API shape changed in alpha.22.


## Alpha.23 core API reorientation outcome

- [x] Added tokenless core builder creation with `PattrnIndex<TSegment, TValue>.Builder()` and `PattrnIndexBuilder<TSegment, TValue>.Create()`.
- [x] Made tokenless builders literal-only for `Add(...)`; wildcard behavior now requires explicit `PatternSegment<TSegment>.Wildcard()`/`Parameter(...)` or an opt-in tokenized builder.
- [x] Updated DI registrations to default to tokenless builders while keeping `UseWildcard(...)` as opt-in tokenized behavior.
- [x] Updated public API snapshots, docs, samples, and package metadata for alpha.23.
- [x] Confirmed core matcher semantics are unchanged.
