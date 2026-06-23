# Validation Reference

Do not claim validation passed unless it was actually run and passed.

## Source of truth

CI is the authoritative verification path for pull requests and protected branches.

The current CI workflow restores, builds, and tests the solution with the .NET SDK selected by `global.json`. Documentation publishing is handled by the documentation workflow. Local commands are optional preflight checks and should not be reported as CI-equivalent proof.

## Pull request validation expectations

Use the smallest validation set that matches the change:

| Change type | Expected validation |
|---|---|
| Documentation-only change with no code, samples, package metadata, or workflow changes | Markdown/link or docs-rendering checks when available; rely on CI for repository verification. |
| Documentation examples, samples, or package README source changes | Docs checks plus CI build/test/package metadata coverage. |
| Product code or test changes | CI restore, build, and test. |
| Package metadata or packing changes | CI build/test plus package inspection where configured. |
| Benchmark workflow or performance-claim changes | Dedicated benchmark workflow artifacts and summaries. |

When a maintainer explicitly says local build/test can be skipped for a documentation-only PR, report those commands as `Not run` and state why.

## Local preflight

Legacy local `eng/` scripts have been retired. Use direct tool commands only when a maintainer wants local preflight, and do not report local preflight as CI-equivalent proof.

Benchmark work should prefer the dedicated benchmark workflow. Local benchmark commands documented in `docs/development/benchmarks.md` are optional investigation helpers, not current product proof.

## Offline local restore

CI is preferred. If a maintainer needs local offline restore, use a local NuGet configuration or package source and run direct `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet pack` commands as appropriate for the change.

Keep environment-specific paths out of durable documentation unless they are framed as generic maintainer guidance.

## Required validation record

For every reported command or workflow, include:

- command or workflow name;
- working directory, when applicable;
- result: `Passed`, `Failed`, `Not run`, or `Not completed`;
- concise output summary;
- failure reason, if any;
- reason a normally relevant command was not run, if skipped.

Generated output, package artifacts, raw logs, local SDKs, and local package caches should not be committed.
