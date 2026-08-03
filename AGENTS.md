# Contributing to Pattrn

Pattrn is an immutable segmented-pattern index for read-heavy .NET
applications. Keep the generic core framework-neutral and preserve deterministic
registration, matching, duplicate, capture, package, trimming, and Native AOT
contracts.

Before editing, inspect the worktree and preserve unrelated changes. Keep
changes scoped and reviewable. Do not commit generated output, benchmark
artifacts, local logs, SDKs, or secrets.

Use focused tests while working, then run the affected solution validation. The
canonical local checks are:

```bash
dotnet restore Pattrn.sln
dotnet build Pattrn.sln --configuration Release --no-restore
python -m unittest discover tools/benchmarks/tests
dotnet test --test-modules "tests/**/bin/Release/net10.0/*.Tests.dll" --root-directory . --results-directory TestResults -- --report-trx
```

Local validation is evidence for the working tree, not CI evidence. Report
skipped or unavailable platform checks explicitly. Run the AOT/trimming harness
when package or core runtime behavior changes, and use focused BenchmarkDotNet
runs for performance investigations rather than committing machine-specific
results.

Do not broaden Pattrn into a router, broker, rules engine, cache, authorization
framework, subscription registry, or continuously changing registration store.
New capabilities require evidence from a concrete application and must fit the
immutable read-heavy lifecycle.

Use Conventional Commit messages for requested commits. Do not push, publish,
create releases, or modify remote repository state unless a maintainer
explicitly authorizes it.
