# Beta readiness reset before public API review

This planning note refreshes the roadmap and beta-readiness picture after the recent core semantics and lifecycle stabilization tranche and before issue #25, **Review public API surface before beta**.

It is not the #25 API review. It does not propose public API changes, runtime behavior changes, product features, generators, analyzers, routing integrations, DTO implementations, benchmark infrastructure, diagnostics changes, or package metadata changes.

## Current state summary

Pattrn remains pre-beta. The product north star is still a small, fast, deterministic compiled index for matching hierarchical segmented keys in .NET backend applications and libraries.

The current repository and issue state support treating the recent core semantics stabilization tranche as mostly complete:

- #26, matching semantics docs/tests audit, is closed as completed.
- #33, registration-order tie-breaking tests, is closed as completed.
- #35, duplicate value behavior tests, is closed as completed.
- #36, prefix matching traversal order tests, is closed as completed.
- #34, duplicate structural pattern behavior tests, is closed as completed.
- #37, capture behavior across parameters and catch-alls tests, is closed as completed.
- #38, builder/index lifecycle and concurrency docs, is closed as completed.

The current reference docs also reflect that stabilization work:

- `docs/reference/matching-semantics-audit.md` maps documented matching and ranking semantics to tests.
- `docs/reference/matching-semantics.md` documents exact, prefix, wildcard, parameter, catch-all, duplicate value, ordering, and lifecycle semantics.
- `docs/reference/ranking-specificity.md` documents the fixed generic specificity order and registration-order tie-breaking.
- `docs/reference/duplicate-behavior.md` separates builder-time structural duplicate policy from match-time duplicate value emission.
- `docs/reference/api.md` and ADR 0014 document builders as mutable single-writer construction objects and compiled indexes as immutable snapshots safe for concurrent readers.

The largest remaining beta risks are no longer basic core semantics coverage. They are public API review, benchmark baseline comparison, diagnostic stability policy, registration DTO / deterministic rebuild design, and package/docs polish.

## Roadmap sections status

| Roadmap area | Status | Notes |
|---|---|---|
| Product identity and operating model | Mostly complete / needs refresh | The roadmap, project profile, ADR index, and docs structure now align around a domain-neutral segmented-pattern index. This reset refreshes the backlog picture rather than rewriting the roadmap. |
| Core semantics stabilization | Mostly complete | #26 and #33-#38 are closed. Matching semantics, duplicate behavior, ranking, captures, and lifecycle docs are now much stronger. #25 still needs to review whether the public API surface expresses those semantics cleanly before beta. |
| Ranking and specificity contract | Mostly complete | ADR 0013 and the ranking reference commit to fixed generic ranking plus consumer-side sorting. Registration-order and prefix traversal coverage is now closed through #33 and #36. Do not add ranking plugins before beta. |
| Performance gates and benchmark pipeline | Incomplete | Benchmark docs list the protected paths and current coverage, but baseline comparison is still missing. #32 remains important before beta performance claims are relied on. |
| Diagnostics and validation hardening | Needs decision | Diagnostics exist and are optional, but #39 still needs to define which diagnostic IDs, messages, and properties are stable or best-effort. |
| Serialization-friendly registrations and deterministic rebuild | Needs design | #41 must decide the registration DTO / rebuild model before #42 and #43 can proceed. Compiled index internals remain private under ADR 0009. |
| Package stabilization | Not started / next after #25 | The stable candidate packages are `Pattrn`, `Pattrn.Strings`, and `Pattrn.DependencyInjection`; `Pattrn.Routing` remains preview. Package stabilization should follow the public API review. |
| Documentation and samples | Partial | Core reference documentation is much stronger. Package docs, examples, limitations, and adoption docs still need beta-pass review after #25 clarifies the intended surface. |
| Beta feedback surface | Not ready | Depends on #25, #32, #39, and a #41 decision or explicit deferral. The beta surface should not invite feedback until stable, preview, and deferred areas are clearly separated. |

## Open issue triage

### Immediate design / audit issues

- #25 Review public API surface before beta
- #32 Add benchmark baseline comparison
- #39 Define diagnostic stability policy
- #41 Design registration DTOs for deterministic rebuild

### Blocked follow-ups

- #42 Add JSON roundtrip tests for registration DTOs — blocked by #41
- #43 Document deterministic rebuild from registrations — blocked by #41

### Deferred benchmark expansion

- #50 Investigate scale and adversarial benchmark scenarios
- #51 Measure retained compiled-index memory size
- #52 Add comparison benchmarks against simple dispatch baselines

These remain useful, but they should not jump ahead of #32 and the DTO/rebuild design decisions that may shape benchmark corpus, persistence, and scenario planning.

## Recommended next sequence

1. Merge this roadmap/beta-readiness reset PR.
2. Do #25 public API review before beta.
3. Do #32 benchmark baseline comparison.
4. Do #39 diagnostic stability policy.
5. Do #41 registration DTO / deterministic rebuild design.
6. Then unblock #42 and #43.
7. Revisit #50-#52 after #32 and #41.

#25 should come next because the project is still pre-beta and public API changes become more expensive after beta. The core semantics are now documented and covered well enough to ask whether the API surface is the right long-term shape. More public-surface, diagnostics, DTO, benchmark, or expansion work should not proceed until the maintainer has reviewed what is stable, preview, deferred, internal, or still worth changing.

## Uploaded roadmap ideas classification

| Idea | Classification | Why | Dependency / next action |
|---|---|---|---|
| Evaluate AOT and trimming compatibility for stable packages | Beta polish / possible pre-beta | Package confidence matters before beta, especially for backend library adoption. | Create a focused issue after #25 or during package stabilization. Keep `Pattrn.Routing` preview. |
| Investigate zero-allocation matching for Pattrn.Strings | 1.0 candidate / design first | It could improve the string helper story, but may affect API direction and benchmark claims. | Wait for #25 and #32. Start with design/spike and benchmarks before API changes. |
| Add property-based semantic tests against a naive oracle | Beta polish | It strengthens confidence after example-based semantics tests, especially around overlaps and captures. | Create a focused deterministic test issue if the maintainer wants more pre-beta confidence. |
| Design internal pattern corpus IR and index plan | Deferred design | Could help benchmarks, snapshots, and future generators, but it risks becoming infrastructure for deferred expansion. | Wait for #41 and the benchmark expansion issues. |
| Add diagnostic index statistics | Deferred / diagnostics design | Useful for explainability, but it touches diagnostics policy and potentially public API shape. | Wait for #39 and #25. |
| Add optional build/resource limits | Deferred / design | Useful for hostile or untrusted inputs, but it may affect builder APIs and validation posture. | Wait for #25, #41, and #50. |
| Design Pattern Doctor report format | Deferred / diagnostics design | Could be useful tooling, but report stability depends on diagnostic stability first. | Wait for #39. |
| Design compatibility snapshots for pattern sets | Deferred / rebuild design | Snapshot design depends on the registration DTO / deterministic rebuild story and should not serialize compiled internals. | Wait for #41 and #42. |
| Source generators/analyzers | Post-1.0 / deferred | Explicit expansion area and source-code tooling, not a beta blocker. | Do not add before beta. Revisit only after stable API feedback. |
| Typed captures | Post-1.0 / deferred | Public API expansion that could complicate the small core model. | Revisit after 1.0 feedback. |
| Observability/OpenTelemetry | Post-1.0 / probably out of core scope | Integration concern, not required for a small matching index beta. | Defer. Consider only as companion guidance if users ask. |
| Segment codec abstraction | Post-1.0 / deferred | Could complicate the core segment model and blur string/domain boundaries. | Defer unless #25 exposes a concrete need. |
| Adaptive index compiler implementation | Post-1.0 / research | Implementation complexity and performance risk are high without benchmark evidence. | Defer until benchmark data justifies it. |
| Optional ASP.NET bridge | Post-1.0 / companion package idea | Expansion beyond current beta scope and explicitly outside core semantics. | Defer. Keep any future bridge outside the core. |

## Recommended new issues

Create only a small number of new issues now, and only after maintainer confirmation. Do not create them in this PR.

### Evaluate AOT and trimming compatibility for stable packages

Scope:

- Audit `Pattrn`, `Pattrn.Strings`, and `Pattrn.DependencyInjection` for trimming/AOT assumptions.
- Identify reflection, dynamic code, or linker-sensitive APIs.
- Document support posture before beta.
- Keep `Pattrn.Routing` preview.

### Add property-based semantic tests against a naive oracle

Scope:

- Generate representative segmented patterns and inputs.
- Compare compiled-index results against a simple reference implementation.
- Cover exact/prefix, literal/parameter/wildcard/catch-all, duplicates, and captures where practical.
- Keep generated cases deterministic and reproducible.

### Investigate zero-allocation matching for Pattrn.Strings

Scope:

- Design or spike whether string helpers can match pre-split or span-like inputs without per-call splitting allocations.
- Benchmark before changing APIs.
- Do not change public APIs until #25 reviews the direction.

## Explicit deferrals

Do not start these before beta:

- source generators
- analyzers
- ASP.NET bridge
- typed captures
- custom ranking plugins
- adaptive index compiler
- observability integration
- broad routing integrations
- DTO implementations before #41
- benchmark expansion before #32

These may be valid post-1.0 or feedback-driven work, but they should not distract from beta stabilization. The beta path should stabilize the small core/string/DI surface, keep routing preview, preserve the domain-neutral boundary, and avoid expanding public API surface before #25.

## Decision points for maintainer

Before starting #25, the maintainer should decide:

- Confirm #25 is the next active issue.
- Decide whether AOT/trimming should be pre-beta or beta-polish.
- Decide whether property-based tests should be added before beta.
- Decide whether zero-allocation `Pattrn.Strings` work should be design-only before beta.
- Confirm #41 should design registration DTOs before any #42/#43 work starts.
- Confirm #50-#52 stay deferred until #32 and #41 are clearer.
- Confirm source generators, analyzers, ASP.NET bridge, typed captures, custom ranking plugins, adaptive index compiler work, and observability integration stay deferred.
