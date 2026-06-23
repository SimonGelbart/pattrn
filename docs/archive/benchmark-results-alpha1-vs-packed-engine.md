# alpha.1 vs packed engine

This file is reserved for the official before/after benchmark report comparing:

- `3.0.0-alpha.1`: object-graph compiled trie with recursive wildcard traversal;
- `3.0.0-alpha.2`: packed compiled arrays with iterative wildcard traversal.

The sandbox implementation increment introduced the packed representation and kept the benchmark harness in place, but a full publishable BenchmarkDotNet run requires a longer uninterrupted runtime than the constrained sandbox window.

Generate the report with:

```bash
dotnet run --project benchmarks/Pattrn.Benchmarks/Pattrn.Benchmarks.csproj --configuration Release -- --filter "*" --join --exporters json --exporters markdown --strategy Throughput --artifacts BenchmarkDotNet.Artifacts
```

Then replace this placeholder with the generated BenchmarkDotNet report and summary.
