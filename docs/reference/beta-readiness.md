# Beta readiness checklist

Pattrn is pre-beta. This checklist defines what must be true before the project should be treated as beta-ready.

Beta readiness does not mean every post-1.0 idea is implemented. It means the stable candidate package surface is clear enough, tested enough, documented enough, and benchmark-backed enough for real user feedback.

## Beta scope

### Stable candidate packages

The following packages are beta candidates:

- `Pattrn`
- `Pattrn.Strings`
- `Pattrn.DependencyInjection`

These packages should have coherent APIs, package metadata, documentation, examples, and validation before beta.

### Preview or non-blocking packages

The following package remains preview and should not block beta unless it breaks the stable candidate package story:

- `Pattrn.Routing`

Routing can remain useful, documented, and benchmarked, but it is not part of the stable beta promise unless a separate decision explicitly changes that.

### Excluded from beta

The following work is explicitly excluded from beta readiness:

- async matching APIs
- thread-safe mutable builders
- `Pattrn.Globbing`
- `Pattrn.AspNetCore`
- source generators
- analyzers
- custom ranking plugins
- multidimensional matching helpers
- compiled-index serialization formats
- framework-specific route precedence in the core

## Required gates

### 1. Product and backlog gate

Beta is not ready until:

- [ ] the roadmap identifies the beta scope;
- [ ] GitHub Issues contain the active pre-beta backlog;
- [ ] beta blockers are labeled clearly;
- [ ] deferred/post-1.0 work is not mixed into beta scope;
- [ ] durable product decisions are captured as ADRs or linked from the relevant issue.

### 2. Public API gate

Beta is not ready until:

- [ ] the public API surface has been reviewed package by package;
- [ ] stable candidate APIs are identified;
- [ ] preview or deferred APIs are identified;
- [ ] intentional breaking changes are completed or tracked;
- [ ] examples and docs use the intended beta APIs;
- [ ] routing remains clearly marked as preview.

### 3. Core semantics gate

Beta is not ready until the following behavior is documented and protected by tests:

- [ ] explicit pattern segment registration;
- [ ] literal matching;
- [ ] named parameter matching;
- [ ] wildcard matching;
- [ ] terminal catch-all matching;
- [ ] exact matching;
- [ ] prefix matching;
- [ ] capture behavior;
- [ ] duplicate structural pattern behavior;
- [ ] duplicate value behavior;
- [ ] match ordering;
- [ ] specificity ranking;
- [ ] registration-order tie-breaking;
- [ ] builder lifecycle;
- [ ] compiled index lifecycle.

### 4. Performance gate

Beta is not ready until:

- [ ] protected hot paths are listed in the benchmark documentation;
- [ ] allocation smoke tests cover protected hot paths;
- [ ] BenchmarkDotNet coverage exists for core protected paths;
- [ ] `TryMatch` benchmark coverage exists;
- [ ] `Pattrn.Strings` benchmark claims match actual benchmark coverage;
- [ ] diagnostic and explanation paths are measured separately from protected hot paths;
- [ ] benchmark documentation explains which artifacts are official performance evidence;
- [ ] benchmark docs do not rely on stale local benchmark output as current proof.

Protected hot paths are:

- `Match(..., Span<TValue>)`
- `TryMatch(...)` when the destination is large enough
- `GetMatchCountUpperBound(...)`
- detailed matching into caller-provided buffers where applicable
- pre-split route matching while routing remains preview

### 5. Diagnostics and validation gate

Beta is not ready until:

- [ ] diagnostics are documented;
- [ ] diagnostic stability posture is defined;
- [ ] expensive explanation paths remain opt-in;
- [ ] diagnostics do not change the value-only hot path;
- [ ] important validation cases have tests;
- [ ] users can debug surprising matches without relying on undocumented behavior.

### 6. Deterministic rebuild gate

Beta is not ready until the project has either implemented or explicitly deferred the registration DTO story.

If implemented before beta:

- [ ] registration DTOs are designed;
- [ ] registration order is represented;
- [ ] segment kind and value are represented;
- [ ] parameter and catch-all metadata are represented;
- [ ] JSON roundtrip tests exist;
- [ ] docs show deterministic rebuild from registrations.

If deferred after beta:

- [ ] the deferral is documented;
- [ ] the roadmap explains what users should do instead;
- [ ] compiled index internals remain private and non-serialized.

In both cases, compiled index internals must not become a public serialization format.

### 7. Package gate

Beta is not ready until:

- [ ] package responsibilities are clear;
- [ ] package README files align with the current product story;
- [ ] package metadata is reviewed;
- [ ] stable candidate packages have coherent examples;
- [ ] `Pattrn.Routing` is marked as preview;
- [ ] post-1.0 package candidates do not block beta.

### 8. Documentation gate

Beta is not ready until docs explain:

- [ ] what Pattrn is;
- [ ] when to use Pattrn;
- [ ] when not to use Pattrn;
- [ ] the stable candidate packages;
- [ ] preview package status;
- [ ] core matching semantics;
- [ ] ranking and specificity;
- [ ] duplicate behavior;
- [ ] diagnostics;
- [ ] performance evidence policy;
- [ ] known limitations;
- [ ] use in applications that also perform async I/O;
- [ ] rebuild or registration-loading guidance, if in beta scope.

### 9. Release hygiene gate

Beta is not ready until:

- [ ] validation commands are documented;
- [ ] CI status is understood;
- [ ] package validation expectations are documented;
- [ ] known limitations are documented;
- [ ] beta feedback should be tracked in GitHub Issues;
- [ ] release notes clearly say the project is beta, not 1.0.

## Beta blocker policy

An issue should be treated as a beta blocker when it affects one of these gates:

- public API stability;
- documented core semantics;
- deterministic behavior;
- protected hot-path performance;
- package scope;
- user-facing correctness;
- docs required for safe beta use.

Issues for post-1.0 expansion, preview-only routing polish, broad integrations, analyzers, source generators, custom ranking, and framework-specific packages should not block beta unless they expose a problem in the stable candidate package surface.

## Done means

Pattrn is beta-ready when:

- stable candidate packages are clear;
- preview/non-blocking packages are clear;
- explicit exclusions are clear;
- public APIs have been reviewed;
- core semantics are documented and tested;
- performance claims are backed by current CI-produced evidence;
- diagnostics and validation behavior are documented;
- package docs and examples align with the intended beta surface;
- known limitations are documented;
- remaining beta feedback can be tracked as normal GitHub Issues.
