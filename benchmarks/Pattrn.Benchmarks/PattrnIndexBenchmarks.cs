using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks;

[MemoryDiagnoser]
public class PattrnIndexBenchmarks
{
    private readonly List<Registration> _registrations = [];
    private PattrnIndex<string, int> _index = null!;
    private string[] _path = [];
    private int[] _valueDestination = [];
    private PatternMatch<int>[] _matchDestination = [];
    private PatternCapture<string>[] _captureDestination = [];

    [Params(
        BenchmarkScenario.ExactOnlySparseDeep,
        BenchmarkScenario.ExactOnlyWideFanOut,
        BenchmarkScenario.WildcardSparse,
        BenchmarkScenario.WildcardDense,
        BenchmarkScenario.PrefixExactOnly,
        BenchmarkScenario.PrefixWildcard,
        BenchmarkScenario.DuplicateHeavyDeduplicate,
        BenchmarkScenario.DuplicateHeavyPreserveDuplicates,
        BenchmarkScenario.NoMatch,
        BenchmarkScenario.ParameterCaptures,
        BenchmarkScenario.CatchAllTerminal)]
    public BenchmarkScenario Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _registrations.Clear();

        var builder = PattrnIndex<string, int>.Builder("*");
        var options = Scenario switch
        {
            BenchmarkScenario.PrefixExactOnly or BenchmarkScenario.PrefixWildcard => MatchOptions.Prefix,
            BenchmarkScenario.DuplicateHeavyPreserveDuplicates => MatchOptions.PreserveDuplicates,
            _ => MatchOptions.Default
        };

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
                AddMarketData(builder, marketCount: 16, symbolCount: 16, includeWildcards: true, includePrefixes: false);
                _path = ["market", "M8", "S8"];
                break;

            case BenchmarkScenario.WildcardDense:
                AddMarketData(builder, marketCount: 80, symbolCount: 80, includeWildcards: true, includePrefixes: false);
                _path = ["market", "M42", "S42"];
                break;

            case BenchmarkScenario.PrefixExactOnly:
                AddMarketData(builder, marketCount: 80, symbolCount: 80, includeWildcards: false, includePrefixes: true);
                _path = ["market", "M42", "S42", "quote"];
                break;

            case BenchmarkScenario.PrefixWildcard:
                AddMarketData(builder, marketCount: 80, symbolCount: 80, includeWildcards: true, includePrefixes: true);
                _path = ["market", "M42", "S42", "quote"];
                break;

            case BenchmarkScenario.DuplicateHeavyDeduplicate:
            case BenchmarkScenario.DuplicateHeavyPreserveDuplicates:
                AddDuplicateHeavy(builder);
                _path = ["market", "M42", "S42"];
                break;

            case BenchmarkScenario.NoMatch:
                AddMarketData(builder, marketCount: 80, symbolCount: 80, includeWildcards: false, includePrefixes: false);
                _path = ["market", "missing", "symbol"];
                break;

            case BenchmarkScenario.ParameterCaptures:
                AddParameterHeavy(builder, routeCount: 2048);
                _path = ["orders", "tenant-512", "order-512"];
                break;

            case BenchmarkScenario.CatchAllTerminal:
                AddCatchAllHeavy(builder, routeCount: 2048);
                _path = ["files", "tenant-512", "a", "b", "c.txt"];
                break;
        }

        _index = builder.Build(options);
        var matchUpperBound = _index.GetMatchCountUpperBound(_path);
        _valueDestination = new int[Math.Max(1, matchUpperBound)];
        _matchDestination = new PatternMatch<int>[Math.Max(1, matchUpperBound)];
        _captureDestination = new PatternCapture<string>[Math.Max(1, _index.GetCaptureCountUpperBound(_path))];
    }

    [Benchmark(Baseline = true)]
    public int[] NaiveScan_MatchToArray()
    {
        var matches = new List<int>();

        foreach (var registration in _registrations)
        {
            if (!Matches(registration.Pattern, _path, _index.Options.IncludePrefixMatches))
            {
                continue;
            }

            if (_index.Options.DeduplicateValues && matches.Contains(registration.Value))
            {
                continue;
            }

            matches.Add(registration.Value);
        }

        return matches.Count == 0 ? [] : [.. matches];
    }

    [Benchmark]
    public int Trie_GetMatchCountUpperBound()
    {
        return _index.GetMatchCountUpperBound(_path);
    }

    [Benchmark]
    public int Trie_MatchToSpan()
    {
        return _index.Match(_path, _valueDestination);
    }

    [Benchmark]
    public int Trie_TryMatchToSpan_SufficientDestination()
    {
        var succeeded = _index.TryMatch(_path, _valueDestination, out var written);
        return succeeded ? written : -1;
    }

    [Benchmark]
    public int[] Trie_MatchToArray()
    {
        return _index.MatchToArray(_path);
    }

    [Benchmark]
    public int Trie_MatchDetailedToSpans()
    {
        return _index.MatchDetailed(_path, _matchDestination, _captureDestination, out _);
    }

    [Benchmark]
    public PatternMatchResult<string, int>[] Trie_MatchDetailedToArray()
    {
        return _index.MatchDetailedToArray(_path);
    }

    private void AddMarketData(
        PattrnIndexBuilder<string, int> builder,
        int marketCount,
        int symbolCount,
        bool includeWildcards,
        bool includePrefixes)
    {
        var value = 0;

        for (var market = 0; market < marketCount; market++)
        {
            if (includePrefixes)
            {
                Add(builder, ["market", $"M{market}"], value++);
            }

            for (var symbol = 0; symbol < symbolCount; symbol++)
            {
                Add(builder, ["market", $"M{market}", $"S{symbol}"], value++);
            }
        }

        if (!includeWildcards)
        {
            return;
        }

        for (var market = 0; market < marketCount; market++)
        {
            Add(builder, ["market", $"M{market}", "*"], value++);
        }

        for (var symbol = 0; symbol < symbolCount; symbol++)
        {
            Add(builder, ["market", "*", $"S{symbol}"], value++);
        }

        Add(builder, ["market", "*", "*"], value++);
    }

    private void AddDuplicateHeavy(PattrnIndexBuilder<string, int> builder)
    {
        for (var value = 0; value < 64; value++)
        {
            Add(builder, ["market", "M42", "S42"], value);
            Add(builder, ["market", "M42", "*"], value);
            Add(builder, ["market", "*", "S42"], value);
            Add(builder, ["market", "*", "*"], value);
        }
    }

    private void AddSparseDeep(PattrnIndexBuilder<string, int> builder, int branchCount, int depth)
    {
        Add(builder, ["root", "target", "l2", "l3", "l4", "l5", "l6", "l7", "l8", "l9"], 0);

        for (var branch = 0; branch < branchCount; branch++)
        {
            var pattern = new string[depth];
            pattern[0] = "root";
            pattern[1] = $"branch-{branch}";
            for (var level = 2; level < depth; level++)
            {
                pattern[level] = $"l{level}";
            }

            Add(builder, pattern, branch + 1);
        }
    }

    private void AddWideFanOut(PattrnIndexBuilder<string, int> builder, int childCount)
    {
        for (var child = 0; child < childCount; child++)
        {
            Add(builder, ["root", $"child-{child}"], child);
        }
    }

    private void AddParameterHeavy(PattrnIndexBuilder<string, int> builder, int routeCount)
    {
        for (var route = 0; route < routeCount; route++)
        {
            AddPattern(builder, [
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Literal($"tenant-{route}"),
                PatternSegment<string>.Parameter("orderId")], route);
        }
    }

    private void AddCatchAllHeavy(PattrnIndexBuilder<string, int> builder, int routeCount)
    {
        for (var route = 0; route < routeCount; route++)
        {
            AddPattern(builder, [
                PatternSegment<string>.Literal("files"),
                PatternSegment<string>.Literal($"tenant-{route}"),
                PatternSegment<string>.CatchAll("path")], route);
        }
    }

    private void Add(PattrnIndexBuilder<string, int> builder, string[] pattern, int value)
    {
        builder.Add(pattern, value);
        _registrations.Add(new Registration(ToSegments(pattern), value));
    }

    private void AddPattern(PattrnIndexBuilder<string, int> builder, PatternSegment<string>[] pattern, int value)
    {
        builder.AddPattern(pattern, value);
        _registrations.Add(new Registration(pattern, value));
    }

    private static PatternSegment<string>[] ToSegments(string[] pattern)
    {
        var segments = new PatternSegment<string>[pattern.Length];
        for (var i = 0; i < pattern.Length; i++)
        {
            segments[i] = pattern[i] == "*"
                ? PatternSegment<string>.Wildcard()
                : PatternSegment<string>.Literal(pattern[i]);
        }

        return segments;
    }

    private static bool Matches(PatternSegment<string>[] pattern, string[] path, bool includePrefixMatches)
    {
        var depth = 0;

        for (var i = 0; i < pattern.Length; i++)
        {
            var segment = pattern[i];
            if (segment.IsCatchAll)
            {
                return includePrefixMatches || true;
            }

            if (depth >= path.Length)
            {
                return false;
            }

            if (segment.IsLiteral && !StringComparer.Ordinal.Equals(segment.LiteralValue, path[depth]))
            {
                return false;
            }

            depth++;
        }

        return includePrefixMatches || depth == path.Length;
    }

    private sealed record Registration(PatternSegment<string>[] Pattern, int Value);
}
