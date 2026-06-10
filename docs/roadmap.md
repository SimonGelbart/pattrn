# Pattrn living beta-readiness roadmap

This roadmap is the product-direction document for `Pattrn` after the alpha.31 performance guardrails and speed triage pass. It promotes the standalone roadmap to the first-class planning source while reflecting the actual API shape that emerged from alpha.23 through alpha.31.

The library is still pre-adoption and pre-beta. Backward compatibility is not a priority in the alpha line. Prefer clear architecture, predictable semantics, and a small long-term API over preserving any current alpha surface.

## North star

`Pattrn` should become a reusable, high-performance segmented-pattern indexing toolkit.

The package should help applications answer this question:

```text
Given a collection of hierarchical patterns and an incoming segmented key,
which registered entries match, why do they match, and how should matches be ranked?
```

The intended model is:

```text
Input:
  registered segmented patterns
  incoming segmented path/key

Output:
  matching values
  captures
  specificity/ranking
  diagnostics, optionally
```

The library should remain useful across domains such as HTTP route matching, filesystem policy matching, message-topic matching, feature-flag targeting, plugin command dispatch, configuration-key matching, namespace/type/member lookup, tenant/resource matching, authorization-path policy lookup, event-stream topic matching, command-line command trees, and resource access rules.

## Product positioning

`Pattrn` is not a framework router, a glob-only library, or a web framework abstraction. It is a generic matching engine with focused companion packages.

The package family should keep this separation:

```text
Pattrn
  generic segmented-pattern matching core

Companion packages
  ergonomic syntax and domain adapters

Consumer applications
  domain semantics, metadata, policy, authorization, and interpretation
```

The core should know about segments, patterns, literals, parameters, wildcards, terminal catch-alls, captures, duplicates, specificity, matching, and optional diagnostics.

The core should not know about HTTP, ASP.NET Core, filesystems, symbols, authorization, tenants, source code, endpoint metadata, business rules, or framework-specific routing semantics. Those belong in companion packages or consuming applications.

## Current status after alpha.31

Alpha.30 confirms that the project is still aligned with the original goal. The work since alpha.22 has moved the library from a useful core matcher into a coherent pre-beta package family:

- alpha.22: consolidated the standalone roadmap into the repository;
- alpha.23: made tokenless builders and explicit `PatternSegment<TSegment>` registration the primary core model;
- alpha.24: added optional pattern identity and deterministic registration-order metadata;
- alpha.25: separated hot matching from diagnostics-oriented explainability;
- alpha.26: established the generic/string normalization boundary;
- alpha.27: added ergonomic string-path builder and index facades;
- alpha.28: added framework-neutral route-template metadata and optional/defaulted suffix expansion;
- alpha.29: added optional route-layer constraint validation above structural matching;
- alpha.30: refreshed the roadmap into this living beta-readiness document and hardened optional/defaulted route expansion metadata through `RouteTemplateExpansion`;
- alpha.31: made speed a first-class roadmap gate, committed the alpha.30 benchmark baseline, added performance guardrails, and protected exact-only matching with direct fast paths.

The current codebase already contains substantial pieces of the long-term product:

- immutable compiled segmented index;
- tokenless default core builder creation;
- explicit `PatternSegment<TSegment>` registrations as the primary core model;
- tokenized segmented registration convenience APIs as opt-in behavior;
- literal, wildcard, named parameter, and terminal catch-all support;
- value-only and detailed match APIs;
- diagnostics-oriented `Explain(...)` APIs;
- optional caller-provided pattern identity in registrations;
- deterministic registration-order metadata in detailed matches;
- capture metadata for named parameters and named catch-alls;
- specificity values in detailed match metadata;
- duplicate value match mode and duplicate pattern registration behavior;
- advisory builder diagnostics and opt-in build validation;
- opt-in rejected-candidate explanation diagnostics;
- string helper package with explicit `StringNormalizationOptions`;
- ergonomic `StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` facades;
- dependency-injection package;
- framework-neutral routing companion package with literal, parameter, terminal catch-all, preserved constraint/default/optional metadata, optional/defaulted suffix expansion metadata, structured diagnostics, and optional route-layer constraint validation;
- benchmark project, committed benchmark snapshots, and explicit performance guardrails;
- offline build/test/pack scripts.

## Compatibility posture

Because the library is not yet used by consumers, alpha compatibility should not constrain design. Breaking changes are acceptable when they make the product simpler, more generic, or more predictable.

Use this rule for alpha work:

```text
If preserving an old alpha API makes the long-term API worse,
break the alpha API and document the migration in the changelog.
```

Compatibility should become meaningful only once a beta/stable contract is intentionally declared. Until then, migration notes are useful, but design correctness wins.

## Core design principles

### 1. Explicit pattern segments first

The primary API should use explicit segment kinds rather than magic string tokens.

Preferred core registration:

```csharp
builder.AddPattern(
    patternId: "users-by-id",
    value: handler,
    segments:
    [
        PatternSegment.Literal("api"),
        PatternSegment.Literal("users"),
        PatternSegment.Parameter("id")
    ]);
```

String-based or tokenized APIs can exist for convenience, but they should compile into explicit pattern segments and should not define the core mental model.

### 2. Separate matching from interpretation

The core can answer:

```text
this pattern matched this path
this value was captured
this match has this specificity
this registration has this identity/order
```

The consumer should decide:

```text
what the value means
whether the match is authorized
whether a route constraint rejects the match
whether domain-specific metadata changes priority
```

### 3. Keep fast paths fast

The main matching path should be optimized for repeated reads against an immutable compiled index. Diagnostics, explanations, rejected-candidate reporting, and normalization notes should be optional because they are more expensive.

Speed is a product constraint. Core span matching and pre-split route matching are protected hot paths. A feature increment is not done if it causes an unexplained hot-path regression or hidden allocation in those protected paths.

### 4. Make behavior predictable

Pattern ordering, duplicate handling, specificity scoring, capture behavior, terminal catch-all behavior, optional route expansion, and tie-breaking should be deterministic, documented, and covered by compatibility tests.

### 5. Keep companion packages thin

Companion packages should translate domain-friendly syntax into the generic core model. They should not force domain concepts back into the core.

## Package-family direction

Current package family:

```text
Pattrn
  Generic segmented index.

Pattrn.Strings
  String path helpers, separators, case sensitivity, normalization, and ergonomic string-path facades.

Pattrn.Routing
  Framework-neutral route-template parser, structural compiler, optional/default expansion metadata, and optional constraint validation.

Pattrn.DependencyInjection
  Registration helpers for Microsoft.Extensions.DependencyInjection.
```

Future package candidates:

```text
Pattrn.Globbing
  Filesystem-style glob pattern support.

Pattrn.Diagnostics
  Optional explainability and validation helpers, if diagnostics grow too large for the core.

Pattrn.AspNetCore
  Optional ASP.NET Core compatibility helpers, only if there is a clear need and a clean boundary.
```

The roadmap originally named `Pattrn.RouteTemplates`. The current implementation is `Pattrn.Routing`. Before beta, decide whether the package name should remain `Routing` or move closer to the roadmap wording. Because alpha compatibility is loose, this can still change if the beta API is clearer.

## Completed increments

| Increment | Status | Notes |
|---|---:|---|
| 1. Core `PatternSegment`-first API | Complete | Tokenless builders are default. Explicit `AddPattern(...)` is the primary core model. |
| 2. Stable matching contract | Complete | Pattern identity, registration order, captures, specificity, and match kind are available in detailed results. |
| 3. Fast matching vs explainability separation | Complete | `Match`, `TryMatch`, and `MatchToArray` remain hot paths. `Explain(...)` is diagnostics-oriented. |
| 4. Generic normalization hooks | Complete | The core remains segmented; string normalization lives in `Pattrn.Strings`. |
| 5. String API ergonomics | Complete | `StringPattrnIndexBuilder<TValue>` and `StringPattrnIndex<TValue>` provide stored-options string workflows. |
| 6. Framework-neutral route-template metadata | Mostly complete | Route templates preserve literals, parameters, constraints, defaults, optionality, catch-alls, and diagnostics. |
| 7. Optional route constraint validation | Complete | Constraint validation is route-layer validation over structural captures, not core matching. |
| 8. Optional/defaulted route expansion hardening | Complete for beta candidate | `RouteTemplateExpansion` links generated structural patterns back to the original template and records omitted parameters. |
| 9. Performance guardrails and speed triage | Complete | Alpha.30 benchmark results are committed, speed gates are documented, and exact-only span/detailed matching has direct fast paths. |

## Beta-readiness roadmap from here

The next work should prioritize stabilization before new feature breadth. Globbing and multi-dimensional composition are still important, but they should wait until the core/string/routing surfaces are harder. Speed remains a release gate for every step below.

### alpha.32 — Specificity and ranking customization

Goal: make match ordering explicit, deterministic, and adaptable.

Recommended scope:

- review the current default specificity model;
- document literal, constrained-parameter, parameter, wildcard, catch-all, and registration-order precedence;
- decide whether the core needs `IPatternSpecificityComparer<TSegment, TValue>`, `SpecificityOptions`, or a smaller hook;
- ensure default ranking remains allocation-conscious;
- add tests for ambiguous and near-ambiguous patterns;
- avoid route-specific precedence in the generic core.

### alpha.33 — Diagnostics and validation hardening

Goal: consolidate diagnostics before adding more companion packages.

Recommended scope:

- review diagnostic models across core, strings, and routing;
- decide which diagnostic codes must be stable for beta;
- document rejected-candidate explanation as opt-in and preview if it remains expensive;
- add validation coverage for duplicate parameter names, ambiguous patterns, unreachable registrations where practical, invalid string options, invalid route syntax, and route constraint failures;
- decide whether `Pattrn.Diagnostics` is warranted or premature.

### alpha.34 — Serialization-friendly registrations

Goal: support persistence without freezing internal compiled index structure.

Recommended scope:

- add stable registration DTOs rather than serializing compiled internals;
- include pattern id, segment kinds, segment values, registration order, and optional metadata key;
- support JSON-friendly roundtrip tests;
- keep compiled trie/index internals private and changeable.

### alpha.35 — Benchmark and performance regression pass

Goal: verify the speed guardrails before beta.

Recommended scope:

- run focused benchmarks after alpha.31's exact-only fast-path protection and later specificity/ranking changes;
- refresh benchmark snapshots after the recent API changes;
- cover small/large indexes, many literals, many parameters, catch-alls, duplicate-heavy cases, string helpers, route helpers, and diagnostic matching;
- document allocation expectations for core, strings, and routing;
- decide whether CI should include light performance smoke checks.

### alpha.36 — Developer experience and sample cleanup

Goal: make the package easy to adopt and hard to misuse.

Recommended scope:

- update all samples to the intended beta-first API style;
- make README examples consistent with explicit-segment-first usage;
- add compact examples for routing with constraint validation and optional/defaulted expansion metadata;
- update migration notes for alpha-era breaking changes;
- verify XML docs are clear enough for package consumers.

### beta.1 — API review / breaking-change cleanup

Goal: intentionally declare the beta feedback surface.

Recommended scope:

- review all public names and overloads;
- remove or rename alpha-era compatibility APIs that make the beta surface worse;
- decide package naming, especially `Pattrn.Routing` versus `Pattrn.RouteTemplates`;
- mark preview surfaces clearly;
- publish beta readiness criteria in the release docs.

## Later roadmap items

These remain aligned with the north star but should not block beta unless a real consumer need appears.

### Globbing companion

Package candidate: `Pattrn.Globbing`.

Support common filesystem/policy glob syntax such as:

```text
*
**
?
*.cs
**/*.cs
**/bin/**
src/**/Controllers/*.cs
```

Keep glob-specific semantics out of the core. Compile into core pattern registrations where possible and isolate any extra glob matcher logic in the companion package.

### Multi-dimensional composition helpers

Support matching by a partition key plus a segmented key without making the core understand the partition meaning.

Examples:

```text
method + path
tenant + resource path
environment + configuration key
permission + resource path
```

Candidate APIs include `PartitionedPatternIndex<TKey, TSegment, TValue>` or `CompositePatternIndex`.

### Optional ASP.NET Core compatibility helpers

Only add an ASP.NET Core package when there is a clear need and a clean boundary. It should adapt framework metadata to the generic route/template layer rather than moving framework behavior into the core.

## Beta readiness criteria

The project is ready for beta when:

- the core API names and overloads are intentionally chosen;
- the string and routing helper APIs are coherent and documented;
- breaking alpha-era overloads are removed, renamed, or explicitly kept;
- specificity and ranking behavior are deterministic and documented;
- diagnostics have a documented stability posture;
- package boundaries are clear;
- samples compile and demonstrate intended usage;
- benchmark baselines are refreshed;
- changelog and migration notes are current;
- release artifacts follow the golden rules: versioned source zip, updated changelog, and roadmap status.

## Explicit non-goals

The package should not become:

```text
a web framework
a full ASP.NET router
a source-code analysis library
a business-rule engine
an authorization framework
a filesystem abstraction
a configuration framework
an AI-agent-specific package
```

It can support these use cases as a lower-level indexing primitive, but it should not own their domain semantics.

## Success criteria

`Pattrn` is successful when:

```text
the core remains small and understandable
matching is fast and allocation-conscious
pattern behavior is deterministic
captures and specificity are easy to inspect
diagnostics are useful but optional
companion packages add ergonomics without polluting the core
consumers can carry their own domain metadata through TValue
framework-specific behavior stays outside the generic core
```
