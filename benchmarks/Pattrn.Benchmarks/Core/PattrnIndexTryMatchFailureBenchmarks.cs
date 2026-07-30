using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks.Core;

[MemoryDiagnoser]
[BenchmarkCategory("CallerBuffer")]
public class PattrnIndexTryMatchFailureBenchmarks
{
    public IEnumerable<BenchmarkScenario> TryMatchFailureScenarios()
    {
        yield return BenchmarkScenario.ExactOnlySparseDeep;
        yield return BenchmarkScenario.ExactOnlyWideFanOut;
        yield return BenchmarkScenario.WildcardSparse;
        yield return BenchmarkScenario.WildcardDense;
        yield return BenchmarkScenario.DuplicateHeavyDeduplicate;
        yield return BenchmarkScenario.DuplicateHeavyPreserveDuplicates;
        yield return BenchmarkScenario.ParameterCaptures;
        yield return BenchmarkScenario.CatchAllTerminal;
    }

    private PattrnIndex<string, int> _index = null!;
    private string[] _path = [];
    private int[] _valueDestination = [];
    private int[] _insufficientValueDestination = [];

    [ParamsSource(nameof(TryMatchFailureScenarios))]
    public BenchmarkScenario Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        var options = Scenario == BenchmarkScenario.DuplicateHeavyPreserveDuplicates
            ? MatchOptions.PreserveDuplicates
            : MatchOptions.Default;

        switch (Scenario)
        {
            case BenchmarkScenario.ExactOnlySparseDeep:
                AddSparseDeep(builder, branchCount: 1024, depth: 10);
                _path = ["root", "target", "l2", "l3", "l4", "l5", "l6", "l7", "l8", "l9"];
                break;

            case BenchmarkScenario.ExactOnlyWideFanOut:
                AddWideFanOut(builder, childCount: 4096);
                _path = ["root", "child-2048"];
                break;

            case BenchmarkScenario.WildcardSparse:
                AddMarketData(builder, marketCount: 16, symbolCount: 16, includeWildcards: true);
                _path = ["market", "M8", "S8"];
                break;

            case BenchmarkScenario.WildcardDense:
                AddMarketData(builder, marketCount: 80, symbolCount: 80, includeWildcards: true);
                _path = ["market", "M42", "S42"];
                break;

            case BenchmarkScenario.DuplicateHeavyDeduplicate:
            case BenchmarkScenario.DuplicateHeavyPreserveDuplicates:
                AddDuplicateHeavy(builder);
                _path = ["market", "M42", "S42"];
                break;

            case BenchmarkScenario.ParameterCaptures:
                AddParameterHeavy(builder, routeCount: 2048);
                _path = ["orders", "tenant-512", "order-512"];
                break;

            case BenchmarkScenario.CatchAllTerminal:
                AddCatchAllHeavy(builder, routeCount: 2048);
                _path = ["files", "tenant-512", "a", "b", "c.txt"];
                break;

            case BenchmarkScenario.NoMatch:
            default:
                throw new InvalidOperationException($"{Scenario} is not a valid TryMatch failure-path benchmark scenario.");
        }

        _index = PattrnIndex<string, int>.Compile(
            builder.ToRegistrations(),
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow },
            options);
        _valueDestination = new int[Math.Max(1, _index.GetMatchCountUpperBound(_path))];
        _insufficientValueDestination = [];

        if (!_index.TryMatchValues(_path, _valueDestination, out var expectedCount))
        {
            throw new InvalidOperationException("Expected benchmark destination to hold all matches.");
        }
        if (expectedCount == 0)
        {
            throw new InvalidOperationException("TryMatch failure benchmark scenario must produce at least one exact match.");
        }
        if (expectedCount <= _insufficientValueDestination.Length)
        {
            throw new InvalidOperationException("TryMatch failure benchmark scenario must produce more matches than the insufficient destination can hold.");
        }

        var succeeded = _index.TryMatchValues(_path, _insufficientValueDestination, out var written);
        if (succeeded || written != 0)
        {
            throw new InvalidOperationException("TryMatch failure benchmark scenario must fail and report zero written values.");
        }
    }

    [Benchmark]
    public int Trie_TryMatchToSpan_InsufficientDestination()
    {
        var succeeded = _index.TryMatchValues(_path, _insufficientValueDestination, out var written);
        return succeeded ? written : -1;
    }

    private static void AddMarketData(
        PattrnIndexBuilder<string, int> builder,
        int marketCount,
        int symbolCount,
        bool includeWildcards)
    {
        var value = 0;

        for (var market = 0; market < marketCount; market++)
        {
            for (var symbol = 0; symbol < symbolCount; symbol++)
            {
                builder.Add(["market", $"M{market}", $"S{symbol}"], value++);
            }
        }

        if (!includeWildcards)
        {
            return;
        }

        for (var market = 0; market < marketCount; market++)
        {
            builder.Add(["market", $"M{market}", "*"], value++);
        }

        for (var symbol = 0; symbol < symbolCount; symbol++)
        {
            builder.Add(["market", "*", $"S{symbol}"], value++);
        }

        builder.Add(["market", "*", "*"], value++);
    }

    private static void AddDuplicateHeavy(PattrnIndexBuilder<string, int> builder)
    {
        for (var value = 0; value < 64; value++)
        {
            builder.Add(["market", "M42", "S42"], value);
            builder.Add(["market", "M42", "*"], value);
            builder.Add(["market", "*", "S42"], value);
            builder.Add(["market", "*", "*"], value);
        }
    }

    private static void AddSparseDeep(PattrnIndexBuilder<string, int> builder, int branchCount, int depth)
    {
        builder.Add(["root", "target", "l2", "l3", "l4", "l5", "l6", "l7", "l8", "l9"], 0);

        for (var branch = 0; branch < branchCount; branch++)
        {
            var pattern = new string[depth];
            pattern[0] = "root";
            pattern[1] = $"branch-{branch}";
            for (var level = 2; level < depth; level++)
            {
                pattern[level] = $"l{level}";
            }

            builder.Add(pattern, branch + 1);
        }
    }

    private static void AddWideFanOut(PattrnIndexBuilder<string, int> builder, int childCount)
    {
        for (var child = 0; child < childCount; child++)
        {
            builder.Add(["root", $"child-{child}"], child);
        }
    }

    private static void AddParameterHeavy(PattrnIndexBuilder<string, int> builder, int routeCount)
    {
        for (var route = 0; route < routeCount; route++)
        {
            builder.AddPattern([
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Literal($"tenant-{route}"),
                PatternSegment<string>.Parameter("orderId")], route);
        }
    }

    private static void AddCatchAllHeavy(PattrnIndexBuilder<string, int> builder, int routeCount)
    {
        for (var route = 0; route < routeCount; route++)
        {
            builder.AddPattern([
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.Literal($"tenant-{route}"),
                PatternSegment<string>.CatchAll("path")], route);
        }
    }
}
