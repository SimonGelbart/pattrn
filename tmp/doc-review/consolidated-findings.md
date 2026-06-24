# Consolidated Findings

## Current state
- Phase 0 bootstrap completed and committed.
- Phase 1 inventory completed with repository-wide documentation-like artifact mapping.

## Confirmed findings
- Primary current entry path is `README.md` -> `docs/README.md` -> tutorials/how-to/reference/package docs/ADRs.
- `docs.site.json` publishes a curated subset of docs and explicitly excludes archive and benchmark-results folders.
- Package README docs are sourced from `docs/packages/*.md` and packed through `src/*/*.csproj` metadata.
- Benchmarks source-of-truth is workflow artifacts (`.github/workflows/benchmarks.yml`), while committed benchmark result docs are explicitly historical.

## Contradictions
- Potential contradiction risk: `CHANGELOG.md` alpha chronology may conflict with pre-beta version reset posture centered on `Directory.Build.props` (`0.1.0-alpha.1`).

## Redundancy
- Release/process history appears duplicated across `CHANGELOG.md` and `docs/archive/pre-beta/release/*.md`.
- Navigation overlap exists between `docs/README.md` and section-level `docs/*/README.md` pages.

## Docs-vs-code gaps
- Not all accepted ADR documents are listed in docs site route manifest (`docs.site.json` currently includes a subset).
- Legacy local workflow sections in `docs/reference/testing.md` may need reconciliation against CI-first validation docs.

## Archive/delete candidates
- Candidate for policy decision: raw generated benchmark markdown files under `docs/benchmark-results/**/raw/*.md`.
- Candidate for consolidation (not immediate deletion): alpha-era chronology in `CHANGELOG.md` vs archive release docs.

## User journey issues
- Current path is clear at top-level, but discoverability gaps likely exist between accepted ADR set and rendered ADR subset.
- Historical/transitional files remain numerous and can distract without stronger route-level guidance.

## Resolved decisions
- None added in Phase 1.

## Open risks
- Cleanup actions may accidentally remove historically required evidence without explicit maintainer retention decisions.
- Manifest coverage decisions (what to render vs source-link only) could create discoverability inconsistencies if not made explicitly.
