using BenchmarkDotNet.Attributes;
using Homework.Routing;

namespace Pattrn.LeanCoreBenchmarks;

[MemoryDiagnoser]
public class FocusedLookupBenchmarks
{
    [ParamsSource(nameof(Cases))]
    public LeanLookupCase Scenario { get; set; } = null!;

    private Subscription[] _registrations = [];
    private MessageTypeId _messageTypeId;
    private MessageRoutingContent _content;
    private ISubscriptionIndex _custom = null!;
    private PattrnSubscriptionIndex _pattrn = null!;
    private GroupedPattrnSubscriptionIndex _groupedGeneral = null!;
    private GroupedPattrnSubscriptionIndex _groupedLean = null!;
    private MessageRouter _customRouter = null!;
    private MessageRouter _pattrnRouter = null!;
    private MessageRouter _groupedGeneralRouter = null!;
    private MessageRouter _groupedLeanRouter = null!;
    private Subscription[] _pattrnDestination = [];
    private Subscription[] _groupedGeneralDestination = [];
    private Subscription[] _groupedLeanDestination = [];
    private FocusedBenchmarkMessage _message = null!;

    public static IEnumerable<LeanLookupCase> Cases() => LeanBenchmarkData.LookupCases;

    [GlobalSetup]
    public void Setup()
    {
        (_registrations, _messageTypeId, _content) = LeanBenchmarkData.CreateLookup(Scenario);
        _custom = LeanBenchmarkData.CreateIndex(LeanImplementation.CustomTree, _registrations);
        _pattrn = (PattrnSubscriptionIndex)LeanBenchmarkData.CreateIndex(LeanImplementation.Pattrn, _registrations);
        _groupedGeneral = (GroupedPattrnSubscriptionIndex)LeanBenchmarkData.CreateIndex(LeanImplementation.GroupedGeneral, _registrations);
        _groupedLean = (GroupedPattrnSubscriptionIndex)LeanBenchmarkData.CreateIndex(LeanImplementation.GroupedLean, _registrations);
        _customRouter = new MessageRouter(_custom);
        _pattrnRouter = new MessageRouter(_pattrn);
        _groupedGeneralRouter = new MessageRouter(_groupedGeneral);
        _groupedLeanRouter = new MessageRouter(_groupedLean);
        _message = new FocusedBenchmarkMessage(_content);
        _pattrnDestination = new Subscription[Math.Max(1, _pattrn.GetMatchCountUpperBound(_messageTypeId, _content))];
        _groupedGeneralDestination = new Subscription[Math.Max(1, _groupedGeneral.GetMatchCountUpperBound(_messageTypeId, _content))];
        _groupedLeanDestination = new Subscription[Math.Max(1, _groupedLean.GetMatchCountUpperBound(_messageTypeId, _content))];

        Validate(_custom);
        Validate(_pattrn);
        Validate(_groupedGeneral);
        Validate(_groupedLean);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CoreLookup")]
    public int CustomTree_CoreLookup() => Consume(_custom.FindSubscriptions(_messageTypeId, _content));

    [Benchmark]
    [BenchmarkCategory("CoreLookup")]
    public int Pattrn_CoreLookup() => Consume(_pattrn.FindSubscriptions(_messageTypeId, _content));

    [Benchmark]
    [BenchmarkCategory("CoreLookup")]
    public int GroupedGeneral_CoreLookup() => Consume(_groupedGeneral.FindSubscriptions(_messageTypeId, _content));

    [Benchmark]
    [BenchmarkCategory("CoreLookup")]
    public int GroupedLean_CoreLookup() => Consume(_groupedLean.FindSubscriptions(_messageTypeId, _content));

    [Benchmark]
    [BenchmarkCategory("CallerBuffer")]
    public int Pattrn_CallerBuffer() => _pattrn.TryFindSubscriptions(
        _messageTypeId,
        _content,
        _pattrnDestination,
        out var written) ? written : -1;

    [Benchmark]
    [BenchmarkCategory("CallerBuffer")]
    public int GroupedGeneral_CallerBuffer() => _groupedGeneral.TryFindSubscriptions(
        _messageTypeId,
        _content,
        _groupedGeneralDestination,
        out var written) ? written : -1;

    [Benchmark]
    [BenchmarkCategory("CallerBuffer")]
    public int GroupedLean_CallerBuffer() => _groupedLean.TryFindSubscriptions(
        _messageTypeId,
        _content,
        _groupedLeanDestination,
        out var written) ? written : -1;

    [Benchmark]
    [BenchmarkCategory("EndToEnd")]
    public int CustomTree_EndToEnd() => Consume(_customRouter.GetConsumers(_message));

    [Benchmark]
    [BenchmarkCategory("EndToEnd")]
    public int Pattrn_EndToEnd() => Consume(_pattrnRouter.GetConsumers(_message));

    [Benchmark]
    [BenchmarkCategory("EndToEnd")]
    public int GroupedGeneral_EndToEnd() => Consume(_groupedGeneralRouter.GetConsumers(_message));

    [Benchmark]
    [BenchmarkCategory("EndToEnd")]
    public int GroupedLean_EndToEnd() => Consume(_groupedLeanRouter.GetConsumers(_message));

    private void Validate(ISubscriptionIndex index)
    {
        var count = index.FindSubscriptions(_messageTypeId, _content).Count();
        if (count != Scenario.MatchCount)
        {
            throw new InvalidOperationException(
                $"{index.GetType().Name} returned {count}, expected {Scenario.MatchCount} for {Scenario}.");
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

    private static int Consume(IEnumerable<ClientId> consumers)
    {
        var count = 0;
        foreach (var unused in consumers)
        {
            count++;
        }

        return count;
    }

}
