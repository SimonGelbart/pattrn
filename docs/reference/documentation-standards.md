# Documentation Standards

Pattrn documentation should be durable, accurate, and easy to maintain.

## Organization

Use the Diataxis structure for new durable docs:

- `docs/tutorials/` for learning-oriented walkthroughs;
- `docs/how-to/` for task-oriented procedures;
- `docs/reference/` for stable technical reference;
- `docs/explanation/` for conceptual background;
- `docs/adr/` for Architecture Decision Records.

Existing alpha-era folders may stay while they are reconciled, archived, or migrated.

## Source of truth

Prefer one canonical source and link to it.

Avoid duplicating:

- package versions;
- detailed release history;
- local validation transcripts;
- generated benchmark output summaries;
- environment-specific paths;
- implementation details that are private by design.

## Staleness rules

When documentation disagrees with code, tests, ADRs, or the current roadmap, either update it or mark it historical.

Do not preserve alpha-era wording merely because it was previously published. Pattrn is pre-beta, and documentation should support the intended long-term product rather than old alpha mechanics.
