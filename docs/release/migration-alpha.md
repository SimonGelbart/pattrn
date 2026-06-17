# Alpha migration notes

These notes summarize breaking changes made during the `3.0.0-alpha.*` stabilization line. The alpha line is preview, so breaking changes are expected, but the goal before beta is to make the remaining surface intentional.

## alpha.13 naming cleanup

Preferred `PatternSegment<TSegment>` factories are now:

```csharp
PatternSegment<TSegment>.Literal(value)
PatternSegment<TSegment>.Parameter(name)
PatternSegment<TSegment>.Wildcard()
PatternSegment<TSegment>.CatchAll()
PatternSegment<TSegment>.CatchAll(name)
```

Removed pre-beta aliases:

```csharp
PatternSegment<TSegment>.Any()
PatternSegment<TSegment>.Wildcard
PatternSegment<TSegment>.CatchAllWildcard
```

Use `Wildcard()` for a single-segment wildcard and `CatchAll()` for a terminal zero-or-more segment branch.

## alpha.13 `Try*` semantics

`TryMatch` and `TryMatchDetailed` no longer publish partial output when the destination spans are too small.

When a `Try*` method returns `false`:

- written counts are `0`;
- caller-provided spans should be treated as unchanged by the operation;
- callers must not inspect destination spans for partial results.

Use `GetMatchCountUpperBound(...)` and `GetCaptureCountUpperBound(...)` to size buffers before calling the span-based APIs.

## alpha.14 duplicate naming cleanup

Builder-time structural duplicate policy is now named:

```csharp
DuplicatePatternRegistrationBehavior
UseDuplicatePatternRegistrationBehavior(...)
```

Match-time duplicate value emission is now named:

```csharp
DuplicateValueMatchMode
MatchOptions.DuplicateValueMatchMode
```

These concepts are separate:

- `DuplicatePatternRegistrationBehavior` controls what happens when the same structural pattern is registered again.
- `DuplicateValueMatchMode` controls whether equal values reached during matching are emitted once or repeatedly.

See `docs/reference/duplicate-behavior.md` for detailed examples.

## alpha.15 DI surface cleanup

`PattrnOptions<TSegment, TValue>` is now internal.

`PattrnRegistrationContext<TSegment, TValue>` no longer exposes the mutable builder. Registration sources should contribute registrations through:

```csharp
context.Add(pattern, value);
context.AddPattern(patternSegments, value);
```

This keeps registration sources focused on adding patterns and avoids exposing implementation details of the builder.

## Routing package status

`Pattrn.Routing` remains preview. As of alpha.31 it supports framework-neutral route-template metadata, structural compilation, optional/defaulted suffix expansion metadata through `RouteTemplateExpansion`, and optional route constraint validation for syntax such as:

```text
/orders/{id}
/orders/{id:int}
/orders/{id?}
/reports/{format=json}
/files/{*path}
```

Constraint tokens and defaults are preserved as metadata, and optional/defaulted suffix parameters expand into multiple structural registrations. Constraint validation, URL decoding, ASP.NET Core semantics, OpenAPI semantics, and broader optional route semantics remain intentionally outside the core matcher.



## alpha.23 tokenless builder default

The preferred core construction path is now tokenless:

```csharp
var builder = PattrnIndex<string, string>.Builder();
```

On a tokenless builder, `Add(...)` registers literal-only segments. Use explicit pattern segments for wildcard behavior:

```csharp
builder.AddPattern(
    [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Wildcard()],
    "orders-wildcard");
```

The tokenized convenience path still exists when a caller intentionally reserves a wildcard segment value:

```csharp
var tokenized = PattrnIndex<string, string>.Builder("*");
tokenized.Add(["orders", "*"], "orders-wildcard");
```

Dependency-injection registrations also default to tokenless builders. Keep `UseWildcard("*")` only for tokenized `Add(...)` registrations; omit it when using `AddPattern(...)` or literal-only `Add(...)`.
