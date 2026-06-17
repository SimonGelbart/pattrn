# Beta readiness review

This review records the `3.0.0-alpha.20` readiness decision before cutting a first beta package line.

No matcher behavior changed in this increment. No public API shape changed in this increment. The purpose is to make the release posture explicit and to confirm that the repository is clean enough to move from alpha implementation work to beta feedback.

## Decision

At alpha.20, the project was ready to prepare `3.0.0-beta.1` after one final package build from the alpha.20 source. Alpha.21 subsequently revised the pre-beta plan so explicit generic pattern segments become the primary core model. Alpha.23 completed the builder side of that plan with tokenless default builder creation. Alpha.24 completed the stable matching-contract metadata: pattern identity and registration-order metadata. The later alpha line completed the hot matching versus explanation boundary, generic/string normalization boundary, string ergonomics, framework-neutral route-template expansion, route constraint validation, and optional/defaulted route expansion metadata hardening. The remaining pre-beta API gaps are specificity/ranking customization, diagnostics hardening, serialization-friendly registrations, benchmark refresh, and route-template naming/maturity decisions.

The beta should still be treated as an API feedback line, not as a new feature line. ASP.NET Core integration, OpenAPI integration, analyzers, source generators, and metadata-aware matching remain deferred. Route-template metadata, optional/defaulted suffix expansion metadata, and optional route constraint validation are now available in the preview routing package as of alpha.30. Alpha.31 additionally makes benchmark comparison and hot-path speed guardrails part of beta readiness.

## Package posture

| Package | Beta posture | Rationale |
|---|---|---|
| `Pattrn` | Stable candidate | Core generic segmented matcher has benchmark evidence, compatibility tests, and a documented product boundary. |
| `Pattrn.Strings` | Stable candidate | Thin convenience package over the core. It remains allocation-conscious documentation-wise but not positioned as the hot path. |
| `Pattrn.DependencyInjection` | Stable candidate with preview extras | Fluent registration is likely stable. Provider and registration-source APIs stay preview until usage feedback confirms they earn their place. |
| `Pattrn.Routing` | Preview | Route-template behavior is useful, but syntax, normalization, constraints, optional segments, URL decoding, and integration semantics are intentionally not frozen. |

## Confirmed before beta

- Public API snapshots were reviewed as part of the alpha.20 pass; no public API change was made.
- Package README mapping is covered by tests and matches the intended package boundaries.
- Route-specific parsing remains in `Pattrn.Routing`; the core remains generic and segmented.
- `TryMatch` and `TryMatchDetailed` no-partial-result semantics are documented and covered by tests.
- Duplicate structural-pattern registration and duplicate value emission are documented as separate concepts.
- Diagnostics remain preview.
- `Specificity` ordering is documented; exact numeric weights remain informational.
- Committed benchmark reports exist for the full alpha.17 run and the focused alpha.19 routing run.
- Generated BenchmarkDotNet artifacts, raw logs, and temporary files are not part of the cleaned source artifact. This is covered by package metadata tests.

## Benchmark interpretation

The full alpha.17 benchmark report supports the core read-path direction and confirms the detailed duplicate-heavy optimization. The focused alpha.19 routing benchmark confirms lower route-helper allocation after caller-buffer/pooling work, with a small-path latency tradeoff from temporary-array pooling.

The README should cite only specific measured results and should avoid broad performance claims that are not backed by committed benchmark reports.

## Known preview areas

These are intentionally not frozen by beta.1:

- `Pattrn.Routing` package.
- Diagnostic kind and severity names.
- DI provider and registration-source convenience APIs.
- Detailed routing convenience APIs.

## Beta.1 release rule

For `3.0.0-beta.1`, prefer bug fixes, documentation polish, and API feedback over new features. New matching semantics should wait until after beta unless they address a release blocker.


## Alpha.21 superseding note

This alpha.20 review remains useful historical evidence, but it is superseded for release sequencing by `docs/roadmap.md`. Alpha.23 completed tokenless builder creation and `AddPattern(...)`-first documentation/API review. Alpha.24 completed pattern identity and registration-order metadata. Alpha.25 separated hot matching from diagnostics-oriented explanation matching. Alpha.26 established the generic/string normalization boundary. Alpha.27 added string API ergonomics on top of that boundary. Alpha.28 expanded framework-neutral route-template metadata and optional/defaulted suffix expansion, alpha.29 added optional route constraint validation, and alpha.30 hardened route expansion metadata while refreshing the roadmap, and alpha.31 added performance guardrails plus exact-only fast-path protection. Beta should now wait for specificity/ranking customization, diagnostics hardening, serialization-friendly registrations, benchmark refresh, developer-experience cleanup, and route-template naming/maturity decisions unless the roadmap is explicitly revised.
