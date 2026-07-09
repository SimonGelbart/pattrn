# .github/workflows/AGENTS.md

Use this file for GitHub Actions workflow changes.

## Context to read

Read the root `AGENTS.md` first. Then read the smallest relevant set:

- `docs/reference/validation.md` for CI validation expectations
- `docs/reference/documentation-site.md` for documentation publishing changes
- `docs/reference/benchmarks.md` for benchmark workflow changes
- `docs/reference/aot-trimming.md` for AOT/trimming workflow changes
- the affected workflow file

## Workflow map

- `ci.yml` is the main restore, build, Python benchmark-tool test, and .NET test workflow.
- `docs.yml` publishes the Astro documentation site through GitHub Pages.
- `aot-trimming.yml` runs trimming and Native AOT compatibility validation.
- `benchmarks.yml` runs BenchmarkDotNet workflows for manual and tag-triggered benchmark evidence.

## Workflow rules

- Keep permissions minimal.
- Preserve concurrency settings unless the task intentionally changes run cancellation behavior.
- Keep generated logs, artifacts, local transcripts, package caches, and SDK installs out of committed files.
- Update `docs/reference/validation.md` or the relevant reference doc when workflow behavior changes materially.

## Validation

For workflow-only changes, inspect the YAML with `yq` when available and review affected commands against the relevant reference documentation. Run local commands only when they are meaningful outside GitHub Actions.
