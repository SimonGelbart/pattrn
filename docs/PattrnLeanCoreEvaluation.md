# Pattrn lean-core evaluation

This is a bounded experiment on branch `experiment/pattrn-lean-core`, based on
`experiment/pattrn-subscription-index` at commit
`ef00abd26e7e4e298bf5b2a82d3f62db6bdc9e58`. The existing Homework adapter,
benchmarks, and `docs/PattrnEvaluation.md` were kept unchanged.

## Baseline

Environment: Pop!_OS 24.04 x64, .NET SDK 10.0.301 / runtime 10.0.9,
Release/workstation GC, BenchmarkDotNet 0.15.8. Focused core and Homework
builds/tests passed. An initial baseline solution-wide restore/build invocation
returned a failure without a diagnostic error; the final direct rerun of
`dotnet restore Pattrn.sln` and `dotnet build Pattrn.sln --configuration
Release --no-restore` passed, so the initial result is recorded as a transient
tooling limitation rather than product evidence.

The requested retained-memory probes (the build column is included because the
probe constructs the index before measuring the retained snapshot) were:

| Registrations | Implementation | Build | Retained |
| ---: | --- | ---: | ---: |
| 10,000 | Custom tree | 81.541 ms | 36.15 MB |
| 10,000 | Pattrn | 870.570 ms | 2.61 MB |
| 100,000 | Custom tree | 840.374 ms | 360.39 MB |
| 100,000 | Pattrn | 121.226 s | 25.22 MB |

The original Homework end-to-end ShortRun was 479.8 ns / 1.23 KB for the
custom tree and 2.309 us / 5.01 KB for Pattrn (4.81x time, 4.08x allocation).
The focused 1,000-registration, mostly-literal, 30-match probe was 211.8 ns /
456 B for the custom-tree core, 2.038 us / 4,352 B for Pattrn core, and
1.710 us / 0 B for the Pattrn caller-buffer path. The full 950-case matrix was
not run.

## Compiler diagnosis and change

Before this branch, canonical validation compared each registration against a
linear list for duplicate detection and then made a second linear pass for the
distinct-pattern count. Segment comparisons therefore made compilation
quadratic. The builder's `PatternCount` repeated the same structural scan.

`PatternSequenceComparer<TSegment>` now hashes and compares length, order,
segment kind, literal values through the configured segment comparer, and
parameter/catch-all names. The compiler uses one comparer-backed hash set in
the registration pass for duplicate diagnostics and `PatternCount`; the
builder uses the same comparer for its count. Registration identity checks and
all existing diagnostics remain separate and unchanged. With
`DuplicatePatternPolicy.Allow`, no duplicate warning/error is emitted; the one
hash-set insertion still supplies the required distinct count.

The focused compiler scaling benchmark used unique one-segment patterns and a
ShortRun (three warmups, three measured iterations):

| Patterns | Mean build | Allocated |
| ---: | ---: | ---: |
| 1,000 | 254.9 us | 1.19 MB |
| 10,000 | 8.52 ms | 13.19 MB |
| 25,000 | 22.18 ms | 29.96 MB |
| 40,000 | 44.00 ms | 53.55 MB |
| 50,000 | 50.47 ms | 60.30 MB |
| 100,000 | 111.65 ms | 121.40 MB |

The 10k-to-100k growth is approximately proportional after the small-input
startup effects; 25k-to-50k was 2.28x and 50k-to-100k was 2.21x. This is
consistent with linear or near-linear growth, not the former quadratic curve.

`PatternIdentityTests` covers structural equality, equal-length kind/literal
and name differences, custom literal comparers, duplicate policies, pattern
counts, and duplicate registration identities.

## Grouped Homework model

`GroupedPattrnSubscriptionIndex` is an experimental adapter alongside the two
existing implementations. Its immutable write state is:

`MessageTypeId -> ContentPattern -> immutable subscription bucket`.

One Pattrn registration is compiled per structural bucket. Add/remove operations
clone only affected message-type candidates, compile before publication, remove
empty buckets/types, and publish the complete old-or-new snapshot. Lookup
enumerates all matching prefixes and flattens each bucket; bucket membership
deduplicates exact subscriptions and does not introduce an ordering contract.
The grouped contract suite reuses the existing index contracts and adds tests
for shared patterns, multi-pattern consumers, partial/empty bucket removal,
duplicate suppression, overlapping root/literal/wildcard buckets, rollback, and
concurrent snapshots.

For the requested 30-consumer, 10-message-type, 4,000-pattern-per-type shape
(1,200,000 logical subscriptions and 40,000 structural patterns), the direct
retained probes were:

| Implementation | Build | Retained | Matches |
| --- | ---: | ---: | ---: |
| Custom tree | 1.178 s | 79.86 MB | 30 |
| Current Pattrn adapter | 2.174 s | 111.23 MB | 30 |
| Grouped + general matcher | 721 ms | 55.23 MB | 30 |
| Grouped + lean matcher | 659 ms | 63.55 MB | 30 |

The corrected original-shape lookup benchmark (dry job) measured:

| Operation | Custom tree | Pattrn | Grouped general | Grouped lean |
| --- | ---: | ---: | ---: | ---: |
| Core lookup | 389.4 us | 400.2 us | 389.9 us | 420.6 us |
| End-to-end lookup | 840.7 us | 810.8 us | 851.9 us | 772.6 us |
| Caller buffer (custom owning fallback*) | 466.4 us | 2,758.5 us | 1,078.6 us | 401.9 us |

## Internal lean path

`LeanGroupedCompiledIndex` is internal to the Homework sample. It stores flat
immutable node, child, open-address lookup, wildcard-edge, and bucket arrays.
It supports only literal segments, one-segment `*`, all-prefix enumeration, a
caller-provided destination, and bucket flattening. It carries no captures,
descriptors, explanations, registration names/IDs, ranking, stable ordering,
owning result arrays, or matcher-level deduplication. The grouped adapter
already guarantees unique subscriptions.

The focused lookup matrix ran both mostly-literal and mixed
literal/wildcard distributions at 0, 1, 30, and 256 matches. The primary
30-match rows from the corrected dry run were:

| Distribution | Operation | Custom tree | Pattrn | Grouped general | Grouped lean |
| --- | --- | ---: | ---: | ---: | ---: |
| Mostly literal | Core | 397.9 us | 392.0 us | 354.2 us | 315.5 us |
| Mostly literal | Caller buffer | — | 435.3 us | 989.9 us | 408.2 us |
| Mostly literal | End-to-end | 811.3 us | 787.4 us | 1,059.0 us | 753.9 us |
| Mixed | Core | 395.0 us | 402.6 us | 363.7 us | 349.3 us |
| Mixed | Caller buffer | — | 411.0 us | 563.4 us | 368.4 us |
| Mixed | End-to-end | 788.4 us | 1,070.9 us | 896.0 us | 731.1 us |

All eight match-count/distribution cases returned their expected 0, 1, 30,
or 256 results. Across the complete matrix, the lean caller-buffer path stayed
below 1.80x the custom-tree core time (the largest ratio was a zero-match
mostly-literal case); the original-shape probe was 401.9 us versus 466.4 us
for the custom tree. The grouped general matcher stayed below 3x the custom
tree in the primary end-to-end cases.

## Batch replacement

The batch benchmark records rebuild/publication behavior rather than treating
single-registration mutation as the primary criterion. Dry-job means were:

| Scenario | Custom tree | Pattrn | Grouped general | Grouped lean |
| --- | ---: | ---: | ---: | ---: |
| Rebuild one affected message type | 99.8 ms | 292.8 ms | 96.3 ms | 107.8 ms |
| Replace 100 subscriptions | 102.1 ms | 291.6 ms | 101.5 ms | 83.3 ms |
| Replace one consumer set | 798.8 ms | 1.824 s | 624.7 ms | 524.0 ms |
| Publish compiled snapshot (implementation-independent*) | 309 us | 485 us | 301 us | 297 us |

\* The custom tree has no caller-buffer API in this adapter, so its row is the
owning `FindSubscriptions` fallback. The snapshot-publish benchmark only swaps
two already-compiled object references; its four values are run-to-run noise,
not implementation-specific matcher costs.

## Profiling and decision gates

The caller-buffer threshold did not trigger profiling: the lean path remained
under twice the custom-tree comparison in every focused case and in the
original-shape probe. Consequently no speculative profiler-driven hot-path
changes were made. The only lean storage repair was a correctness fix that
keeps each node's direct-child range contiguous before recursive descendants
are appended.

Compiler growth is now practical and near-linear. Grouping makes the original
Homework construction practical (well below the 30-second cap), and both
grouped lookup variants meet the requested lookup ratios. Retained memory is
lower for the grouped variants than the custom tree, but not by the selected
2x material-improvement threshold: grouped general is 0.69x the custom retained
size and grouped lean is 0.80x. The ungrouped general Pattrn adapter is larger
than the custom tree at this workload. The lean matcher has a deliberately
narrow feature surface, but its compact implementation is still experimental
and its memory advantage is not decisive.

## Limitations

The benchmark matrix uses focused BDN Dry runs for the 0/1/30/256 lookup and
batch tables, ShortRun for compiler scaling, and direct workload probes. These
are local investigation evidence, not CI proof. No full Cartesian benchmark,
incremental update, public API redesign, detailed matching optimization,
Native AOT change, or remote action was made. The final solution restore/build
and affected test modules passed locally; the Native AOT harness passed Linux
trimmed/AOT publish and smoke checks and reported only its documented
Windows-host/toolchain skips. These are local preflight results, not CI proof.

Preserve as an internal/research component
