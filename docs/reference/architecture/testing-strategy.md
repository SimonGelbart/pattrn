# Testing Strategy

Pattrn uses tests to protect deterministic behavior, public API shape, and package quality.

## Required coverage areas

- core exact matching;
- wildcard, parameter, and catch-all matching;
- captures;
- duplicate structural pattern behavior;
- duplicate value match modes;
- match ordering and specificity metadata;
- diagnostics and explainability separation;
- string normalization helpers;
- route-template parsing and route-layer validation;
- DI registration behavior;
- package metadata and public API snapshots.

## Compatibility posture

Pattrn is pre-beta. Tests may be updated for deliberate alpha breaking changes, but the change should be documented in the roadmap, changelog, or an ADR.

## Benchmarks

Benchmarks are release-gate evidence for performance-sensitive changes. A feature is not done if it creates an unexplained hot-path latency or allocation regression.
