# Consolidated Findings

## Current state
- Phase 0 bootstrap completed and committed.
- Phase 1 inventory completed with repository-wide documentation-like artifact mapping.
- Phase 2 internal consistency audit completed with claim-level contradiction/redundancy/staleness analysis.
- Phase 3 docs-vs-code gap analysis completed with claim-level verification against source/tests/samples.
- Phase 4 documentation map completed with explicit path ownership, topic ownership, docs-site curation rules, and standards update proposals.

## Confirmed findings
- Primary current entry path is `README.md` -> `docs/README.md` -> tutorials/how-to/reference/package docs/ADRs.
- `docs.site.json` publishes a curated subset of docs and explicitly excludes archive and benchmark-results folders.
- Package README docs are sourced from `docs/packages/*.md` and packed through `src/*/*.csproj` metadata.
- Benchmarks source-of-truth is workflow artifacts (`.github/workflows/benchmarks.yml`), while committed benchmark result docs are explicitly historical.
- Core product-boundary and ranking claims are internally consistent across README, reference docs, accepted ADRs, source, and tests.
- Core docs claims for `TryMatch`/`TryMatchDetailed` failure semantics, catch-all behavior, specificity ordering, diagnostics separation, and build-validation controls are aligned with implementation and tests.
- Phase 4 establishes explicit ownership boundaries for README, docs index, Diataxis folders, package docs, archive, benchmark-results, ADRs, and `docs.site.json` public routing.

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

## Archive/delete candidates
- Candidate for policy decision: raw generated benchmark markdown files under `docs/benchmark-results/**/raw/*.md`.
- Candidate for consolidation (not immediate deletion): alpha-era chronology in `CHANGELOG.md` versus archive release docs.
- Candidate for consolidation policy: root/package README duplication strategy.

## User journey issues
- Current path is clear at top-level, but discoverability gaps exist between full accepted ADR set and rendered ADR subset.
- Some current reference pages link directly into archive release notes, which can blur active vs historical guidance.
- Key low-allocation/public API behaviors are discoverable in code/tests but underrepresented in docs (builder maintenance and routing try/upper-bound helpers).

## Resolved decisions
- None added in Phase 4.

## Open risks
- Cleanup actions may accidentally remove historically required evidence without explicit maintainer retention decisions.
- Manifest coverage decisions (rendered vs source-link ADRs) can create discoverability inconsistencies if not made explicitly.
- Versioning posture confusion may persist for readers if `CHANGELOG.md` remains unreframed against ADR 0012.
- Public API discoverability risk remains if docs continue to document only partial builder/routing helper surfaces.
- Without codifying map ownership into `docs/reference/documentation-standards.md`, path responsibilities may drift again.
