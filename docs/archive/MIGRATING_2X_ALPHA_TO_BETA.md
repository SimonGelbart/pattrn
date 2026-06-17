# Migrating from v2 alpha to v2 beta

`v2.0.0-beta.1` starts the v2 beta track. The core matching behavior did not change, but dependency-injection duplicate handling is now stricter.

## Duplicate DI registrations now fail fast

In the alpha line, repeated default or named index registrations for the same closed generic type silently kept the first registration.

In beta.1, duplicate registrations throw `InvalidOperationException` during service registration.

Before:

```csharp
services.AddPattrnIndex<string, Handler>("*", builder => builder.Add(["market", "*"], handler));
services.AddPattrnIndex<string, Handler>("*", builder => builder.Add(["admin", "*"], handler)); // ignored in alpha
```

After:

```csharp
services.AddPathPatternRegistrationSource<string, Handler, MarketRoutes>();
services.AddPathPatternRegistrationSource<string, Handler, AdminRoutes>();

services.AddPattrnIndex<string, Handler>(options => options.WildcardSegment = "*");
```

Use registration sources when multiple modules need to contribute to the same compiled index. Use named indexes when the application needs multiple independent indexes with the same generic type.

## Named indexes

Register each named index once:

```csharp
services.AddPattrnIndex<string, Handler>(
    name: "market-data",
    configureOptions: options => options.WildcardSegment = "*");
```

If several modules need to contribute to the `market-data` index, register multiple `IPattrnRegistrationSource<string, Handler>` implementations and branch on `context.Name` when needed.

## Matching APIs

No changes are required for matching code. `IPattrnIndex<TSegment, TValue>`, `PattrnIndex<TSegment, TValue>`, `MatchOptions`, span matching, dotted helpers, and separated helpers keep the same semantics as the v2 alpha line.
