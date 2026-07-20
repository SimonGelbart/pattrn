---
name: composing-cli-workflows
description: Compose minimal-context CLI workflows across repository discovery, search, selective viewing, structured data queries, analysis, refactoring, and .NET verification. Use when a task needs two or more CLI skills, when planning an exploration pipeline, or when choosing an efficient sequence of fd, rg, bat, jq, yq, sd, ast-grep, ctags, tokei, dotnet, fzf, and xargs commands.
---

# Compose CLI Workflows

## Boundary

Own sequencing multiple tools into an investigation, edit, or verification pipeline with bounded output at each step.
Use `choosing-cli-tools` first when the main question is which single tool to use, or when choosing between defaults, fallbacks, add-on tools, IDE features, MCP tools, and built-in capabilities.

Choose the shortest sequence that answers the task:

1. Choose tools with `choosing-cli-tools` when fallback or custom tooling is possible.
2. Discover paths with `fd` or `rg --files`.
3. Narrow results with `rg`, `jq`, or `yq`.
4. Inspect only relevant lines or symbols with `bat`, `sed`, `ctags`, or `ast-grep`.
5. Preview transformations with `sd` or `ast-grep`.
6. Apply changes and verify with the narrowest relevant query.

For a multi-package or public-contract change, add two gates before the first
edit: capture `git status --short` and `git diff --stat`, then build a compact
requirement matrix linking each requirement to files, tests, validation, and
deferrals. Do not let a broad source search substitute for that matrix.

Use waves for parallel work: run independent read-only discovery together,
serialize overlapping writers, and run independent artifact checks together
only when their output directories do not overlap. Serialize build, test, and
pack commands that share `bin/` or `obj/`. Review the final diff only after the
last source change; a review from an earlier diff is stale.

Before running a pipeline, state the intended output shape: filenames, line ranges, symbols, counts, or changed files.
Do not run a pipeline that produces unbounded source output.

Use non-interactive commands during agent execution. Use `fzf` only when generating a command for a human-driven terminal workflow.

Skip steps that add no value. Avoid loading whole files, printing large trees, or adding pipeline stages merely because a tool exists.

When the user explicitly requests a complete file read, follow that request
even though bounded reads are the default. For final reporting, preserve the
exact command and classify unsupported-command fallbacks separately.

Read [the workflow recipes](./reference/workflow-recipes.md) when selecting a multi-tool pipeline or restoring a less common combination.
