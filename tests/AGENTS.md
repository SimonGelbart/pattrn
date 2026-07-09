# tests/AGENTS.md

Use this file for unit tests, compatibility tests, package metadata tests, public API snapshots, and test helpers.

## Context to read

Read the root `AGENTS.md` first. Then read the smallest relevant set:

- `docs/reference/validation.md`
- `docs/reference/architecture/testing-strategy.md`
- `docs/reference/coding/csharp.md`
- relevant product docs or ADRs for the behavior under test

## Testing posture

Pattrn tests protect deterministic behavior, public API shape, and package quality. Coverage should stay focused on behavior that matters to the package contract:

- exact, wildcard, parameter, and catch-all matching;
- captures;
- duplicate structural pattern behavior;
- duplicate value match modes;
- match ordering and specificity metadata;
- diagnostics and explainability separation;
- string normalization helpers;
- route-template parsing and route-layer validation;
- DI registration behavior;
- package metadata and public API snapshots.

## Test style

- Follow the existing TUnit-based style and helper assertions.
- Prefer small behavior-focused tests over broad scenario fixtures.
- Add or update tests for meaningful behavior changes.
- If a deliberate pre-beta breaking change requires test updates, document the behavior change in the roadmap, changelog, or an ADR when it affects public behavior.

## Validation

For test-only changes, run the relevant test project or the CI-equivalent test path when feasible. Do not claim validation passed unless the command ran and passed.
