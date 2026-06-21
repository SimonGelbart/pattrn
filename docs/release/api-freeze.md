# API freeze policy

> Historical note: this file preserves earlier alpha-line release notes. Current roadmap and versioning policy live in `docs/roadmap.md` and `docs/adr/0012-simplify-pre-beta-versioning-and-roadmap-milestones.md`.


The `3.0.0` alpha track intentionally reopens the public API because the package is still pre-stable and the product boundary is being clarified.

The current alpha direction is:

- keep the core package segmented, generic, and dependency-free;
- keep route-like string syntax in `Pattrn.Routing`;
- keep string convenience APIs in `Pattrn.Strings`;
- keep dependency injection in `Pattrn.DependencyInjection`;
- preserve span-first low-allocation matching APIs;
- document preview APIs before beta;
- do not add framework-specific behavior to the core.

The current API status is tracked in:

```text
docs/release/api-stabilization.md
```

Before `3.0.0-beta.1`, the project should complete:

1. specificity and ranking customization;
2. a full benchmark report after any matcher/compiler refactor and before beta/RC;
3. final public API snapshot sign-off;
4. final preview/stable labels for core, strings, DI, routing, diagnostics, and duplicate behavior APIs;
5. confirmation that package README mapping is accurate;
6. confirmation that no deferred route-specific features are being pulled into the core package.

The detailed beta checklist is tracked in `docs/release/beta-checklist.md`.

After `3.0.0-rc.1`, public API changes should be limited to bug fixes or changes required by benchmark evidence.

## Alpha.16 status

`3.0.0-alpha.16` completed the final pre-beta API and documentation sweep. It added migration notes, package README mapping, and a beta checklist without changing matcher behavior. The remaining pre-beta blocker is a full benchmark report plus final sign-off on preview/stable labels.

## Alpha.17 note

No public API changes were made in `3.0.0-alpha.17`. The release addresses the alpha.16 benchmark outlier for detailed duplicate-heavy matching.

## Alpha.18 note

`3.0.0-alpha.18` adds route-path caller-buffer split APIs in the preview routing package and keeps core API/semantics unchanged. This is a preview routing-package surface, not a core freeze candidate yet.


## Alpha.19 status

`3.0.0-alpha.19` cleaned generated artifacts and reorganized docs so the source package has a professional layout. It also committed the focused routing benchmark analysis.

## Alpha.20 status

`3.0.0-alpha.20` completed the beta-readiness review for the then-current public API shape.

## Alpha.21 status

`3.0.0-alpha.21` supersedes the direct beta jump with one more architecture-hardening decision: make `PatternSegment<TSegment>` / `AddPattern(...)` the primary core model and add tokenless builder creation before beta. Public API changes after beta should be treated as intentional beta feedback responses, not routine cleanup.


## Alpha.22 status

`3.0.0-alpha.22` consolidated the standalone roadmap and explicitly allowed breaking alpha changes while the package is unused and pre-beta.

## Alpha.23 status

`3.0.0-alpha.23` completed the core construction simplification: tokenless builders are now available and default for core creation, explicit `PatternSegment<TSegment>` registration is the primary model, and tokenized wildcard registration remains opt-in convenience.

## Alpha.24 status

`3.0.0-alpha.24` completed the stable matching-contract increment: registrations can carry optional `PatternId`, and detailed match results expose `PatternId` plus deterministic `RegistrationOrder`.

## Alpha.25 status

`3.0.0-alpha.25` completed the fast matching versus explainability separation increment: value-only `Match`, `TryMatch`, and `MatchToArray` remain the hot path, while `Explain(...)` provides diagnostics-oriented accepted matches and opt-in rejected-candidate hints.

## Alpha.26 status

`3.0.0-alpha.26` completed the generic normalization boundary increment: the core remains an already-segmented matcher, and `Pattrn.Strings` owns explicit `StringNormalizationOptions` for separator, case sensitivity, empty-segment, trimming, and custom string segment normalization policies. `3.0.0-alpha.27` completed the first string ergonomics pass by adding `StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` facades on top of that boundary. `3.0.0-alpha.28` through `3.0.0-alpha.30` expanded and hardened the framework-neutral routing companion. `3.0.0-alpha.31` added performance guardrails and exact-only hot-path protection. The next pre-beta API gap is specificity/ranking customization and route-template naming/maturity decisions.
