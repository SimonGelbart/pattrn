# Consolidated Findings

## Current state
- Phase 0 bootstrap completed and committed.
- Phase 1 inventory completed with repository-wide documentation-like artifact mapping.
- Phase 2 internal consistency audit completed with claim-level contradiction/redundancy/staleness analysis.

## Confirmed findings
- Primary current entry path is `README.md` -> `docs/README.md` -> tutorials/how-to/reference/package docs/ADRs.
- `docs.site.json` publishes a curated subset of docs and explicitly excludes archive and benchmark-results folders.
- Package README docs are sourced from `docs/packages/*.md` and packed through `src/*/*.csproj` metadata.
- Benchmarks source-of-truth is workflow artifacts (`.github/workflows/benchmarks.yml`), while committed benchmark result docs are explicitly historical.
- Core product-boundary and ranking claims are internally consistent across README, reference docs, and accepted ADRs (`0001`, `0003`, `0004`, `0013`).

## Contradictions
- `CHANGELOG.md` presents a `3.0.0-alpha.*` release train without strong historical framing, conflicting with current ADR/roadmap version-reset posture (`0.1.0-alpha.1` centralized pre-beta policy).
- Current benchmark authority docs are CI-first, but archive release checklist/history docs still include language that treats committed benchmark reports as release-proof artifacts.

## Redundancy
- Root `README.md` and `docs/packages/pattrn.md` are near-exact duplicates (mostly link-path differences), creating drift risk.
- Package-boundary and validation-authority statements are repeated across multiple current docs; mostly consistent but duplicated.
- Navigation responsibilities overlap between docs hub (`docs/README.md`) and section-level README pages.

## Docs-vs-code gaps
- Not all accepted ADR documents are listed in docs site route manifest (`docs.site.json` currently includes a subset).
- Legacy local workflow sections in `docs/reference/testing.md` remain transitional and should continue being framed subordinate to CI authority.

## Archive/delete candidates
- Candidate for policy decision: raw generated benchmark markdown files under `docs/benchmark-results/**/raw/*.md`.
- Candidate for consolidation (not immediate deletion): alpha-era chronology in `CHANGELOG.md` versus archive release docs.
- Candidate for consolidation policy: root/package README duplication strategy.

## User journey issues
- Current path is clear at top-level, but discoverability gaps exist between full accepted ADR set and rendered ADR subset.
- Some current reference pages link directly into archive release notes, which can blur active vs historical guidance.

## Resolved decisions
- None added in Phase 2.

## Open risks
- Cleanup actions may accidentally remove historically required evidence without explicit maintainer retention decisions.
- Manifest coverage decisions (rendered vs source-link ADRs) can create discoverability inconsistencies if not made explicitly.
- Versioning posture confusion may persist for readers if `CHANGELOG.md` remains unreframed against ADR 0012.
