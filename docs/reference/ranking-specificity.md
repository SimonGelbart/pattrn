# Ranking and specificity

Pattrn can return more than one match for the same segmented input. The built-in ordering is deterministic and domain-neutral.

The core does not know about HTTP route precedence, filesystem glob precedence, authorization policy priority, tenant rules, or any other domain-specific ranking policy. Consumers that need a domain order should use detailed match metadata and sort or filter outside the core.

## Built-in ordering

For exact-length matching, the core emits matches in this broad specificity order when multiple branches can match the same input:

```text
literal
  > named parameter
  > anonymous wildcard
  > terminal catch-all
```

A literal segment is more specific than a named parameter. A named parameter is more specific than an anonymous wildcard because it carries capture metadata. An anonymous wildcard is more specific than a terminal catch-all because it consumes exactly one segment instead of the remaining input.

This ordering applies consistently to value-only and detailed match APIs.

## Specificity metadata

Detailed match results expose `Specificity` as an integer comparison key. Higher values are more specific.

The relative ordering is part of the compatibility contract:

```text
literal specificity > parameter specificity > wildcard specificity > catch-all specificity
```

Exact numeric values are implementation details. Do not persist them, compare them to hard-coded constants, or treat them as a serialization format.

Use them for relative comparisons and consumer-side sorting:

```csharp
var selected = index
    .MatchDetailedToArray(path)
    .OrderByDescending(match => match.Specificity)
    .ThenBy(match => match.RegistrationOrder)
    .FirstOrDefault();
```

See [select a best match](../how-to/select-best-match.md) for a task-oriented guide to choosing one result from the detailed match set.

## Registration-order tie-breaker

When two accepted registrations have the same structural specificity, Pattrn preserves registration order.

This matters for duplicate structural registrations when duplicates are preserved:

```csharp
var index = PattrnIndex<string, string>
    .Builder()
    .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
    .Add(["orders", "new"], "first")
    .Add(["orders", "new"], "second")
    .Build(MatchOptions.PreserveDuplicates);
```

For `orders/new`, `first` is emitted before `second`.

The registration-order tie-breaker is also exposed through detailed matches as `RegistrationOrder`.

## Duplicate value mode does not change registration order

`DuplicateValueMatchMode.Deduplicate` controls whether equal values are emitted once during matching. It does not reorder registrations and it does not remove registrations from the compiled index.

When equal values are deduplicated, the first accepted match in deterministic order wins.

Use `DuplicateValueMatchMode.PreserveDuplicates` when auditing or debugging raw accepted registrations.

## Catch-all empty remainder

A terminal catch-all can match an empty remainder. For example, `files/{*path}` can match `files`.

When a catch-all accepts an empty remainder, an exact registration at the same node is still emitted before the catch-all match:

```text
files          // exact registration
files/{*path}  // catch-all accepting an empty remainder
```

The detailed catch-all match has zero captures for the catch-all name when there are no remaining input segments. The deterministic order is compatibility-covered; exact numeric specificity values remain implementation details.

## Prefix mode

Prefix mode allows shorter patterns to match longer inputs. Prefix mode is still deterministic, but it is traversal ordered rather than a global priority queue.

The traversal emits registrations at a prefix node before walking deeper matching branches. Within a node or competing branch at the same depth, the same generic specificity rules apply.

Use detailed match metadata for consumer-side sorting if a prefix scenario needs one global ranking order across all matched prefix and descendant registrations.

This means prefix mode intentionally distinguishes two questions:

| Question | Answer |
|---|---|
| Which registrations were accepted during traversal? | Prefix registrations are emitted as soon as their node is reached. |
| Which accepted registration should a domain prefer globally? | The consumer decides using detailed match metadata and its own value metadata. |

## Route-template boundaries

`Pattrn.Routing` compiles route-template syntax into the generic core model. Literal route segments, parameters, and catch-alls therefore follow the same generic ranking contract as manually registered `PatternSegment<TSegment>` patterns.

Route constraints, default values, and optional metadata do not add core specificity. They are route-layer metadata and validation concerns.

Do not read this contract as ASP.NET Core endpoint-routing precedence. If an application needs framework-specific or domain-specific route priority, it should sort detailed matches outside the core or use a future companion package that explicitly owns that behavior.

## Domain-specific ranking

Do not put domain ranking rules in `Pattrn`.

Examples of ranking that belongs outside the core:

- HTTP route precedence;
- route constraint priority;
- filesystem glob precedence;
- authorization policy priority;
- tenant-specific override order;
- user-defined handler priority.

Companion packages and applications can expose metadata, validate domain-specific rules, or sort detailed matches after the core returns deterministic results.

## Stability posture

Stable before beta:

- deterministic result order;
- broad specificity order;
- registration-order tie-breaking for equal specificity;
- exposed `Specificity` and `RegistrationOrder` metadata.

Not stable before beta:

- exact numeric specificity weights;
- potential future consumer-side helpers around sorting detailed matches;
- domain-specific precedence in companion packages.
