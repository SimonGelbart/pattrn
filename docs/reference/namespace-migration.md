# Namespace migration

The pre-beta namespace cleanup separates public contracts from implementation
details. Assemblies and package references remain unchanged; consumers only
need to update imports and fully qualified type names.

## Core mappings

| Previous root namespace | Current namespace |
|---|---|
| `Pattrn.PatternSegment*` | `Pattrn.Patterns` |
| `Pattrn.PatternCapture*` | `Pattrn.Patterns` |
| `Pattrn.PattrnRegistration`, `Pattrn.RegistrationId` | `Pattrn.Registrations` |
| `Pattrn.PattrnIndex`, `Pattrn.PatternMatch*`, `Pattrn.MatchOptions` | `Pattrn.Matching` |
| `Pattrn.PattrnIndexBuilder` | `Pattrn.Builders` |
| diagnostics, explanations, and rejected-candidate types | `Pattrn.Diagnostics` |
| implementation-only core types | `Pattrn.Internal.*` |

## Companion mappings

- String normalization options and policies are in `Pattrn.Strings.Normalization`;
  string facades and extensions remain in `Pattrn.Strings`.
- DI provider and service extensions remain in `Pattrn.DependencyInjection`;
  registration-source contracts and fluent registration types are in
  `Pattrn.DependencyInjection.Registration`.

For a typical core consumer, replace `using Pattrn;` with:

```csharp
using Pattrn.Builders;
using Pattrn.Matching;
using Pattrn.Patterns;
```

Add `Pattrn.Diagnostics`, `Pattrn.Registrations`, or the relevant companion
namespace when those contracts are used. No compatibility aliases are kept in
the old root namespace because this is an intentional pre-beta breaking
cleanup.
