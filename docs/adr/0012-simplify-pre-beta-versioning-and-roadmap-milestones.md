# ADR 0012: Simplify pre-beta versioning and roadmap milestones

## Status

Accepted

## Context

The previous alpha line used package versions and roadmap labels such as `3.0.0-alpha.31` and `alpha.32`. The project is not yet used by real consumers, so this version train adds process overhead without protecting anyone.

## Historical context

Earlier alpha documentation used numbered alpha releases to record progress across API shaping, routing experiments, diagnostics, benchmarks, and package metadata work. Those entries are still useful historical notes, but they should not force the future roadmap to look like a formal release train.

The product roadmap explicitly allows breaking alpha-era choices when preserving them would make the long-term product worse.

## Decision

Simplify pre-beta versioning and roadmap milestones.

- Roadmap milestones use descriptive names rather than alpha release numbers.
- Package versions are centralized in `Directory.Build.props`.
- Package dependency versions are centralized in `Directory.Packages.props`.
- The current pre-beta package line resets to `0.1.0-alpha.1` because no external users depend on the previous alpha versions.
- SemVer, breaking-change, and stable release policies are finalized before beta/1.0, not during foundation cleanup.

## Consequences

The roadmap becomes easier to read and maintain. Documentation can discuss product readiness without repeating package version details.

If a previous alpha package was ever published externally, consumers should be told that the line was reset before beta. Current maintainer understanding is that no real users depend on it.

## Alternatives considered

- Keep `3.0.0-alpha.*`: rejected because it is over-complicated for an unused custom pre-beta project.
- Keep alpha numbers as roadmap milestones but simplify package versions: rejected because it preserves fake release precision in planning docs.
- Move directly to beta: rejected because ADRs, docs, ranking, diagnostics, serialization, benchmarks, and samples still need cleanup.

## Follow-up work

Update roadmap, README, package metadata tests, and packaging docs to distinguish product milestones from package versions.
