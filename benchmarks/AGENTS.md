# benchmarks/AGENTS.md

Use this file for BenchmarkDotNet benchmarks, benchmark tooling, benchmark workflows, grouped summaries, and performance-claim work.

## Context to read

Read the root `AGENTS.md` first. Then read:

- `docs/reference/benchmarks.md`
- `docs/reference/validation.md`
- affected benchmark code, workflow files, and benchmark tools

## Benchmark evidence rules

- Current benchmark evidence comes from the `Benchmarks` GitHub Actions workflow, not locally committed machine output.
- Do not treat old local benchmark output or committed reports as current product proof.
- Keep raw BenchmarkDotNet output, generated grouped summaries, comparison output, and local benchmark artifacts out of commits unless the repository explicitly documents otherwise.
- Treat unclassified benchmark rows and `Unknown` guardrails as follow-up items before using a run to support a performance-sensitive decision.

## Benchmark scope

Prefer focused benchmark investigation when a full run is unnecessary. Use the documented groups and filters for core, detailed matching, strings, routing, builder, and diagnostics work.

## Validation

Run benchmark-tool unit tests when changing benchmark tooling. Use benchmark workflow artifacts and summaries for performance claims. Do not make performance claims from unvalidated local output.
