---
name: validate-change
description: Select, run, and report proportionate validation for a completed repository change.
---

# Validate Change

Use this workflow after files change or when the user asks for validation.

1. Inspect `git status --short`, `git diff --stat`, and the current diff to identify the change type and unrelated worktree state.
2. Read `docs/reference/validation.md` and select the smallest relevant checks.
3. For broad changes, compare against a baseline when available and inspect deleted tests, public API snapshots, package metadata, documentation, and AOT/benchmark surfaces implied by the diff.
4. Run locally available checks without installing dependencies solely for validation. Serialize commands that share build outputs; parallelize independent checks with separate output directories.
5. If a requested command is rejected by the local toolchain, retain that exact result and run the repository-documented equivalent when available.
6. Classify every check using the canonical result vocabulary from the validation reference.
7. Report the exact command, working directory when useful, concise result, reason for any omitted or incomplete check, and whether the evidence predates the final source change.

Local validation is preflight evidence, not CI evidence. Keep generated output and raw logs out of commits.
