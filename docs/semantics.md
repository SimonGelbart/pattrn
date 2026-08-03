# Semantic contract

This document is authoritative for the generic segmented matcher.

## Segments

- A literal consumes one input segment when the configured segment comparer
  considers the values equal.
- A wildcard consumes exactly one segment and has no capture name.
- A parameter consumes exactly one segment and captures it under its ordinal
  name.
- A terminal catch-all consumes zero or more remaining segments and captures
  all consumed segments when named. Non-terminal catch-alls are invalid.

Patterns preserve segment order and length. Segment kind is part of pattern
identity. Parameter and catch-all names are compared ordinally when present.

## Matching operations

Exact matching accepts only registrations that consume the complete input.
Best-prefix matching returns only registrations at the deepest accepted depth.
All-prefix enumeration returns accepted registrations from shallowest depth to
deepest depth. Within a depth, specificity and registration order are
deterministic; callers must not infer domain-specific routing priority.

Empty patterns match an empty path in exact mode and are valid prefix roots.
Root patterns therefore participate in prefix queries for every input. A literal
empty segment is distinct from an absent segment and is matched through the
configured comparer.

## Captures

Named parameters capture one segment. Named catch-alls capture a segment range,
including an empty range when they consume no input. Anonymous wildcards and
catch-alls do not produce named captures. Duplicate capture names and invalid
catch-all placement are compilation errors.

## Duplicates and ordering

Registration IDs must be unique. Duplicate structural patterns are controlled by
`DuplicatePatternPolicy`: `Error` reports an error, `Warn` reports a warning,
and `Allow` emits no duplicate-pattern diagnostic. Duplicate registrations are
still retained as separate registrations when compilation succeeds.

By default, equal values are deduplicated while retaining the first value in
deterministic match order. `MatchOptions.PreserveDuplicates` retains every
accepted registration. Owning and caller-buffer projections preserve the same
relative ordering.

## Comparers

Literal equality and hashing always use the configured segment comparer. A
case-insensitive comparer can therefore make literal patterns with different
spelling structurally equal. Parameter and catch-all names use ordinal string
identity and are not normalized by the literal comparer.

For the API surface and upper-bound rules, see [the core API reference](reference/api.md).
