namespace Pattrn.Benchmarks;

public enum BenchmarkScenario
{
    ExactOnlySparseDeep,
    ExactOnlyWideFanOut,
    WildcardSparse,
    WildcardDense,
    PrefixExactOnly,
    PrefixWildcard,
    DuplicateHeavyDeduplicate,
    DuplicateHeavyPreserveDuplicates,
    NoMatch,
    ParameterCaptures,
    CatchAllTerminal
}

public enum BuilderBenchmarkScenario
{
    BuildLargeExact,
    BuildLargeParameters,
    CompileDiagnosticsClean,
    CompileDiagnosticsDuplicate
}
