# Repository Context Skill

Use this skill for read-only repository discovery before planning or changing Pattrn.

## Authority

This skill is non-authoritative. Follow, in order, direct maintainer instructions, accepted ADRs, root `AGENTS.md`, nested `AGENTS.md`, and stable reference documentation.

## Inputs

- The task request.
- Current branch, revision, and `git status --short`.
- Applicable root and nested `AGENTS.md` files.

## Procedure

1. Confirm any requested base revision before editing.
2. Inspect `git status --short` and identify unrelated or conflicting changes.
3. Read root `AGENTS.md` and nested instructions for candidate files.
4. Read only the reference documentation, coding profiles, ADRs, and product docs that govern the requested area.
5. Summarize the discovered authority, affected areas, likely validation, and blockers.

## Boundaries

- Do not edit files while performing read-only discovery.
- Do not access remote resources unless the task explicitly authorizes network use.
- Do not treat this skill as permission to branch, commit, push, publish, or open a pull request.
