using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks;

[MemoryDiagnoser]
public class ExplainBenchmarks
{
    private PattrnIndex<string, int> _index = null!;
    private string[] _matchingPath = [];
    private string[] _noMatchPath = [];
    private string[] _captureHeavyPath = [];
    private PatternExplanationOptions _withRejectedCandidates;

    [GlobalSetup]
    public void Setup()
    {
        var builder = PattrnIndex<string, int>.Builder("*");

        builder.Add(["market", "NASDAQ", "MSFT"], 1);
        builder.Add(["market", "NASDAQ", "*"], 2);
        builder.AddPattern([
            PatternSegment<string>.Literal("market"),
            PatternSegment<string>.Parameter("exchange"),
            PatternSegment<string>.Parameter("symbol")], 3);
        builder.AddPattern([
            PatternSegment<string>.Literal("files"),
            PatternSegment<string>.CatchAll("path")], 4);

        _index = builder.Build();
        _matchingPath = ["market", "NASDAQ", "MSFT"];
        _noMatchPath = ["orders", "missing", "item"];
        _captureHeavyPath = ["files", "tenant-42", "reports", "daily.csv"];
        _withRejectedCandidates = PatternExplanationOptions.IncludeRejections;
    }

    [Benchmark]
    public PatternMatchExplanation<string, int> Explain_MatchingPath()
    {
        return _index.Explain(_matchingPath);
    }

    [Benchmark]
    public PatternMatchExplanation<string, int> Explain_MatchingPathWithRejectedCandidates()
    {
        return _index.Explain(_matchingPath, _withRejectedCandidates);
    }

    [Benchmark]
    public PatternMatchExplanation<string, int> Explain_NoMatchWithRejectedCandidates()
    {
        return _index.Explain(_noMatchPath, _withRejectedCandidates);
    }

    [Benchmark]
    public PatternMatchExplanation<string, int> Explain_CaptureHeavyPath()
    {
        return _index.Explain(_captureHeavyPath);
    }
}
