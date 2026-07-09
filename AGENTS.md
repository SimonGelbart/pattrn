# AGENTS.md

This repository accepts assisted and automated changes when they are reproducible, reviewable, documented, and validated honestly.

Before changing files, follow the nearest applicable `AGENTS.md` in the affected path. Do not read every reference document by default. Read accepted ADRs only when the change touches a decision they govern.

## Required rules

- Do not claim validation passed unless it actually ran and passed.
- Do not hide failed validation.
- Do not commit transient artifacts, generated output, raw logs, local transcripts, local SDKs, package caches, or temporary delivery notes.
- Keep public documentation durable, agent-neutral, and free of environment-specific instructions unless framed as generic maintainer guidance.
- Prefer focused Conventional Commit messages when a commit is requested.
- Meaningful implementation changes should include tests and documentation, or a clear explanation of why they are not needed.

## Reporting

Use the smallest validation set that matches the change. Local preflight is useful, but do not report it as CI-equivalent proof.

When reporting work, state what changed, what did not change, which validation commands ran, which failed or were not run, and whether branch, commit, tag, pull request, or remote state changed.
