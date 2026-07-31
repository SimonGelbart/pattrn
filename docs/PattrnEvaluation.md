# Pattrn evaluation against Homework

## Question and scope

Can Pattrn replace Homework's custom subscription tree with a simpler, correct,
and competitive immutable dispatch index?

The experiment lives on `experiment/pattrn-subscription-index`. It imports
Homework commit `9c06265ef4424ed86691e088f563325ea1fa3d08` into this repository,
records the Pattrn baseline at `a2541b5`, retains `SubscriptionIndex`, and adds
`PattrnSubscriptionIndex` behind the unchanged `ISubscriptionIndex` interface.
`samples/Pattrn.HomeworkSample/IMPORT.md` records provenance and makes no
relicensing claim.

## Semantic result

The adapter preserves Homework's intended contract:

- one compiled Pattrn index per `MessageTypeId`;
- literal segments, explicit single-segment `*` wildcards, literal empty
  segments, and root patterns;
- all matching prefixes rather than exact or best-prefix matching;
- complete `Subscription` values and exact duplicate deduplication;
- lock-free readers over atomically published immutable snapshots;
- transactional, affected-message-type rebuilds for additions and removals.

The same functional tests pass against both implementations. Additional tests
cover duplicate registration, all six overlapping root/literal/wildcard
matches, empty segments, too-long patterns, removal and re-addition, snapshot
isolation, failed-compile rollback, caller buffers, and concurrent old-or-new
publication states.

The imported custom tree contained an overlap bug: one shared traversal path
was mutated while exploring literal and wildcard branches, so one of six valid
matches could be lost. The copied implementation now tracks traversal depth
instead. This is a correctness fix to the retained baseline implementation,
not a change to Homework's intended contract.

Pattrn's all-prefix results are deterministic and shallow-to-deep, with stable
registration ordering within a pattern. Homework does not promise result order,
so compatibility tests compare sets and do not expose this as a new contract.

## Benchmark environment

- OS: Pop!_OS 24.04 LTS
- CPU: AMD Ryzen 9 PRO 8945HS, 8 cores / 16 logical processors
- SDK: .NET SDK 10.0.301
- Runtime: .NET 10.0.9, x64 RyuJIT, concurrent workstation GC
- Configuration: Release
- BenchmarkDotNet: 0.15.8

The original workload has 30 clients x 10 message types x 4,000 subscriptions,
or 1,200,000 registrations, and materializes routed consumers.

Three pre-adapter ShortRun baselines on this machine measured 526.759 ns,
509.667 ns, and 534.861 ns, each allocating 1.27 KB. The side-by-side warmed
run measured:

| Implementation | Mean | Allocated | Relative time | Relative allocation |
| --- | ---: | ---: | ---: | ---: |
| Custom tree | 502.9 ns | 1.23 KB | 1.00x | 1.00x |
| Pattrn adapter | 2.217 us | 5.01 KB | 4.41x | 4.08x |

Pattrn is below the absolute 10 us lookup limit, but fails the requested 2x
relative lookup and allocation gates. `ISubscriptionIndex` returns
`IEnumerable<Subscription>`, while Pattrn's public all-prefix operation returns
an owning array. Returning that array directly is the adapter's lowest-allocation
compatible path. A separate reusable caller-buffer benchmark was allocation
free, showing that the engine can avoid read allocations when the consumer API
accepts a destination span.

For the 1,000-registration, mostly-literal, 30-match matrix case:

| Operation | Custom | Pattrn | Pattrn allocation |
| --- | ---: | ---: | ---: |
| Core lookup | 217.6 ns | 1.972 us | 4,352 B |
| Lookup plus `ToList` | 201.3 ns | 2.185 us | 4,616 B |
| End-to-end router | 432.9 ns | 2.213 us | 5,024 B |
| Reusable caller buffer | n/a | 1.666 us | 0 B |

## Construction, mutation, and memory

At 1,000 mostly-literal registrations, representative warmed mutations were:

| Mutation | Custom | Pattrn | Ratio |
| --- | ---: | ---: | ---: |
| Add 1 | 32.9 us | 8.87 ms | 269.6x |
| Add 100 | 147.9 us | 9.40 ms | 63.8x |
| Add 10,000 | 20.27 ms | 805.5 ms | 39.8x |
| Remove 1 | 38.8 us | 8.21 ms | 236.2x |
| Remove 100 | 138.3 us | 8.93 ms | 71.0x |
| Remove one consumer | 442.6 us | 7.98 ms | 18.1x |
| Publish an already-compiled snapshot | n/a | 3.15 ns | n/a |

Atomic publication is negligible; rebuilding is the limiting operation. The
requested 10x mutation gate is missed in every measured case.

One-shot high-cardinality literal probes show the tradeoff between compact
compiled data and compilation cost:

| Registrations | Implementation | Build time | Retained bytes |
| ---: | --- | ---: | ---: |
| 10,000 | Custom tree | 89.8 ms | 36.15 MB |
| 10,000 | Pattrn | 950.2 ms | 2.61 MB |
| 100,000 | Custom tree | 960.7 ms | 360.39 MB |
| 100,000 | Pattrn | 161.98 s | 25.22 MB |
| 1,200,000 | Custom tree | not timed in this probe | 4.33 GB |
| 1,200,000 | Pattrn | did not complete within 6 minutes | not available |

Pattrn's retained memory is far below the requested 2x ceiling, but build time
crosses the one-second ceiling around 10,000 registrations and becomes
non-viable at larger high-cardinality workloads.

## Matrix status and stopping decision

The benchmark project defines the full compatible Cartesian matrix requested:

- registrations: 1,000, 10,000, 100,000, and 1,200,000;
- five pattern distributions;
- compatible match counts of 0, 1, 5, 30, and 256;
- core, materialized, end-to-end, owning-array, caller-buffer, build, mutation,
  and publication operations.

This expands to 950 BenchmarkDotNet cases: 608 lookup, 80 construction, 260
mutation, and two original-workload continuity cases. Catalog and generated
result shapes are covered by tests. Warmed runs completed for the original
pair, an eight-operation lookup slice, and a thirteen-operation mutation slice.
The complete execution was intentionally stopped after the 100,000 build took
162 seconds and the 1,200,000 Pattrn probe failed to complete within six minutes.
Running hundreds more cases cannot reverse the already-failed hard construction
and mutation gates and would create uncontrolled multi-hour memory pressure.
This is a capped validation result, not a claim that the full matrix passed.

## Package consumption and API friction

A Release `Pattrn.0.1.0-alpha.1.nupkg` was packed to a local feed. A clean
temporary `net10.0` console project restored, compiled, and ran literal,
wildcard, and all-prefix matching using only the core `Pattrn` package. Strings,
dependency-injection, and routing packages are not required.

Observed friction:

- discovering `EnumeratePrefixValuesToArray` as distinct from exact and
  best-prefix operations requires careful documentation reading;
- pattern conversion is explicit and verbose, though wildcard semantics are
  unambiguous;
- the compatible `IEnumerable` read path must allocate an owning result array;
- zero-allocation lookup requires an upper-bound query and caller-provided span;
- removing one registration requires recompiling the affected message type;
- duplicate pattern policy must be selected explicitly even though canonical
  subscription deduplication is maintained outside Pattrn;
- deterministic result order is stronger than Homework requires and must not
  become an accidental consumer contract.

The adapter is understandable and the immutable publication model is correct,
but at roughly 350 lines plus canonical mutable state it is not substantially
simpler than the custom tree.

## Recommendation

Correctness, package isolation, snapshot safety, and retained memory are strong.
Lookup meets the absolute latency target but misses both relative read gates;
ordinary updates miss the mutation gate by 18x to 270x, and large compiles are
not operationally viable. Do not replace Homework's tree or delete either
implementation from this experiment. A future experiment should begin with an
incremental or structurally shared compiler/update model and a consumer-facing
buffer API, then rerun this unchanged matrix.

**Rework immutable update model**
