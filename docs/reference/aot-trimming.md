# Trimming and Native AOT compatibility

This reference records Pattrn's package-level trimming and Native AOT support posture before beta.

## Support posture by package

| Package | Trimming | Native AOT | Notes |
|---|---:|---:|---|
| `Pattrn` | Supported | Supported | The stable candidate core is dependency-light and does not use reflection, dynamic code generation, or linker-sensitive APIs. |
| `Pattrn.Strings` | Supported | Supported | String helpers use explicit splitting, normalization delegates supplied by callers, and the generic core. |
| `Pattrn.DependencyInjection` | Supported | Supported | The DI package uses `Microsoft.Extensions.DependencyInjection` registrations and keyed-service resolution. Consumers remain responsible for the trim/AOT safety of their own services, registration-source implementations, callbacks, comparers, and value types. |
| `Pattrn.Routing` | Out of scope | Out of scope | Routing remains preview and is intentionally excluded from the stable-package validation harness. |

## Audit summary

The stable candidate packages were audited for reflection APIs, dynamic code generation, linker-sensitive attributes, dependency-originated warnings, and DI/container patterns. The audited stable package code does not call `System.Reflection`, `System.Reflection.Emit`, expression compilation, `Activator`, `MakeGeneric*`, `Assembly` scanning, `RequiresUnreferencedCode`, or `RequiresDynamicCode` APIs. The only stable dependency surface that can affect trim/AOT posture is `Microsoft.Extensions.DependencyInjection` in `Pattrn.DependencyInjection`.

## Validation harness

`tests/Pattrn.AotCompatibility/` is a publishable console application that references the stable candidate projects directly:

- `src/Pattrn/Pattrn.csproj`
- `src/Pattrn.Strings/Pattrn.Strings.csproj`
- `src/Pattrn.DependencyInjection/Pattrn.DependencyInjection.csproj`

The harness does not reference packed NuGet artifacts and does not include `Pattrn.Routing`.

The application smoke-tests representative stable-package behavior:

- core index construction;
- literal, parameter, wildcard, and catch-all matching;
- parameter and catch-all captures;
- light `Explain(...)` diagnostics;
- string builder/factory helpers;
- default and case-insensitive normalization behavior;
- string-path matching;
- default, named, keyed, provider-backed dependency-injection registration and resolution.

The project roots the three stable assemblies for trimming so local validation is not limited only to members directly reached from `Program.cs`.

## Local validation commands

Run the capability-aware scripts from the repository root:

```bash
tests/Pattrn.AotCompatibility/validate-aot.sh
```

```powershell
tests/Pattrn.AotCompatibility/validate-aot.ps1
```

Both scripts attempt these validations for the required RIDs `linux-x64` and `win-x64`:

- trimmed self-contained publish;
- Native AOT publish;
- runtime smoke execution when the host can execute the produced binary.

The scripts write deterministic local outputs under `tests/Pattrn.AotCompatibility/artifacts/`. Generated publish outputs and logs must not be committed.

## Warning policy

Trim and Native AOT warnings from stable Pattrn packages are treated as compatibility failures unless they are explicitly justified and documented. The expected beta support posture requires zero unexplained trim/AOT warnings for `Pattrn`, `Pattrn.Strings`, and `Pattrn.DependencyInjection`.

Accepted warnings must remain visible in documentation or linked follow-up issues with a clear support caveat. CI automation for this validation is tracked separately by issue #73.

## What this proves

Passing validation provides evidence that the stable candidate Pattrn packages can be trimmed and Native AOT published for `linux-x64` and `win-x64` through the local harness without unexplained stable-package trim/AOT warnings, and that the produced executable can run representative smoke behavior when the host can execute it.

## What this does not prove

This validation is package-posture evidence, not a guarantee that every consuming application is trim/AOT safe. Applications must still validate their own code, dependencies, callbacks, custom normalization delegates, DI registration sources, comparers, value types, and hosting stack under their target RIDs and deployment settings.
