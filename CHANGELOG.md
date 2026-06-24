# Changelog

This file is the public release-history record.

The `3.0.0-alpha.*` entries below are preserved as **historical pre-beta chronology**. Current pre-beta versioning and roadmap policy are defined by [ADR 0012](docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md) and the current [roadmap](docs/roadmap.md).

## 3.0.0-alpha.31

Performance guardrails and speed triage update.

- Committed the uploaded alpha.30 BenchmarkDotNet run under `docs/benchmark-results/2026-06-17-alpha30/`.
- Added benchmark comparison CSVs against the previous alpha.17 core/builder and alpha.19 routing baselines.
- Added `docs/benchmark-results/performance-guardrails.md` to make speed a release gate.
- Updated benchmark docs to treat core span matching and pre-split route matching as protected hot paths.
- Added direct exact-only fast paths for value span matching, detailed span matching, value array matching, and detailed array matching when the compiled index has no wildcard/parameter/catch-all branches and prefix matching is disabled.
- Short-circuited capture upper-bound calculation to zero for exact-only literal indexes.
- Updated README, roadmap, release docs, and package metadata to `3.0.0-alpha.31`.

## 3.0.0-alpha.30

Roadmap refresh and optional/defaulted route expansion hardening update.

- Refreshed `docs/roadmap.md` into a living beta-readiness roadmap that marks completed increments, reorders future work toward stabilization, and keeps globbing/composition deferred until the core/string/routing surfaces are harder.
- Added `RouteTemplateExpansion` metadata for route-template optional/defaulted suffix expansion.
- Added `RouteTemplate.ExpandDetailed(...)` and `RoutePattern.ExpandDetailed(...)` so callers can inspect generated structural patterns, omitted optional/defaulted parameters, expansion order, and the original template link.
- Added parsed-template overloads for `AddRoute(...)`, `ContainsRoute(...)`, `RemoveRoute(...)`, and `RemoveAllRoute(...)`.
- Kept expanded route registrations tied to the caller-provided `patternId` while keeping optionality in the routing companion and out of the generic core matcher.
- Updated tests, public API snapshot, routing docs, release docs, package metadata, and roadmap status.
- Bumped package metadata to `3.0.0-alpha.30`.

## 3.0.0-alpha.29

Optional route constraint validation update.

- Added routing-layer constraint validation above structural route matching.
- Added `IRouteConstraintValidator`, `RouteConstraintValidatorRegistry`, `RouteConstraintValidationOptions`, `RouteConstraintValidationResult`, and `RouteConstraintValidationFailure`.
- Added built-in framework-neutral validators for `int`, `long`, `guid`, `bool`, `datetime`, `decimal`, `alpha`, `min`, `max`, `length`, and `regex`.
- Added `RouteTemplate.ValidateConstraints(...)` helpers for detailed match results and named captures.
- Kept constraint validation out of the generic core matcher and kept structural route matching behavior unchanged.
- Updated tests, public API snapshots, routing docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.29`.

## 3.0.0-alpha.28

Framework-neutral route-template expansion update.

- Added structured route-template metadata through `RouteTemplate`, `RouteSegment`, `RouteParameter`, `RouteConstraint`, and `RouteTemplateDiagnostic`.
- Added `RoutePattern.ParseTemplate(...)`, `TryParseTemplate(...)`, and `Expand(...)`.
- Preserved route parameter constraints such as `{id:int}` and `{id:min(1)}` without evaluating them in the generic core matcher.
- Preserved route default values and optional markers as metadata.
- Added optional/defaulted suffix expansion so templates such as `/orders/{id?}` compile into multiple generic `PatternSegment<string>` registrations.
- Kept route constraint validation, ASP.NET Core behavior, OpenAPI behavior, URL decoding, and framework metadata outside the core matcher.
- Updated tests, public API snapshots, routing docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.28`.

## 3.0.0-alpha.27

String API ergonomics update.

- Added `StringPattrnIndexBuilder<TValue>` as an ergonomic string-path builder facade over `StringNormalizationOptions`.
- Added type-inference friendly `StringPattrnIndexBuilder` factory methods.
- Added `StringPattrnIndex<TValue>` as a string-path index wrapper that stores normalization options once for matching, detailed matching, and explanation calls.
- Added `StringNormalizationOptions.CreateStringBuilder<TValue>()`, `CreateTokenizedStringBuilder<TValue>()`, and `NormalizeSegmentValue(...)`.
- Kept route-template, glob, filesystem, and URL semantics out of the generic string helper layer.
- Updated tests, public API snapshots, docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.27`.

## 3.0.0-alpha.26

Generic normalization hooks update.

- Added `StringNormalizationOptions` in `Pattrn.Strings` to make string splitting and normalization explicit.
- Added string case-sensitivity, empty-segment, and trimming option enums.
- Added options-based string helper overloads for registration, removal, containment checks, matching, and upper-bound calculations.
- Added options-based string builder factories that configure the core segment comparer without putting string parsing into the core package.
- Added custom segment-normalization delegate support for companion-package string workflows.
- Preserved the core boundary: the generic core still matches already-segmented paths and remains free of URL, filesystem, routing, and authorization semantics.
- Updated tests, public API snapshots, docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.26`.

## 3.0.0-alpha.25

Fast matching versus explainability separation update.

- Added diagnostics-oriented `Explain(...)` APIs on the core index and extension methods.
- Added `PatternMatchExplanation<TSegment, TValue>` for allocation-friendly diagnostic results containing accepted detailed matches, input path, match options, and upper-bound metadata.
- Added `PatternExplanationOptions` with rejected-candidate collection disabled by default.
- Added opt-in `PatternRejectedCandidate<TSegment>` diagnostics for branch-level rejected-candidate hints.
- Kept `Match`, `TryMatch`, and value-only `MatchToArray` as the hot matching path.
- Documented the boundary between accepted detailed matches and optional explanation diagnostics.
- Updated public API snapshots, docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.25`.

## 3.0.0-alpha.24

Stable matching contract update.

- Added optional caller-provided pattern identity to core registrations through `patternId`.
- Exposed `PatternId` and `RegistrationOrder` on low-level `PatternMatch<TValue>` descriptors and allocated `PatternMatchResult<TSegment, TValue>` results.
- Added identified registration range overloads for segmented and explicit pattern-segment registrations.
- Forwarded pattern identity through string and route registration helpers.
- Preserved value-only matching behavior and kept domain metadata in `TValue`.
- Fixed compiled detailed metadata alignment when build-time duplicate-value deduplication is enabled.
- Updated public API snapshots, docs, roadmap status, and package metadata.
- Bumped package metadata to `3.0.0-alpha.24`.

## 3.0.0-alpha.23

Core API reorientation update.

- Added tokenless builder creation through `PattrnIndex<TSegment, TValue>.Builder()` and `PattrnIndexBuilder<TSegment, TValue>.Create()`.
- Made tokenless builders the default core construction path. On tokenless builders, `Add(...)` registers literal-only segmented patterns.
- Kept tokenized wildcard registration as opt-in convenience through builder factories that accept a wildcard segment token.
- Confirmed explicit `PatternSegment<TSegment>` / `AddPattern(...)` registration is the primary core model.
- Updated dependency-injection registration to default to tokenless builders; `UseWildcard(...)` now opts into tokenized `Add(...)` behavior.
- Updated README, architecture docs, API docs, migration notes, release docs, samples, and public API snapshots.
- Bumped package metadata to `3.0.0-alpha.23`.
- Confirmed core matcher semantics are unchanged.

## 3.0.0-alpha.22

Consolidated roadmap update.

- Added `docs/roadmap.md` as the first-class consolidated product roadmap.
- Promoted the standalone roadmap direction into the repository documentation.
- Recorded the alpha compatibility posture: breaking changes are acceptable while the library has no consumers and remains pre-beta.
- Reframed alpha.22 as roadmap consolidation and moved tokenless builder/API reorientation to the next implementation increment.
- Updated release-process guidance with the golden rules: every update must produce a versioned source zip, update the changelog, and report roadmap status.
- Bumped package metadata to `3.0.0-alpha.22` for artifact/version consistency.
- Confirmed no matcher semantics or public API shape changed in this increment.

## 3.0.0-alpha.21

State-of-the-art architecture hardening review.

- Added `docs/design/state-of-the-art-architecture-review.md`.
- Documented `PatternSegment<TSegment>` / `AddPattern(...)` as the primary core registration model.
- Demoted tokenized wildcard registration to convenience positioning in README/reference docs.
- Recorded alpha.22 recommendation to add tokenless builder creation before beta.
- Confirmed no matcher semantics or public API shape changed in this increment.

## 3.0.0-alpha.20

Beta-readiness review.

- Added `docs/release/beta-readiness-review.md` as the final alpha review record before beta.
- Rechecked package maturity and documented the intended beta posture for each package.
- Marked the beta checklist readiness items complete where covered by tests, docs, package metadata, and committed benchmark results.
- Confirmed no matcher semantics or public API shape changed in this increment.
- Bumped packages to `3.0.0-alpha.20`.

## 3.0.0-alpha.19

Repository/documentation artifact cleanup for beta-readiness.

### Changed

- Reorganized documentation into audience-based folders under `docs/`:
  - `docs/getting-started/`
  - `docs/reference/`
  - `docs/design/`
  - `docs/development/`
  - `docs/release/`
  - `docs/archive/`
- Added `docs/README.md` as the documentation index.
- Updated package README mappings to use the new documentation paths.
- Updated packaging and limitations documentation to reflect the current four-package v3 alpha shape.
- Kept benchmark reports under `docs/benchmark-results/` with raw reports grouped by dated run.

### Removed

- Removed generated `BenchmarkDotNet.Artifacts/` from the source artifact.
- Removed stray temporary test file `DuplicateHeavyMatchingTests.cs.tmp`.

### Benchmarks

- Committed the focused alpha.19 `RoutingBenchmarks` run under `docs/benchmark-results/2026-06-15-alpha19-routing/`.
- Added `docs/benchmark-results/routing-latest.md` for the latest focused routing report.
- Documented that alpha.18 reduced route-helper allocation but introduced a small-path latency tradeoff in string convenience helpers.

### Semantics

- No code or matching semantics changed.
- Public API shape is unchanged.

## 3.0.0-alpha.18

Routing helper allocation-reduction increment.

### Added

- Added `RoutePattern.GetPathSegmentCount(string)` for caller-buffer sizing.
- Added `RoutePattern.SplitPath(string, Span<string>)` to split route paths into caller-provided buffers.
- Added `RoutePattern.TrySplitPath(string, Span<string>, out int)` with no partial writes on failure.
- Expanded routing benchmarks to compare string convenience helpers, caller-buffer splitting, and pre-split core matching.

### Changed

- Updated string-based `MatchRoute(...)` and detailed route helpers to rent temporary segment arrays internally instead of allocating a fresh `string[]` on each call.
- Kept substring allocation behavior unchanged: raw route strings still produce string segment instances before entering the generic core.
- Updated routing docs and benchmark notes to distinguish convenience helpers from hot-path pre-split/core APIs.
- Bumped all packages to `3.0.0-alpha.18`.

### Semantics

- Core matching behavior is unchanged.
- Route syntax behavior is unchanged.
- `SplitPath(string, Span<string>)` throws when the destination span is too small.
- `TrySplitPath(...)` returns `false`, writes `0`, and leaves the destination untouched when the destination span is too small.

## 3.0.0-alpha.16

- Committed the first full BenchmarkDotNet report under `docs/benchmark-results/2026-06-15-alpha16/`.
- Updated `docs/benchmark-results/latest.md` with alpha.16 benchmark analysis and improvement targets.
- Completed a pre-beta final API and documentation sweep.
- Added `docs/release/beta-checklist.md` for remaining beta gates.
- Added `docs/release/migration-alpha.md` for alpha.13 through alpha.15 breaking-change migration notes.
- Added `docs/development/package-readmes.md` to document which README is packed by each package.
- Updated API stabilization, API freeze, package maturity, release decisions, benchmark status, and README documentation.
- Bumped all packages to `3.0.0-alpha.16`.
- No matcher semantics changed in this increment.

## 3.0.0-alpha.15

DI surface review and preview/stability documentation checkpoint.

### Changed

- Made `PattrnOptions<TSegment, TValue>` internal because it is an implementation detail of fluent DI registration.
- Removed public `PattrnRegistrationContext<TSegment, TValue>.Builder` access to keep registration sources focused on contributing patterns instead of configuring index-wide policy.
- Retained `IPattrnProvider<TSegment, TValue>` as a preview convenience for named-index resolution while documenting keyed-service resolution as the framework-native alternative.
- Updated DI docs to classify fluent registration as stable candidate and provider/registration-source APIs as preview.
- Added package maturity documentation for core, strings, DI, and routing packages.

### Added

- Added `PattrnRegistrationContext<TSegment, TValue>.AddPattern(...)` overloads so DI registration sources can contribute generic `PatternSegment<TSegment>` registrations without direct builder access.
- Added a DI surface test covering `AddPattern(...)` source registration and named-parameter captures.

### Semantics

- Matching behavior is unchanged.
- DI registration behavior is unchanged except for the reduced public registration-source context surface.
- All packages remain preview in the alpha line; routing and diagnostics remain preview for later stabilization.

## 3.0.0-alpha.11

Benchmark and API stabilization checkpoint.

- Expanded BenchmarkDotNet coverage for core value matching, detailed matches, named-parameter captures, terminal catch-alls, routing helpers, diagnostics, validation, and build paths.
- Added separate routing and builder benchmark classes so package-specific overhead can be measured independently from core matching.
- Added API stabilization documentation that classifies the core, routing, strings, and DI surfaces before beta.
- Updated benchmark documentation and placeholder reports for the alpha.11 checkpoint.
- No public matcher semantics changed in this increment.

## 3.0.0-alpha.10

Duplicate-pattern policy and build-validation increment.

### Added

- Added `DuplicatePatternRegistrationBehavior` with `Append`, `Throw`, `Replace`, and `Ignore`.
- Added `PattrnIndexBuilder<TSegment, TValue>.DuplicatePatternRegistrationBehavior`.
- Added `UseDuplicatePatternRegistrationBehavior(...)` for explicit registration-time duplicate handling.
- Added `ValidateOnBuild(...)` overloads to reject diagnostics during `Build()`.
- Added `DisableBuildValidation()` to return to permissive builds.

### Semantics

- The default duplicate-pattern behavior remains `Append`, preserving the previous builder behavior.
- Duplicate-pattern behavior is a builder registration policy. It is separate from `DuplicateValueMatchMode`, which controls duplicate values emitted by a compiled index at match time.
- Build validation is opt-in and generic. It can reject diagnostics by severity or by caller-supplied predicate without introducing route-, authorization-, or filesystem-specific rules into the core.

## 3.0.0-alpha.9

Generic builder diagnostics increment.

### Added

- Added `PattrnIndexBuilder<TSegment, TValue>.GetDiagnostics()`.
- Added `PatternDiagnostic<TSegment>`, `PatternDiagnosticKind`, and `PatternDiagnosticSeverity`.
- Added advisory diagnostics for duplicate structural patterns, ambiguous parameter names, wildcard/literal overlap, and catch-all overlap.
- Added `docs/reference/diagnostics.md`.

### Semantics

- Diagnostics are advisory and do not change matching behavior.
- Duplicate or overlapping patterns remain valid unless a higher-level package or application policy rejects them.
- The diagnostics API remains generic and does not introduce route-specific validation into the core package.

## 3.0.0-alpha.8

- Added the optional `Pattrn.Routing` companion package.
- Added route-template parsing for literals, named parameters, and terminal named catch-alls.
- Added route builder helpers such as `AddRoute`, `ContainsRoute`, `RemoveRoute`, and `RemoveAllRoute`.
- Added route path matching helpers such as `MatchRouteToArray` and `MatchRouteDetailedToArray`.
- Kept constraints, optional segments, URL normalization, ASP.NET Core integration, and OpenAPI semantics out of the core package.

## 3.0.0-alpha.7

Generic terminal catch-all increment for the segmented core.

### Added

- Added `PatternSegment<TSegment>.CatchAll()` for anonymous terminal catch-all registrations.
- Added `PatternSegment<TSegment>.CatchAll(string name)` for named terminal catch-all registrations.
- Added `PatternSegmentKind.CatchAll`, `PatternSegment<TSegment>.IsCatchAll`, and `PatternSegment<TSegment>.CatchAll()`.
- Added `PatternMatchKind.CatchAll` for detailed match results produced by pure catch-all patterns.
- Added catch-all tests for zero-segment matches, multi-segment matches, detailed captures, prefix matching, removal, and terminal validation.

### Semantics

- Catch-all segments match zero or more remaining input segments.
- Catch-all segments must be terminal.
- Named catch-alls emit one `PatternCapture<TSegment>` per captured segment. The core intentionally does not join captured segments into strings.
- Route-like syntax, optional route segments, URL normalization, and constraint parsing remain out of the core package for future companion packages.

## 3.0.0-alpha.6

Detailed match-result increment built on the generic pattern-segment model.

### Added

- Added low-allocation detailed match APIs: `GetCaptureCountUpperBound`, `MatchDetailed`, and `TryMatchDetailed`.
- Added `PatternMatch<TValue>` to describe value, pattern kind, specificity value, and capture span offsets.
- Added `PatternCapture<TSegment>` for named parameter captures.
- Added `PatternMatchKind` to identify exact, wildcard, parameter, and mixed pattern shapes.
- Added `PatternMatchResult<TSegment, TValue>` and `MatchDetailedToArray` as convenience allocation-based APIs.
- Compiled registration-level metadata so two structurally equivalent parameter patterns can preserve different parameter names.

### Unchanged

- Existing value-only `Match`, `TryMatch`, and `MatchToArray` APIs remain the hot path.
- Core still does not parse route strings or enforce route constraints.

## 3.0.0-alpha.5

- Added `PatternSegment<TSegment>` and `PatternSegmentKind` to describe generic segmented patterns without route-string parsing.
- Added builder APIs for generic pattern segments: `AddPattern`, `AddPatternRange`, `ContainsPattern`, `RemovePattern`, and `RemoveAllPattern`.
- Preserved the existing low-level `TSegment` APIs and avoided overload ambiguity with empty collection expressions.
- Enabled literal registration of values equal to the configured legacy wildcard segment through `PatternSegment<TSegment>.Literal(...)`.
- Documented that named parameters are matched as single-segment wildcard branches in this value-only alpha; capture metadata is reserved for the later detailed-match API.

## 3.0.0-alpha.4

- Reworked compiled-index construction to use iterative traversal instead of recursive node compilation.
- Made builder child dictionaries lazy so leaf-heavy indexes avoid per-node dictionary allocations.
- Added deep-build hardening tests for exact and wildcard patterns.
- Kept public APIs and matching semantics unchanged.

## 3.0.0-alpha.3

Internal core performance hardening increment focused on exact-child lookup in the packed read engine.

### Changed

- Added adaptive exact-child lookup for wide packed nodes.
- Kept small child ranges on linear scan to avoid unnecessary lookup-table overhead.
- Added per-node open-addressed child lookup slots for larger fan-out nodes.
- Preserved configured segment comparer semantics, including case-insensitive matching and hash collisions.
- Kept wildcard traversal, matching semantics, and public APIs unchanged from `3.0.0-alpha.2`.

### Validation

- Added coverage for wide exact fan-out, custom comparers, hash collisions, and wildcard coexistence.
- Core, strings, and dependency-injection tests continue to pass.

## 3.0.0-alpha.2

Internal read-engine increment focused on the roadmap packed compiled representation.

### Changed

- Replaced the immutable matching runtime with packed node, child, and value arrays.
- Stored exact child references as integer node indexes instead of object references.
- Stored exact child entries and per-node value ranges in contiguous arrays.
- Replaced recursive wildcard matching traversal with an iterative stack traversal.
- Replaced the wildcard traversal `List<T>` allocation with a stack-backed traversal stack and `ArrayPool<T>` fallback.
- Compiled only one mode-specific value array per index instead of storing both raw and locally deduplicated values.
- Removed stale object-graph runtime types from the core assembly.
- Kept the public API unchanged from `3.0.0-alpha.1`.

### Validation

- Core, strings, and dependency-injection tests continue to pass.
- The benchmark harness remains the baseline path for before/after performance reports.

## 3.0.0-alpha.1

Breaking CUPID cleanup focused on a smaller, more composable product surface.

### Changed

- Slimmed `IPattrnIndex<TSegment, TValue>` to hot segmented-path operations only.
- Removed `ReadOnlyMemory<TSegment>` and `IEnumerable<TSegment>` members from the core interface and concrete index.
- Added core extension methods for `ReadOnlyMemory<TSegment>` and `IEnumerable<TSegment>` convenience matching.
- Moved dotted/separated string helpers out of the core package into the new `Pattrn.Strings` package.
- Simplified `Pattrn.DependencyInjection` around the fluent registration builder.
- Removed the old DI overload family.
- Removed provider-aware DI builder callbacks.
- Removed `IServiceProvider` access from `PattrnRegistrationContext`; use constructor injection in registration sources instead.
- Kept neutral `RepositoryType=none` package metadata because no Git repository exists yet; `RepositoryUrl` remains omitted.
- Removed `LangVersion=latest` and `AnalysisLevel=latest` to use SDK defaults for reproducibility.

### Added

- New `Pattrn.Strings` package.
- New `Pattrn.Strings.Tests` project.
- Public API snapshot coverage for the strings package.

### Unchanged

- Matching semantics are unchanged.
- The core package remains dependency-free.
- The DI package still supports default and named/keyed singleton indexes.
- Target framework remains .NET 10 only.
