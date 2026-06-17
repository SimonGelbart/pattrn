# Package maturity

All `3.0.0-alpha.*` packages are preview packages. `3.0.0-alpha.20` records the beta-readiness posture, `3.0.0-alpha.21` records the explicit-segment-first architecture decision, `3.0.0-alpha.22` consolidates the roadmap and confirms that breaking alpha changes are acceptable before beta, `3.0.0-alpha.23` implements tokenless builder creation, `3.0.0-alpha.24` adds pattern identity and registration-order metadata to detailed match results, `3.0.0-alpha.25` separates hot matching from diagnostics-oriented explanation matching, `3.0.0-alpha.26` establishes the generic/string normalization boundary, `3.0.0-alpha.27` adds string-path ergonomics on top of that boundary, `3.0.0-alpha.28` expands framework-neutral route-template metadata and optional/defaulted suffix expansion, `3.0.0-alpha.29` adds optional route constraint validation above structural matching, `3.0.0-alpha.30` refreshes the roadmap while hardening optional/defaulted route expansion metadata, and `3.0.0-alpha.31` adds performance guardrails plus exact-only matching fast-path protection.

The project currently separates maturity by package so the core can stabilize without forcing companion packages to freeze too early.

| Package | Alpha maturity | Expected beta posture | Notes |
|---|---|---|---|
| `Pattrn` | Preview | Stable candidate | Core generic segmented index. This is the package that should stabilize first. |
| `Pattrn.Strings` | Preview | Stable candidate | Thin helpers over the core. Low risk, but still follows the alpha version line. |
| `Pattrn.DependencyInjection` | Preview | Stable candidate with preview extras | Fluent registration is likely to stay. Provider and registration-source APIs remain preview until usage feedback confirms they earn their place. |
| `Pattrn.Routing` | Preview / experimental | Preview | Route-template parsing is intentionally separate from the core and should remain preview for the first `3.0.0` stable core release unless usage evidence says otherwise. |

## Current decisions

- The core remains generic and segmented.
- Tokenless builder creation plus `PatternSegment<TSegment>` / `AddPattern(...)` is the primary core registration model.
- Route syntax remains outside the core.
- Routing constraint validation is available as an optional routing-layer step; route constraints/defaults/optional suffixes are preserved as metadata in the routing package.
- Diagnostics are preview, including opt-in rejected-candidate explanation hints.
- Numeric `Specificity` values are informational; only broad ordering semantics are documented.
- Duplicate registration behavior and duplicate value match behavior are intentionally separate.
- `Pattrn.DependencyInjection` keeps `IPattrnProvider<TSegment, TValue>` for now as a named-index convenience abstraction, but it remains preview.
- `PattrnRegistrationContext<TSegment, TValue>` exposes registration methods instead of the mutable builder, keeping registration sources focused on contributing patterns.

## Beta requirements

Before `3.0.0-beta.1`:

1. Complete the next implementation increment: specificity and ranking customization.
2. Run and commit a full benchmark report after any matcher/compiler refactor and before beta/RC.
3. Review public API snapshots one final time.
4. Keep DI provider and registration-source APIs preview unless there is an explicit freeze decision.
5. Keep routing clearly preview unless it receives dedicated stabilization work.
6. Keep diagnostics preview unless diagnostic names and severities are explicitly frozen.
7. Confirm package README mapping stays accurate.

## Alpha.16 package-readme decision

Each package ships documentation scoped to its boundary. See `docs/development/package-readmes.md` for the mapping. This is part of keeping the core generic and avoiding route, string, or DI-specific semantics leaking into `Pattrn`.


## Alpha.20 readiness outcome

- `Pattrn`, `Pattrn.Strings`, and the fluent `Pattrn.DependencyInjection` registration path are stable candidates for beta feedback.
- `Pattrn.Routing` remains preview.
- Diagnostics remain preview.
- DI provider and registration-source extras remain preview.
- The project should avoid new matching semantics until beta feedback or release blockers justify them.


## Alpha.21 architecture outcome

`Pattrn` remains the first package to stabilize, but beta should wait for the focused builder simplification recommended by the consolidated roadmap. Tokenized wildcard registration remains convenience; explicit generic pattern segments are the stable direction.


## Alpha.23 core API outcome

`Pattrn` now has tokenless builder creation as the default path. `Pattrn.DependencyInjection` also defaults to tokenless builders and uses `UseWildcard(...)` only when tokenized registration convenience is desired. This moved the core package closer to beta; alpha.24 and alpha.25 then completed match identity/order metadata and the fast/explainability split; alpha.26 established the generic/string normalization boundary; alpha.27 added ergonomic string facades without adding domain semantics to the core; alpha.28 expanded route-template metadata and optional/defaulted suffix expansion while keeping routing semantics outside the core; alpha.29 added optional route-layer constraint validation without changing core matching; alpha.30 hardened optional/defaulted route expansion metadata, and alpha.31 made speed guardrails part of the pre-beta contract.
