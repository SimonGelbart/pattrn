using BenchmarkDotNet.Attributes;

#if PR125_BASE
using Pattrn;
#else
using Pattrn.Builders;
using Pattrn.Diagnostics;
using Pattrn.Matching;
using Pattrn.Patterns;
#endif

namespace Pattrn.Benchmarks.Core;

internal static class Pr125BenchmarkIndexFactory
{
    internal static PattrnIndex<string, int> Build(PattrnIndexBuilder<string, int> builder)
        => PattrnIndex<string, int>.Compile(
            builder.ToRegistrations(),
            new PattrnCompileOptions { DuplicatePatternPolicy = DuplicatePatternPolicy.Allow },
            MatchOptions.PreserveDuplicates);
}

/// <summary>
/// Task-scoped base/head comparison cases for PR #125 traversal changes.
///
/// The file is intentionally compatible with both namespace layouts. Copy it
/// to the base worktree and define PR125_BASE when building that worktree.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("PR125", "ExactOnly")]
public class Pr125ExactTraversalBenchmarks
{
    private static readonly string[] Path = ["market", "NASDAQ", "MSFT"];

    private PattrnIndex<string, int> _index = null!;
    private PatternMatch<int>[] _matchDestination = [];
    private int[] _valueDestination = [];

    [Params(8, 128, 1024)]
    public int CandidateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        for (var candidate = 0; candidate < CandidateCount; candidate++)
        {
            builder.Add(Path, candidate);
        }

        _index = Pr125BenchmarkIndexFactory.Build(builder);
        var upperBound = _index.GetMatchCountUpperBound(Path);
        if (upperBound != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} exact candidates, got {upperBound}.");
        }

        var owningMatchCount = _index.MatchToArray(Path).Length;
        if (owningMatchCount != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} owning exact matches, got {owningMatchCount}.");
        }

        var owningValueCount = _index.MatchValuesToArray(Path).Length;
        if (owningValueCount != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} owning exact values, got {owningValueCount}.");
        }

        _matchDestination = new PatternMatch<int>[upperBound];
        _valueDestination = new int[upperBound];
    }

    [Benchmark]
    [BenchmarkCategory("Owning")]
    public int Owning_ExactMatches()
        => _index.MatchToArray(Path).Length;

    [Benchmark]
    [BenchmarkCategory("CallerBuffer")]
    public int CallerBuffer_ExactMatches()
        => _index.TryMatch(Path, _matchDestination, out var written) ? written : -1;

    [Benchmark]
    [BenchmarkCategory("Owning")]
    public int Owning_NonDetailedValues()
        => _index.MatchValuesToArray(Path).Length;

    [Benchmark]
    [BenchmarkCategory("ExactOnly")]
    public int ExactOnly_GetMatchCountUpperBound()
        => _index.GetMatchCountUpperBound(Path);
}

/// <summary>Best-prefix and all-prefix owning result projections.</summary>
[MemoryDiagnoser]
[BenchmarkCategory("PR125", "Prefix")]
public class Pr125PrefixTraversalBenchmarks
{
    private static readonly string[] Path = ["market", "NASDAQ", "MSFT", "quote"];

    private PattrnIndex<string, int> _index = null!;

    [Params(8, 128, 1024)]
    public int CandidateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        // Keep one accepted root prefix so all-prefix enumeration exercises
        // both selection and traversal across more than one accepted depth.
        builder.AddPattern(Array.Empty<PatternSegment<string>>(), -1);
        for (var candidate = 0; candidate < CandidateCount; candidate++)
        {
            builder.Add(["market", "NASDAQ", "MSFT"], candidate);
        }

        _index = Pr125BenchmarkIndexFactory.Build(builder);

        var bestPrefixCount = _index.MatchPrefixToArray(Path).Length;
        if (bestPrefixCount != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} best-prefix candidates, got {bestPrefixCount}.");
        }

        var bestPrefixUpperBound = _index.GetPrefixMatchCountUpperBound(Path);
        if (bestPrefixUpperBound != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} best-prefix upper-bound candidates, got {bestPrefixUpperBound}.");
        }

        var allPrefixCount = _index.EnumeratePrefixMatchesToArray(Path).Length;
        if (allPrefixCount != CandidateCount + 1)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount + 1} all-prefix candidates, got {allPrefixCount}.");
        }

        var allPrefixUpperBound = _index.GetEnumeratePrefixMatchCountUpperBound(Path);
        if (allPrefixUpperBound != CandidateCount + 1)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount + 1} all-prefix upper-bound candidates, got {allPrefixUpperBound}.");
        }
    }

    [Benchmark]
    [BenchmarkCategory("Owning")]
    public int Owning_BestPrefixMatches()
        => _index.MatchPrefixToArray(Path).Length;

    [Benchmark]
    [BenchmarkCategory("Owning")]
    public int Owning_AllPrefixMatches()
        => _index.EnumeratePrefixMatchesToArray(Path).Length;
}

/// <summary>Detailed owning and caller-buffer capture projections.</summary>
[MemoryDiagnoser]
[BenchmarkCategory("PR125", "Detailed")]
public class Pr125DetailedTraversalBenchmarks
{
    private static readonly string[] Path = ["orders", "tenant-42", "order-42", "line-1"];

    private PattrnIndex<string, int> _index = null!;
    private PatternMatchDetailedSlice<int>[] _matchDestination = [];
    private PatternCaptureSlice<string>[] _captureDestination = [];

    [Params(8, 128, 1024)]
    public int CandidateCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        var pattern = new[]
        {
            PatternSegment<string>.Literal("orders"),
            PatternSegment<string>.Parameter("tenant"),
            PatternSegment<string>.Parameter("order"),
            PatternSegment<string>.CatchAll("path")
        };

        for (var candidate = 0; candidate < CandidateCount; candidate++)
        {
            builder.AddPattern(pattern, candidate);
        }

        _index = Pr125BenchmarkIndexFactory.Build(builder);

        var matchUpperBound = _index.GetMatchCountUpperBound(Path);
        if (matchUpperBound != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} detailed candidates, got {matchUpperBound}.");
        }

        var detailedResultCount = _index.MatchDetailedToArray(Path).Length;
        if (detailedResultCount != CandidateCount)
        {
            throw new InvalidOperationException(
                $"Expected {CandidateCount} detailed results, got {detailedResultCount}.");
        }

        var captureUpperBound = _index.GetCaptureCountUpperBound(Path);
        var expectedCaptureCount = CandidateCount * 3;
        if (captureUpperBound != expectedCaptureCount)
        {
            throw new InvalidOperationException(
                $"Expected {expectedCaptureCount} captures, got {captureUpperBound}.");
        }

        _matchDestination = new PatternMatchDetailedSlice<int>[matchUpperBound];
        _captureDestination = new PatternCaptureSlice<string>[
            Math.Max(1, captureUpperBound)];
    }

    [Benchmark]
    [BenchmarkCategory("Owning")]
    public int Detailed_OwningMatches()
        => _index.MatchDetailedToArray(Path).Length;

    [Benchmark]
    [BenchmarkCategory("CallerBuffer")]
    public int Detailed_CallerBufferMatches()
        => _index.MatchDetailed(
            Path,
            _matchDestination,
            _captureDestination,
            out _);
}
