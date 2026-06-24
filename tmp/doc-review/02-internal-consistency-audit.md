# Phase 2 — Internal Documentation Consistency Audit

## Executive summary
- Current docs are mostly aligned on core direction: pre-beta status, package boundaries, explicit pattern-segment model, deterministic matching, and routing-preview posture.
- The largest inconsistency is version/release posture: current docs say pre-beta versions were reset and centralized, while `CHANGELOG.md` remains an unframed `3.0.0-alpha.*` release train.
- Benchmark authority is mostly clear (CI workflow artifacts are authoritative), but some historical release files still prescribe committed benchmark reports, creating mixed source-of-truth signals.
- Navigation and canonical-source ownership are partially clear but duplicated in several places, especially `README.md` vs `docs/packages/pattrn.md`, plus partial ADR route coverage in `docs.site.json`.
- No blocker was found for continuing to Phase 3, but several maintainer decisions are needed before any cleanup/destructive doc changes.

## Claim matrix

| Claim ID | Topic | Claim text | Source file | Heading | Audience | Status | Canonical source | Conflicting files | Recommended action | Evidence |
|---|---|---|---|---|---|---|---|---|---|---|
| C01 | project status | Pattrn is pre-beta; breaking changes are acceptable when improving long-term product quality. | `README.md` | intro | user | consistent | `docs/reference/project-profile.md` | none | keep; continue using project profile + roadmap as source pair | `README.md:7`, `docs/reference/project-profile.md:47`, `docs/roadmap.md:9-12` |
| C02 | versioning & release posture | Roadmap milestones are not package versions; package line reset and centralized pre-beta. | `docs/roadmap.md` | Versioning posture | maintainer/user | contradiction between current docs | `docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md` | `CHANGELOG.md` | mark `CHANGELOG.md` as historical alpha chronology or restructure it to avoid presenting alpha train as current release posture | `docs/roadmap.md:22-25`, `docs/adr/0012...md:21-25`, `CHANGELOG.md:3-413` |
| C03 | package boundaries | Core remains segmented/domain-neutral; strings/DI/routing are companion layers. | `docs/explanation/package-boundaries.md` | Core package | user | consistent | `docs/adr/0001-core-remains-segmented-and-domain-neutral.md` + `docs/adr/0004-companion-packages.md` | none | keep; cross-link ADRs from boundary docs where not already explicit | `docs/explanation/package-boundaries.md:3-41`, `docs/adr/0001...md:17-22`, `docs/adr/0004...md:17-22` |
| C04 | package stability posture | Routing is preview; core/strings/DI are stable candidates pre-beta. | `docs/reference/project-profile.md` | Package family | user | overlapping but useful | `docs/reference/project-profile.md` | minor nuance split in `docs/packages/pattrn-dependency-injection.md` | keep both levels; add a short note in high-level tables that DI has preview extras | `docs/reference/project-profile.md:28-32`, `docs/packages/pattrn-dependency-injection.md:5-6` |
| C05 | pattern semantics | Explicit `PatternSegment<TSegment>` is primary; tokenized wildcard API is convenience only. | `docs/reference/pattern-segments.md` | page intro | user | consistent | `docs/adr/0003-explicit-pattern-segments-are-primary.md` | none | keep | `docs/reference/pattern-segments.md:3-6,32-44`, `docs/adr/0003...md:17-20` |
| C06 | matching semantics | Deterministic ordering uses literal > parameter > wildcard > catch-all; registration order ties. | `docs/reference/ranking-specificity.md` | Built-in ordering | user | consistent | `docs/adr/0013-use-fixed-ranking-with-consumer-side-sorting.md` | none | keep | `docs/reference/ranking-specificity.md:9-16,46-49`, `docs/reference/matching-semantics.md:190-200`, `docs/adr/0013...md:23-31` |
| C07 | validation authority | CI is authoritative; local commands are optional preflight only. | `docs/reference/validation.md` | Source of truth | maintainer | consistent | `docs/reference/validation.md` | none | keep | `docs/reference/validation.md:7-10`, `docs/reference/testing.md:3`, `docs/reference/building-offline.md:3-7` |
| C08 | benchmark authority | Benchmark workflow artifacts are current truth; committed benchmark docs are historical only. | `docs/reference/benchmarks.md` | Source of truth | maintainer/user | contradiction between current docs | `docs/reference/benchmarks.md` | archive pre-beta release checklist/history docs | keep benchmark reference as canonical and add stronger historical disclaimers in old release docs that still require committed reports | `docs/reference/benchmarks.md:9-18,37,150`, `docs/archive/pre-beta/release/beta-checklist.md:21,72` |
| C09 | archive status | Archive is historical context, not current user path. | `docs/archive/README.md` | intro | maintainer/user | consistent | `docs/archive/README.md` + `docs/README.md` historical section | none | keep | `docs/archive/README.md:3-6`, `docs/README.md:67-70,104-107` |
| C10 | navigation & rendering | Docs site renders a curated subset; archive/benchmark-results excluded from routes. | `docs.site.json` | docs/exclude | maintainer | overlapping but useful | `docs.site.json` + `docs/reference/documentation-site.md` | none | keep | `docs.site.json:201-204`, `docs/reference/documentation-site.md:18-30` |
| C11 | ADR discoverability | ADR index describes broad accepted ADR set, but docs-site routes include only five accepted ADR pages. | `docs/adr/README.md` | Read order | user/maintainer | maintainer decision needed | `docs/adr/README.md` | `docs.site.json` | decide whether all accepted ADRs should be rendered routes or intentionally index/source-link only | `docs/adr/README.md:9-15`, `docs.site.json:171-199` |
| C12 | content duplication | Root README and core package README are near-identical (mostly link-path differences). | `README.md` / `docs/packages/pattrn.md` | whole doc | user/package user | exact duplicate | undecided (likely package README as package-facing source and root README as repo entry) | none (intentional duplication but high drift risk) | choose canonical owner + sync method (or acceptable drift policy) | `diff README.md docs/packages/pattrn.md` shows only relative-link differences |
| C13 | roadmap progress wording | README/product-direction text still says “next product work is project foundation and ADR cleanup”, which appears already completed by current docs structure and ADR set. | `README.md` / `docs/packages/pattrn.md` | Product direction | user | stale historical wording | `docs/roadmap.md` | README + package README product-direction section | reword README/package direction text to point to current roadmap milestone status rather than old sequence sentence | `README.md:189`, `docs/roadmap.md:270-289` |
| C14 | current docs linking to archive | Some current docs directly point to archive release notes as “current” support context, which can blur active source-of-truth boundaries. | `docs/reference/api.md` | Preview status | user | current doc linking too strongly to archive | `docs/reference/project-profile.md` + `docs/roadmap.md` + ADRs | `docs/reference/api.md` and package docs with archive links | keep archive links but demote/label as historical in-context | `docs/reference/api.md:235`, `docs/packages/pattrn-dependency-injection.md:117` |

## Contradiction list
1. **Versioning posture contradiction**: ADR/roadmap state pre-beta version reset and centralized current line, while `CHANGELOG.md` is a live-structured `3.0.0-alpha.*` train without a strong historical framing header.
2. **Benchmark authority contradiction**: current benchmark/reference docs say CI artifacts are authoritative, while archive release checklist material still frames committed benchmark reports as release proof.

## Redundancy list
1. **Exact duplicate**: `README.md` and `docs/packages/pattrn.md` are effectively the same document except relative links.
2. **Overlapping but useful**: package boundary statements are repeated across roadmap, explanation, architecture reference, and ADRs; useful for audience targeting but high drift risk.
3. **Overlapping but useful**: validation authority appears in validation/testing/building-offline/repository-layout; consistent but repeated.

## Stale historical wording list
1. `README.md` / `docs/packages/pattrn.md` product-direction sequence still references project-foundation/ADR-cleanup as future next work despite those items appearing complete.
2. `CHANGELOG.md` retains alpha-train presentation that can read as current release posture rather than historical chronology under ADR 0012.
3. Archive release docs still contain “before beta” action framing that is no longer the main current planning source.

## Canonical-source recommendations
1. **Versioning and release posture**: canonical = `docs/adr/0012...` + `docs/roadmap.md`; treat `CHANGELOG.md` as historical chronology unless explicitly modernized.
2. **Package boundaries and semantics**: canonical = ADRs (`0001`, `0003`, `0004`, `0013`) + reference docs (`api`, `matching-semantics`, `ranking-specificity`).
3. **Validation and benchmarks**: canonical = `docs/reference/validation.md` and `docs/reference/benchmarks.md`; archive benchmark docs should remain explicitly historical evidence.
4. **Navigation/routing of docs pages**: canonical = `docs.site.json` + `docs/reference/documentation-site.md`; align ADR render coverage decision explicitly.
5. **Root vs package README ownership**: pick one canonical content owner and explicitly define how the other is synchronized.

## Blocking questions
- None for proceeding to Phase 3 (docs-vs-code comparison).

## Nonblocking questions
1. Should `CHANGELOG.md` be reframed as historical alpha chronology (or reduced) to align with ADR 0012 pre-beta versioning reset?
2. Should all accepted ADRs be added to `docs.site.json` routes for consistent discoverability?
3. Should current docs keep linking to archive release documents from user-facing reference pages, or should those links move to docs index/archive sections only?
4. Should root `README.md` and `docs/packages/pattrn.md` remain duplicated intentionally, or should one become canonical with generation/sync policy?

## Recommended next phase
Proceed to **Phase 3 (docs-to-code alignment audit)**, using this phase’s canonical-source recommendations and unresolved questions as risk flags when judging whether docs statements match code/tests/samples/package metadata.
