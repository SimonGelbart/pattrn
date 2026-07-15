# Validation Reporting Skill

Use this skill when planning, running, or reporting validation for Pattrn changes.

## Authority

This skill is non-authoritative. The root `AGENTS.md`, accepted ADRs, `docs/reference/validation.md`, direct maintainer instructions, and CI configuration govern validation expectations.

## Required result vocabulary

Report each check as exactly one of:

- `Passed`
- `Failed`
- `Not run`
- `Not completed`
- `Skipped`

For `Skipped`, include a capability-based reason. For `Failed` and `Not completed`, include the observed reason. Local validation is not CI evidence.

## Procedure

1. Match validation scope to the change type.
2. Prefer locally available commands and parsers; do not install dependencies solely to validate.
3. Preserve command honesty: report exact command, working directory when useful, classification, and concise output summary.
4. Keep raw logs, generated reports, and temporary evidence out of commits.
5. If work is interrupted or handoff-sensitive, place temporary evidence under ignored `.agent-work/` and summarize it in the final report.

## Boundaries

- Passing local checks does not authorize push, publication, release, or pull-request creation.
- A checklist or issue template can request validation but cannot prove it ran.
