using BenchmarkDotNet.Attributes;
using Pattrn.Routing;

namespace Pattrn.Benchmarks;

[MemoryDiagnoser]
public class RoutingBenchmarks
{
    private PattrnIndex<string, int> _index = null!;
    private string _template = string.Empty;
    private string _path = string.Empty;
    private string[] _preSplitPath = [];
    private string[] _splitDestination = [];
    private int[] _valueDestination = [];
    private PatternMatch<int>[] _matchDestination = [];
    private PatternCapture<string>[] _captureDestination = [];

    [Params(
        RoutingBenchmarkScenario.ParseSimple,
        RoutingBenchmarkScenario.ParseParameters,
        RoutingBenchmarkScenario.ParseCatchAll,
        RoutingBenchmarkScenario.MatchDetailed)]
    public RoutingBenchmarkScenario Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _template = Scenario switch
        {
            RoutingBenchmarkScenario.ParseSimple => "/orders/new",
            RoutingBenchmarkScenario.ParseParameters => "/customers/{customerId}/orders/{orderId}",
            RoutingBenchmarkScenario.ParseCatchAll => "/files/{*path}",
            _ => "/customers/{customerId}/orders/{orderId}"
        };

        _path = Scenario == RoutingBenchmarkScenario.ParseCatchAll
            ? "/files/a/b/c.txt"
            : "/customers/42/orders/99";

        var builder = PattrnIndex<string, int>.Builder("*");
        for (var i = 0; i < 1024; i++)
        {
            builder.AddRoute($"/customers/{{customerId}}/orders/order-{i}", i);
        }

        builder.AddRoute("/customers/{customerId}/orders/{orderId}", 2048);
        builder.AddRoute("/files/{*path}", 4096);
        _index = builder.Build();

        _preSplitPath = RoutePattern.SplitPath(_path);
        _splitDestination = new string[Math.Max(1, RoutePattern.GetPathSegmentCount(_path))];
        _valueDestination = new int[Math.Max(1, _index.GetMatchCountUpperBound(_preSplitPath))];
        _matchDestination = new PatternMatch<int>[Math.Max(1, _index.GetMatchCountUpperBound(_preSplitPath))];
        _captureDestination = new PatternCapture<string>[Math.Max(1, _index.GetCaptureCountUpperBound(_preSplitPath))];
    }

    [Benchmark]
    public PatternSegment<string>[] RoutePattern_Parse()
    {
        return RoutePattern.Parse(_template);
    }

    [Benchmark]
    public int RoutePattern_GetPathSegmentCount()
    {
        return RoutePattern.GetPathSegmentCount(_path);
    }

    [Benchmark]
    public string[] RoutePattern_SplitPath()
    {
        return RoutePattern.SplitPath(_path);
    }

    [Benchmark]
    public int RoutePattern_SplitPathToSpan()
    {
        return RoutePattern.SplitPath(_path, _splitDestination);
    }

    [Benchmark]
    public int RouteIndex_MatchPreSplitToSpan()
    {
        return _index.Match(_preSplitPath, _valueDestination);
    }

    [Benchmark]
    public int RouteIndex_MatchRouteWithCallerBufferToSpan()
    {
        var written = RoutePattern.SplitPath(_path, _splitDestination);
        return _index.Match(_splitDestination.AsSpan(0, written), _valueDestination);
    }

    [Benchmark]
    public int RouteIndex_MatchRouteToSpan()
    {
        return _index.MatchRoute(_path, _valueDestination);
    }

    [Benchmark]
    public int RouteIndex_MatchPreSplitDetailedToSpans()
    {
        return _index.MatchDetailed(_preSplitPath, _matchDestination, _captureDestination, out _);
    }

    [Benchmark]
    public int RouteIndex_MatchRouteWithCallerBufferDetailedToSpans()
    {
        var written = RoutePattern.SplitPath(_path, _splitDestination);
        return _index.MatchDetailed(_splitDestination.AsSpan(0, written), _matchDestination, _captureDestination, out _);
    }

    [Benchmark]
    public int RouteIndex_MatchRouteDetailedToSpans()
    {
        return _index.MatchRouteDetailed(_path, _matchDestination, _captureDestination, out _);
    }

    [Benchmark]
    public PatternMatchResult<string, int>[] RouteIndex_MatchRouteDetailedToArray()
    {
        return _index.MatchRouteDetailedToArray(_path);
    }
}
