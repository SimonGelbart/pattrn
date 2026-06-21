# Architecture Boundaries

## Core package

`Pattrn` owns:

- segments and pattern segments;
- literals, parameters, wildcards, and terminal catch-alls;
- captures;
- immutable compiled indexes;
- value matching and detailed matching;
- duplicate registration and duplicate value behavior;
- deterministic specificity metadata;
- optional generic diagnostics.

`Pattrn` does not own:

- HTTP;
- ASP.NET Core endpoint behavior;
- filesystem semantics;
- glob syntax;
- authorization semantics;
- tenant or resource policy meaning;
- OpenAPI semantics;
- source generation or analyzers;
- compiled-index serialization.

## Companion packages

Companion packages translate domain-friendly syntax into the generic core model.

- `Pattrn.Strings` owns string splitting and normalization.
- `Pattrn.DependencyInjection` owns thin DI registration ergonomics.
- `Pattrn.Routing` owns route-template parsing and route-layer validation while it remains preview.

Companion packages should not force their domain semantics back into the core.
