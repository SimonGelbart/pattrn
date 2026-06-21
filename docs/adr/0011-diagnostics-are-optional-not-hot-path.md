# ADR 0011: Diagnostics are optional and not hot-path behavior

## Status

Accepted

## Context

Diagnostics are valuable for explainability, validation, rejected-candidate analysis, and tooling. They are also more allocation-heavy and slower than normal matching.

## Historical context

The implementation already separates hot matching APIs from diagnostics-oriented `Explain(...)` APIs and opt-in rejected-candidate explanations. Builder diagnostics and route diagnostics exist, but normal matching should not require them.

## Decision

Diagnostics remain optional and must not become required for normal matching.

The default matching path should return matches quickly. Diagnostic APIs may collect additional explanations, validation warnings, and rejected candidates when explicitly requested.

## Consequences

Users get useful explainability without paying for it on every match.

Diagnostic stability must be documented separately from core matching stability because diagnostic wording and code maturity may evolve during preview.

## Alternatives considered

- Always compute diagnostics while matching: rejected because it would pollute hot paths.
- Remove diagnostics until beta: rejected because diagnostics are important for trust and adoption.

## Follow-up work

Clarify diagnostic stability posture and important validation cases before beta.
