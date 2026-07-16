# AGENTS.md

This repository accepts assisted and automated changes when they are reproducible, reviewable, documented, and validated honestly.

## Authority and precedence

Direct maintainer instructions for the current task take precedence, followed by accepted ADRs, this file, stable references under `docs/reference/`, product documentation, and existing code and test conventions. Nested `AGENTS.md` files may add subtree-specific guidance but must not weaken higher-precedence sources.

If a request conflicts with an accepted ADR or repository evidence, stop and report the conflict rather than silently overriding it.

## Proportionate discovery

Before changing files, inspect repository state and read the instructions and canonical sources that govern the affected area. Use the repository-context skill for detailed discovery when it is useful; do not impose a fixed reading sequence or unrelated historical investigation on routine work.

Canonical task-specific sources include:

- `docs/reference/repository-layout.md` for repository structure;
- `docs/reference/git-workflow.md` for local and remote Git practices;
- `docs/reference/validation.md` for validation scope, evidence, and result classifications;
- `docs/reference/documentation-standards.md` for durable documentation;
- `.agent-work.README.md` for temporary local evidence.

## Universal safety and authority boundaries

- Preserve unrelated work and keep changes within the authorized scope.
- Do not expose secrets or commit transient artifacts, generated logs, local transcripts, SDKs, caches, or temporary delivery notes.
- Do not claim validation passed unless it actually ran and passed; never hide failures or incomplete work.
- Issue readiness, assignment, implementation authorization, validation, skills, and runtime configuration do not authorize remote actions.
- Do not push, publish, create tags or releases, open pull requests, or otherwise change remote state unless a maintainer explicitly authorizes that action.
- Local Git mechanics are governed separately by the task and `docs/reference/git-workflow.md`; they are not remote publication.
- Prefer focused commits with Conventional Commit messages.

## Reporting

Report what changed, what did not change, the exact validation performed, and every branch, commit, tag, pull-request, or remote-state change. Use the result classifications defined only in `docs/reference/validation.md`, and do not present local validation as CI evidence.
