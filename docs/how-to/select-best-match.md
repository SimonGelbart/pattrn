# Select a best match

Pattrn returns matches in deterministic built-in order. Many applications can use the first match directly:

```csharp
var first = index.MatchToArray(path).FirstOrDefault();
```

Use detailed matches when the application needs to explain the decision, inspect captures, or apply domain metadata after the generic matcher has run.

## Use the built-in order

For exact-length matching, the built-in order is already the generic best-match order:

```text
literal
  > named parameter
  > anonymous wildcard
  > terminal catch-all
  > registration order for equal specificity
```

Example:

```csharp
var match = index.MatchDetailedToArray(["orders", "new"]).FirstOrDefault();
```

This keeps the core domain-neutral. The result is the best generic pattern match, not the best HTTP route, filesystem glob, authorization rule, or tenant override.

## Sort detailed matches explicitly

When code needs an explicit selection step, sort detailed matches by the exposed metadata instead of depending on numeric specificity constants:

```csharp
var selected = index
    .MatchDetailedToArray(path)
    .OrderByDescending(match => match.Specificity)
    .ThenBy(match => match.RegistrationOrder)
    .FirstOrDefault();
```

`Specificity` should be used as a relative comparison key. Do not persist it, compare it to hard-coded numbers, or treat it as a serialized format.

## Keep domain priority outside Pattrn

Applications can carry priority in their own values or metadata and apply it after matching:

```csharp
public sealed record HandlerRegistration(
    string Name,
    int DomainPriority,
    Func<IReadOnlyDictionary<string, string>, ValueTask> HandleAsync);

var selected = index
    .MatchDetailedToArray(path)
    .OrderByDescending(match => match.Value.DomainPriority)
    .ThenByDescending(match => match.Specificity)
    .ThenBy(match => match.RegistrationOrder)
    .FirstOrDefault();
```

Use this pattern for domain rules such as HTTP route precedence, authorization priority, tenant overrides, or feature-flag priority.

## Prefix matching

Prefix matching is deterministic but traversal ordered. Prefix registrations are emitted before deeper descendant registrations:

```text
api
api/orders
api/orders/new
```

For prefix scenarios that need one global best-match result, choose and document the application rule. For example, store the registered pattern length in the value when the domain wants longest-prefix-first behavior:

```csharp
public sealed record HandlerRegistration(
    string Name,
    int PatternLength,
    int DomainPriority);

var longestPrefixThenSpecificity = index
    .MatchDetailedToArray(path)
    .OrderByDescending(match => match.Value.PatternLength)
    .ThenByDescending(match => match.Specificity)
    .ThenBy(match => match.RegistrationOrder)
    .FirstOrDefault();
```

If the public API does not expose exactly the metadata your domain needs, store it in the value registered with the pattern.

## Preserve duplicates for auditing

Default matching deduplicates equal values. That is usually the right behavior for dispatch, but it can hide which structural pattern produced the selected value.

Use `MatchOptions.PreserveDuplicates` while auditing ranking or duplicate behavior:

```csharp
var index = builder.Build(MatchOptions.PreserveDuplicates);
var allAccepted = index.MatchDetailedToArray(path);
```

Switch back to default match options when the application only wants each equal value once.
