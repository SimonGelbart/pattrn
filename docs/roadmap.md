# Pattrn Product Roadmap

> **Pattrn is a fast, immutable, segmented-pattern index for .NET 10 backend apps and libraries.**
>
> It gives applications deterministic matching, captures, specificity metadata, and optional diagnostics without tying the core to routing, globbing, authorization, filesystems, or framework-specific semantics.

## Status

Pattrn is pre-beta and pre-1.0.

The project is not constrained by old alpha-era APIs or release numbering. Breaking alpha-era APIs, docs, and package metadata is acceptable when doing so makes the long-term product smaller, clearer, faster, or more predictable.

Use this rule during pre-beta work:

```text
If preserving an old alpha API or process makes the long-term product worse,
break the alpha-era choice and document the migration.
```

## Versioning posture

Roadmap milestones describe product readiness, not package-version numbers.

Package versions are centralized in `Directory.Build.props`. Dependency versions are centralized in `Directory.Packages.props`. Until beta, the package version line is intentionally simple and may reset because no real users depend on the previous alpha train.

SemVer, breaking-change, and stable release policies should be finalized before beta/1.0.

## North Star

Pattrn should become a reusable, high-performance segmented-pattern matching toolkit for .NET backend applications and library authors.

It answers this question:

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
  specificity/ranking metadata
  optional diagnostics
```

## Target Users

Primary users:

- backend application developers;
- infrastructure/library authors;
- framework-adjacent library authors;
- developers building dispatch, policy, routing-like, or lookup systems.

Representative use cases:

- backend path and key matching;
- plugin command dispatch;
- message-topic matching;
- event-stream topic matching;
- feature-flag targeting;
- authorization-path policy lookup;
- tenant/resource matching;
- configuration-key matching;
- namespace/type/member lookup;
- command-line command trees;
- route-like matching outside a full web framework;
- filesystem-policy matching later through a companion package.

## Product Positioning

Pattrn should compete as infrastructure.

It should not initially compete by claiming to be a full replacement for ASP.NET Core routing, filesystem globbing, authorization frameworks, or business-rule engines.

The strongest positioning is:

```text
A compiled segmented-pattern index for backend apps and libraries.
```

Pattrn can support routing, globbing, authorization, and policy use cases as a lower-level engine, but it should not own those domains in the core.

## Product Boundaries

The core should know about:

- segments;
- pattern segments;
- literals;
- parameters;
- wildcards;
- terminal catch-alls;
- captures;
- duplicate registrations;
- specificity metadata;
- deterministic matching;
- optional diagnostics.

The core should not know about:

- HTTP;
- ASP.NET Core;
- filesystems;
- endpoint metadata;
- authorization policy semantics;
- tenants;
- business rules;
- OpenAPI;
- source-code analysis;
- framework-specific route behavior.

Those concerns belong in companion packages or consuming applications.

## Package Strategy

### Stable-scope packages for beta and 1.0

#### `Pattrn`

The core package.

Responsibilities:

- generic segmented-pattern matching;
- explicit `PatternSegment<TSegment>` registration;
- immutable compiled index;
- fast value matching;
- detailed matching;
- captures;
- deterministic ranking metadata;
- duplicate behavior;
- optional diagnostics.

#### `Pattrn.Strings`

String-path ergonomics.

Responsibilities:

- string splitting;
- separators;
- case sensitivity;
- trimming;
- empty segment behavior;
- custom string normalization;
- ergonomic string-path builders and indexes.

#### `Pattrn.DependencyInjection`

Backend application integration.

Responsibilities:

- registering compiled indexes as singletons;
- named/keyed index registration;
- keeping DI usage thin and framework-neutral.

### Preview package

#### `Pattrn.Routing`

Routing remains important, but it should not block the first stable product.

Responsibilities:

- framework-neutral route-template parsing;
- structural route-template compilation;
- route metadata preservation;
- optional/defaulted segment expansion;
- route-layer validation.

Non-responsibilities:

- replacing ASP.NET Core endpoint routing;
- implementing endpoint dispatch;
- owning HTTP method semantics;
- enforcing framework-specific route precedence;
- becoming a web framework abstraction.

### Post-1.0 candidates

- `Pattrn.Globbing`
- `Pattrn.AspNetCore`
- optional advanced diagnostics package if diagnostics outgrow the core;
- composite or partitioned matching helpers;
- ranking extensibility only after real demand appears.

## Product Principles

### 1. Explicit segments first

The primary model should use explicit pattern segment kinds.

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

String and tokenized APIs can exist for convenience, but they should compile into explicit pattern segments and should not define the core mental model.

### 2. Matching is separate from interpretation

The core can answer which pattern matched, what was captured, and which specificity metadata applies. The consumer decides what the value means, whether a domain constraint rejects the match, and whether metadata changes priority.

### 3. Fast paths stay fast

The main matching path should optimize for repeated reads against an immutable compiled index.

Diagnostics, rejected-candidate explanations, route validation, and normalization notes should be optional because they are more expensive.

### 4. Behavior must be deterministic

The following behavior must be deterministic, documented, and tested:

- match ordering;
- specificity ranking;
- duplicate pattern handling;
- duplicate value handling;
- capture behavior;
- terminal catch-all behavior;
- optional/defaulted route expansion;
- tie-breaking;
- diagnostics stability posture.

### 5. Companion packages stay thin

Companion packages should translate domain-friendly syntax into the generic core model.

They should not force domain concepts back into the core.

### 6. .NET 10 only for now

Pattrn should target .NET 10 only for the current product cycle.

### 7. Stabilize before expanding

Do not add globbing, ASP.NET Core integration, source generators, analyzers, or custom ranking plugins before the core/string/DI product is beta-ready.

## Ranking Strategy

Ranking should use:

```text
fixed built-in deterministic ranking
+
exposed metadata for consumer-side custom sorting
```

Avoid a public ranking plugin or comparer before beta. Advanced users can sort detailed matches themselves using exposed metadata.

A configurable ranking comparer or `SpecificityOptions` type can be added later if real users need it.

## Roadmap

### Project foundation and ADRs

Goal: make product direction, workflow, and historical choices explicit before stabilizing more APIs.

Scope:

- create `docs/adr/` and record historical product-boundary decisions;
- add durable repository workflow/reference docs;
- align README and roadmap with product positioning;
- simplify pre-beta versioning and centralize package/dependency versions;
- remove stale documentation that treats local execution mechanics as product policy;
- link or archive historical architecture review docs.

Exit criteria:

- ADR index exists;
- major architectural choices have status, context, decision, consequences, and historical context;
- README and roadmap agree on product positioning;
- validation commands and package metadata reflect the real repository.

### Ranking and specificity contract

Goal: make match ordering explicit, deterministic, and stable enough for beta feedback.

Scope:

- document literal, parameter, wildcard, and catch-all precedence;
- document registration-order tie-breaking;
- document duplicate structural pattern behavior and duplicate value behavior;
- document capture behavior and detailed match metadata;
- show consumer-side custom sorting using metadata;
- keep built-in ranking fixed for beta;
- do not add a public ranking comparer yet.

Exit criteria:

- ranking rules are documented;
- ambiguous and near-ambiguous pattern tests exist;
- default ranking remains allocation-conscious;
- no route-specific precedence leaks into the generic core.

### Internal architecture cleanup

Goal: keep the engine fast while making the implementation easier to reason about.

Candidate internal separations:

- exact-only matcher;
- wildcard/catch-all matcher;
- prefix matcher;
- detailed match collector;
- capture collector;
- capture counter;
- rejected-candidate explainer;
- specificity/ranking helper;
- child lookup helper.

Exit criteria:

- public API remains stable unless a deliberate pre-beta breaking change is chosen;
- hot paths remain fast;
- matching internals are easier to test and audit.

### Diagnostics and validation hardening

Goal: make Pattrn trustworthy and explainable without polluting hot paths.

Scope:

- duplicate structural patterns;
- duplicate parameter names;
- ambiguous pattern families;
- unreachable registrations where practical;
- invalid catch-all placement;
- invalid string options;
- invalid route syntax;
- route constraint failures;
- rejected-candidate explanations.

Exit criteria:

- diagnostic model is documented;
- diagnostic stability posture is explicit;
- important validation cases have tests;
- expensive explanations remain opt-in.

### Serialization-friendly registrations

Goal: support backend apps that load patterns from configuration, databases, generated files, or external metadata.

Scope:

- add stable registration DTOs rather than serializing compiled internals;
- include pattern id, segment kind, segment value, parameter name, catch-all marker, registration order, optional value key, and optional metadata key;
- document how to rebuild an index from registrations.

Exit criteria:

- registration DTOs roundtrip through JSON;
- docs show how to rebuild an index from registrations;
- compiled index internals are not serialized.

### Benchmark and CI hardening

Goal: make product quality measurable.

Scope:

- refresh benchmark baselines;
- document allocation expectations;
- strengthen CI with analyzers, public API checks, package validation, test coverage reporting, docs link checks, and light allocation/performance smoke checks where practical.

Exit criteria:

- protected hot paths remain within guardrails;
- CI verifies build, tests, package metadata, and public API stability;
- release criteria are measurable.

### Documentation and samples

Goal: make adoption obvious for backend developers and library authors.

Scope:

- organize docs using Diataxis;
- keep README short, accurate, and adoption-oriented;
- ensure samples compile and use intended beta-first API style;
- archive or clearly mark alpha-era docs as historical;
- keep migration notes current.

Exit criteria:

- tutorials, how-to guides, reference docs, and explanation docs cover the stable candidate APIs;
- samples compile;
- stale docs are reconciled or archived.

### Beta feedback surface

Goal: declare the intended beta API surface and invite real-world feedback.

Stable candidate scope:

- `Pattrn`;
- `Pattrn.Strings`;
- `Pattrn.DependencyInjection`.

Preview or non-blocking scope:

- `Pattrn.Routing`.

Excluded from beta:

- globbing;
- ASP.NET Core package;
- source generators;
- custom ranking plugin;
- analyzer package;
- async APIs;
- multidimensional matching helpers.

### Focused stable release

Goal: ship a stable, focused, trustworthy matching toolkit.

Stable 1.0 scope:

- core segmented-pattern matching;
- explicit pattern segment API;
- immutable compiled indexes;
- value matching;
- detailed matching;
- captures;
- deterministic ranking metadata;
- duplicate behavior;
- string path ergonomics;
- DI registration;
- stable documentation;
- clear limitations.

## Explicit Non-Goals

Pattrn should not become:

- a web framework;
- a full ASP.NET Core router;
- an authorization framework;
- a business-rule engine;
- a filesystem abstraction;
- an OpenAPI engine;
- a configuration framework;
- a source-code analysis library;
- an AI-agent-specific package.

It can support these domains as a lower-level indexing primitive, but it should not own their semantics.
