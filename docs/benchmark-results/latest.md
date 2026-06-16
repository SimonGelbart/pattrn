# Latest benchmark results

The latest committed full BenchmarkDotNet baseline is:

```text
docs/benchmark-results/2026-06-17-alpha30/
```

See [2026-06-17-alpha30/README.md](2026-06-17-alpha30/README.md) for the alpha.30 full run, comparison against the alpha.17/alpha.19 baselines, and the performance red flags that shaped the alpha.31 speed-guardrail work.

Speed guardrails are documented in [performance-guardrails.md](performance-guardrails.md).

## Current interpretation

`3.0.0-alpha.30` confirms that the core indexed matcher remains much faster than naive scanning and that span-based core matching remains allocation-free. It also exposed red flags in exact wide-fanout matching, duplicate-heavy detailed materialization, route parsing, and parameter-heavy build time. Alpha.31 starts addressing this by protecting the exact-only core span path and by making benchmark comparison a roadmap gate.
