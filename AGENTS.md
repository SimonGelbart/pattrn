# AGENTS.md

This repository accepts assisted and automated changes when they are reproducible, reviewable, documented, and validated honestly.

## Repository-wide authority

This file is the canonical repository-wide instruction source for assisted and automated changes. Nested `AGENTS.md` files may add stable subtree-specific guidance, but they must not weaken this file, accepted ADRs, or direct maintainer instructions.

## Proportionate repository discovery

Before changing the repository, perform discovery that is proportionate to the task:

1. Confirm the requested base revision or branch when the task specifies one.
2. Inspect `git status --short` and stop before editing if existing changes conflict with the task.
3. Read this file and any nested `AGENTS.md` files that apply to files you may touch.
4. Read the stable reference documents needed for the change. Start with:
   - `docs/reference/project-profile.md`
   - `docs/reference/repository-layout.md`
   - `docs/reference/git-workflow.md`
   - `docs/reference/validation.md`
   - `docs/reference/documentation-standards.md`
5. For implementation work, also read the applicable architecture references, language profiles under `docs/reference/coding/`, `docs/adr/README.md`, and any accepted ADR relevant to the affected area.
6. For documentation-only work, read the affected product documentation and any ADR or reference page that governs it.

Do not expand discovery into unrelated historical cleanup. If repository evidence conflicts with the request, stop and report the conflict.

## Rule precedence

Use this precedence order:

1. Direct maintainer instruction for the current task
2. Accepted ADRs in `docs/adr/`
3. This `AGENTS.md` file
4. Stable reference documentation in `docs/reference/`
5. Product documentation in `docs/tutorials/`, `docs/how-to/`, `docs/explanation/`, and existing product folders
6. Existing code and test conventions

If a requested change contradicts an accepted ADR, do not silently ignore the conflict. Either adapt the change to follow the ADR or add a new ADR that supersedes or amends the previous decision.

## Non-negotiable rules

- Do not push to a remote unless the maintainer explicitly asks.
- Do not open a pull request unless the maintainer explicitly asks.
- Do not treat issue readiness, task assignment, local implementation permission, validation planning, skills, or runtime configuration as authority to push, publish, tag, release, or open a pull request.
- Do not claim validation passed unless it was actually run and passed.
- Do not hide failed validation.
- Do not commit transient artifacts, generated test output, raw validation logs, local transcripts, local SDKs, package caches, or temporary delivery notes.
- Keep public repository documentation durable and agent-neutral.
- Keep environment-specific instructions out of committed docs unless they are framed as generic maintainer guidance.
- Prefer focused commits with Conventional Commit messages.
- Every meaningful implementation change should include tests and documentation, or a clear explanation of why they are not needed.

## Agent-facing support files

- `.agents/skills/**` contains platform-agnostic, non-authoritative workflow guidance. Skills help agents do work consistently; they do not override this file, ADRs, or maintainer instructions.
- `.codex/**` contains Codex-specific runtime configuration. It follows repository instructions and skills; it does not redefine repository authority.
- `.agent-work/` is ignored temporary workspace for local evidence, handoff notes, and interrupted work. It is non-authoritative and must not be committed. The .agent-work/ is ignored convention is for temporary evidence only.

## Repository shape

Pattrn uses this durable shape:

```text
pattrn/
  AGENTS.md
  CHANGELOG.md
  docs/
    README.md
    tutorials/
    how-to/
    reference/
      architecture/
      coding/
    explanation/
    adr/
  src/
  tests/
  benchmarks/
  samples/
```

Older pre-beta documentation lives under `docs/archive/pre-beta/`. Do not expand archived folders without checking whether the material belongs in the Diataxis structure instead.

## Honesty standard

When reporting work, state clearly:

- what changed;
- what did not change;
- which validation commands ran;
- which validation commands failed or were not run;
- whether a branch, commit, tag, pull request, or remote state was changed.

Validation results must be classified as `Passed`, `Failed`, `Not run`, `Not completed`, or `Skipped` with the capability-based reason when applicable. Local validation is not CI evidence.
