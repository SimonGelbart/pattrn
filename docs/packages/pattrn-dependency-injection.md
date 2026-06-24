# Dependency injection

`Pattrn.DependencyInjection` integrates the core index with `Microsoft.Extensions.DependencyInjection`.

The package is pre-beta. The fluent registration model is the stable-candidate surface. Named-index provider and registration-source APIs remain preview until beta.

## Design goals

- Keep DI outside the dependency-free core package.
- Keep registration explicit and fluent.
- Build immutable singleton indexes at startup.
- Support named indexes without making DI concepts part of the core matcher.
- Avoid service-locator style registration contexts.

## Default index

```csharp
services.AddPattrnIndex<string, string>(registration => registration
    .UseMatchOptions(MatchOptions.Prefix)
    .Configure(builder => builder.Add(["market", "NASDAQ"], "client-a")));
```

The compiled index is registered as a singleton for both:

```csharp
PattrnIndex<TSegment, TValue>
IPattrnIndex<TSegment, TValue>
```

## Named indexes

```csharp
services.AddPattrnIndex<string, string>("market-data", registration => registration
    .Configure(builder => builder.AddPattern(
        [PatternSegment<string>.Literal("market"), PatternSegment<string>.Literal("NASDAQ"), PatternSegment<string>.Wildcard()],
        "client-a")));
```

Named indexes are registered through .NET keyed services:

```csharp
var index = provider.GetRequiredKeyedService<IPattrnIndex<string, string>>("market-data");
```

The package also provides a preview convenience provider:

```csharp
var index = provider
    .GetRequiredService<IPattrnProvider<string, string>>()
    .GetRequired("market-data");
```

`IPattrnProvider<TSegment, TValue>` is retained as a preview convenience API for application code that should not depend directly on keyed-service calls.

## Registration sources

Registration sources allow modules to contribute registrations without exposing `IServiceProvider` or the mutable builder through the registration context. Prefer constructor injection for dependencies.

```csharp
public sealed class MarketRoutes(RouteCatalog catalog)
    : IPattrnRegistrationSource<string, Handler>
{
    public void AddRegistrations(PattrnRegistrationContext<string, Handler> context)
    {
        foreach (var route in catalog.Routes)
        {
            context.AddPattern(route.Pattern, route.Handler);
        }
    }
}

services
    .AddPathPatternRegistrationSource<string, Handler, MarketRoutes>()
    .AddPattrnIndex<string, Handler>("market-data", registration => registration
        .FromRegisteredSources());
```

`PattrnRegistrationContext<TSegment, TValue>` exposes:

- `Name`, `IsNamed`, and `IsDefault` for contextual registration;
- `Add(...)` for literal segmented patterns on tokenless builders, or literal/wildcard tokenized patterns after `UseWildcard(...)`;
- `AddPattern(...)` for generic `PatternSegment<TSegment>` registrations.

DI registrations are tokenless by default. Call `UseWildcard(...)` only when you intentionally want tokenized `Add(...)` behavior. The context intentionally does not expose `IServiceProvider`. It also no longer exposes the mutable builder directly. This keeps registration sources focused on contributing patterns rather than configuring index-wide policy.

## Generic pattern-segment registrations from sources

```csharp
public sealed class OrderRoutes : IPattrnRegistrationSource<string, Handler>
{
    public void AddRegistrations(PattrnRegistrationContext<string, Handler> context)
    {
        context.AddPattern(
            [
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Parameter("id")
            ],
            new Handler());
    }
}
```

## Maturity notes

Stable-candidate:

- `AddPattrnIndex(...)` fluent registration.
- singleton compiled index registration.
- named index registration through keyed services.

Preview:

- `IPattrnProvider<TSegment, TValue>`.
- `IPattrnRegistrationSource<TSegment, TValue>`.
- `PattrnRegistrationContext<TSegment, TValue>`.

See [project profile](../reference/project-profile.md) and [roadmap](../roadmap.md) for current package-level stability posture. Historical release notes are available from the [archive index](../archive/README.md).
