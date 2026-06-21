# Dependency Policy

## Core

The core package should stay dependency-light and framework-neutral.

Do not add dependencies to `Pattrn` unless they are clearly necessary for the generic segmented-pattern engine.

## Companion packages

Companion packages may reference dependencies that match their boundary:

- `Pattrn.DependencyInjection` may reference Microsoft dependency-injection abstractions.
- `Pattrn.Strings` should avoid framework dependencies unless needed for string normalization ergonomics.
- `Pattrn.Routing` should remain framework-neutral and must not depend on ASP.NET Core routing behavior.

## Tests and benchmarks

Test and benchmark dependencies are centralized in `Directory.Packages.props`.

Update package versions centrally. Do not put package versions on individual `PackageReference` items unless there is a documented exception.
