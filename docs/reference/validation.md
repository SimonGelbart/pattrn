# Validation Reference

Do not claim validation passed unless it was actually run and passed.

## Standard commands

From the repository root:

```bash
bash eng/restore.sh
bash eng/build.sh
bash eng/test.sh
bash eng/pack.sh
bash eng/inspect-packages.sh
```

Benchmark work should also run a focused benchmark command from `docs/development/benchmarks.md` or `bash eng/benchmark.sh` when practical.

## Offline restore

The scripts can restore from a local package bundle. Set `NUGET_BUNDLE_PACKAGES` when the bundle is not in one of the standard local locations:

```bash
export NUGET_BUNDLE_PACKAGES=/absolute/path/to/offline-nuget-bundle/packages
```

Set `DOTNET` if the required .NET 10 SDK is not discoverable on `PATH`:

```bash
export DOTNET=/absolute/path/to/dotnet
```

## Required validation record

For every reported command, include:

- command;
- working directory;
- result: `Passed`, `Failed`, `Not run`, or `Not completed`;
- concise output summary;
- failure reason, if any.

Generated output, package artifacts, raw logs, local SDKs, and local package caches should not be committed.
