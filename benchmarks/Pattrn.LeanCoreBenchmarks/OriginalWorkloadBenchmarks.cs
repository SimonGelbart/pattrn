using BenchmarkDotNet.Attributes;
using Homework.Routing;

namespace Pattrn.LeanCoreBenchmarks;

[MemoryDiagnoser]
public class OriginalWorkloadBenchmarks
{
    [Params(
        LeanImplementation.CustomTree,
        LeanImplementation.Pattrn,
        LeanImplementation.GroupedGeneral,
        LeanImplementation.GroupedLean)]
    public LeanImplementation Implementation { get; set; }

    private Subscription[] _registrations = [];
    private MessageTypeId _messageTypeId;
    private MessageRoutingContent _content;
    private ISubscriptionIndex _index = null!;
    private MessageRouter _router = null!;
    private Subscription[] _destination = [];
    private OriginalMessage0 _message = null!;

    [GlobalSetup]
    public void Setup()
    {
        (_registrations, _messageTypeId, _content) = LeanBenchmarkData.CreateOriginalQuery();
        _index = LeanBenchmarkData.CreateIndex(Implementation, _registrations);
        _router = new MessageRouter(_index);
        _message = new OriginalMessage0(_content);
        _destination = _index switch
        {
            PattrnSubscriptionIndex pattrn => new Subscription[Math.Max(1, pattrn.GetMatchCountUpperBound(_messageTypeId, _content))],
            GroupedPattrnSubscriptionIndex grouped => new Subscription[Math.Max(1, grouped.GetMatchCountUpperBound(_messageTypeId, _content))],
            _ => [],
        };

        var matches = _index.FindSubscriptions(_messageTypeId, _content).Count();
        if (matches != 30)
        {
            throw new InvalidOperationException($"Original workload returned {matches} matches; expected 30.");
        }
    }

    [Benchmark]
    [BenchmarkCategory("OriginalCoreLookup")]
    public int CoreLookup() => Consume(_index.FindSubscriptions(_messageTypeId, _content));

    [Benchmark]
    [BenchmarkCategory("OriginalEndToEnd")]
    public int EndToEndLookup() => Consume(_router.GetConsumers(_message));

    [Benchmark]
    [BenchmarkCategory("OriginalCallerBuffer")]
    public int CallerBufferLookup()
    {
        return _index switch
        {
            PattrnSubscriptionIndex pattrn => pattrn.TryFindSubscriptions(_messageTypeId, _content, _destination, out var pattrnWritten) ? pattrnWritten : -1,
            GroupedPattrnSubscriptionIndex grouped => grouped.TryFindSubscriptions(_messageTypeId, _content, _destination, out var groupedWritten) ? groupedWritten : -1,
            _ => Consume(_index.FindSubscriptions(_messageTypeId, _content)),
        };
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
