# Duplicate behavior

Duplicate handling is split into two intentionally separate concepts:

| Question | API | When it applies |
|---|---|---|
| What happens when the same structural pattern is registered again? | `DuplicatePatternRegistrationBehavior` | Builder registration time |
| What happens when the same value is reached by one or more matching patterns? | `DuplicateValueMatchMode` / `MatchOptions` | Compiled-index match time |

This separation is important. A builder can accept, replace, ignore, or reject duplicate structural registrations before an index exists. A compiled index can then either emit repeated equal values or deduplicate them while matching.

## Structural duplicate registrations

A structural pattern is the shape stored in the index. Parameter names are metadata, not part of structural identity.

These two patterns are structurally equivalent:

```csharp
builder.AddPattern(
    [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("id")],
    "first");

builder.AddPattern(
    [PatternSegment<string>.Literal("orders"), PatternSegment<string>.Parameter("name")],
    "second");
```

They both compile to:

```text
orders / single-segment parameter
```

The builder-time policy controls what the second registration does.

### Append

```csharp
builder.UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append);
```

`Append` keeps all values registered for the structural pattern. It is the default because it preserves the original library behavior and supports dispatch scenarios where one pattern maps to multiple handlers.

### Throw

```csharp
builder.UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Throw);
```

`Throw` rejects the second and later registrations for the same structural pattern. Use it when each pattern should have exactly one value.

### Replace

```csharp
builder.UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Replace);
```

`Replace` removes all previously registered values and metadata for the structural pattern, then stores the new registration. Use it for map-like configuration where the last registration should win.

### Ignore

```csharp
builder.UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Ignore);
```

`Ignore` keeps the first registration and discards later registrations for the same structural pattern. Use it when the first source should win.

## Duplicate values during matching

`DuplicateValueMatchMode` controls whether equal values reached during a match are emitted once or every time they are reached.

```csharp
var deduplicating = builder.Build();

var preserving = builder.Build(
    new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));
```

The default is `DuplicateValueMatchMode.Deduplicate`.

This mode compares emitted values with the builder's configured value comparer. It does not change pattern registration, it does not remove registrations from the index, and it does not add a separate priority policy. When equal values are reached by multiple matches, the first accepted value in deterministic ranking order wins.

## Interaction examples

### Append plus default match deduplication

```csharp
var index = PattrnIndex<string, string>
    .Builder("*")
    .UseDuplicatePatternRegistrationBehavior(DuplicatePatternRegistrationBehavior.Append)
    .Add(["orders", "new"], "handler")
    .Add(["orders", "new"], "handler")
    .Build();
```

The builder stores two registrations. The default match mode emits the equal value once.

### Append plus preserve duplicate values

```csharp
var index = builder.Build(
    new MatchOptions(duplicateValueMatchMode: DuplicateValueMatchMode.PreserveDuplicates));
```

The same value can be emitted more than once if it was registered more than once or reached through overlapping patterns.

### Different patterns with the same value

```csharp
builder.Add(["orders", "new"], "handler");
builder.Add(["orders", "*"], "handler");
```

For input `orders/new`, both patterns match. Default match options emit `handler` once. `PreserveDuplicates` emits it twice.

## Ordering and ranking

Duplicate behavior and ranking are separate. Builder duplicate policies decide which structural registrations are accepted. Match duplicate-value mode decides whether equal values are emitted once or repeatedly. Ranking decides the deterministic order of the accepted matches.

See [ranking and specificity](ranking-specificity.md) for the ordering contract.

## Diagnostics and validation

`GetDiagnostics()` can report structural duplicates and overlaps. Diagnostics are advisory unless `ValidateOnBuild(...)` is enabled.

Use validation when ambiguity should fail index construction:

```csharp
builder.ValidateOnBuild();
```

## Recommended defaults

Use these defaults unless a scenario clearly needs something else:

| Scenario | Recommended policy |
|---|---|
| One pattern can map to many handlers | `DuplicatePatternRegistrationBehavior.Append` |
| Each pattern must have exactly one value | `DuplicatePatternRegistrationBehavior.Throw` |
| Last registration wins | `DuplicatePatternRegistrationBehavior.Replace` |
| First registration wins | `DuplicatePatternRegistrationBehavior.Ignore` |
| Normal read path | `DuplicateValueMatchMode.Deduplicate` |
| Auditing or debugging raw registrations | `DuplicateValueMatchMode.PreserveDuplicates` |

## Preview status

Duplicate behavior APIs remain pre-beta. The current names are intentionally explicit about the difference between registration-time duplicate policy and match-time duplicate value emission:

```text
DuplicatePatternRegistrationBehavior  // builder-time structural duplicate policy
DuplicateValueMatchMode               // match-time duplicate value emission policy
```
