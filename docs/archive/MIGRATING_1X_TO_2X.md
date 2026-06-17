# Migrating from v1 to v2

`Pattrn` v2 keeps the v1 matching semantics but adds dependency-injection readiness and a companion DI package. Most direct core users can upgrade with minimal code changes. Applications that want DI should switch to the new abstraction and registration extensions.

## What did not change

Core matching behavior is unchanged:

- exact-length matching remains the default;
- prefix matching remains explicit through `MatchOptions.Prefix` or `new MatchOptions(PrefixMatchMode.IncludePrefixPatterns)`;
- the wildcard segment still matches exactly one input segment;
- recursive wildcard `**` is still not supported;
- deduplication remains enabled by default;
- matching remains synchronous by design.

Existing direct construction still works:

```csharp
var index = PattrnIndex<string, string>
    .Builder("*")
    .Add(["market", "NASDAQ", "*"], "client-a")
    .Build();
```

## New core abstraction

v2 introduces `IPattrnIndex<TSegment, TValue>` for application-layer dependency injection and testing seams:

```csharp
public sealed class Consumer(IPattrnIndex<string, Handler> index)
{
    public Handler[] Match(string[] path) => index.MatchToArray(path);
}
```

The concrete `PattrnIndex<TSegment, TValue>` implements this interface. You can still depend on the concrete type in performance-sensitive code when you prefer.

## New DI package

Install or reference the companion package when using Microsoft.Extensions.DependencyInjection:

```text
Pattrn.DependencyInjection
```

Register a default singleton index:

```csharp
services.AddPattrnIndex<string, Handler>(
    wildcardSegment: "*",
    configureBuilder: builder => builder
        .Add(["market", "NASDAQ", "*"], new Handler("client-a"))
        .Add(["market", "*", "MSFT"], new Handler("client-b")));
```

Then inject either the abstraction or concrete implementation:

```csharp
public sealed class MarketConsumer(IPattrnIndex<string, Handler> index)
{
}
```

The DI package registers compiled indexes as singletons. The builder is used only during service registration/building and is not registered as a mutable runtime service.

## Multiple indexes

When an application needs multiple indexes for the same closed generic type, use named indexes:

```csharp
services.AddPattrnIndex<string, Handler>(
    name: "market-data",
    wildcardSegment: "*",
    configureBuilder: builder => builder.Add(["market", "NASDAQ", "*"], new Handler("market")));

services.AddPattrnIndex<string, Handler>(
    name: "events",
    wildcardSegment: "*",
    configureBuilder: builder => builder.Add(["events", "created"], new Handler("event")));
```

Resolve named indexes through the provider abstraction:

```csharp
var indexProvider = serviceProvider.GetRequiredService<IPattrnProvider<string, Handler>>();
var market = indexProvider.GetRequired("market-data");
```

Named indexes are also registered as keyed singleton services for applications that use keyed DI directly.

## Modular registrations

For larger applications, prefer registration sources so multiple modules can contribute patterns to a single index without registering duplicate indexes:

```csharp
public sealed class MarketRoutes : IPattrnRegistrationSource<string, Handler>
{
    public void AddRegistrations(PattrnRegistrationContext<string, Handler> context)
    {
        context.Add(["market", "NASDAQ", "*"], new Handler("market-client"));
    }
}

services.AddPathPatternRegistrationSource<string, Handler, MarketRoutes>();
services.AddPattrnIndex<string, Handler>(options => options.WildcardSegment = "*");
```

## Duplicate DI registrations

v2 fails fast when the same default index or same named index is registered twice for the same closed generic type. If multiple modules need to add patterns, use `IPattrnRegistrationSource<TSegment, TValue>` instead of calling `AddPattrnIndex` multiple times.

## Package split

The core package remains dependency-free. DI helpers live in the companion package:

```text
Pattrn
Pattrn.DependencyInjection
```
