# Select a best match

Pattrn returns matches in deterministic built-in order. Many applications can use the first match directly:

```csharp
var first = index.MatchValuesToArray(path).FirstOrDefault();
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

## Select from the deterministic result order

When code needs an explicit selection step, select from the returned order. The matcher already applies structural specificity and registration order internally:

```csharp
var selected = index.MatchDetailedToArray(path).FirstOrDefault();
```

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
    .FirstOrDefault();
```

Use this pattern for domain rules such as HTTP route precedence, authorization priority, tenant overrides, or feature-flag priority.

## Prefix matching

Best-prefix matching returns only the deepest accepted prefix. Use prefix enumeration when every accepted level is needed:

```text
api
api/orders
api/orders/new
```

For best-prefix scenarios, use the dedicated API directly:

```csharp
var longestPrefix = index.MatchPrefixToArray(path).FirstOrDefault();
var everyPrefix = index.EnumeratePrefixMatchesToArray(path);
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
