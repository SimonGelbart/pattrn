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

Use multiple agents only when the change has independent unknowns, crosses a
package or public-contract boundary, or has a material allocation, AOT, or
behavioral risk. Do not turn a routine internal cleanup into a multi-agent
workflow merely because agents are available.

For a broad or higher-risk bounded change, use this sequence:

```text
contract matrix and baseline
→ parallel read-only exploration
→ exactly one product-source writer
→ per-milestone focused tests and diff inspection
→ independent compatibility and artifact/API review
→ final affected gate
```

For every change, write a short acceptance checklist before editing. It must
identify the invariant, affected files, focused tests, validation boundary,
and explicit deferrals. For a broad or higher-risk change, expand it into a
compact matrix connecting each requirement to its source, affected files/API,
focused tests, validation command, and explicit deferral. State whether
removing now-unreferenced internals is required by the milestone or is a
follow-up; do not leave that decision to final review.

For a routine internal refactor with no public-contract, package, or runtime
posture change, prefer this lean path:

```text
baseline and acceptance checklist
→ one writer with focused tests
→ one independent review
→ affected build/test and diff check
```

Parallelize independent read-only exploration and final reviews only when the
expected information gain outweighs their coordination cost. Never run
concurrent writers against overlapping product files or concurrent .NET
commands that share `bin/` or `obj/` outputs. Keep each worker inside the
declared package and invariant scope. After every bounded milestone, run the
narrowest relevant tests, `git diff --check`, and a scoped diff inspection.

Reviewers must label each note as **blocking**, **actionable in this
milestone**, or **follow-up**. Collect actionable findings before returning to
the writer and prefer one repair batch. A follow-up does not expand the
milestone without an acceptance-checklist requirement or maintainer direction.
After a source change, rerun only the reviews and validation affected by that
change; earlier evidence is stale only for the changed surface. For example,
a test-only change needs refreshed build/test evidence, not another AOT or
benchmark run when product sources are unchanged.

Stop after a second repair cycle unless a confirmed blocker remains. At that
point, request an architectural or scope decision rather than repeatedly
expanding the work.

Stop for an architectural decision, non-converging repair, or a required
validation capability that is unavailable; do not silently broaden the
milestone.

The repository agents are role boundaries, not extra authorization. They must
not push, publish, open pull requests, modify remote state, or alter unrelated
worktree changes. Do not introduce a durable run ledger for ordinary changes;
use one only when an explicitly unattended workflow needs persistent evidence.

## Token-efficient verification

- Do not stream verbose successful build, migration, reset, or test output into the conversation.
- Capture complete verification output in an ignored local artifact.
- Report only the command, exit status, duration, suite summary, warnings, and relevant failure excerpt.
- On failure, inspect the smallest useful log section first and expand only when needed.
- Preserve full logs locally when they are required as evidence.
- When a verification command is still running, inspect its artifact only after completion. Read the smallest useful failure excerpt; never load full passing artifacts unless exact evidence is required.
- Before loading long skill, browser, or tool documentation, read only the required instructions. If the tool requires its full documentation, keep it out of user-facing updates and summarize only the rules relevant to the task. Do not reload the same documentation in the same task.

## Efficient tool batching

In Code Mode, within each bounded stage, run independent, functions.exec-available tool calls concurrently in one functions.exec call. Use await Promise.allSettled([...]) when partial results are useful, and inspect every result; use await Promise.all([...]) only when any failure should abort the batch. Keep dependencies, waits/resumes, approvals, conflicting or interdependent mutations, and adaptive investigations where each result may change the next step sequential. Do not split otherwise batchable inspections across outer tool calls.
