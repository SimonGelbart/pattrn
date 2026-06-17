# Migration from the starter-kit SubscriptionTree model

The starter kit included a reference implementation around market-data subscriptions. `Pattrn` generalizes that idea into a clean .NET 10 library.

The reference code should remain reference material only. Do not copy proprietary source into the library.

## Concept mapping

| Starter-kit concept | Pattrn equivalent |
| --- | --- |
| `SubscriptionTree` | `PattrnIndexBuilder<TSegment, TValue>` plus compiled `PattrnIndex<TSegment, TValue>` |
| `MessageRouter` | Caller-owned dispatch code using matched values |
| `MessageTypeId` / path fields | `TSegment` sequence |
| `ClientId` | `TValue`, for example `ClientId`, `string`, handler, or metadata object |
| implicit prefix subscription behavior | explicit `MatchOptions.Prefix` |
| domain-specific wildcard handling | configured wildcard segment, for example `"*"` |

## Before: domain-specific subscription tree

Conceptually, the reference flow was:

```csharp
var tree = new SubscriptionTree();
tree.Subscribe(clientId, messageTypeId);
var clients = tree.Match(messageTypeId);
```

That shape tied the structure to one domain and made prefix behavior implicit.

## After: generic path-pattern index

```csharp
var builder = PattrnIndex<string, string>.Builder("*");

builder
    .Add(["NASDAQ"], "market-level-client")
    .Add(["NASDAQ", "MSFT"], "symbol-client")
    .Add(["*", "MSFT"], "all-msft-client");

var index = builder.Build(MatchOptions.Prefix);

var clients = index.MatchToArray(["NASDAQ", "MSFT"]);
```

Use `MatchOptions.Prefix` only when you intentionally want the starter-kit-compatible behavior where a shorter pattern can match a longer path.

## Recommended migration steps

1. Convert each domain message key into a segment sequence.
2. Choose a wildcard segment value, usually `"*"` for string segments.
3. Store the domain payload as `TValue`. This can be a client ID, handler, rule object, or metadata record.
4. Register all patterns through `PattrnIndexBuilder<TSegment, TValue>`.
5. Compile once with `Build(...)`.
6. Share the compiled `PattrnIndex<TSegment, TValue>` between readers.
7. Rebuild and swap the index when subscriptions change.

## Updating live subscriptions

The compiled index is immutable. For live systems, use copy/rebuild/swap semantics:

```csharp
var builder = PattrnIndex<string, string>.Builder("*");

// add current subscriptions
var index = builder.Build(MatchOptions.Prefix);

// later: mutate a builder or create a new one, then publish the new immutable snapshot
builder.Add(["NASDAQ", "AAPL"], "client-aapl");
var nextIndex = builder.Build(MatchOptions.Prefix);

Volatile.Write(ref index, nextIndex);
```

Readers can use the immutable index concurrently without locking. Coordinate publication of new snapshots in the calling application.

## Dedupe behavior

Deduplication is enabled by default. If the same client or handler is registered through overlapping exact and wildcard patterns, the default match result includes it once.

Disable deduplication only when duplicate registrations are meaningful:

```csharp
var index = builder.Build(MatchOptions.PreserveDuplicates);
```

## Prefix behavior migration warning

The most important semantic difference from the starter kit is that prefix matching is not implicit.

```text
pattern: NASDAQ
path:    NASDAQ.MSFT
default: no match
prefix:  match
```

This is intentional. A general-purpose library should not surprise users by treating shorter patterns as subscriptions to all deeper paths unless the caller explicitly asks for that behavior.
