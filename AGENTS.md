# AGENTS.md

This repository accepts assisted and automated changes when they are scoped, reviewable, and validated honestly.

## Working agreement

- Inspect the worktree before editing and preserve unrelated changes.
- Read only the accepted ADRs, references, product docs, and nested instructions relevant to the task.
- Use current code, tests, and documentation as evidence for the change. If a request conflicts with an accepted ADR, stop and report the conflict.
- Keep changes focused. Do not commit secrets, generated logs, local transcripts, SDKs, caches, or temporary artifacts.
- Do not delete or weaken tests silently. When an API is removed, map each deleted test to a migrated contract test or document why its behavior is intentionally obsolete.
- Prefer focused commits with Conventional Commit messages when commits are requested.

## Authority

- Issue readiness, assignment, local implementation, validation, and agent configuration do not authorize remote actions.
- Do not push, publish, create tags or releases, open pull requests, or otherwise change remote state unless a maintainer explicitly asks.
- Local Git conventions are documented in `docs/reference/git-workflow.md`.

## Validation and reporting

- Select the smallest relevant validation set using `docs/reference/validation.md`; local checks are not CI evidence.
- Never claim a check passed unless it ran and passed, and never hide failures or incomplete work.
- Establish a baseline worktree status and relevant validation result before a broad change when practical; distinguish pre-existing failures from regressions.
- Report the changed scope, exact checks and result classifications, and any commit, branch, or remote action.
- If a requested command is unsupported by the installed toolchain, report that result and run the repository-documented equivalent when one exists.

## Pattrn product invariants

Pattrn is an allocation-conscious, immutable segmented-pattern matcher for .NET.
Keep the generic core framework-neutral; string, dependency-injection, and
routing behavior belongs in companion packages.

- Preserve deterministic registration order, duplicate behavior, ranking, and
  matching results.
- Keep diagnostics and explanation APIs separate from the default hot path.
- Treat public API snapshots and package documentation as compatibility
  contracts; update them intentionally when public behavior changes.
- Preserve trimming and Native AOT compatibility for the stable candidate
  packages. Do not introduce reflection, dynamic code, or linker-sensitive
  behavior without explicit justification and validation.
- Treat `Pattrn.Routing` as preview and do not broaden generic-core semantics
  to accommodate framework-specific routing behavior.
- Support performance claims with the relevant benchmark evidence. A local
  benchmark is investigation evidence, not CI proof.

## Bounded agent workflow

For a bounded change, use this sequence when the task benefits from multiple
agents:

```text
contract matrix and baseline
→ parallel read-only exploration
→ exactly one product-source writer
→ per-milestone focused tests and diff inspection
→ independent compatibility and artifact/API review
→ final affected gate
```

Before writing, create a compact matrix connecting each requirement to its
source, affected files/API, focused tests, validation command, and explicit
deferral. Use it to expose issue/ADR conflicts and to track deviations.

Parallelize independent read-only exploration and final reviews, but never run
concurrent writers against overlapping product files or concurrent .NET
commands that share `bin/` or `obj/` outputs. Keep each worker inside the
declared package and invariant scope. After every bounded milestone, run the
narrowest relevant tests, `git diff --check`, and a scoped diff inspection.
If a reviewer identifies a fix, rerun the affected review after the last source
change; earlier findings are stale evidence.

Stop for an architectural decision, non-converging repair, or a required
validation capability that is unavailable; do not silently broaden the
milestone.

The repository agents are role boundaries, not extra authorization. They must
not push, publish, open pull requests, modify remote state, or alter unrelated
worktree changes. Do not introduce a durable run ledger for ordinary changes;
use one only when an explicitly unattended workflow needs persistent evidence.
