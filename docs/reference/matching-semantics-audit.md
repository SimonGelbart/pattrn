# Matching semantics docs/tests audit

This audit maps the core matching semantics documented in [`matching-semantics.md`](matching-semantics.md) and [`ranking-specificity.md`](ranking-specificity.md) to the current core test suite. It is an audit artifact for beta stabilization; it does not change product semantics.

Status key: `Covered` means the behavior is protected by an existing or focused audit test. `Gap: missing test` or `Gap: missing docs` means the behavior should be covered by a narrower follow-up before beta. `Docs-only boundary` means the documented statement is a package/product boundary rather than core runtime behavior.

| Semantic area | Documented in | Test coverage | Status | Notes / follow-up |
|---|---|---|---|---|
| Exact-length matching by default | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`PrefixMatchingIsExplicitlyDisabledByDefault`) | Covered |  |
| Explicit prefix matching | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`PrefixMatchingCanBeEnabledForStarterKitCompatibleBehavior`) | Covered |  |
| Pattern longer than input never matches, even in prefix mode | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`PatternLongerThanInputDoesNotMatchInPrefixMode`) | Covered | Added focused audit coverage. |
| Literal segments | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`ExactPathMatchesRegisteredValue`) | Covered |  |
| Configured wildcard token | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`WildcardCanMatchFirstMiddleAndLastSegments`) | Covered |  |
| `PatternSegment.Literal` matching a value equal to the wildcard token | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/PatternSegmentTests.cs` (`LiteralPatternSegmentCanRepresentConfiguredWildcardSegmentValue`) | Covered |  |
| `PatternSegment.Parameter` matching exactly one segment | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/PatternSegmentTests.cs` (`BuilderAcceptsGenericPatternSegments`); `tests/Pattrn.Tests/DetailedMatchTests.cs` | Covered | Capture details are also tracked by #37. |
| Anonymous wildcard matching exactly one segment | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`WildcardCanMatchFirstMiddleAndLastSegments`) | Covered |  |
| Wildcard not matching zero segments | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`WildcardDoesNotMatchZeroSegments`) | Covered | Added focused audit coverage. |
| Wildcard becoming prefix-capable only after consuming one segment in prefix mode | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`WildcardCanOnlyBecomePrefixAfterConsumingOneSegment`) | Covered | Added focused audit coverage. |
| Terminal catch-all | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`CatchAllMatchesZeroOrMoreRemainingSegments`) | Covered |  |
| Catch-all matching zero remaining segments | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`CatchAllMatchesZeroOrMoreRemainingSegments`, `NamedCatchAllCanMatchEmptyRemainder`); `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` | Covered |  |
| Catch-all matching multiple remaining segments | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`CatchAllMatchesZeroOrMoreRemainingSegments`, `NamedCatchAllCapturesRemainingSegments`) | Covered |  |
| Catch-all must be terminal | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`CatchAllMustBeTerminal`) | Covered |  |
| Named parameter captures | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/DetailedMatchTests.cs`; `tests/Pattrn.Tests/PatternSegmentTests.cs` | Covered | Capture details are also tracked by #37. |
| Named catch-all captures | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`NamedCatchAllCapturesRemainingSegments`) | Covered | Capture details are also tracked by #37. |
| Catch-all empty remainder capture behavior | `docs/reference/ranking-specificity.md`; `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CatchAllPatternTests.cs` (`NamedCatchAllCanMatchEmptyRemainder`); `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` (`CatchAllWithEmptyRemainderIsEmittedAfterExactRegistrationAtTheSameNode`) | Covered | Capture details are also tracked by #37. |
| Empty pattern matching empty path by default | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`EmptyPatternMatchesOnlyEmptyPathByDefault`) | Covered |  |
| Empty pattern matching every path in prefix mode | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`EmptyPatternMatchesAnyPathWhenPrefixMatchingIsEnabled`) | Covered |  |
| String helpers rejecting empty strings | `docs/reference/matching-semantics.md` | `tests/Pattrn.Strings.Tests/StringExtensionTests.cs` (`DottedHelpersRejectEmptyPathBecauseCoreSpanApisRepresentEmptyPathsExplicitly`) | Covered | Companion behavior is covered in the strings test project. |
| Overlapping patterns returning all accepted values | `docs/reference/matching-semantics.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`OverlappingExactAndWildcardPatternsReturnAllMatches`) | Covered |  |
| Value deduplication enabled by default | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`DeduplicationIsEnabledByDefaultAcrossOverlappingPatterns`); `tests/Pattrn.Tests/DuplicateBehaviorInteractionTests.cs` | Covered | Duplicate value behavior is also tracked by #35. |
| `PreserveDuplicates` returning repeated/equal values | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/CoreMatchingTests.cs` (`DeduplicationCanBeDisabled`); `tests/Pattrn.Tests/DuplicateBehaviorInteractionTests.cs` | Covered | Duplicate value behavior is also tracked by #35. |
| Duplicate structural registrations | `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/DuplicatePatternRegistrationBehaviorTests.cs`; `tests/Pattrn.Tests/DuplicateBehaviorInteractionTests.cs` | Covered | Narrower duplicate structural behavior is tracked by #34. |
| Result ordering: literal > parameter > wildcard > catch-all | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md`; ADR 0013 | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` (`ValueAndDetailedResultsUseTheSameGenericSpecificityOrder`) | Covered |  |
| Detailed match `Specificity` relative order | `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs`; `tests/Pattrn.Tests/CatchAllPatternTests.cs` | Covered |  |
| Exact numeric specificity weights are implementation detail | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md` | N/A | Docs-only boundary | Tests should avoid hard-coded numeric weights. |
| Same-specificity registration-order tie-breaker | `docs/reference/ranking-specificity.md`; ADR 0013 | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` (`EqualSpecificityPreservesRegistrationOrderWhenDuplicatesArePreserved`, `EqualCatchAllSpecificityPreservesRegistrationOrder`) | Covered | Registration-order tie-breaking is also tracked by #33. |
| `RegistrationOrder` metadata | `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` | Covered |  |
| Prefix traversal order: prefix node registrations before deeper descendants | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md`; ADR 0013 | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` (`PrefixModeEmitsPrefixRegistrationsBeforeDeeperRegistrations`) | Covered | Prefix traversal behavior is also tracked by #36. |
| Prefix mode specificity within same depth | `docs/reference/matching-semantics.md`; `docs/reference/ranking-specificity.md` | `tests/Pattrn.Tests/RankingSpecificityContractTests.cs` (`PrefixModeStillUsesSpecificityWithinTheSameDepth`) | Covered | Prefix traversal behavior is also tracked by #36. |
| Builder mutable/not thread-safe | `docs/reference/matching-semantics.md`; ADR 0014 | N/A | Docs-only boundary | This is a documented construction boundary; concurrent mutation is intentionally not promised. |
| Compiled index immutable/safe for concurrent readers | `docs/reference/matching-semantics.md`; ADR 0014 | `tests/Pattrn.Tests/ConcurrencyTests.cs` (`CompiledIndexSupportsConcurrentReaders`) | Covered |  |
| Domain-specific ranking is outside core | `docs/reference/ranking-specificity.md`; ADR 0001; ADR 0013 | N/A | Docs-only boundary | No core test needed because this is a product boundary. |
| Route-template precedence is outside core | `docs/reference/ranking-specificity.md`; ADR 0001; ADR 0013 | N/A | Docs-only boundary | No route-specific precedence should be added to core. |

## Follow-up issue check

Follow-up issue needed: no

Reason: The audit maps current docs to tests, adds focused coverage for small unambiguous gaps, and links existing follow-up issues #33, #34, #35, #36, and #37 for narrower semantics coverage. No new docs/tests/code semantic mismatch was found.

Suggested issue: none
