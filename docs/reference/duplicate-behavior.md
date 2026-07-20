# Duplicate behavior

Phase 01 separates structural duplicate diagnostics from duplicate values emitted
by matching.

## Structural patterns

The compiler compares canonical patterns using the configured segment comparer for
literal values and ordinal comparison for capture names. The default
`DuplicatePatternPolicy.Reject` reports `PTRN1003` as an error. `Warn` reports a
warning, and `Allow` compiles without a duplicate-pattern diagnostic. Duplicate
registration IDs are always errors.

```csharp
var result = PattrnIndex<string, string>.CompileWithDiagnostics(
    registrations,
    new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Warn });
```

The builder does not have a second duplicate-policy state machine. It stores the
ordered canonical registrations and delegates compilation and diagnostics to the
static compiler.

## Duplicate values

`MatchOptions` controls only whether equal values are retained:

```csharp
var deduplicating = PattrnIndex<string, string>.Compile(registrations);
var preserving = PattrnIndex<string, string>.Compile(
    registrations,
    new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow },
    MatchOptions.PreserveDuplicates);
```

The default uses `DuplicateValueMatchMode.Deduplicate`; `PreserveDuplicates`
retains every accepted registration. Deduplication uses the configured value
comparer and keeps the first value in deterministic result order.

Exact, best-prefix, and prefix-enumeration methods are independent of these
options. They differ in which candidates they select, not in duplicate policy.
