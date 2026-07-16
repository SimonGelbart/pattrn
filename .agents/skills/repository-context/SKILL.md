---
name: repository-context
description: Discover repository instructions, constraints, and relevant context before planning or changing files.
---

# Repository Context Skill

Use this skill for read-only repository discovery before planning or changing Pattrn.

## Authority

This skill is non-authoritative. Follow the precedence in root `AGENTS.md` and any applicable nested instructions.

## Inputs

- The task request.
- Current branch, revision, and `git status --short`.
- Applicable root and nested `AGENTS.md` files.

## Procedure

1. Inspect the requested task context and current repository state.
2. Identify unrelated or conflicting changes that must be preserved.
3. Read root and applicable nested instructions for candidate files.
4. Read only the references, coding profiles, ADRs, and product docs that govern the requested area.
5. Summarize the applicable authority, affected areas, likely validation, and blockers.

## Boundaries

- Do not edit files while performing read-only discovery.
- Do not access remote resources unless the task explicitly authorizes network use.
- Do not treat this skill as permission to branch, commit, push, publish, or open a pull request.
