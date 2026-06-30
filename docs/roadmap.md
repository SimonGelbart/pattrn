# Pattrn Roadmap

## North star

Pattrn is a small, fast, deterministic compiled index for matching hierarchical segmented keys in .NET backend applications and libraries.

Given:

```text
registered segmented patterns
+
an incoming segmented key/path
```

Pattrn answers:

```text
what matched
why it matched
what was captured
how matches are ranked
```

Pattrn should be useful anywhere an application or library needs deterministic matching over structured hierarchical keys without adopting a full framework-specific router, globber, authorization engine, business-rule engine, or filesystem abstraction.

## Product stage

Pattrn is pre-beta and pre-1.0.

The project is not bound by old alpha-era APIs, package metadata, roadmap structure, or historical documentation. Breaking alpha-era choices is acceptable when doing so makes the long-term product smaller, clearer, faster, more deterministic, or easier to maintain.

Roadmap milestones describe product readiness, not package version numbers.

## Product positioning

Pattrn should compete as infrastructure.

The strongest positioning is:

```text
A compiled segmented-pattern index for backend apps and libraries.
```

Pattrn can support routing, globbing, authorization, feature-flag, policy, dispatch, and lookup use cases as a lower-level engine, but the core should not own those domains.

## Target users

Primary users:

- backend application developers;
- infrastructure and platform library authors;
- framework-adjacent library authors;
- developers building dispatch, policy, routing-like, or hierarchical lookup systems.

Representative use cases:

- backend path/key matching;
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
- future filesystem-policy matching through a companion package.

## Product boundaries

The core owns:

- segments;
- pattern segments;
- literals;
- parameters;
- wildcards;
- terminal catch-alls;
- captures;
- duplicate registrations;
- duplicate value behavior;
- deterministic specificity/ranking metadata;
- immutable compiled indexes;
- value matching;
- detailed matching;
- optional diagnostics.

The core does not own:

- HTTP;
- ASP.NET Core endpoint behavior;
- filesystem semantics;
- glob syntax;
- authorization policy semantics;
- tenant semantics;
- business rules;
- OpenAPI;
- source generation;
- analyzers;
- framework-specific route precedence;
- async I/O;
- application dispatch.

Those concerns belong in companion packages or consuming applications.

## Package strategy

### Stable candidate packages for beta

#### `Pattrn`

The generic segmented-pattern matching core.

Responsibilities:

- explicit pattern segment registration;
- immutable compiled indexes;
- value matching;
- detailed matching;
- captures;
- duplicate behavior;
- deterministic ranking metadata;
- optional diagnostics.

#### `Pattrn.Strings`

String-path ergonomics over the core model.

Responsibilities:

- string splitting;
- separators;
- trimming;
- empty segment behavior;
- case sensitivity;
- custom string normalization;
- ergonomic string-path builders and indexes.

#### `Pattrn.DependencyInjection`

Thin backend application integration.

Responsibilities:

- registering compiled indexes as singletons;
- named or keyed index registration;
- keeping DI usage framework-neutral and optional.

### Preview package

#### `Pattrn.Routing`

Routing remains preview until explicitly stabilized.

Responsibilities:

- framework-neutral route-template parsing;
- structural route-template compilation into core pattern segments;
- route metadata preservation;
- optional/defaulted segment expansion;
- route-layer validation.

Non-responsibilities:

- replacing ASP.NET Core endpoint routing;
- implementing endpoint dispatch;
- owning HTTP method semantics;
- enforcing ASP.NET Core route precedence;
- becoming a web framework abstraction.

### Post-1.0 candidates

Post-1.0 candidates should be added only after the core/string/DI product is stable and real user demand exists.

Candidates:

- `Pattrn.Globbing`;
- `Pattrn.AspNetCore`;
- optional advanced diagnostics package;
- analyzers;
- source generators;
- composite or partitioned matching helpers;
- custom ranking extensibility.

## Product principles

### 1. Explicit segments first

The primary model is explicit pattern segment registration.

String and tokenized APIs can exist for convenience, but they should compile into explicit pattern segments and should not define the core mental model.

### 2. Matching is separate from interpretation

The core answers which pattern matched, what was captured, and which specificity metadata applies.

The consumer decides what the value means, whether domain constraints reject the match, and whether domain metadata changes priority.

### 3. Fast paths stay fast

The main matching path optimizes for repeated reads against an immutable compiled index.

Diagnostics, rejected-candidate explanations, route validation, string parsing, and normalization are useful but should remain outside the protected hot path.

### 4. Behavior must be deterministic

The following behavior must be deterministic, documented, and tested:

- match ordering;
- specificity ranking;
- duplicate pattern handling;
- duplicate value handling;
- capture behavior;
- terminal catch-all behavior;
- prefix matching behavior;
- tie-breaking;
- diagnostics stability posture.

### 5. Builders are construction objects

Builders are mutable, single-writer construction objects.

Compiled indexes are immutable snapshots and are safe for concurrent readers.

Applications that load registrations concurrently should collect registrations first, order them deterministically, apply them to a builder from one writer, and then publish the compiled index.

### 6. Companion packages stay thin

Companion packages translate domain-friendly syntax into the generic core model.

They should not force domain concepts back into the core.

### 7. .NET 10 only for now

Pattrn targets .NET 10 for the current product cycle.

Broader target framework support is deferred until the stable product surface is clearer.

### 8. Stabilize before expanding

Do not add globbing, ASP.NET Core integration, source generators, analyzers, custom ranking plugins, or multidimensional matching helpers before the core/string/DI product is beta-ready.

## Roadmap

### 0. Product identity and operating model

Goal: turn Pattrn from a locally grown experiment into a serious infrastructure library with a clear product direction, durable decisions, quality gates, and a living backlog.

Scope:

- rewrite the roadmap around the north star;
- keep current docs focused on current product truth;
- leave discarded alpha-era notes to Git history;
- capture durable decisions as ADRs;
- use GitHub Issues as the living backlog;
- define initial milestones: pre-beta, beta, 1.0, post-1.0;
- define issue labels for area, type, priority, and status.

Exit criteria:

- roadmap is strategic rather than a task dump;
- ADRs capture durable decisions;
- current docs do not expose obsolete alpha-era history;
- GitHub Issues contain the active backlog;
- roadmap, docs, and backlog agree on product direction.

### 1. Core semantics stabilization

Goal: make the core matching model stable enough for beta feedback.

Scope:

- explicit pattern segment API;
- literal behavior;
- named parameter behavior;
- wildcard behavior;
- terminal catch-all behavior;
- exact matching;
- prefix matching;
- capture behavior;
- duplicate structural pattern behavior;
- duplicate value behavior;
- match ordering;
- registration-order tie-breaking;
- builder lifecycle;
- compiled index lifecycle.

Exit criteria:

- core matching semantics are documented;
- tests protect the documented behavior;
- public API shape is reviewed before beta;
- no domain-specific semantics leak into the core;
- no async matching API is introduced;
- no thread-safe mutable builder contract is introduced.

### 2. Ranking and specificity contract

Goal: make match ordering explicit, deterministic, and stable enough for beta feedback.

Scope:

- document built-in precedence;
- document registration-order tie-breaking;
- document duplicate behavior effects on ranking;
- document prefix traversal ordering;
- expose enough metadata for consumer-side sorting;
- keep built-in ranking fixed for beta;
- defer public ranking plugins or custom comparers.

Exit criteria:

- users can predict why one match appears before another;
- detailed match metadata supports consumer-side sorting;
- tests cover ambiguous and near-ambiguous pattern families;
- route-specific precedence does not leak into the generic core.

### 3. Performance gates and benchmark pipeline

Goal: make “fast” a product contract, not a marketing claim, without bloating ordinary CI.

Scope:

- add fast allocation smoke tests for protected hot paths;
- keep full BenchmarkDotNet runs out of ordinary CI;
- split focused benchmark workflows by product area;
- preserve one benchmark artifact layout and summary generator;
- add benchmark coverage for all protected hot paths;
- add `TryMatch` benchmark coverage;
- add `Pattrn.Strings` benchmark coverage or remove claimed string benchmark coverage;
- add a benchmark coverage matrix;
- add CI/PR benchmark comparison integration for full and focused benchmark artifacts (tracked by #70);
- document which results are official performance evidence.

Protected hot paths:

- `Match(..., Span<TValue>)`;
- `TryMatch(...)` when destination is large enough;
- `GetMatchCountUpperBound(...)`;
- detailed matching into caller-provided buffers where applicable;
- pre-split route matching while routing remains preview.

Exit criteria:

- smoke tests block obvious hot-path allocation regressions;
- official benchmark artifacts are the source of current performance evidence;
- benchmark docs match actual benchmark coverage;
- core span hot paths remain allocation-free;
- benchmark workflow can compare candidate results against a known baseline;
- README performance claims are backed by current benchmark artifacts.

### 4. Diagnostics and validation hardening

Goal: make Pattrn trustworthy and explainable without polluting hot paths.

Scope:

- duplicate structural pattern diagnostics;
- duplicate parameter name diagnostics;
- ambiguous pattern-family diagnostics;
- wildcard and catch-all overlap diagnostics where practical;
- invalid catch-all placement;
- invalid string options;
- invalid route syntax in routing package;
- rejected-candidate explanations;
- diagnostic stability policy;
- benchmarks for diagnostic and explanation paths.

Exit criteria:

- diagnostic model is documented;
- expensive explanations remain opt-in;
- diagnostics do not change the value-only hot path;
- important validation cases have tests;
- users can debug surprising matches without guessing.

### 5. Serialization-friendly registrations and deterministic rebuild

Goal: support backend applications that load patterns from configuration, databases, generated files, plugins, or external metadata.

Scope:

- design stable registration DTOs;
- include pattern id;
- include registration order;
- include segment kind and value;
- include parameter name and catch-all marker;
- include optional value key and metadata key;
- support JSON roundtrip;
- document deterministic rebuild;
- document async/concurrent loading followed by single-writer build;
- keep compiled index internals private and non-serialized.

Exit criteria:

- registration DTOs roundtrip through JSON;
- docs show how to rebuild an index from registrations;
- applications can load registrations concurrently and build deterministically;
- compiled index internals remain private.

### 6. Package stabilization

Goal: prepare the beta package set.

Stable candidate scope:

- `Pattrn`;
- `Pattrn.Strings`;
- `Pattrn.DependencyInjection`.

Preview scope:

- `Pattrn.Routing`.

Scope:

- audit package README files;
- align package docs with actual package responsibilities;
- ensure examples compile;
- review package metadata;
- define package stability posture;
- keep companion packages thin;
- keep routing clearly marked as preview.

Exit criteria:

- stable candidate packages have coherent docs and examples;
- package responsibilities are clear;
- routing remains useful but non-blocking;
- no post-1.0 package candidate blocks beta.

### 7. Documentation and samples

Goal: make adoption obvious for backend developers and library authors.

Scope:

- keep README short and adoption-oriented;
- maintain a clear newcomer path;
- polish first-index tutorial;
- add or polish package selection guide;
- add best-match how-to;
- add performance model explanation;
- add use-in-async-apps guide;
- add rebuild-from-registrations guide;
- document limitations honestly;
- add docs link checks if practical;
- ensure samples compile.

Exit criteria:

- new users can understand what Pattrn is in minutes;
- tutorials, how-to guides, reference docs, and explanation docs cover stable candidate APIs;
- documentation does not expose obsolete alpha-era history;
- samples compile;
- limitations are clear.

### 8. Beta feedback surface

Goal: declare the intended beta API surface and invite real-world feedback.

Stable candidate scope:

- `Pattrn`;
- `Pattrn.Strings`;
- `Pattrn.DependencyInjection`.

Preview or non-blocking scope:

- `Pattrn.Routing`.

Excluded from beta:

- async matching APIs;
- thread-safe mutable builder;
- globbing package;
- ASP.NET Core package;
- source generators;
- custom ranking plugin;
- analyzer package;
- multidimensional matching helpers;
- compiled-index serialization format.

Exit criteria:

- beta scope is explicit;
- public API surface is reviewed;
- performance gates are active;
- docs explain stable vs preview packages;
- known limitations are documented;
- GitHub Issues track beta feedback.

### 9. Focused 1.0

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
- dependency-injection registration;
- stable documentation;
- clear limitations;
- benchmark-backed performance posture.

Exit criteria:

- stable APIs are documented and protected;
- stable semantics are tested;
- package boundaries are clear;
- performance claims are backed by current benchmark evidence;
- preview/post-1.0 work does not leak into the stable contract.

## Explicit non-goals

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

## Backlog policy

The roadmap is strategic.

Implementation work should live in GitHub Issues.

Use:

```text
Roadmap = product direction and sequencing
Issues = concrete work items
ADRs = durable decisions
Docs = current user-facing truth
```

Initial milestones:

- pre-beta;
- beta;
- 1.0;
- post-1.0.

Suggested issue label groups:

```text
area:core
area:strings
area:di
area:routing
area:benchmarks
area:docs
area:diagnostics
area:ci
area:serialization

type:feature
type:cleanup
type:docs
type:benchmark
type:decision
type:refactor
type:test

priority:p0
priority:p1
priority:p2

status:needs-design
status:ready
status:blocked
status:deferred
```
