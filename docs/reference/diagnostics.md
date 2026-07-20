# Diagnostics

Compilation diagnostics are separate from normal matching. They are returned by
`CompileWithDiagnostics` or `BuildWithDiagnostics`; `Build` throws
`PattrnCompilationException` when the report contains errors.

```csharp
var result = PattrnIndex<string, string>.CompileWithDiagnostics(
    registrations,
    new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn });

if (result.TryGetIndex(out var index))
{
    // The index is complete and the report can still contain warnings.
}
```

`PattrnDiagnosticReport` owns an immutable diagnostic array and exposes intrinsic
`HasErrors` and `HasWarnings` flags. `PattrnDiagnostic` contains a stable code,
`Error` or `Warning` severity, a human-readable message, and optional
`RegistrationId` and zero-based `PatternSegmentIndex` attribution.

| Code | Meaning | Default severity |
|---|---|---|
| `PTRN1001` | Empty registration identity | Error |
| `PTRN1002` | Duplicate registration identity | Error |
| `PTRN1003` | Duplicate canonical pattern | Error, or Warning with `DuplicatePatternPolicy.Warn` |
| `PTRN1004` | Non-terminal catch-all | Error |
| `PTRN1005` | Duplicate capture name | Error |

Duplicate IDs are always errors. Duplicate-pattern policy is comparer-sensitive:
literal segments use the configured segment comparer and capture names use ordinal
comparison. Warnings remain warnings when `TreatWarningsAsErrors` suppresses
index publication. Compilation never publishes a partial index.

Overlap analysis and rejected-candidate explanations are not part of the phase-01
diagnostic contract. Use the stable report fields rather than parsing messages.
