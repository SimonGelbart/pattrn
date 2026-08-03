# Performance model

Pattrn is designed for workloads where reads greatly outnumber registration
changes.

## Read path

Registrations are compiled once into compact immutable storage. Published
snapshots can be read concurrently without locks. Exact, best-prefix, and
all-prefix operations share the compiled structure while keeping their result
contracts explicit. Caller-buffer APIs let a caller reuse result storage when
the appropriate upper bound is known.

Owning result APIs allocate arrays or detailed result objects by design. String
helpers also allocate while splitting and normalizing input. Diagnostics and
explanations are intentionally outside the normal hot path.

## Compilation and updates

Canonical pattern analysis uses structural hashing for near-linear growth in the
number of registrations rather than repeated quadratic scans. Compilation still
constructs a complete immutable snapshot and can be expensive for large
registration sets. Individual mutations are therefore not the target workload;
batch replacement is the intended update model.

## Choosing an implementation

Pattrn is a generic matcher. A specialized domain trie may be simpler or faster
when its segment vocabulary, result shape, and update lifecycle are known in
advance. Grouping equal structural patterns in an application can reduce
registration and compilation overhead when many values share one pattern.

Consumers should benchmark their own data, match distribution, comparer, result
count, and update cadence. Local BenchmarkDotNet runs are investigation
evidence, not product-wide performance claims. Pattrn does not claim to be
faster than a specialized application tree.
