# Generic pattern segments

`PatternSegment<TSegment>` is the domain-neutral pattern model for the current pre-beta core API.

It lets callers describe pattern intent without relying on a reserved wildcard value in the segment domain:

```csharp
var builder = PattrnIndex<string, string>.Builder();

builder.AddPattern(
    [
        PatternSegment<string>.Literal("orders"),
        PatternSegment<string>.Parameter("id")
    ],
    "order-handler");
```

The core package does **not** parse route strings such as `/orders/{id}`. Route-template parsing belongs in an optional package layered on top of the generic segmented core.

## Segment kinds

| Kind | Meaning |
|---|---|
| `Literal` | Matches one exact segment value. |
| `Wildcard` | Matches any single input segment without assigning a name. |
| `Parameter` | Matches any single input segment and carries a logical name for detailed match APIs. |
| `CatchAll` | Matches zero or more remaining segments and must be terminal. |

The default value of `PatternSegment<TSegment>` is an anonymous wildcard. This keeps default struct values safe.

## Literal wildcard values

The `Add(ReadOnlySpan<TSegment>, TValue)` convenience API is literal-only on the default tokenless builder. If a builder is created with a reserved wildcard segment, `Add(...)` treats that configured value as a wildcard.

```csharp
var builder = PattrnIndex<string, string>.Builder("*");
builder.Add(["*"], "wildcard"); // wildcard branch
```

The generic pattern API can represent a literal segment that equals a tokenized builder's configured wildcard token:

```csharp
builder.AddPattern([PatternSegment<string>.Literal("*")], "literal-star");
builder.AddPattern([PatternSegment<string>.Wildcard()], "wildcard");
```

For input `"*"`, both registrations can match. For any other single segment, only the wildcard registration matches.

## Catch-all semantics

`PatternSegment<TSegment>.CatchAll()` and `CatchAll(name)` represent a terminal catch-all segment. Catch-all complements the other segment kinds:

- `Literal` still matches one exact segment.
- `Wildcard` and `Parameter` still match one segment.
- `CatchAll` matches the remaining suffix (including an empty suffix).

Named catch-all captures are exposed through detailed match APIs, with one capture per matched remaining segment.

## Detailed captures

`Parameter("id")` is matched through the same single-segment branch shape as `Wildcard()`, but the registration metadata preserves the parameter name for detailed matching.

```csharp
var index = PattrnIndex<string, string>
    .Builder()
    .AddPattern(
        [
            PatternSegment<string>.Literal("orders"),
            PatternSegment<string>.Parameter("id")
        ],
        "order-handler",
        patternId: "orders-by-id")
    .Build();

var matches = index.MatchDetailedToArray(["orders", "123"]);
var patternId = matches[0].PatternId; // "orders-by-id"
var registrationOrder = matches[0].RegistrationOrder; // 0
var id = matches[0].Captures[0].Value; // "123"
```

For hot paths, avoid per-match allocations by using `MatchDetailed(...)` with caller-provided `Span<PatternMatch<TValue>>` and `Span<PatternCapture<TSegment>>` buffers. Use the existing value-only `Match` APIs when you only need values.
