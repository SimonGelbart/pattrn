# Validation Reference

Do not claim validation passed unless it was actually run and passed.

## Source of truth

CI is the authoritative verification path for pull requests and protected branches.

The current CI workflow uses `actions/setup-dotnet` with `global-json-file: global.json`, then restores, builds, runs the Python benchmark-tool tests, and runs the .NET test assemblies with Microsoft.Testing.Platform. Documentation publishing is handled by the documentation workflow. Local commands are optional preflight checks and should not be reported as CI-equivalent proof.

SDK selection is controlled by the root `global.json` `sdk` section. The repository currently pins the .NET SDK feature band and allows roll-forward to a later .NET 10 feature band so local development and CI use a compatible .NET 10 SDK while remaining pre-beta-friendly.

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

## CI validation path

The CI workflow currently runs these checks on pull requests, pushes to `main`, and manual dispatches:

```bash
dotnet restore Pattrn.sln
dotnet build Pattrn.sln --configuration Release --no-restore
python -m unittest discover tools/benchmarks/tests
dotnet test --test-modules "tests/**/bin/Release/net10.0/*.Tests.dll" --root-directory . --results-directory TestResults -- --report-trx
```

The final `dotnet test` command intentionally uses Microsoft.Testing.Platform test modules from the Release build output so the workflow can collect TRX results under `TestResults/`.

## Local preflight

Legacy local `eng/` scripts have been retired. Use direct tool commands only when a maintainer wants local preflight, and do not report local preflight as CI-equivalent proof.

A normal local preflight for repository changes is:

```bash
dotnet restore Pattrn.sln
dotnet build Pattrn.sln --configuration Release --no-restore
dotnet test Pattrn.sln --configuration Release --no-build
python -m unittest discover tools/benchmarks/tests
```

Run `dotnet --version` or `dotnet --info` when you need to verify the active SDK selected by `global.json`, especially after changing SDK configuration.

Benchmark work should prefer the dedicated benchmark workflow. Local benchmark commands documented in `docs/reference/benchmarks.md` are optional investigation helpers, not current product proof.

## Offline restore

The supported default path is online restore through ordinary `dotnet restore`, followed by ordinary build and test commands. Offline restore is non-default maintainer recovery work only: if a maintainer needs it, use a local NuGet configuration or package source with the same direct `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet pack` commands that fit the change.

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
