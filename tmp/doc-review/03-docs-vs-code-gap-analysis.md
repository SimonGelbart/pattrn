# Phase 3 — Documentation-vs-Code Gap Analysis

## Executive summary
- Core behavioral docs are broadly aligned with implementation and tests for matching semantics, captures, specificity ordering, duplicate policies, diagnostics separation, and builder validation controls.
- The largest gaps are documentation coverage gaps (not contradictions): several public APIs are implemented and tested but not clearly documented in current user-facing docs.
- No clear documented claim was found that is directly contradicted by code or tests in the audited areas.
- Main missing areas are full builder maintenance APIs (`Contains`/`Remove`/`RemoveAll`/`Clear`), full route matching helper APIs (`TryMatchRoute*`, path-specific upper bounds), and explicit catch-all coverage in `pattern-segments.md` segment-kind table.

## Docs-vs-code claim matrix

| Claim ID | Documented claim | Document path | Claimed API/type/behavior | Source evidence | Test evidence | Sample evidence | Status | Recommended action | Notes |
|---|---|---|---|---|---|---|---|---|---|
| G01 | Core `IPattrnIndex` hot-path API includes span matching, detailed matching, `Try*`, upper bounds, and `Explain`. | `docs/reference/api.md` | Core public matching surface | `src/Pattrn/IPattrnIndex.cs:12-128` | `tests/Pattrn.Tests/PublicApi.Shipped.txt:3-17`, `tests/Pattrn.Tests/PublicApiSnapshotTests.cs:8-29` | `samples/Pattrn.Samples/Program.cs:47-53` | verified | Keep as-is | API doc is accurate for listed methods. |
| G02 | `TryMatch` failure does not write partial results and reports zero written. | `docs/reference/api.md` | `TryMatch` buffer semantics | `src/Pattrn/PattrnIndex.cs:167-245` | `tests/Pattrn.Tests/TryMatchTests.cs:25-56` | None | verified | Keep as-is | Matches code and tests exactly. |
| G03 | `TryMatchDetailed` failure does not write partial matches/captures and reports zero counts. | `docs/reference/api.md` | `TryMatchDetailed` buffer semantics | `src/Pattrn/PattrnIndex.cs:417-495` | `tests/Pattrn.Tests/DetailedMatchTests.cs:56-127`, `tests/Pattrn.Tests/CompatibilitySemanticsTests.cs:141-156` | None | verified | Keep as-is | Documented behavior is strongly test-covered. |
| G04 | Terminal catch-all is supported, must be terminal, matches empty or many-segment remainder, and captures each remaining segment. | `docs/reference/matching-semantics.md`, `docs/reference/ranking-specificity.md` | Catch-all behavior and captures | `src/Pattrn/PattrnIndexBuilder.cs:274-289`, `src/Pattrn/PattrnIndex.cs:635-724` | `tests/Pattrn.Tests/CatchAllPatternTests.cs:8-99`, `tests/Pattrn.Tests/CompatibilitySemanticsTests.cs:59-79` | `docs/packages/pattrn-routing.md:143-151` | verified | Keep as-is | Core and routing docs align with implementation. |
| G05 | Specificity ordering is deterministic: literal > parameter > wildcard > catch-all; registration order breaks ties. | `docs/reference/ranking-specificity.md`, `docs/reference/matching-semantics.md` | Ranking contract | `src/Pattrn/PattrnIndexBuilder.cs:247-297`, `src/Pattrn/PattrnIndex.cs:498-555` | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs:8-159`, `tests/Pattrn.Routing.Tests/RouteCompatibilitySemanticsTests.cs:12-21` | `docs/how-to/select-best-match.md:13-41` | verified | Keep as-is | ADR 0013-aligned and test-backed. |
| G06 | Diagnostics are optional and not hot-path default; rejected candidates are opt-in. | `docs/reference/diagnostics.md`, `docs/reference/api.md` | `Explain` and `PatternExplanationOptions` semantics | `src/Pattrn/PatternExplanationOptions.cs:10-37`, `src/Pattrn/PattrnIndex.cs:635-655` | `tests/Pattrn.Tests/ExplainabilitySeparationTests.cs:31-60` | `README.md:84-93` | verified | Keep as-is | ADR 0011-aligned. |
| G07 | Build validation is opt-in and driven by severity/predicate; diagnostics remain advisory otherwise. | `docs/reference/diagnostics.md`, `docs/reference/api.md` | `ValidateOnBuild`, `DisableBuildValidation`, diagnostics policy | `src/Pattrn/PattrnIndexBuilder.cs:155-194`, `src/Pattrn/PattrnIndexBuilder.cs:1144-1161` | `tests/Pattrn.Tests/BuilderDiagnosticTests.cs:91-103` | None | verified | Keep as-is | Behavior and docs match. |
| G08 | Generic pattern-segment docs present segment kinds table, but omit explicit catch-all kind despite public API support. | `docs/reference/pattern-segments.md` | `PatternSegment<T>.CatchAll*`, `IsCatchAll` | `src/Pattrn/PatternSegment.cs:53-127` | `tests/Pattrn.Tests/PublicApi.Shipped.txt:101-115`, `tests/Pattrn.Tests/CatchAllPatternTests.cs:8-66` | None | implemented but undocumented | Add catch-all row and brief semantics to segment-kind table | Intro text mentions catch-all intent, but table omits it. |
| G09 | API reference documents pattern-segment registration helpers but not the broader builder maintenance surface (`Contains`, `Remove*`, `Clear`, tokenized/literal `Add*` range variants). | `docs/reference/api.md` | Builder maintenance and convenience APIs | `src/Pattrn/PattrnIndexBuilder.cs:367-744`, `src/Pattrn/PattrnIndexBuilder.cs:804-811` | `tests/Pattrn.Tests/BuilderPolishTests.cs:39-137`, `tests/Pattrn.Tests/PublicApi.Shipped.txt:131-165` | None | implemented but undocumented | Expand API reference with complete builder API table or explicit “additional public methods” section | Current docs can understate usable builder surface. |
| G10 | Routing package docs describe `MatchRoute*` array methods but omit `TryMatchRoute`, `TryMatchRouteDetailed`, `GetRouteMatchCountUpperBound`, and `GetRouteCaptureCountUpperBound`. | `docs/packages/pattrn-routing.md` | Route extension helper surface | `src/Pattrn.Routing/RoutePattrnIndexExtensions.cs:15-48`, `src/Pattrn.Routing/RoutePattrnIndexExtensions.cs:73-153` | `tests/Pattrn.Routing.Tests/PublicApi.Shipped.txt:76-84`, `tests/Pattrn.Routing.Tests/RoutePatternExtensionTests.cs:80-88` | None | implemented but undocumented | Add “span/try/upper-bound route helpers” subsection with failure semantics | Important for low-allocation route call paths. |
| G11 | Core APIs remain synchronous and hot-path-oriented. | `README.md`, `docs/reference/api.md` | No async match APIs in core public surface | `src/Pattrn/IPattrnIndex.cs:12-128` | `tests/Pattrn.Tests/PublicApi.Shipped.txt:3-17` | `samples/Pattrn.Samples/Program.cs:28-53` | verified | Keep as-is | ADR 0007-aligned. |
| G12 | Routing remains preview and route-layer constraints/defaults/optionals do not alter core specificity. | `docs/packages/pattrn-routing.md`, `docs/reference/ranking-specificity.md` | Routing boundary semantics | `src/Pattrn.Routing/RoutePattrnIndexBuilderExtensions.cs`, `src/Pattrn.Routing/RouteConstraintValidation*.cs` | `tests/Pattrn.Routing.Tests/RouteCompatibilitySemanticsTests.cs:47-70`, `tests/Pattrn.Routing.Tests/RouteConstraintValidationTests.cs:25-116` | `README.md:141-157` | verified | Keep as-is | ADR 0005 and ADR 0013 aligned. |

## Public API documentation gaps
1. `PatternSegment<TSegment>.CatchAll()` / `.CatchAll(name)` and `IsCatchAll` are public and tested but incompletely covered in `docs/reference/pattern-segments.md` segment-kind table.
2. `PattrnIndexBuilder<TSegment, TValue>` public maintenance APIs (`Contains*`, `Remove*`, `RemoveAll*`, `Clear`, `AddRange*`) are public/tested but not fully surfaced in `docs/reference/api.md`.
3. `Pattrn.Routing` route helper APIs for try/upper-bound workflows (`TryMatchRoute*`, `GetRoute*UpperBound`) are public/tested but not called out in package docs.

## Documented-but-not-implemented claims
- None confirmed in audited current docs.

## Implemented-but-undocumented behavior
- Catch-all segment kind omission in `docs/reference/pattern-segments.md` table.
- Full builder maintenance API surface omission in `docs/reference/api.md`.
- Route span/try/upper-bound extension APIs omission in `docs/packages/pattrn-routing.md`.

## Behavior with tests but no docs
- Builder maintenance operations (`Contains`, `Remove`, `RemoveAll`, `Clear`) and convenience range overloads are strongly covered (`tests/Pattrn.Tests/BuilderPolishTests.cs`) but only partially documented in reference API page.
- Route try/upper-bound helpers are public and snapshot-checked (`tests/Pattrn.Routing.Tests/PublicApi.Shipped.txt`) with functional route-extension tests, but not surfaced in routing package docs.

## Behavior with docs but no tests
- No high-confidence behavior-level gaps found in the audited scope; most core/routing behavior claims are covered by semantic tests and public API snapshots.

## Invalid or risky examples/snippets
- No clearly broken snippets were found in the audited current docs.
- Risk note: some examples are intentionally fragmentary (for example DI snippets assume an existing `provider`), which is acceptable but should remain clearly contextual.

## Blocking questions
- None.

## Nonblocking questions
1. Should `docs/reference/api.md` aim to enumerate the full public builder surface, or intentionally stay “common-path only” with a link to generated API references?
2. Should `docs/packages/pattrn-routing.md` add a compact quick-reference table for low-allocation route helpers (`TryMatchRoute*`, `GetRoute*UpperBound`) to improve discoverability?
3. Should `docs/reference/pattern-segments.md` include a dedicated catch-all subsection (not only table row) to mirror current catch-all coverage in matching/ranking docs?

## Recommended next phase
- Phase 4 should execute targeted doc edits for the confirmed coverage gaps above, preserve current behavior statements that are already verified, and avoid semantic rewrites where docs/tests/code already align.
