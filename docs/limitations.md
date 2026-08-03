# Limitations

- Compiled indexes are immutable. Updates require building and publishing a
  replacement snapshot.
- Frequent individual mutations are a poor fit because affected registrations
  are recompiled.
- Owning result APIs allocate; caller-buffer APIs require the caller to size and
  manage destinations.
- Generic matching can lose to a specialized trie tailored to one domain.
- Applications with many values for one structural pattern may benefit from
  grouping those values before compilation.
- String and DI packages are convenience layers with their own allocation and
  integration costs.
- Pattrn is not a dynamic routing or subscription service.
- No distributed storage, retries, durability, delivery, or messaging
  semantics are provided.
- No endpoint execution, authorization policy, cache invalidation, or
  framework-specific route precedence is provided.

The supported product remains a framework-neutral immutable segmented-pattern
index for read-heavy applications.
