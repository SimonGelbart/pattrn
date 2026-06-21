# ADR 0002: Target .NET 10 only for the current product cycle

## Status

Accepted

## Context

The project is pre-beta and should optimize for a clear implementation and validation story before broadening compatibility. Multi-targeting would increase build, test, package, and support complexity.

## Historical context

The solution already targets `net10.0` across production projects, tests, samples, and benchmarks. Offline validation artifacts were prepared around the .NET 10 SDK. No current consumer requires older target frameworks.

## Decision

Pattrn targets .NET 10 only for the current pre-beta product cycle.

Do not broaden target frameworks until there is evidence of real user demand and the extra compatibility matrix is worth maintaining.

## Consequences

Build and validation stay simpler. The implementation can use modern .NET and C# capabilities without compatibility shims.

Users on older runtimes cannot consume the packages until a later compatibility decision is made.

## Alternatives considered

- Multi-target .NET Standard or older .NET versions now: rejected as premature support burden.
- Use only APIs available on older frameworks while targeting .NET 10: rejected because it would constrain implementation without actually supporting those frameworks.

## Follow-up work

Revisit target frameworks during beta feedback or before 1.0 only if real consumers ask for it.
