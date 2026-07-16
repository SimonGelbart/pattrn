# AGENTS.md

This repository accepts assisted and automated changes when they are scoped, reviewable, and validated honestly.

## Working agreement

- Inspect the worktree before editing and preserve unrelated changes.
- Read only the accepted ADRs, references, product docs, and nested instructions relevant to the task.
- Use current code, tests, and documentation as evidence for the change. If a request conflicts with an accepted ADR, stop and report the conflict.
- Keep changes focused. Do not commit secrets, generated logs, local transcripts, SDKs, caches, or temporary artifacts.
- Prefer focused commits with Conventional Commit messages when commits are requested.

## Authority

- Issue readiness, assignment, local implementation, validation, and agent configuration do not authorize remote actions.
- Do not push, publish, create tags or releases, open pull requests, or otherwise change remote state unless a maintainer explicitly asks.
- Local Git conventions are documented in `docs/reference/git-workflow.md`.

## Validation and reporting

- Select the smallest relevant validation set using `docs/reference/validation.md`; local checks are not CI evidence.
- Never claim a check passed unless it ran and passed, and never hide failures or incomplete work.
- Report the changed scope, exact checks and result classifications, and any commit, branch, or remote action.
