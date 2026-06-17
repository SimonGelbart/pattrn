# Migrating from 2.1.0 to 2.2.0-alpha.4

`2.2.0-alpha.4` intentionally includes breaking core API cleanup while the library has no external consumers.

## Count properties

`ValueCount` was renamed because it described pattern/value registrations, not distinct values.

Before:

```csharp
var buffer = new string[index.ValueCount];
```

After:

```csharp
var buffer = new string[index.MatchCountUpperBound];
```

Use `RegistrationCount` when you want the total number of compiled pattern/value registrations. Use `MatchCountUpperBound` when sizing a destination span for `Match` or `TryMatch`.

## Match options

`MatchOptions` now uses enum-based modes.

Before:

```csharp
var prefix = new MatchOptions(includePrefixMatches: true);
var preserveDuplicates = new MatchOptions(deduplicateValues: false);
var both = new MatchOptions(includePrefixMatches: true, deduplicateValues: false);
```

After:

```csharp
var prefix = MatchOptions.Prefix;
var preserveDuplicates = MatchOptions.PreserveDuplicates;
var both = new MatchOptions(
    PrefixMatchMode.IncludePrefixPatterns,
    DuplicateValueMatchMode.PreserveDuplicates);
```

The convenience properties `IncludePrefixMatches` and `DeduplicateValues` remain available for inspection, but new configuration code should prefer `PrefixMatchMode` and `DuplicateValueMatchMode`.

## Matching behavior

Matching semantics did not change:

- exact-length matching remains the default;
- prefix matching remains opt-in;
- single-segment wildcard behavior is unchanged;
- deduplication remains enabled by default;
- duplicate preservation remains opt-in.

## Exact-only read-path specialization

`2.2.0-alpha.4` adds an internal fast path for compiled indexes that do not contain wildcard branches. This does not require source changes and does not change matching semantics.
