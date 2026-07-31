using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Homework.Benchmarks.Scenarios;
using Homework.Routing;

namespace Homework.Benchmarks.Lookup;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[BenchmarkCategory("HomeworkLookupMatrix")]
public class SubscriptionLookupBenchmarks
{
    private HomeworkBenchmarkData _data = null!;
    private ISubscriptionIndex _custom = null!;
    private PattrnSubscriptionIndex _pattrn = null!;
    private MessageRouter _customRouter = null!;
    private MessageRouter _pattrnRouter = null!;
    private Subscription[] _destination = [];

    public IEnumerable<HomeworkLookupScenario> Scenarios() =>
        HomeworkBenchmarkScenarioCatalog.LookupScenarios();

    [ParamsSource(nameof(Scenarios))]
    public HomeworkLookupScenario Scenario { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = HomeworkBenchmarkDataFactory.CreateLookup(Scenario);
        _custom = HomeworkBenchmarkDataFactory.CreateIndex(
            SubscriptionIndexImplementation.CustomTree,
            _data.Registrations);
        _pattrn = (PattrnSubscriptionIndex)HomeworkBenchmarkDataFactory.CreateIndex(
            SubscriptionIndexImplementation.Pattrn,
            _data.Registrations);
        _customRouter = new MessageRouter(_custom);
        _pattrnRouter = new MessageRouter(_pattrn);
        _destination = new Subscription[
            Math.Max(1, _pattrn.GetMatchCountUpperBound(_data.MessageTypeId, _data.RoutingContent))];

        ValidateResultCount(_custom);
        ValidateResultCount(_pattrn);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CoreLookup")]
    public int CustomTree_CoreLookup() => Consume(
        _custom.FindSubscriptions(_data.MessageTypeId, _data.RoutingContent));

    [Benchmark]
    [BenchmarkCategory("CoreLookup")]
    public int Pattrn_CoreLookup() => Consume(
        _pattrn.FindSubscriptions(_data.MessageTypeId, _data.RoutingContent));

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LookupToList")]
    public List<Subscription> CustomTree_LookupToList() =>
        _custom.FindSubscriptions(_data.MessageTypeId, _data.RoutingContent).ToList();

    [Benchmark]
    [BenchmarkCategory("LookupToList")]
    public List<Subscription> Pattrn_LookupToList() =>
        _pattrn.FindSubscriptions(_data.MessageTypeId, _data.RoutingContent).ToList();

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("EndToEndRouter")]
    public List<ClientId> CustomTree_EndToEndRouter() =>
        _customRouter.GetConsumers(_data.Message).ToList();

    [Benchmark]
    [BenchmarkCategory("EndToEndRouter")]
    public List<ClientId> Pattrn_EndToEndRouter() =>
        _pattrnRouter.GetConsumers(_data.Message).ToList();

    [Benchmark]
    [BenchmarkCategory("PattrnOwningCapability")]
    public Subscription[] Pattrn_OwningArray() =>
        _pattrn.CaptureSnapshot().FindSubscriptions(_data.MessageTypeId, _data.RoutingContent);

    [Benchmark]
    [BenchmarkCategory("PattrnCallerBufferCapability")]
    public int Pattrn_CallerBuffer() =>
        _pattrn.TryFindSubscriptions(
            _data.MessageTypeId,
            _data.RoutingContent,
            _destination,
            out var written)
            ? written
            : -1;

    private void ValidateResultCount(ISubscriptionIndex index)
    {
        var actual = index.FindSubscriptions(_data.MessageTypeId, _data.RoutingContent).Count();
        if (actual != Scenario.MatchCount)
        {
            throw new InvalidOperationException(
                $"{index.GetType().Name} returned {actual} matches; expected {Scenario.MatchCount} for {Scenario}.");
        }
    }

    private static int Consume(IEnumerable<Subscription> subscriptions)
    {
        var count = 0;
        foreach (var unused in subscriptions)
        {
            count++;
        }

        return count;
    }
}
