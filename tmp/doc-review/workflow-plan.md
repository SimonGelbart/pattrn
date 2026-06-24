# Documentation Review Workflow Plan

## Objective
Establish a durable, deterministic documentation review workflow that keeps documentation clear, comprehensive, non-duplicative, and aligned with code, tests, samples, package metadata, accepted ADRs, and roadmap direction.

## Phase order
1. Phase 0 — Bootstrap workflow memory and tracking artifacts.
2. Phase 1+ — Execute scoped documentation review phases in approved order, updating findings and decisions each phase.

## Cross-phase operating rules
- Every phase must read existing files under `tmp/doc-review/` before starting work.
- Every phase must update `tmp/doc-review/consolidated-findings.md`.
- Questions must be split into blocking and nonblocking entries in `tmp/doc-review/open-questions.md`.
- Maintainer decisions must be recorded in `tmp/doc-review/maintainer-decisions.md` before implementation or destructive changes that depend on those decisions.

## Branch and commit rules
- Work on the current branch unless maintainer instruction says otherwise.
- If current branch is `main` or another protected/default branch, stop and ask before proceeding.
- Commit each completed phase as a focused reviewable unit.
- Do not push or open a pull request unless explicitly asked by the maintainer.

## Validation honesty rule
- Do not claim validation passed unless it actually ran and passed.
- Record all validation outcomes (passed, failed, not run, not completed) and reasons in `tmp/doc-review/validation-log.md`.
