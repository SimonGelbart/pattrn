# State-of-the-Art Architecture Review — alpha.21

This review records the alpha.21 architecture-hardening decision before beta. It intentionally adds no matcher behavior, parser behavior, route semantics, glob semantics, source generators, analyzers, or framework integration.

## Reviewed goal

The product remains a small generic segmented-pattern index:

```text
registered segmented patterns + incoming segmented path
  -> matching values, captures, specificity metadata, and optional diagnostics
```

The core must stay domain-neutral. Syntax conveniences belong in companion packages or thin extension layers, not in the matcher engine.

## Main finding

The alpha.20 source is technically strong, but the public construction story still presents two competing mental models:

```text
Tokenized segment model:
  Builder("*")
  Add(["orders", "*"], value)

Explicit generic pattern model:
  AddPattern([
      PatternSegment<string>.Literal("orders"),
      PatternSegment<string>.Parameter("id")
  ], value)
```

The explicit `PatternSegment<TSegment>` model is the better pre-beta foundation because it is domain-neutral, does not reserve a segment value, supports named captures, supports terminal catch-alls, and composes naturally with routing/string/glob companion packages.

## Decisions

### 1. `PatternSegment<TSegment>` is the primary registration model

`AddPattern(...)` should become the documentation-first API for the core package.

The core mental model should be:

```text
Literal       -> exact segment
Wildcard      -> anonymous single-segment wildcard
Parameter     -> named single-segment wildcard with capture metadata
CatchAll      -> terminal zero-or-more segment wildcard
```

The tokenized `Add(...)` API remains useful for compact examples and string-oriented convenience, but it should no longer be the first concept users learn.

### 2. Tokenless builder creation should be added before beta

Before alpha.23, the `Builder("*")` shape forced every builder to name a wildcard token even when callers only used explicit pattern segments. That made the legacy model appear more fundamental than it should be.

Alpha.23 introduced tokenless builder creation as the primary construction path:

```csharp
var builder = PattrnIndex<string, Handler>.Builder();
```

A token-enabled factory should remain available for callers that intentionally want tokenized pattern registration:

```csharp
var builder = PattrnIndex<string, Handler>.Builder(wildcardSegment: "*");
```

The implemented API shape follows the design target: callers who use `AddPattern(...)` do not have to reserve a wildcard segment value.

### 3. Tokenized `Add(...)` should be retained as convenience for now

Alpha.23 retained tokenized registration as opt-in convenience. It is terse, already covered by compatibility tests, and useful where a reserved token is acceptable.

Implemented alpha.23 posture:

```text
AddPattern(...)      primary core API
Add(...)             tokenized convenience API
Builder()            primary tokenless factory
Builder("*")         tokenized convenience factory
```

Tokenized convenience can later move to extension methods if the public API still feels too broad after the tokenless builder lands.

### 4. Literal wildcard behavior must be explicit

The docs should keep calling out this distinction:

```csharp
builder.Add(["*"], value);                              // tokenized wildcard, when tokenized mode is enabled
builder.AddPattern([PatternSegment<string>.Literal("*")], value); // literal star
builder.AddPattern([PatternSegment<string>.Wildcard()], value);    // wildcard segment
```

This distinction is the strongest reason to lead with explicit pattern segments.

### 5. No engine refactor is justified in alpha.21

The packed immutable index, adaptive exact-child lookup, iterative traversal, and existing duplicate-heavy detailed-match optimization are adequate for beta preparation. There is no evidence from the handoff benchmarks that a broader algorithm rewrite is needed before the API mental model is simplified.

### 6. Routing allocation is acceptable for preview

Routing string helpers remain convenience APIs. The documented hot path is still pre-split/reused segments feeding the core span APIs. No routing-specific optimization should block beta unless new focused benchmarks show a regression in a target scenario.

### 7. Diagnostics and DI extras stay preview

Diagnostics are useful but should remain preview until diagnostic codes, names, and severities are intentionally frozen. Fluent DI registration is a stable candidate; provider/registration-source extras remain preview convenience.

## Alpha.22 recommendation

This recommendation was implemented in alpha.23. The focused core API simplification increment was:

1. Add tokenless builder creation.
2. Update docs and samples so `AddPattern(...)` is first.
3. Keep tokenized APIs as explicitly named/documented convenience.
4. Ensure tokenless builders do not accidentally reserve `default(TSegment)` or any other segment as a wildcard token.
5. Keep matcher internals and semantics unchanged unless required by the builder API split.
6. Run the full unit test suite and an API snapshot review after the change.
7. Run focused benchmarks only if matcher or compiler code changes.

Alpha.23 completed these items without changing core matching semantics. The next architecture step is the stable matching contract: pattern identity and registration-order metadata.

## Non-decisions

This review does not approve adding route constraints, optional segments, globbing, ASP.NET integration, OpenAPI integration, analyzers, source generators, async APIs, metadata filtering, or domain-specific policy hooks before beta.

## Validation

This alpha.21 review is documentation/design-only. The alpha.20 code baseline was restored, built, and tested offline with the supplied .NET SDK and NuGet cache before these documentation changes were made.
