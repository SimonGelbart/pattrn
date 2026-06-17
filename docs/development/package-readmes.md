# Package README mapping

Each package ships a README that matches its package boundary. This avoids documenting companion-package behavior as if it were part of the core package.

| Package | Packed README | Purpose |
|---|---|---|
| `Pattrn` | root `README.md` | Core package overview, generic segmented matching, and links to package-specific docs. |
| `Pattrn.Strings` | `docs/getting-started/strings.md` | Dotted/separated string helper methods. |
| `Pattrn.DependencyInjection` | `docs/getting-started/dependency-injection.md` | DI registration, named indexes, provider preview status, and registration-source guidance. |
| `Pattrn.Routing` | `docs/getting-started/routing.md` | Route-template preview syntax and route helper APIs. |

## Current rule

The core README may mention companion packages, but detailed behavior belongs in the companion package README. Route-specific parsing rules must stay in the routing README. String splitting/normalization rules must stay in the strings README. DI lifetime and registration rules must stay in the DI README.

