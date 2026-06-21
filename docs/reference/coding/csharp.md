# C# Coding Standards

Follow the existing repository style.

## Defaults

- Target .NET 10.
- Keep nullable enabled.
- Treat nullable warnings as errors.
- Prefer immutable public models where practical.
- Keep hot-path allocations visible and intentional.
- Use spans and caller-provided buffers in performance-sensitive matching paths.
- Keep route, string, DI, and diagnostics behavior outside the core unless an ADR changes the boundary.

## Public API

Public API changes require:

- tests;
- public API snapshot updates when applicable;
- documentation updates;
- changelog or roadmap notes when behavior changes.

## Tests

Use the existing TUnit-based test style and helper assertions.
