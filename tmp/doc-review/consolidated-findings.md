# Consolidated Findings

## Current state
- Phase 0 bootstrap completed and committed.
- Phase 1 inventory completed with repository-wide documentation-like artifact mapping.
- Phase 2 internal consistency audit completed with claim-level contradiction/redundancy/staleness analysis.
- Phase 3 docs-vs-code gap analysis completed with claim-level verification against source/tests/samples.
- Phase 4 documentation map completed with explicit path ownership, topic ownership, docs-site curation rules, and standards update proposals.
- Phase 5 consolidation/archive/delete planning completed with overlap checks, canonicalization actions, and approval gating.
- Phase 6 user-journey review completed across the required newcomer path.
- Phase 7 implementation/validation completed for approved non-destructive documentation cleanup decisions.

## Confirmed findings
- Primary current entry path is `README.md` -> `docs/README.md` -> tutorials/how-to/reference/package docs/ADRs.
- `docs.site.json` publishes a curated subset of docs and explicitly excludes archive and benchmark-results folders.
- Package README docs are sourced from `docs/packages/*.md` and packed through `src/*/*.csproj` metadata.
- Benchmarks source-of-truth is workflow artifacts (`.github/workflows/benchmarks.yml`), while committed benchmark result docs are explicitly historical.
- Core product-boundary and ranking claims are internally consistent across README, reference docs, accepted ADRs, source, and tests.
- Core docs claims for `TryMatch`/`TryMatchDetailed` failure semantics, catch-all behavior, specificity ordering, diagnostics separation, and build-validation controls are aligned with implementation and tests.
- Phase 4 establishes explicit ownership boundaries for README, docs index, Diataxis folders, package docs, archive, benchmark-results, ADRs, and `docs.site.json` public routing.
- Phase 5 establishes a non-destructive execution order: governance consolidation first, then approved archival/deletion work with explicit gates.
- Phase 6 confirms the required newcomer path works end-to-end, but highlights onboarding clarity issues caused by stale direction text, fragmented package-install guidance, and mixed newcomer/maintainer/historical navigation on the docs hub.

## Contradictions
- `CHANGELOG.md` presents a `3.0.0-alpha.*` release train without strong historical framing, conflicting with current ADR/roadmap version-reset posture (`0.1.0-alpha.1` centralized pre-beta policy).
- Current benchmark authority docs are CI-first, but archive release checklist/history docs still include language that treats committed benchmark reports as release-proof artifacts.
- No direct current-docs-versus-code contradiction was confirmed in Phase 3 audited behavior areas.

## Redundancy
- Root `README.md` and `docs/packages/pattrn.md` are near-exact duplicates (mostly link-path differences), creating drift risk.
- Package-boundary and validation-authority statements are repeated across multiple current docs; mostly consistent but duplicated.
- Navigation responsibilities overlap between docs hub (`docs/README.md`) and section-level README pages.

## Docs-vs-code gaps
- Not all accepted ADR documents are listed in docs site route manifest (`docs.site.json` currently includes a subset).
- Legacy local workflow sections in `docs/reference/testing.md` remain transitional and should continue being framed subordinate to CI authority.
- `docs/reference/pattern-segments.md` segment-kind table omits catch-all despite public `PatternSegment<T>.CatchAll*` APIs and test coverage.
- `docs/reference/api.md` does not fully surface the broader public builder maintenance/convenience APIs (`Contains*`, `Remove*`, `RemoveAll*`, `Clear`, `AddRange*`).
- `docs/packages/pattrn-routing.md` does not explicitly cover route try/upper-bound helper APIs (`TryMatchRoute*`, `GetRoute*UpperBound`) that are public and tested.

## Documentation map outcomes (Phase 4)
- Defined a canonical path-purpose map with explicit allowed/disallowed content boundaries for core docs locations.
- Defined topic-level canonical ownership for product overview, APIs, semantics, diagnostics, benchmarks, validation/process docs, roadmap/history, and ADRs.
- Confirmed governance rule: ADRs remain decision records only, not a general-purpose docs bucket.
- Confirmed governance rule: committed benchmark reports are historical evidence; current benchmark proof comes from CI-owned artifacts/summaries.
- Proposed docs-site curation guardrails to keep public routes focused on current docs and exclude archive/transitional/raw output areas.

## Consolidation/archive/delete outcomes (Phase 5)
- Completed required overlap checks across README/docs hub, roadmap/release history, reference/package docs, archive/current docs, benchmark docs, changelog/archive notes, and docs-site manifest/index alignment.
- Produced consolidation plan with canonical sources and implementation sequencing.
- Identified archive-move candidates as policy-controlled actions rather than immediate edits.
- Identified delete candidates and explicitly marked approval requirements before destructive actions.
- Added approval matrix to separate safe consolidation from actions requiring maintainer sign-off.

## User journey outcomes (Phase 6)
- Required path navigation works in sequence.
- New-user onboarding is partially diluted by mixed maintainer/historical navigation blocks in `docs/README.md`.
- Package install/selection guidance is present but not sufficiently explicit on the core package page.
- Stale direction text in README/package README can conflict with roadmap-based "current status" understanding.
- Reference coverage remains strong, with catch-all visibility in `pattern-segments.md` still a discoverability gap.

## Archive/delete candidates
- Candidate for policy decision: raw generated benchmark markdown files under `docs/benchmark-results/**/raw/*.md`.
- Candidate for consolidation (not immediate deletion): alpha-era chronology in `CHANGELOG.md` versus archive release docs.
- Candidate for consolidation policy: root/package README duplication strategy.

## User journey issues
- Current path is clear at top-level, but discoverability gaps exist between full accepted ADR set and rendered ADR subset.
- Some current reference pages link directly into archive release notes, which can blur active vs historical guidance.
- Key low-allocation/public API behaviors are discoverable in code/tests but underrepresented in docs (builder maintenance and routing try/upper-bound helpers).
- Newcomer-first flow is currently mixed with maintainer and historical sections in the docs hub.
- Core package install and package-selection guidance can be made more explicit for first-time users.

## Resolved decisions
- D-001 (`Q1`/`Q10`): Perform full ADR audit/classification, then render all accepted non-superseded ADRs in `docs.site.json` unless intentionally maintainer-only; make `docs/adr/README.md` the canonical complete ADR index.
- D-002 (`Q2`): Adopt summary-only benchmark retention target, but keep existing `docs/benchmark-results/**/raw/*.md` in the first implementation pass pending dedicated retention cleanup.
- D-003 (`Q3`/`Q12`): Keep `CHANGELOG.md` public and reframe alpha-train entries in place as historical/pre-beta context; do not move changelog sections to archive yet.
- D-004 (`Q4`/`Q15`): Avoid direct links from current reference/package docs to specific archive release files unless essential; prefer current docs and route historical context through `docs/archive/README.md` or explicit historical labels.
- D-005 (`Q5`/`Q11`): Trim root `README.md` to short-entry scope and keep package-depth content in `docs/packages/pattrn.md`; avoid near-duplicate long-form content.
- D-006 (`Q6`): Expand `docs/reference/api.md` with compact builder maintenance API coverage (`Contains*`, `Remove*`, `RemoveAll*`, `Clear`, range/convenience overload families).
- D-007 (`Q7`): Add low-allocation routing helper section in `docs/packages/pattrn-routing.md` for `TryMatchRoute*` and `GetRoute*UpperBound`.
- D-008 (`Q8`): Add catch-all coverage to `docs/reference/pattern-segments.md` segment-kind table and a short catch-all subsection.
- D-009 (`Q9`): Update `docs/reference/documentation-standards.md` now with path-ownership map and `docs.site.json` public-surface curation rules.
- D-010 (`Q13`): Make strict newcomer path the first visible section in `docs/README.md`; move and condense maintainer/historical/archive/benchmark sections.
- D-011 (`Q14`): Add direct core package install instructions to `docs/packages/pattrn.md` and add a compact package-selection decision table with one canonical owner and links.

## Phase 7 implementation outcomes
- `README.md` was trimmed to short-entry scope (overview, package summary, first-use example, docs-hub links) and stale direction wording was removed.
- `docs/packages/pattrn.md` became the canonical package-selection/install owner with direct install snippet and compact package-choice table.
- `docs/README.md` now starts with a strict newcomer path and moves maintainer/historical sections lower in condensed form.
- `docs/reference/api.md` now includes compact builder maintenance API coverage (`Contains*`, `Remove*`, `RemoveAll*`, `Clear`, convenience overload families).
- `docs/packages/pattrn-routing.md` now includes a dedicated low-allocation helper section for `TryMatchRoute*` and `GetRoute*UpperBound`.
- `docs/reference/pattern-segments.md` now includes catch-all in the segment-kind table and a dedicated catch-all subsection.
- `docs/reference/documentation-standards.md` now codifies documentation path-ownership and docs-site curation rules.
- `docs/adr/README.md` now serves as the complete canonical ADR index (statuses, topic labels, public relevance, task-based read order).
- `docs.site.json` now renders all accepted non-superseded ADRs currently present in `docs/adr/`.
- `CHANGELOG.md` now frames `3.0.0-alpha.*` entries as historical pre-beta chronology and points to ADR 0012 + roadmap for current version policy.

## Open risks
- Benchmark evidence risk remains during transition: raw benchmark markdown stays committed until dedicated retention cleanup validates summary/artifact sufficiency.
- ADR index/manifest synchronization can drift if future ADR additions or status changes are not updated in both `docs/adr/README.md` and `docs.site.json`.
