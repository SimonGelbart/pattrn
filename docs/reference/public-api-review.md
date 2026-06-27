# Public API review before beta

This document is the maintainer-facing public API review for issue #25. It is an audit and recommendation record only: it does not implement broad API changes, does not redesign the library, and does not stabilize `Pattrn.Routing`.

## Executive summary

The overall recommendation is to treat `Pattrn`, `Pattrn.Strings`, and `Pattrn.DependencyInjection` as stable beta candidates with targeted follow-up decisions, while keeping `Pattrn.Routing` preview and non-blocking.

- `Pattrn` is the strongest stable candidate. Its public surface aligns with the segmented, domain-neutral core model, synchronous matching, immutable compiled indexes, fixed ranking, duplicate behavior, captures, and optional diagnostics documented in the reference docs and accepted ADRs.
- `Pattrn.Strings` is a stable beta candidate as an ergonomic companion package when its allocation caveats stay explicit. Splitting, trimming, empty-segment handling, case sensitivity, and normalization belong here; zero-allocation string matching should remain deferred to [#68](https://github.com/SimonGelbart/pattrn/issues/68).
- `Pattrn.DependencyInjection` is a stable beta candidate if it remains a thin optional startup-registration layer over immutable indexes. It should not grow framework-specific behavior beyond ordinary `Microsoft.Extensions.DependencyInjection` patterns.
- `Pattrn.Routing` should remain preview, framework-neutral, and non-blocking for beta. Route-template parsing, expansion, route metadata preservation, and constraint validation are useful, but they must not imply ASP.NET Core endpoint routing compatibility or route precedence in the core.

The main pre-beta decisions are diagnostic stability ([#39](https://github.com/SimonGelbart/pattrn/issues/39)), registration DTO posture for deterministic rebuild ([#41](https://github.com/SimonGelbart/pattrn/issues/41), [#42](https://github.com/SimonGelbart/pattrn/issues/42), [#43](https://github.com/SimonGelbart/pattrn/issues/43)), AOT/trimming posture ([#66](https://github.com/SimonGelbart/pattrn/issues/66)), and semantic/property-test confidence ([#67](https://github.com/SimonGelbart/pattrn/issues/67)). Benchmark baseline comparison ([#32](https://github.com/SimonGelbart/pattrn/issues/32)) and string zero-allocation investigation ([#68](https://github.com/SimonGelbart/pattrn/issues/68)) should remain linked follow-ups rather than scope for this review.

## Package status table

| Package | Recommendation | Beta status | Notes |
|---|---|---|---|
| `Pattrn` | Stable candidate with targeted follow-ups | Stable candidate | Core public APIs match the domain-neutral segmented model, synchronous matching, immutable compiled index lifecycle, fixed ranking, and documented duplicate/capture semantics. Diagnostics need a stability decision in #39. |
| `Pattrn.Strings` | Stable candidate with allocation caveats | Stable candidate | String splitting, separators, trimming, empty-segment behavior, case sensitivity, and normalization are correctly isolated from the core. Zero-allocation string APIs remain deferred to #68. |
| `Pattrn.DependencyInjection` | Stable candidate if kept thin | Stable candidate | Service registration APIs support startup construction and immutable singleton index use without adding DI concepts to the core. Keep named provider semantics small and documented. |
| `Pattrn.Routing` | Keep preview | Preview / non-blocking | Route parsing and validation remain useful but should not be treated as beta-stable or as ASP.NET endpoint-routing semantics. Routing remains outside initial stable scope by ADR 0005. |

## Public API inventory

### `Pattrn`

#### Stable candidate

- `PattrnIndex<TSegment, TValue>` and `IPattrnIndex<TSegment, TValue>` as immutable, concurrent-reader-safe compiled indexes.
- `PattrnIndexBuilder<TSegment, TValue>` as the single-writer mutable construction API.
- `PatternSegment<TSegment>` and `PatternSegmentKind` for explicit literal, parameter, wildcard, and terminal catch-all segments.
- `PatternCapture<TSegment>`, `PatternMatch<TValue>`, `PatternMatchResult<TSegment, TValue>`, `PatternMatchKind`, `Specificity`, `PatternId`, and `RegistrationOrder` metadata for detailed matches and consumer-side sorting.
- `MatchOptions`, `PrefixMatchMode`, and `DuplicateValueMatchMode` for stable match-time behavior.
- `DuplicatePatternRegistrationBehavior` for build-time duplicate registration policy.
- Span-based matching APIs: `GetMatchCountUpperBound`, `Match`, `TryMatch`, `GetCaptureCountUpperBound`, `MatchDetailed`, and `TryMatchDetailed`.
- Materializing convenience APIs: `MatchToArray`, `MatchDetailed`, and `MatchDetailedToArray`, with allocations understood as convenience behavior.
- Optional diagnostics and explanation APIs: `PatternDiagnostic<TSegment>`, `PatternDiagnosticKind`, `PatternDiagnosticSeverity`, `PatternMatchExplanation<TSegment, TValue>`, `PatternRejectedCandidate`, and `PatternExplanationOptions`, subject to #39.
- Interface extension helpers in `PattrnIndexExtensions` for `IPattrnIndex<TSegment, TValue>` consumers.

#### Needs follow-up decision

- Diagnostic stability: decide which diagnostic kinds, severities, and explanation fields are stable for beta in #39.
- Registration DTO / deterministic rebuild: decide whether DTO design is beta-blocking or deferred in #41, with #42 and #43 covering tests and documentation if accepted.
- AOT/trimming posture for the stable packages in #66.
- Additional semantic confidence from property-based tests in #67.
- Benchmark baseline comparison in #32 before using benchmark deltas as release-gate evidence.

#### Deferred / preview / internal

- Compiled trie/index internals, nodes, child tables, writer helpers, and accumulator types are implementation details and must not become serialized or mergeable public formats.
- Async matching APIs remain out of scope by ADR 0007.
- Public ranking comparers, ranking plugins, `SpecificityOptions`, and route-aware precedence hooks remain out of scope by ADR 0013.
- Thread-safe mutable builders remain out of scope by ADR 0014.
- Source generators, analyzers, ASP.NET bridges, typed-capture DTOs, and compiled-index serialization are deferred.

### `Pattrn.Strings`

#### Stable candidate

- `StringNormalizationOptions` for separator choice, case sensitivity, trimming, empty-segment behavior, segment normalization, builder creation, and splitting.
- `StringCaseSensitivity`, `StringEmptySegmentBehavior`, and `StringSegmentTrimBehavior` as explicit string-policy enums.
- `StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` as string-path builder/index facades over the core.
- Static factory helpers on `StringPattrnIndexBuilder` and `StringNormalizationOptions` for dotted, slash, custom-separated, and tokenized builders.
- `PattrnStringExtensions` for convenience registration and matching against `PattrnIndexBuilder<string, TValue>` and `IPattrnIndex<string, TValue>`.

#### Needs follow-up decision

- Confirm docs consistently state that string helpers split and normalize strings before delegating to core, and therefore do not inherit the allocation profile of already-segmented core span APIs.
- Keep zero-allocation string matching investigation in #68 rather than making it a beta blocker unless the maintainer explicitly changes scope.
- Confirm whether the `Dotted`, `Separated`, `Slash`, and `Tokenized` naming set is clear enough before beta; if not, create a focused naming issue rather than changing this PR.

#### Deferred / preview / internal

- Zero-allocation string APIs, parser/source-generator approaches, and specialized string token DTOs are deferred to #68 or later.
- URL, filesystem, route, glob, culture-specific domain policy, and ASP.NET semantics remain outside `Pattrn.Strings`.

### `Pattrn.DependencyInjection`

#### Stable candidate

- `PattrnServiceCollectionExtensions.AddPattrnIndex<TSegment, TValue>` for registering immutable indexes at startup.
- `PattrnRegistrationBuilder<TSegment, TValue>` and `PattrnRegistrationContext<TSegment, TValue>` for configuring registrations without exposing core internals.
- `IPattrnRegistrationSource<TSegment, TValue>` for optional registration sources discovered from DI.
- `IPattrnProvider<TSegment, TValue>` for named index lookup when multiple indexes exist.
- Keyed-service-backed named index lookup through the provider APIs, while DI storage/options implementation types remain internal.

#### Needs follow-up decision

- Confirm named provider semantics are intentionally stable for beta, including missing-name behavior and whether names are case-sensitive.
- Confirm lifetimes are documented as startup-built immutable indexes suitable for singleton service use.
- Confirm DI remains limited to `Microsoft.Extensions.DependencyInjection` abstractions and does not grow ASP.NET-specific behavior.

#### Deferred / preview / internal

- Keyed-service specialization, options-monitor rebuild loops, background refresh, hosted-service integration, ASP.NET endpoint registration, and framework-specific discovery remain out of scope.
- Mutable or hot-reload index replacement semantics should not be implied by the DI API before a separate design decision.

### `Pattrn.Routing`

#### Preview

- `RoutePattern`, `RouteTemplate`, `RouteTemplateParser`, `RouteSegment`, `RouteSegmentKind`, `RouteParameter`, `RouteConstraint`, `RouteTemplateExpansion`, and `RouteTemplateDiagnostic` provide route-template parsing, structural compilation, expansion, and metadata preservation.
- `RouteConstraintValidation`, `RouteConstraintValidationOptions`, `RouteConstraintValidationResult`, `RouteConstraintValidationFailure`, `IRouteConstraintValidator`, `DelegateRouteConstraintValidator`, `RouteConstraintValidators`, and `RouteConstraintValidatorRegistry` provide route-layer validation helpers.
- `RoutePattrnIndexBuilderExtensions` and `RoutePattrnIndexExtensions` provide preview convenience APIs for adding, removing, splitting, and matching route paths through `PattrnIndex<string, TValue>`.

#### Needs follow-up decision

- Confirm package metadata and package docs visibly mark `Pattrn.Routing` as preview and non-blocking.
- Confirm route constraint validation is described as route-layer helper behavior, not core specificity or ASP.NET endpoint-routing compatibility.
- Confirm whether any route APIs should be explicitly marked preview in documentation before beta.

#### Deferred / internal

- ASP.NET Core integration, endpoint routing semantics, route precedence, URL decoding, link generation, HTTP method matching, OpenAPI behavior, and route-specific ranking in the core are out of scope.
- Routing should not block beta stabilization of `Pattrn`, `Pattrn.Strings`, or `Pattrn.DependencyInjection` unless the maintainer explicitly decides otherwise.

## Stable candidate APIs

The stable candidate surface is coherent because the public concepts map directly to durable product decisions:

- The core uses explicit segmented matching rather than route, glob, filesystem, or HTTP interpretation. This follows the product profile, package-boundary docs, and ADR 0001.
- The primary read path is synchronous because matching is in-memory and does not perform I/O. This follows ADR 0007.
- Builders and compiled indexes have a clear lifecycle: builders are mutable single-writer construction objects, while compiled indexes are immutable snapshots safe for concurrent readers. This follows ADR 0014.
- Ranking is fixed and deterministic. Detailed match metadata exposes enough information for consumer-side sorting without a public ranking plugin. This follows ADR 0013.
- Compiled index internals are private. Persistence should be based on registrations or consumer-owned metadata, not serialized compiled trie state. This follows ADR 0009 and links to #41, #42, and #43.
- Companion package boundaries remain clear. `Pattrn.Strings` owns string normalization, `Pattrn.DependencyInjection` owns thin DI registration, and `Pattrn.Routing` owns framework-neutral route helpers while preview. This follows ADR 0004 and ADR 0005.
- Allocation expectations are understandable: span-based core APIs support caller-provided buffers; materializing `ToArray` APIs allocate intentionally; string and routing helpers allocate for parsing/splitting/normalization unless a future design changes that.

## Preview / deferred APIs

The following areas should not be treated as beta-stable in this review:

- `Pattrn.Routing` remains preview and non-blocking.
- Source generators remain deferred.
- Analyzers remain deferred.
- ASP.NET Core bridges and endpoint-routing semantics remain deferred.
- Typed captures and registration DTO implementations remain deferred unless separately accepted.
- Custom ranking plugins, ranking comparers, `SpecificityOptions`, and route-specific precedence remain deferred.
- Compiled-index serialization remains out of scope.
- Async matching APIs remain out of scope.
- Thread-safe mutable builders remain out of scope.
- String zero-allocation APIs remain deferred to #68.
- Benchmark infrastructure and baseline comparison implementation remain out of scope for this PR and linked to #32.
- Diagnostics redesign is out of scope; only the stability policy decision is linked to #39.

## Proposed API changes before beta

### Proposal: Define diagnostic stability policy

Package: `Pattrn`

Current API: `PatternDiagnostic<TSegment>`, `PatternDiagnosticKind`, `PatternDiagnosticSeverity`, `PatternMatchExplanation<TSegment, TValue>`, `PatternRejectedCandidate`, `PatternExplanationOptions`, builder diagnostics, and `Explain` APIs.

Problem: Diagnostics are optional and not hot-path, but consumers may still depend on diagnostic kinds, severities, and explanation fields after beta.

Recommendation: Use #39 to decide which diagnostics are stable, which are best-effort, and how additions or message changes are versioned. Keep this PR as documentation only.

Breaking change before beta: Possible, depending on #39.

Risk: Stabilizing too much diagnostic detail could make future implementation improvements expensive; stabilizing too little could make diagnostics hard to rely on.

Follow-up issue: [#39 Define diagnostic stability policy](https://github.com/SimonGelbart/pattrn/issues/39).

### Proposal: Decide registration DTO beta posture

Package: `Pattrn`, possibly companion packages if DTOs include string or route metadata later.

Current API: Builders accept registrations directly; compiled index internals are private and not serialized.

Problem: Applications that need deterministic rebuild need an endorsed registration-level persistence story without exposing compiled internals.

Recommendation: Use #41 to design registration DTOs, #42 for JSON roundtrip tests, and #43 for deterministic rebuild documentation if the maintainer decides this is beta-blocking. Do not implement DTOs in this PR.

Breaking change before beta: No immediate break from this PR; possible additive API later.

Risk: Adding DTOs too early may freeze shape before real persistence needs are clear; deferring them may leave beta users to create their own models.

Follow-up issues: [#41](https://github.com/SimonGelbart/pattrn/issues/41), [#42](https://github.com/SimonGelbart/pattrn/issues/42), [#43](https://github.com/SimonGelbart/pattrn/issues/43).

### Proposal: Confirm AOT and trimming posture for stable packages

Package: `Pattrn`, `Pattrn.Strings`, `Pattrn.DependencyInjection`.

Current API: Stable-candidate packages target .NET 10 and use ordinary generic and DI patterns.

Problem: Beta users may assume stable packages have known Native AOT and trimming behavior.

Recommendation: Use #66 to evaluate and document compatibility. Decide whether this is beta-blocking or beta polish.

Breaking change before beta: Unclear; likely documentation or attribute/package metadata changes if issues are found.

Risk: Discovering compatibility gaps after beta may require harder public API or packaging changes.

Follow-up issue: [#66 Evaluate AOT and trimming compatibility for stable packages](https://github.com/SimonGelbart/pattrn/issues/66).

### Proposal: Increase semantic confidence with property-based tests

Package: `Pattrn`, with possible companion-package coverage later.

Current API: Matching semantics are documented and covered by focused tests and compatibility snapshots.

Problem: The stable candidate core has many interactions among literals, parameters, wildcards, catch-alls, prefix mode, duplicates, captures, and ranking.

Recommendation: Use #67 to compare against a naive oracle before beta if practical. Do not add test infrastructure in this PR.

Breaking change before beta: No API change expected; tests may reveal bugs requiring fixes.

Risk: Without broad semantic tests, edge-case regressions may escape before beta.

Follow-up issue: [#67 Add property-based semantic tests against a naive oracle](https://github.com/SimonGelbart/pattrn/issues/67).

### Proposal: Keep string zero-allocation work deferred

Package: `Pattrn.Strings`.

Current API: String helpers split and normalize strings before matching, with allocation-conscious core APIs available after callers provide segmented inputs.

Problem: Users may expect string convenience APIs to share core span hot-path allocation behavior.

Recommendation: Keep `Pattrn.Strings` stable for beta with explicit allocation caveats. Investigate new zero-allocation string APIs separately in #68.

Breaking change before beta: No.

Risk: If docs are unclear, users may benchmark the wrong layer or rely on convenience APIs for hot paths.

Follow-up issue: [#68 Investigate zero-allocation matching for Pattrn.Strings](https://github.com/SimonGelbart/pattrn/issues/68).

### Proposal: Confirm DI named-provider contract

Package: `Pattrn.DependencyInjection`.

Current API: `IPattrnProvider<TSegment, TValue>` and registration extensions support named indexes through ordinary DI/keyed-service registration.

Problem: Missing-name behavior, name comparer policy, and lifetime guidance are public once beta users adopt named indexes.

Recommendation: Decide whether existing named-provider behavior is stable as-is, and add a focused follow-up issue only if the maintainer wants API or documentation changes before beta.

Breaking change before beta: Possibly, if name-comparer or missing-name behavior changes.

Risk: Ambiguous provider semantics could cause small but annoying beta breaks.

Follow-up issue: Ready-to-create draft if needed:

```text
Title: Confirm Pattrn.DependencyInjection named index provider contract before beta
Body: Review `IPattrnProvider<TSegment, TValue>` and DI named-index registration behavior before beta. Confirm missing-name behavior, name comparison policy, and lifetime documentation. Keep the package thin and avoid ASP.NET-specific behavior.
```

## Follow-up issues needed

### Must decide before beta

- [#39 Define diagnostic stability policy](https://github.com/SimonGelbart/pattrn/issues/39) — decide stable vs best-effort diagnostic contracts.
- [#41 Design registration DTOs for deterministic rebuild](https://github.com/SimonGelbart/pattrn/issues/41) — decide whether registration DTOs are beta-blocking or deferred.
- [#66 Evaluate AOT and trimming compatibility for stable packages](https://github.com/SimonGelbart/pattrn/issues/66) — decide whether compatibility is beta-blocking or beta polish.
- Ready-to-create issue if maintainer wants it: confirm `Pattrn.DependencyInjection` named-index provider contract before beta.

### Beta polish

- [#42 Add JSON roundtrip tests for registration DTOs](https://github.com/SimonGelbart/pattrn/issues/42) — only after DTO design is accepted.
- [#43 Document deterministic rebuild from registrations](https://github.com/SimonGelbart/pattrn/issues/43) — only after DTO/design posture is accepted.
- [#65 Recheck roadmap and beta readiness after core semantics stabilization](https://github.com/SimonGelbart/pattrn/issues/65) — align roadmap/readiness after API decisions.
- [#67 Add property-based semantic tests against a naive oracle](https://github.com/SimonGelbart/pattrn/issues/67) — strengthen semantic confidence.

### Post-beta / 1.0

- [#32 Add benchmark baseline comparison](https://github.com/SimonGelbart/pattrn/issues/32) — important for performance evidence, but not part of this audit PR.
- [#68 Investigate zero-allocation matching for Pattrn.Strings](https://github.com/SimonGelbart/pattrn/issues/68) — keep outside beta API review unless the maintainer changes priority.
- Routing stabilization follow-up should be created only when the maintainer decides to move `Pattrn.Routing` out of preview.

## Non-goals confirmed

- No async matching APIs.
- No thread-safe mutable builder.
- No compiled-index serialization format.
- No source generators.
- No analyzers.
- No ASP.NET bridge.
- No custom ranking plugins.
- No route-specific precedence in core.
- No domain-specific semantics in core.
- No string zero-allocation API implementation.
- No benchmark infrastructure implementation.
- No DTO implementation or JSON roundtrip implementation.
- No diagnostics redesign.
- No runtime behavior changes.

## Maintainer decisions requested

- Confirm `Pattrn` is a stable beta candidate, subject to targeted follow-ups.
- Confirm `Pattrn.Strings` is a stable beta candidate with explicit allocation caveats and #68 deferred.
- Confirm `Pattrn.DependencyInjection` is a stable beta candidate if it remains thin.
- Confirm `Pattrn.Routing` remains preview and non-blocking for beta.
- Decide whether #39 diagnostic stability is beta-blocking.
- Decide whether #41 registration DTO design is beta-blocking, beta polish, or post-beta.
- Decide whether #66 AOT/trimming evaluation is beta-blocking or beta polish.
- Decide whether the DI named-provider contract needs a new focused issue before beta.
- Confirm no additional broad API changes should be implemented as part of #25.
