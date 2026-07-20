# Benchmarks

Benchmark results are evidence for maintainers, not a user-facing performance promise. CI workflow artifacts are the source of truth; local runs are exploratory.

## What is protected

The benchmark suite covers:

- core exact matching and caller-provided TryMatch buffers;
- explicit prefix matching;
- detailed matching and Explain separately;
- string splitting and normalization;
- route helpers while routing remains preview;
- builder construction and duplicate-heavy registrations.

Diagnostics and explanations must remain separate from the normal matching groups because they intentionally allocate and perform extra work.

## CI workflow

The Benchmarks workflow runs BenchmarkDotNet and uploads raw reports, metadata, grouped summaries, and job summaries. Full single-run mode is the comparable baseline. Sharded mode is useful for coverage but may run groups on different runners.

The workflow supports focused groups such as Core, Strings, Routing, Builder, and Diagnostics. Use the workflow inputs rather than treating a local report as current product evidence.

## Local investigation

From the repository root:

```bash
dotnet restore benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj
dotnet run --project benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj --configuration Release
```

Use BenchmarkDotNet filters or the workflow's group mappings for focused experiments. Do not commit generated reports, machine-specific baselines, or raw logs.

## Reporting

When reporting benchmark evidence, identify the commit, SDK/runtime, operating system, benchmark mode/group, and whether the result is a single-run or aggregate artifact. Compare like-for-like groups and state when a result is exploratory or incomplete.
