# Product scope

## Problem

Applications often need to match hierarchical, already-segmented keys against
many registered patterns. The index must answer reads deterministically while
keeping registration changes away from concurrent readers.

## Core proposition

Pattrn compiles canonical segmented registrations into an immutable snapshot.
The snapshot supports exact matching, deepest best-prefix matching, and
all-prefix enumeration. Matching remains synchronous and framework-neutral.

## Intended lifecycle

Build a mutable registration source during startup, configuration reload, or a
deliberate batch update. Compile and validate the complete registration set,
then publish the resulting snapshot atomically. Readers query the snapshot until
the next complete replacement.

## Supported use cases

- Read-heavy indexes of hierarchical application keys.
- Configuration or policy lookup where updates are occasional.
- In-memory dispatch tables with explicit segmented patterns.
- Generic libraries that need literals, wildcards, parameters, captures, and
  prefix operations without adopting a routing framework.

`Pattrn.Strings` and `Pattrn.DependencyInjection` are convenience packages kept
thin around the core. `Pattrn.Routing` is experimental compatibility content,
not part of the primary product story.

## Non-goals

Pattrn is not a dynamic subscription registry, HTTP routing framework, message
broker, rules engine, cache, distributed key-value store, authorization
framework, event-processing platform, continuously changing registration store,
general expression language, or new package ecosystem.

It provides no distributed storage, retries, durability, delivery guarantees,
endpoint execution, or framework-specific precedence rules.

## Future feature criteria

A future feature is considered only when a concrete application demonstrates a
repeated need, the feature fits immutable read-heavy publication, the semantic
contract can be stated precisely, and the implementation can be validated
without making the default matching path substantially more complex. Features
that belong to an application or a specialized adapter should remain outside
the core package.
