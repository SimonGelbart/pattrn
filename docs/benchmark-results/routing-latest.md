# Latest routing benchmark results

The latest committed routing benchmark rows are included in the alpha.30 full BenchmarkDotNet baseline:

```text
docs/benchmark-results/2026-06-17-alpha30/
```

The previous focused routing baseline remains available at:

```text
docs/benchmark-results/2026-06-15-alpha19-routing/
```

## Current interpretation

Pre-split route matching remains the routing hot path and remains allocation-free. Raw-string route helpers are convenience APIs: they split strings and may allocate. Route-template parsing is materially slower than alpha.19 because the routing package now preserves constraints, defaults, optionality, expansion metadata, and diagnostics. That cost is acceptable only because parsing is not the core matching hot path.
