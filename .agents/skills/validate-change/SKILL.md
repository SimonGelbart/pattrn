---
name: validate-change
description: Select, run, and report proportionate validation for a completed repository change.
---

# Validate Change

Use this workflow after files change or when the user asks for validation.

1. Inspect `git status --short` and the current diff to identify the change type.
2. Read `docs/reference/validation.md` and select the smallest relevant checks.
3. Run locally available checks without installing dependencies solely for validation.
4. Classify each check using the canonical result vocabulary from the validation reference.
5. Report the exact command, working directory when useful, concise result, and reason for any omitted or incomplete check.

Local validation is preflight evidence, not CI evidence. Keep generated output and raw logs out of commits.
