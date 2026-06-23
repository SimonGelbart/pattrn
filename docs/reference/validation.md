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

## Local legacy helpers

The `eng/` scripts are legacy/local convenience helpers. They may be useful for maintainer preflight work, offline restore experiments, or reproducing older validation notes, but they are not the canonical verification source.

If you choose to use them locally, run them from the repository root:

```bash
bash eng/restore.sh
bash eng/build.sh
bash eng/test.sh
bash eng/pack.sh
bash eng/inspect-packages.sh
```

Benchmark work should prefer the dedicated benchmark workflow once it is available. Until then, local benchmark commands documented in `docs/development/benchmarks.md` are historical/transitional preflight helpers, not current product proof.

## Offline local restore

The legacy scripts can restore from a local package bundle. Set `NUGET_BUNDLE_PACKAGES` when the bundle is not in one of the standard local locations:

```bash
export NUGET_BUNDLE_PACKAGES=/absolute/path/to/offline-nuget-bundle/packages
```

Set `DOTNET` if the required .NET SDK is not discoverable on `PATH`:

```bash
export DOTNET=/absolute/path/to/dotnet
```

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
