# Diagnostics Reference

`PattrnIndexBuilder<TSegment, TValue>.GetDiagnostics()` reports advisory diagnostics for the current builder contents.

Diagnostics are intentionally generic and do not change matching behavior. They help callers decide whether a pattern set should be accepted, warned about, or rejected by a higher-level package or application policy.

Runtime explanation is separate from builder diagnostics. Use `index.Explain(path)` when you need a troubleshooting result for a specific input path. Rejected-candidate hints are disabled by default and can be requested with `PatternExplanationOptions.IncludeRejections`.

## Stability classifications

Diagnostic surfaces use three stability classifications before beta:

| Classification | Meaning |
|---|---|
| Stable | Part of the beta compatibility promise. Existing stable names, numeric values, documented meanings, and documented API semantics should not change incompatibly after beta. |
| Best-effort | Useful human/debugging information, but the exact value, wording, formatting, or order may change as diagnostics improve. Do not use best-effort text or ordering as a durable machine contract. |
| Preview | Public surface exists, but stability is intentionally unresolved before a linked follow-up design is complete. Preview members may remain useful, but consumers should avoid treating their detailed semantics as stable tooling contracts. |

## Builder diagnostic stability

Existing builder diagnostic kinds keep their names, numeric values, and meanings as part of the beta compatibility promise. New diagnostic kinds may be added after beta as additive changes, so consumers should not assume the `PatternDiagnosticKind` enum set is exhaustive.

| Kind | Numeric value | Default severity | Stability | Meaning |
|---|---:|---|---|---|
| `DuplicatePattern` | 0 | Warning | Stable | More than one value is registered for the same structural pattern. |
| `AmbiguousParameterNames` | 1 | Warning | Stable | Structurally equivalent parameterized patterns use different capture names, such as `{id}` and `{name}` at the same segment position. |
| `OverlappingWildcard` | 2 | Info | Stable kind/severity; overlap metadata needs follow-up | A wildcard branch overlaps one or more literal branches at the same position. Literal matches still rank ahead of wildcard matches. |
| `OverlappingCatchAll` | 3 | Info | Stable kind/severity; overlap metadata needs follow-up | A terminal catch-all overlaps a shorter or more specific pattern below the same prefix. More specific matches still rank ahead of catch-all matches. |

For existing builder diagnostics, these surfaces are stable:

- `PatternDiagnosticKind` enum member names;
- `PatternDiagnosticKind` numeric values;
- the documented meaning of each existing diagnostic kind;
- the default severity for each existing diagnostic kind;
- `PatternDiagnostic<TSegment>.Kind`;
- `PatternDiagnostic<TSegment>.Severity`;
- `PatternDiagnostic<TSegment>.Pattern` for single-pattern diagnostics;
- `PatternDiagnostic<TSegment>.RegistrationCount` semantics for count-based diagnostics.

`PatternDiagnostic<TSegment>.Message` is best-effort human/debugging text. Exact message wording, punctuation, formatting, and phrasing are not compatibility promises. Consumers that need durable behavior should use structured fields such as `Kind`, `Severity`, `Pattern`, and `RegistrationCount` rather than parsing `Message`.

`GetDiagnostics()` should be deterministic for the same registered pattern set, but exact diagnostic ordering is not a compatibility promise. Consumers that need stable snapshots should sort by structured fields such as kind numeric value, severity, structural pattern, and registration count.

## Pattern metadata

`PatternDiagnostic<TSegment>.Pattern` identifies the structural pattern associated with the diagnostic:

- for single-pattern diagnostics such as `DuplicatePattern` and `AmbiguousParameterNames`, `Pattern` is stable and exact for the diagnosed structural pattern;
- for `OverlappingWildcard`, `Pattern` identifies the wildcard branch being diagnosed;
- for `OverlappingCatchAll`, `Pattern` identifies the catch-all branch being diagnosed.

Full structured relationship metadata for overlap diagnostics is not yet modeled. For example, overlap diagnostics do not currently expose all overlapped literal, wildcard, catch-all, or prefix relationships as stable structured data. [Issue #71](https://github.com/SimonGelbart/pattrn/issues/71) is the follow-up for structured overlap diagnostic metadata.

## Example

```csharp
var builder = PattrnIndex<string, string>
    .Builder("*")
    .Add(["orders", "*"], "wildcard")
    .Add(["orders", "new"], "literal");

var diagnostics = builder.GetDiagnostics();
```

This reports an `OverlappingWildcard` diagnostic for `orders/*`. The registration set is still valid and matching remains deterministic.

## Runtime explanation stability

Runtime explanation has mixed stability. The API exists for debugging, validation, and tooling, but rejected-candidate reason details are still being designed.

| Surface | Stability | Notes |
|---|---|---|
| `Explain(...)` | Stable | Explanation remains an opt-in diagnostic API separate from hot matching. |
| `PatternExplanationOptions.Default` | Stable | Default explanation options keep rejected-candidate collection disabled. |
| `PatternExplanationOptions.IncludeRejections` | Stable | Convenience value for collecting rejected candidates. |
| `PatternExplanationOptions.IncludeRejectedCandidates` | Stable | Indicates whether rejected-candidate hints are collected. |
| `PatternMatchExplanation<TSegment, TValue>.Path` | Stable | Copy of the explained input path. |
| `PatternMatchExplanation<TSegment, TValue>.Matches` | Stable | Accepted detailed matches for the explained input path. |
| `PatternMatchExplanation<TSegment, TValue>.MatchOptions` | Stable | Matching options used by the explained index. |
| `PatternMatchExplanation<TSegment, TValue>.ExplanationOptions` | Stable | Explanation options used to produce the result. |
| `PatternMatchExplanation<TSegment, TValue>.MatchCountUpperBound` | Stable | Path-specific accepted-match upper bound at explanation time. |
| `PatternMatchExplanation<TSegment, TValue>.CaptureCountUpperBound` | Stable | Path-specific named-capture upper bound at explanation time. |
| `PatternMatchExplanation<TSegment, TValue>.HasMatches` | Stable | Indicates whether accepted matches were produced. |
| `PatternMatchExplanation<TSegment, TValue>.MatchCount` | Stable | Count of accepted detailed matches. |
| `PatternRejectedCandidate.PathDepth` | Stable | Zero-based input depth where the rejected branch stopped matching. |
| Rejected-candidate reason taxonomy | Preview | The set of reason categories is not yet stable. |
| `PatternRejectedCandidate.Reason` semantics | Preview | Human-readable reason text is useful for debugging, but should not be the only durable machine-consumable signal for tooling. |

`PatternRejectedCandidate.Reason` may remain useful human-readable text, but consumers should not rely on its exact text or current taxonomy as the only durable machine-consumable signal when rejected candidates are intended for tooling. [Issue #72](https://github.com/SimonGelbart/pattrn/issues/72) is the beta-blocking follow-up for strong-typed rejected-candidate reasons.

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

The `ValidateOnBuild(...)` API shape is stable, with additive diagnostic behavior:

- validation methods exist on builders;
- `ValidateOnBuild(minimumSeverity)` rejects diagnostics at or above that severity;
- `ValidateOnBuild(predicate)` lets callers control rejection;
- `DisableBuildValidation()` disables build validation;
- validation does not change default matching semantics.

Because new diagnostic kinds may be added after beta, callers that opt into broad severity thresholds may see future versions reject pattern sets that previously passed. Use a custom predicate when a package or application needs a narrower compatibility policy.

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
