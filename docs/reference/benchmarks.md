# Benchmarks

Benchmark results are evidence for maintainers, not a user-facing performance promise. CI workflow artifacts are the source of truth; local runs are exploratory.

## What is protected

The supported benchmark project covers a bounded set of core scenarios:

- compilation and canonical pattern analysis;
- literal and wildcard exact matching;
- best-prefix and all-prefix operations;
- caller-provided buffers and owning value results;
- focused detailed and diagnostic scenarios.

Diagnostics and explanations remain separate from the normal matching path
because they intentionally allocate and perform extra work. String, routing,
and Homework experiments are not supported benchmark baselines.

## CI workflow

The benchmark workflow is manual and focused. It runs selected BenchmarkDotNet
filters and uploads raw reports, metadata, grouped summaries, and job summaries.
It does not run a complete Cartesian matrix or enforce performance thresholds.

Use the workflow inputs for focused core or compilation runs rather than treating
a local report as current product evidence.

## Local investigation

From the repository root:

```bash
dotnet restore benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj
dotnet run --project benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj --configuration Release -- --filter '*PattrnIndexBenchmarks*' --job Dry
```

Use BenchmarkDotNet filters or the workflow's group mappings for focused experiments. Do not commit generated reports, machine-specific baselines, or raw logs.

## Reporting

When reporting benchmark evidence, identify the commit, SDK/runtime, operating system, benchmark mode/group, and whether the result is a single-run or aggregate artifact. Compare like-for-like groups and state when a result is exploratory or incomplete.
