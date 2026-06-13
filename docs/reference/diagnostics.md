# Builder Diagnostics

`PattrnIndexBuilder<TSegment, TValue>.GetDiagnostics()` reports advisory diagnostics for the current builder contents.

Diagnostics are intentionally generic and do not change matching behavior. They help callers decide whether a pattern set should be accepted, warned about, or rejected by a higher-level package or application policy.

Runtime explanation is separate from builder diagnostics. Use `index.Explain(path)` when you need an allocation-friendly troubleshooting result for a specific input path. Rejected-candidate hints are disabled by default and can be requested with `PatternExplanationOptions.IncludeRejections`.

## Current diagnostics

| Kind | Severity | Meaning |
|---|---:|---|
| `DuplicatePattern` | Warning | More than one value is registered for the same structural pattern. |
| `AmbiguousParameterNames` | Warning | Structurally equivalent parameterized patterns use different capture names, such as `{id}` and `{name}` at the same segment position. |
| `OverlappingWildcard` | Info | A wildcard branch overlaps one or more literal branches at the same position. Literal matches still rank ahead of wildcard matches. |
| `OverlappingCatchAll` | Info | A terminal catch-all overlaps a shorter or more specific pattern below the same prefix. More specific matches still rank ahead of catch-all matches. |

## Example

```csharp
var builder = PattrnIndex<string, string>
    .Builder("*")
    .Add(["orders", "*"], "wildcard")
    .Add(["orders", "new"], "literal");

var diagnostics = builder.GetDiagnostics();
```

This reports an `OverlappingWildcard` diagnostic for `orders/*`. The registration set is still valid and matching remains deterministic.

## Runtime explanation

`Explain(...)` returns accepted detailed matches and optional rejected-candidate hints for one input path:

```csharp
var explanation = index.Explain(
    ["customers", "42"],
    PatternExplanationOptions.IncludeRejections);

var acceptedMatches = explanation.Matches;
var rejectedHints = explanation.RejectedCandidates;
```

Rejected hints are branch-level diagnostics, not domain validation failures. Route constraints, authorization decisions, filesystem policies, and other domain-specific rejections still belong above the generic core.

## Policy belongs above the core

The core package does not decide whether duplicates, wildcard overlap, or catch-all overlap are errors. Different domains have different rules:

- topic dispatchers may intentionally allow many handlers per pattern;
- route-like systems may reject duplicate route templates;
- authorization systems may reject ambiguous parameter names;
- command dispatchers may allow overlaps but require explicit best-match selection.

Use diagnostics as policy input in application code or companion packages such as `Pattrn.Routing`.

## Duplicate-pattern behavior

Diagnostics are advisory, but duplicate structural patterns can be handled immediately during registration when that is the desired policy:

```csharp
var builder = PattrnIndex<string, string>
    .Builder("*")
    .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw);
```

| Behavior | Meaning |
|---|---|
| `Append` | Add another value to the existing structural pattern. This is the default and preserves the historical behavior. |
| `Throw` | Reject the new registration when the structural pattern already exists. |
| `Replace` | Remove existing values for the structural pattern and keep only the new registration. |
| `Ignore` | Keep existing values and ignore the new duplicate registration. |

This policy is registration-time behavior. It is intentionally separate from `DuplicateValueMatchMode`, which controls whether duplicate values are deduplicated or preserved when a compiled index emits match results.

## Build validation

Build validation is opt-in. It turns selected diagnostics into build failures without changing the default permissive core behavior.

Reject warnings and errors:

```csharp
var index = builder
    .ValidateOnBuild()
    .Build();
```

Reject informational diagnostics too:

```csharp
var index = builder
    .ValidateOnBuild(PatternDiagnosticSeverity.Info)
    .Build();
```

Use a custom policy:

```csharp
var index = builder
    .ValidateOnBuild(diagnostic => diagnostic.Kind == PatternDiagnosticKind.OverlappingCatchAll)
    .Build();
```

Disable validation again:

```csharp
builder.DisableBuildValidation();
```

Build validation belongs in the core because it is a generic policy mechanism. Domain-specific validation rules still belong above the core.
