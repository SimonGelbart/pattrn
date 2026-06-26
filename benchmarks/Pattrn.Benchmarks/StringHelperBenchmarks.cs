using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks;

[MemoryDiagnoser]
public class StringHelperBenchmarks
{
    private IPattrnIndex<string, int> _index = null!;
    private IPattrnIndex<string, int> _normalizedIndex = null!;
    private string _dottedPath = string.Empty;
    private string _separatedPath = string.Empty;
    private string _normalizedPath = string.Empty;
    private StringNormalizationOptions _normalizationOptions = null!;
    private int[] _valueDestination = [];
    private int[] _normalizedValueDestination = [];

    [GlobalSetup]
    public void Setup()
    {
        _dottedPath = "market.NASDAQ.MSFT";
        _separatedPath = "market/NASDAQ/MSFT";
        _normalizedPath = "// market // nasdaq // msft //";

        var builder = PattrnIndex<string, int>.Builder("*");
        builder.AddDotted("market.NASDAQ.MSFT", 1);
        builder.AddDotted("market.NASDAQ.*", 2);
        builder.AddDotted("market.*.*", 3);
        _index = builder.Build();

        _valueDestination = new int[Math.Max(1, _index.GetDottedMatchCountUpperBound(_dottedPath))];

        _normalizationOptions = new StringNormalizationOptions('/')
        {
            EmptySegmentBehavior = StringEmptySegmentBehavior.Ignore,
            TrimBehavior = StringSegmentTrimBehavior.TrimWhitespace,
            NormalizeSegment = static segment => segment.ToUpperInvariant()
        };

        var normalizedBuilder = _normalizationOptions.CreateTokenizedBuilder<int>("*");
        normalizedBuilder.AddSeparated("market/NASDAQ/MSFT", 1, _normalizationOptions);
        normalizedBuilder.AddSeparated("market/NASDAQ/*", 2, _normalizationOptions);
        normalizedBuilder.AddSeparated("market/*/*", 3, _normalizationOptions);
        _normalizedIndex = normalizedBuilder.Build();

        var normalizedUpperBound = _normalizedIndex.GetSeparatedMatchCountUpperBound(
            _normalizedPath,
            _normalizationOptions);
        _normalizedValueDestination = new int[Math.Max(1, normalizedUpperBound)];
    }

    [Benchmark]
    public int String_MatchDottedToSpan()
    {
        return _index.MatchDotted(_dottedPath, _valueDestination);
    }

    [Benchmark]
    public int String_MatchSeparatedToSpan()
    {
        return _index.MatchSeparated(_separatedPath, _valueDestination, '/');
    }

    [Benchmark]
    public int[] String_MatchDottedToArray()
    {
        return _index.MatchDottedToArray(_dottedPath);
    }

    [Benchmark]
    public int String_MatchWithNormalizationOptions()
    {
        return _normalizedIndex.MatchSeparated(_normalizedPath, _normalizedValueDestination, _normalizationOptions);
    }
}
