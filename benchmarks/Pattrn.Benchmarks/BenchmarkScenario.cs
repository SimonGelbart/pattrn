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

public enum RoutingBenchmarkScenario
{
    ParseSimple,
    ParseParameters,
    ParseCatchAll,
    MatchDetailed
}

public enum BuilderBenchmarkScenario
{
    BuildLargeExact,
    BuildLargeParameters,
    GetDiagnosticsClean,
    GetDiagnosticsAmbiguous,
    ValidateOnBuild
}
