# Compatibility semantics

This document records behavior that should not change accidentally while Pattrn moves toward beta.

The project is still pre-stable, so intentional breaking changes are allowed. However, any change to the semantics below should be reviewed as a product decision, not an incidental implementation side effect.

## Product boundary

The core package remains a generic segmented-pattern index. It does not parse route strings, decode URLs, join catch-all captures into strings, enforce route constraints, or implement framework-specific policies.

Route-like syntax belongs in `Pattrn.Routing` and should remain a thin translation layer over the generic core.

## Match ordering

When multiple structural branches match the same input at the same depth, detailed and value-only results should remain deterministic:

```text
literal
  > named parameter
  > anonymous wildcard
  > terminal catch-all
```

This ordering follows the compiled traversal: exact literal children are visited before wildcard/parameter children, and catch-all branches are emitted last for the same node.

`PatternMatch<TValue>.Specificity` and `PatternMatchResult<TSegment, TValue>.Specificity` expose the generic specificity value used to compare these broad categories. Higher values are more specific. The current numeric weights are implementation details, but the ordering above is compatibility-covered. See [ranking and specificity](ranking-specificity.md) for the full contract.

## Captures

Named captures are emitted in pattern order.

For a named single-segment parameter, one capture is emitted with the input segment value and its zero-based input segment index.

For a named terminal catch-all, one capture is emitted for each remaining input segment. The core does not concatenate these values and does not apply string or route normalization.

A terminal catch-all can match an empty remainder. In that case the detailed match is emitted with zero captures for the catch-all name.

## Duplicate-pattern behavior

`DuplicatePatternRegistrationBehavior` is a registration-time builder policy:

| Behavior | Compatibility expectation |
|---|---|
| `Append` | Keep existing registrations and append the new value. |
| `Throw` | Reject a structurally duplicate pattern during registration. |
| `Replace` | Replace existing registrations for the structural pattern. |
| `Ignore` | Keep existing registrations and ignore the new duplicate value. |

This is separate from `DuplicateValueMatchMode`, which controls duplicate values emitted by a built index during matching. Default deduplication keeps the first accepted value in deterministic rank order.

## Routing package preview semantics

`Pattrn.Routing` is preview. Its API may change before beta, but current behavior should not change accidentally.

The routing package supports:

```text
/literal/{parameter}/{*catchAll}
{id:int}
{id?}
{id=default}
```

Current preview behavior includes:

- literal route segments;
- named parameters;
- terminal named catch-alls;
- preserved route constraints, defaults, and optional metadata;
- optional/defaulted suffix expansion through `RouteTemplateExpansion`;
- route-layer constraint validation over structural captures.

Invalid route shapes such as a non-terminal catch-all remain invalid:

```text
/files/{*path}/tail
```

The routing package does not URL-decode, case-fold, collapse duplicate separators, join catch-all captures, implement HTTP method semantics, or claim ASP.NET Core endpoint-routing compatibility.

Route constraints, defaults, and optional metadata are not core ranking inputs. Structural route matches follow the same literal, parameter, wildcard, catch-all, and registration-order rules as the generic core. Route-layer validation can reject or diagnose a structural match, but it does not create framework-specific route precedence in `Pattrn`.
