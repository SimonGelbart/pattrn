# Documentation Standards

Pattrn documentation should be durable, accurate, and easy to maintain.

## Diataxis organization

Use the Diataxis structure for new durable docs:

- `docs/tutorials/` for learning-oriented walkthroughs;
- `docs/how-to/` for task-oriented procedures;
- `docs/reference/` for stable technical reference;
- `docs/explanation/` for conceptual background;
- `docs/adr/` for Architecture Decision Records.

## Path ownership map

Use these canonical ownership boundaries.

| Path | Canonical purpose |
|---|---|
| `README.md` | Short product overview + first-use example + links to docs hub. |
| `docs/README.md` | Canonical docs navigation with newcomer path first. |
| `docs/tutorials/` | Guided learning flows. |
| `docs/how-to/` | Task-oriented procedures. |
| `docs/reference/` | Stable contracts, semantics, evidence policy, and durable maintainer reference. |
| `docs/explanation/` | Design rationale and conceptual framing. |
| `docs/packages/` | Package-scoped README content for NuGet package docs. |
| `docs/adr/` | Decision records only (status-bearing ADRs and ADR index). |
| `docs.site.json` | Public docs-surface curation manifest. |

## Source-of-truth rules

Prefer one canonical source and link to it instead of duplicating long-form content.

Avoid duplicating:

- package versions;
- detailed release history;
- local validation transcripts;
- generated benchmark output summaries;
- environment-specific paths;
- implementation details that are private by design.

## Current vs historical rules

- Current user-facing docs should prioritize current reference/roadmap/project-profile sources.
- Use Git history for discarded alpha-era migration notes, local benchmark reports, release-planning notes, and design drafts.
- Keep `CHANGELOG.md` as public release history while clearly framing alpha-train entries as historical pre-beta context.
- Current benchmark proof comes from CI workflow artifacts and summaries, not committed local benchmark output.

## Durable maintainer reference

Maintainer-facing docs may be rendered publicly when they describe durable repository operation rather than local scratch work. Current durable maintainer references include project profile, validation, repository layout, Git workflow, documentation standards, documentation-site operation, benchmark evidence policy, AI documentation-management workflow, and offline-build guidance.

Keep local transcripts, raw benchmark output, generated logs, package caches, and one-off review notes out of the rendered docs surface.

## docs.site.json curation rules

`docs.site.json` is a curation layer for current public docs, not a list of every markdown file.

- Include current public pages in active user/maintainer paths.
- Do not route raw logs, generated benchmark dumps, or transitional scratch/review notes.
- Keep ADR routing aligned with `docs/adr/README.md`: render all accepted, non-superseded ADRs unless an ADR is intentionally maintainer-only.

## Staleness rules

When documentation disagrees with code, tests, accepted ADRs, or the current roadmap, either update it or mark it historical.

Do not preserve alpha-era wording merely because it was previously published. Pattrn is pre-beta, and documentation should support the intended long-term product rather than old alpha mechanics.
