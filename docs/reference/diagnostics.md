# Diagnostics

Diagnostics are opt-in. Normal value matching does not construct explanations or rejected-candidate records.

## Build diagnostics

Builders can inspect registered patterns before publishing an index:

```csharp
var diagnostics = builder.GetDiagnostics();
builder.ValidateOnBuild(PatternDiagnosticSeverity.Warning);
```

`ValidateOnBuild` can use a minimum severity or a predicate. `DisableBuildValidation` turns validation off. A diagnostic contains its `Kind`, `Severity`, message, pattern, and registration count. Use structured fields for decisions; `Message` is human-readable text.

| Kind | Numeric value | Default severity | Stability |
|---|---:|---|---|
| `DuplicatePattern` | 0 | Warning | Stable |
| `AmbiguousParameterNames` | 1 | Warning | Stable |
| `OverlappingWildcard` | 2 | Info | Stable kind/severity; overlap metadata needs follow-up |
| `OverlappingCatchAll` | 3 | Info | Stable kind/severity; overlap metadata needs follow-up |

## Runtime explanations

Use `Explain` when troubleshooting or building developer-facing tooling:

```csharp
var explanation = index.Explain(path);
var withRejections = index.Explain(path, PatternExplanationOptions.IncludeRejections);
```

An explanation includes the input path, accepted detailed matches, match options, upper bounds, and optionally rejected candidates. It allocates and may perform extra traversal, so keep it off repeated matching paths.

## Rejected candidates

`PatternRejectedCandidate` exposes the path depth, a structured `ReasonKind`, and a diagnostic reason. Rejection records are hints for explanation and debugging, not a replacement for the matching contract.

## Stability

The matching result, capture, duplicate, and ranking contracts are documented in [matching semantics](matching-semantics.md). Diagnostic messages and the exact set/order of optional rejection hints may evolve before beta. Consumers needing durable behavior should use enum kinds and other structured fields rather than parse message text.
