# Pattrn.Routing

`Pattrn.Routing` is an optional companion package for route-like string templates.

It keeps route syntax out of the core package by translating templates into the generic `PatternSegment<string>` model used by `Pattrn`.

The package is framework-neutral. It is not an ASP.NET Core router, an OpenAPI path-template implementation, or a URL normalizer. Constraint validation is available as an optional routing-layer step after structural matching; it is not part of the generic core matcher.

## Supported syntax

```text
/orders/{id}
/orders/{id:int}
/orders/{id:int:min(1)}
/orders/{id?}
/reports/{format=json}
/customers/{customerId}/orders/{orderId}
/files/{*path}
```

Supported segment kinds and metadata:

| Syntax | Meaning |
|---|---|
| `orders` | literal segment |
| `{id}` | named single-segment parameter |
| `{id:int}` | named parameter with preserved `int` constraint metadata |
| `{id:int:min(1)}` | named parameter with multiple preserved constraints |
| `{id?}` | optional suffix parameter expanded into an omitted variant |
| `{format=json}` | defaulted suffix parameter expanded into an omitted variant and preserving the default text |
| `{*path}` | terminal named catch-all |

The `*` character outside braces is treated as a literal route segment. This avoids reintroducing a reserved wildcard token into route templates.

## Boundary

Route templates compile structurally into the generic core matcher:

```text
/orders/{id:int}
  -> Literal("orders"), Parameter("id")
```

The constraint token `int` is preserved on `RouteTemplate` metadata, but it is not evaluated by `Pattrn`. Constraint validation is optional and runs after structural matches and captures are available. It remains in the routing companion package and never changes the generic core matcher.

The routing package also does not URL-decode, join catch-all captures, apply endpoint metadata, infer HTTP methods, or implement ASP.NET Core route precedence.

## Parsing route metadata

```csharp
using Pattrn.Routing;

var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1)}");
var parameter = template.Segments[1].Parameter!;

Console.WriteLine(parameter.Name);                  // id
Console.WriteLine(parameter.Constraints[0].Name);   // int
Console.WriteLine(parameter.Constraints[1].Name);   // min
Console.WriteLine(parameter.Constraints[1].Argument); // 1
```

`TryParseTemplate(...)` returns structured diagnostics instead of throwing:

```csharp
if (!RoutePattern.TryParseTemplate("/orders/{id?}/items", out _, out var diagnostics))
{
    Console.WriteLine(diagnostics[0].Code);
}
```

Optional/defaulted parameters must form a contiguous suffix. That keeps expansion deterministic and avoids framework-specific interpretation of omitted middle segments.

## Registering routes

```csharp
using Pattrn;
using Pattrn.Routing;

var builder = PattrnIndex<string, string>.Builder();

builder.AddRoute("/orders/{id:int}", "order-handler", patternId: "orders-by-id");
builder.AddRoute("/files/{*path}", "file-handler", patternId: "files-catch-all");

var index = builder.Build();
```

`AddRoute(...)` compiles route templates into explicit generic pattern segments. Optional/defaulted suffix parameters are expanded into multiple registrations:

```csharp
builder.AddRoute("/orders/{id?}", "orders", patternId: "orders-optional-id");
```

This registers structural patterns for both `/orders` and `/orders/{id}` while preserving the supplied pattern identity on each expansion. Parsed templates can be reused when a caller wants metadata and registration from the same parse operation:

```csharp
var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int=6}/{day:int?}");

builder.AddRoute(template, "archive-handler", patternId: "archive-template");
```

## Inspecting optional/defaulted expansion metadata

Use `ExpandDetailed(...)` when a caller needs to understand exactly which generic patterns will be registered and which suffix parameters were omitted by each expansion:

```csharp
var template = RoutePattern.ParseTemplate("/archive/{year:int}/{month:int=6}/{day:int?}");
var expansions = template.ExpandDetailed();

foreach (var expansion in expansions)
{
    Console.WriteLine(expansion.ExpansionIndex);
    Console.WriteLine(expansion.IncludedSegmentCount);
    Console.WriteLine(expansion.IsFullTemplate);
    Console.WriteLine(string.Join(", ", expansion.OmittedParameters.Select(parameter => parameter.Name)));
}
```

Expansions are ordered from shortest omitted-suffix variant to the full template. Each `RouteTemplateExpansion` links back to the original `RouteTemplate`, exposes the generated generic `PatternSegment<string>[]`, and records omitted optional/defaulted parameters. Optionality remains in the routing companion; the generic core still only sees explicit structural patterns.

## Matching route paths

```csharp
var values = index.MatchRouteToArray("/orders/123");
```

## Low-allocation route helper APIs

Use these helpers when you want caller-controlled buffers and explicit capacity checks:

```csharp
var maxValues = index.GetRouteMatchCountUpperBound("/orders/123");
var maxCaptures = index.GetRouteCaptureCountUpperBound("/orders/123");

var values = new string[maxValues];
var matches = index.TryMatchRoute("/orders/123", values, out var written);
```

For detailed matching with caller-provided buffers:

```csharp
var matchBuffer = new PatternMatch<string>[index.GetRouteMatchCountUpperBound("/orders/123")];
var captureBuffer = new PatternCapture<string>[index.GetRouteCaptureCountUpperBound("/orders/123")];

var ok = index.TryMatchRouteDetailed(
    "/orders/123",
    matchBuffer,
    captureBuffer,
    out var matchesWritten,
    out var capturesWritten);
```

`TryMatchRoute(...)` and `TryMatchRouteDetailed(...)` follow the same no-partial-write contract as core `Try*` APIs when buffers are too small.

## Detailed matches and captures

```csharp
var matches = index.MatchRouteDetailedToArray("/orders/123");

var value = matches[0].Value;
var capture = matches[0].Captures[0];
```

For `/orders/{id}` matched against `/orders/123`, the capture is:

```text
Name: id
Value: 123
SegmentIndex: 1
```

For `/files/{*path}` matched against `/files/a/b/c.txt`, the catch-all produces one capture per remaining segment:

```text
path = a      at segment index 1
path = b      at segment index 2
path = c.txt  at segment index 3
```

The package does not join catch-all segments into a slash-separated string. Joining is domain-specific and should be done by the caller or a higher-level adapter.


## Optional constraint validation

Structural route matching and constraint validation are separate steps. A template such as `/orders/{id:int}` structurally matches `/orders/abc` because `{id}` is a route parameter. The routing layer can then reject the detailed match by validating the preserved `int` constraint against the captured value.

```csharp
var template = RoutePattern.ParseTemplate("/orders/{id:int:min(1)}");

var index = PattrnIndex<string, string>
    .Builder()
    .AddRoute("/orders/{id:int:min(1)}", "handler", patternId: "orders-by-id")
    .Build();

var match = index.MatchRouteDetailedToArray("/orders/42")[0];
var validation = template.ValidateConstraints(match);

if (validation.IsValid)
{
    Console.WriteLine(match.Value);
}
```

Built-in framework-neutral validators include:

```text
int
long
guid
bool
datetime
decimal
alpha
min
max
length
regex
```

Unknown constraints fail closed by default. Use `AllowUnknownConstraints` only when another layer will evaluate those tokens later.

Custom validators can be registered without changing the core matcher:

```csharp
var registry = RouteConstraintValidatorRegistry.CreateDefault()
    .Add("knownTenant", static (value, constraint) => value == "acme");

var options = new RouteConstraintValidationOptions
{
    ValidatorRegistry = registry
};

var result = template.ValidateConstraints(match, options);
```

Constraint failures are reported as routing-specific `RouteConstraintValidationFailure` entries. They are not core `PatternDiagnostic` values because the generic core does not know route semantics.

## Allocation behavior

The string-based helpers are convenience APIs. They rent the temporary segment array internally, but a raw route string still has to be converted into string segments before calling the generic core matcher. That means each segment is still materialized as a `string`.

For hot paths, prefer one of these approaches.

### Split into a caller-provided buffer

```csharp
var segmentCount = RoutePattern.GetPathSegmentCount(path);
var segments = new string[segmentCount];
var written = RoutePattern.SplitPath(path, segments);

var destination = new Handler[index.GetMatchCountUpperBound(segments.AsSpan(0, written))];
var matches = index.Match(segments.AsSpan(0, written), destination);
```

`TrySplitPath(...)` is available when a caller wants to avoid exceptions for too-small buffers. On failure, it reports `written = 0` and does not write partial segments.

### Pre-segment once and call the core span APIs

```csharp
var pattern = RoutePattern.Parse("/orders/{id:int}");
builder.AddPattern(pattern, handler);

var pathSegments = RoutePattern.SplitPath("/orders/123");
var values = index.MatchToArray(pathSegments);
```

The generic core remains the allocation-conscious hot path. A future dedicated routing index could avoid per-segment string materialization, but that would be a separate package-level design decision rather than a core matcher change.
