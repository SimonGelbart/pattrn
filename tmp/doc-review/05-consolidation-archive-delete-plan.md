# Phase 5 — Consolidation, Archive, and Delete Plan

## Executive summary

This phase defines a deterministic consolidation/archive/delete plan without changing product docs.

Required overlap checks were completed:
- `README.md` vs `docs/README.md`: complementary roles, but root README remains too long for "short overview" intent.
- `docs/roadmap.md` vs release/history/archive docs: roadmap is current; archive release notes still carry alpha-train detail and should remain historical-only.
- `docs/reference/project-profile.md` vs README/package docs: high-level package/status claims align; duplication risk remains in repeated package summaries.
- `docs/reference/api.md` vs package docs: core API is canonical, but package docs duplicate fragments and include additional surface details.
- `docs/reference/pattern-segments.md` vs tutorials/examples/package docs: concept is canonical in reference; package/tutorial examples duplicate portions and catch-all table gap remains.
- `docs/reference/matching-semantics.md` vs ranking/specificity/duplicate docs: mostly aligned; boundaries are clear but fragmented across multiple pages.
- `docs/reference/diagnostics.md` vs examples/tutorials: examples are consistent; diagnostics discoverability is split across README/package pages.
- `docs/reference/validation.md` vs workflow/local build notes: CI-first canonical rule exists, but transitional local/offline notes remain in reference side pages.
- `docs/reference/documentation-standards.md` vs `AGENTS.md`: both align on durability/canonical-source goals, but standards doc does not yet codify path ownership/map rules from Phase 4.
- `docs/archive/pre-beta/**` vs current docs: archive is correctly historical but contains repeated release-policy material that can confuse current posture.
- `docs/benchmark-results/**` vs `docs/reference/benchmarks.md`: benchmark reference correctly marks committed results historical; archive-like benchmark dumps remain retained.
- `CHANGELOG.md` vs archived release planning notes: substantial overlap and alpha-train emphasis; needs framing consolidation strategy.
- `docs.site.json` vs `docs/README.md`: mostly aligned current public path, but ADR coverage policy remains partial/unclear.

## Consolidation table

| Topic | Current files | Canonical file | Action | Reason | Maintainer decision needed? |
|---|---|---|---|---|---|
| Product entry path | `README.md`, `docs/README.md` | `README.md` (short entry) + `docs/README.md` (index) | Consolidate README to short overview and keep deep links in docs index | Reduce duplication/length drift while preserving first-use flow | No |
| Package overview duplication | `README.md`, `docs/packages/pattrn.md`, `docs/reference/project-profile.md` | `docs/reference/project-profile.md` for package posture + `docs/packages/pattrn.md` for package README | Consolidate by defining explicit ownership and trimming repeated narrative | Current near-duplicate long-form content increases drift risk | Yes |
| Release/version posture | `docs/roadmap.md`, `CHANGELOG.md`, `docs/archive/pre-beta/release/*.md` | `docs/roadmap.md` + ADR 0012 for current posture | Consolidate by reframing changelog and keeping archive release files historical | Remove conflict between current reset posture and alpha train presentation | Yes |
| Core API reference | `docs/reference/api.md`, `docs/packages/pattrn.md`, `README.md` | `docs/reference/api.md` | Consolidate API detail into reference; keep package/README summary links only | Avoid fragmented API contract statements | No |
| Pattern segment semantics | `docs/reference/pattern-segments.md`, `docs/tutorials/*`, `docs/packages/pattrn.md` | `docs/reference/pattern-segments.md` | Consolidate semantics in reference; keep examples in tutorials/package docs | Prevent concept drift and close catch-all discoverability gap | No |
| Matching/ranking/duplicates split | `docs/reference/matching-semantics.md`, `docs/reference/ranking-specificity.md`, `docs/reference/duplicate-behavior.md` | `docs/reference/matching-semantics.md` (links to specialized pages) | Consolidate navigation and cross-links, not content merge | Keep specialization while reducing fragmentation | No |
| Diagnostics guidance | `docs/reference/diagnostics.md`, `README.md`, package docs | `docs/reference/diagnostics.md` | Consolidate policy/behavior in reference; keep high-level mentions elsewhere | Ensure one diagnostics contract source | No |
| Validation workflow | `docs/reference/validation.md`, `docs/reference/testing.md`, `docs/reference/building-offline.md` | `docs/reference/validation.md` | Consolidate CI-first policy references and demote local/offline notes as secondary | Align with AGENTS and validation policy | No |
| Documentation governance | `docs/reference/documentation-standards.md`, `AGENTS.md`, `docs/README.md`, `docs.site.json` | `docs/reference/documentation-standards.md` | Consolidate Phase 4 map rules into standards | Reduce ambiguity on docs ownership and public surface | No |
| Benchmarks source of truth | `docs/reference/benchmarks.md`, `docs/benchmark-results/**` | `docs/reference/benchmarks.md` | Consolidate by preserving history but tightening historical framing | Prevent stale benchmark evidence being read as current proof | Yes |
| ADR discoverability | `docs/adr/README.md`, `docs.site.json` | `docs/adr/README.md` + explicit docs-site policy | Consolidate ADR rendering policy | Partial rendering creates discoverability inconsistency | Yes |

## Archive move table

| Path | Proposed action | Destination if moved | Replacement link if consolidated | Deletion safe? | Reason |
|---|---|---|---|---|---|
| `docs/release/`-style historical content now under current docs (none newly detected) | No move now | n/a | n/a | n/a | Most old release content is already under `docs/archive/pre-beta/release/`. |
| `CHANGELOG.md` older alpha-train sections (if split) | Move older alpha-train narrative to archive note file (future) | `docs/archive/pre-beta/release/` | Keep changelog pointer to archive + roadmap/ADR 0012 | No (until approved) | Historically useful but currently dominant in main changelog. |
| Direct archive links in current package/reference docs (selected) | Move link emphasis to docs index/archive section (future) | n/a | Use `docs/archive/README.md` as replacement link | No (link policy change) | Keeps current docs path focused while preserving historical access. |

## Delete candidate table

| Path | Proposed action | Destination if moved | Replacement link if consolidated | Deletion safe? | Reason |
|---|---|---|---|---|---|
| `docs/benchmark-results/**/raw/*.md` | Candidate delete after retention decision | n/a | Keep run-level `README.md` + benchmark workflow artifacts references | No | Likely generated/raw dumps; historical value uncertain and needs maintainer decision. |
| Redundant repeated narrative blocks in `README.md` and `docs/packages/pattrn.md` | Delete duplicate sections after canonical ownership is approved | n/a | Link to canonical doc sections | No | Reduces drift while preserving user paths. |
| Any duplicate archive release notes with identical intent | Candidate prune after line-by-line archival review | n/a | Keep archive index entries | No | Must avoid deleting unique historical decision context. |

## File action table

| Path | Proposed action | Destination if moved | Replacement link if consolidated | Deletion safe? | Reason |
|---|---|---|---|---|---|
| `README.md` | Consolidate/trim scope | n/a | `docs/README.md`, `docs/reference/*`, `docs/packages/*` | No | Should remain short product overview and first-use entry point. |
| `docs/README.md` | Consolidate as single current docs navigation hub | n/a | n/a | No | Current user path should stay centralized and explicit. |
| `docs/reference/documentation-standards.md` | Expand with path-ownership/public-surface rules | n/a | n/a | n/a | Needed to codify governance decisions from Phases 4-5. |
| `docs/reference/testing.md` | Consolidate language to CI-first + clearly secondary local/offline notes | n/a | `docs/reference/validation.md` | No | Reduce policy fragmentation. |
| `docs/reference/building-offline.md` | Keep as secondary maintainer procedure with strong CI-first framing | n/a | `docs/reference/validation.md` | No | Useful for edge cases; should not compete with validation authority. |
| `docs/benchmark-results/**` | Preserve as historical evidence; no immediate structural move | n/a | `docs/reference/benchmarks.md` | Partially (raw markdown only, pending decision) | Historical retention is useful; raw dump scope unresolved. |
| `CHANGELOG.md` | Reframe current vs historical release history | Potential archival of older narrative chunks | `docs/roadmap.md`, ADR 0012, archive index | No | Current alpha-train prominence conflicts with current versioning posture. |
| `docs.site.json` | Consolidate explicit public-surface inclusion/exclusion policy | n/a | n/a | n/a | Keep current docs surface deterministic; resolve ADR coverage policy. |

## Approval table

| Action | Requires approval? | Why |
|---|---|---|
| Delete `docs/benchmark-results/**/raw/*.md` | Yes | Historical evidence retention policy is unresolved (Q2). |
| Restructure/reduce `CHANGELOG.md` historical alpha sections | Yes | Public release-history interpretation may change for users. |
| Remove/trim duplicated content in `README.md` and `docs/packages/pattrn.md` | Yes | Affects primary user entry and NuGet package README experience. |
| Expand docs.site ADR routing to all accepted ADRs (or keep curated subset) | Yes | Changes public docs discoverability and site surface. |
| Retarget archive links from current reference/package pages | Yes | Changes navigation expectations and historical discoverability. |

## Files requiring approval before deletion

- `docs/benchmark-results/**/raw/*.md`
- Any major content block removed from `README.md` or `docs/packages/pattrn.md`
- Any archived release note planned for deletion (including `docs/archive/pre-beta/release/*.md`)
- Any historical content moved out of `CHANGELOG.md`

## Proposed implementation commit grouping

1. **Governance alignment commit**
   - Update `docs/reference/documentation-standards.md` with approved ownership/public-surface rules.
2. **Navigation and consolidation commit**
   - Apply approved README/docs index/package docs deduplication and cross-link updates.
3. **History framing commit**
   - Apply approved changelog and archive-link framing updates.
4. **Manifest and historical evidence commit**
   - Apply approved `docs.site.json` ADR policy and any approved benchmark raw-file retention/deletion changes.

## Blocking questions

- None for planning artifacts.

## Nonblocking questions

1. Should `README.md` be reduced to a short entry-point summary with package-specific depth removed in favor of `docs/packages/pattrn.md`?
2. Should `CHANGELOG.md` keep full alpha chronology in place with stronger historical framing, or move older alpha details to archive files?
3. Should `docs.site.json` render all accepted ADR pages for discoverability consistency?
4. Should raw benchmark markdown in `docs/benchmark-results/**/raw/*.md` be retained indefinitely or reduced to summary-only historical evidence?
5. Should direct links from current package/reference docs to specific archive release files be replaced by links to `docs/archive/README.md`?

## Risks

- Over-consolidation could remove context that still helps migration/history readers.
- Deleting benchmark raw files without decision could remove evidence needed for historical audits.
- Changelog reframing without clear policy could create external ambiguity about release lineage.
- Inconsistent ADR rendering policy may continue to fragment docs-site discoverability.

## Recommended next phase

Phase 6 should execute **approved, non-destructive consolidation edits first** (governance + cross-links + framing), then perform any approved archival/deletion actions in separate commits with explicit maintainer sign-off.
