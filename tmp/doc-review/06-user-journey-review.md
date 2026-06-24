# Phase 6 — User Journey Review

## Executive summary
- The required path is navigable end-to-end: `README.md` -> `docs/README.md` -> tutorial -> package docs -> how-to -> reference -> explanation -> roadmap.
- Core onboarding answers are mostly present, but key answers are scattered and sometimes mixed with maintainer/historical material.
- Main journey risks are (1) stale roadmap wording in onboarding pages, (2) incomplete catch-all visibility in `docs/reference/pattern-segments.md`, and (3) weak package-install/package-selection guidance for first-time users.
- No blocking issue prevents continuing to the next non-destructive documentation-improvement phase.

## User path table

| Step | File | Works? | Problem | Proposed fix |
|---|---|---|---|---|
| 1 | `README.md` | Yes | Entry point works, but product-direction text is stale (`"next product work is ..."`) and page is longer than a short onboarding entry. | Refresh direction wording to point to current roadmap status and keep README as concise entry + links. |
| 2 | `docs/README.md` | Yes | Good central hub, but current user path is mixed with maintainer and long historical link blocks on the same page. | Split page into clearer sections: "Start here (new users)" first, then maintainer and historical sections later/condensed. |
| 3 | `docs/tutorials/first-index.md` | Yes | Good first tutorial, but does not explicitly route readers to "when not to use" boundaries after first success path. | Add explicit next-step link to `docs/reference/limitations.md` in "Next steps". |
| 4 | `docs/packages/pattrn.md` (and package docs) | Partial | Package differences are described, but core package page lacks a direct install snippet and package-selection decision aid is distributed across pages. | Add an install section for `Pattrn`; add a compact "which package should I install?" decision table in docs hub or package index page. |
| 5 | `docs/how-to/select-best-match.md` | Yes | Strong ranking guidance; not a navigation blocker. | Keep as-is; optionally add short links back to duplicate/diagnostics references for related troubleshooting tasks. |
| 6 | `docs/reference/{api,pattern-segments,matching-semantics,ranking-specificity,duplicate-behavior,diagnostics,limitations}.md` | Partial | Coverage is strong overall, but `pattern-segments.md` segment-kind table omits catch-all despite public support. | Add explicit catch-all row/section in `pattern-segments.md`. |
| 7 | `docs/explanation/package-boundaries.md` | Yes | Clear package boundary rationale and cross-links. | Keep as-is. |
| 8 | `docs/roadmap.md` | Yes | Current status/direction is present, but onboarding pages still contain old sequencing text that conflicts with roadmap progression. | Align README/package README direction text with roadmap milestones. |

## Missing-answer table

| Page | Missing answer | Severity | Proposed fix |
|---|---|---|---|
| `docs/packages/pattrn.md` | Which package(s) should I install first and how to install core package directly. | confusing | Add explicit `PackageReference` install snippet and a one-line package-choice pointer to companion package docs. |
| `docs/reference/pattern-segments.md` | How catch-all fits alongside literals/wildcards/parameters in segment-kind overview. | confusing | Add catch-all in segment-kind table and brief capture semantics note. |
| `README.md` and `docs/packages/pattrn.md` | What is current direction now (vs old "next work" sequence). | confusing | Replace stale product-direction paragraph with roadmap-aligned status language. |
| `docs/README.md` | How to validate documentation-only changes from the primary docs navigation path. | minor | Add direct link to `docs/reference/validation.md` in the "Current product docs" section. |
| `docs/tutorials/first-index.md` | When not to use core-only approach / when to switch to companion packages and limitations. | minor | Add explicit link to `docs/reference/limitations.md` in next steps. |

## Link/navigation fixes

| Source page | Current link/problem | Recommended link/fix | Reason |
|---|---|---|---|
| `docs/README.md` | New-user path is intermixed with maintainer and historical sections. | Reorder so first visible block is a strict newcomer flow; move maintainer/historical blocks below with shorter summaries. | Reduces cognitive load and avoids routing newcomers through archive-first reading. |
| `docs/README.md` | No direct validation link in primary "Current product docs" block. | Add `reference/validation.md` link in current docs list. | Answers documentation-change validation question in the standard path. |
| `README.md` | Product-direction paragraph is stale relative to roadmap progression. | Replace with "see roadmap for current milestone status" wording and remove old sequence text. | Prevents onboarding confusion about current vs historical phases. |
| `docs/reference/api.md` | Preview-status section links directly to archive release decisions. | Prefer current `project-profile.md`/`roadmap.md` as primary links; keep archive link clearly labeled historical if retained. | Keeps current status guidance anchored in current docs while preserving historical access. |
| `docs/tutorials/first-index.md` | Next steps omit limitations/non-goals page. | Add `../reference/limitations.md` to next steps. | Ensures early answer for "when not to use Pattrn". |

## Content too advanced for the current page
- `README.md`: includes deep package/routing/performance details that are useful but long for first-entry onboarding.
- `docs/README.md`: includes dense maintainer + historical release/benchmark link blocks that can distract first-time users.
- `docs/packages/pattrn.md`: combines onboarding and broad product-direction language; better to keep package page package-focused and defer roadmap detail.

## Archive distractions
- `docs/README.md` exposes many direct links into `docs/archive/pre-beta/release/*` in the same hub used by new users.
- `docs/reference/api.md` preview-status section points to `../archive/pre-beta/release/release-decisions.md` from a current reference page.
- Historical benchmark links in `docs/README.md` are clearly labeled, but still increase navigation noise for first-time users.

## Blocking questions
- None.

## Nonblocking questions
1. Should `docs/README.md` be split into a strict newcomer path section plus collapsed/secondary maintainer-historical sections?
2. Should `docs/packages/pattrn.md` gain a direct install section and compact package-selection decision table?
3. Should current reference pages stop linking directly to specific archive release files and instead route through `docs/archive/README.md`?

## Recommended next phase
- Phase 7 should apply non-destructive onboarding/navigation improvements: docs hub reordering, install/package-choice clarity, stale direction text cleanup, and catch-all visibility update in pattern-segment reference.
