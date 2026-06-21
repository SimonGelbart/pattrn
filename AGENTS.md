# AGENTS.md

This repository accepts assisted and automated changes when they are reproducible, reviewable, documented, and validated honestly.

## Mandatory read order

Before changing the repository, read this file. For implementation work, also read:

1. `docs/reference/project-profile.md`
2. `docs/reference/repository-layout.md`
3. `docs/reference/git-workflow.md`
4. `docs/reference/validation.md`
5. `docs/reference/documentation-standards.md`
6. `docs/reference/architecture/architecture-principles.md`
7. `docs/reference/architecture/boundaries.md`
8. `docs/reference/architecture/dependency-policy.md`
9. `docs/reference/architecture/testing-strategy.md`
10. the relevant language profile under `docs/reference/coding/`
11. `docs/adr/README.md`
12. any accepted ADR relevant to the area being changed

For documentation-only work, read the first five items, the ADR index, and any affected product documentation.

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
- Do not claim validation passed unless it was actually run and passed.
- Do not hide failed validation.
- Do not commit transient artifacts, generated test output, raw validation logs, local transcripts, local SDKs, package caches, or temporary delivery notes.
- Keep public repository documentation durable and agent-neutral.
- Keep environment-specific instructions out of committed docs unless they are framed as generic maintainer guidance.
- Prefer focused commits with Conventional Commit messages.
- Every meaningful implementation change should include tests and documentation, or a clear explanation of why they are not needed.

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
  eng/
  src/
  tests/
  benchmarks/
  samples/
```

Some older alpha documentation still lives under `docs/getting-started/`, `docs/design/`, `docs/development/`, and `docs/release/`. Do not expand those folders without checking whether the material belongs in the Diataxis structure instead.

## Honesty standard

When reporting work, state clearly:

- what changed;
- what did not change;
- which validation commands ran;
- which validation commands failed or were not run;
- whether a branch, commit, tag, pull request, or remote state was changed.
