using BenchmarkDotNet.Attributes;
using Homework.Routing;
using System.Runtime.CompilerServices;

namespace Pattrn.LeanCoreBenchmarks;

public enum BatchReplacementScenario
{
    RebuildOneAffectedMessageType,
    Replace100Subscriptions,
    ReplaceOneConsumerSubscriptionSet,
    PublishCompiledSnapshot,
}

[MemoryDiagnoser]
public class BatchReplacementBenchmarks
{
    [Params(
        LeanImplementation.CustomTree,
        LeanImplementation.Pattrn,
        LeanImplementation.GroupedGeneral,
        LeanImplementation.GroupedLean)]
    public LeanImplementation Implementation { get; set; }

    [ParamsSource(nameof(Scenarios))]
    public BatchReplacementScenario Scenario { get; set; }

    private Subscription[] _allSubscriptions = [];
    private Subscription[] _oneType = [];
    private Subscription[] _replace100 = [];
    private Subscription[][] _replaceConsumerTypes = [];
    private object _currentSnapshot = new();
    private object _nextSnapshot = new();

    public static IEnumerable<BatchReplacementScenario> Scenarios() =>
        Enum.GetValues<BatchReplacementScenario>();

    [GlobalSetup]
    public void Setup()
    {
        _allSubscriptions = LeanBenchmarkData.CreateOriginalSubscriptions();
        _oneType = _allSubscriptions
            .Where(subscription => subscription.MessageTypeId.Equals(LeanBenchmarkData.OriginalMessageTypeId(0)))
            .ToArray();
        _replace100 = [.. _oneType];
        for (var index = 0; index < 100; index++)
        {
            _replace100[index] = new Subscription(
                new ClientId($"replacement-{index}"),
                _replace100[index].MessageTypeId,
                new ContentPattern($"replacement-{index}"));
        }

        _replaceConsumerTypes = Enumerable.Range(0, 10)
            .Select(typeIndex => _allSubscriptions
                .Where(subscription => subscription.MessageTypeId.Equals(LeanBenchmarkData.OriginalMessageTypeId(typeIndex)))
                .Select(subscription => subscription.ConsumerId.Equals(new ClientId("Client.0"))
                    ? new Subscription(
                        new ClientId("Client.0.Replacement"),
                        subscription.MessageTypeId,
                        subscription.ContentPattern)
                    : subscription)
                .ToArray())
            .ToArray();
    }

    [Benchmark]
    public int RebuildBatch()
    {
        if (Scenario == BatchReplacementScenario.PublishCompiledSnapshot)
        {
            var next = _nextSnapshot;
            _nextSnapshot = Volatile.Read(ref _currentSnapshot);
            Volatile.Write(ref _currentSnapshot, next);
            return RuntimeHelpers.GetHashCode(next);
        }

        if (Scenario == BatchReplacementScenario.ReplaceOneConsumerSubscriptionSet)
        {
            var total = 0;
            foreach (var subscriptions in _replaceConsumerTypes)
            {
                total += Build(Implementation, subscriptions);
            }

            return total;
        }

        return Build(
            Implementation,
            Scenario == BatchReplacementScenario.Replace100Subscriptions ? _replace100 : _oneType);
    }

    private static int Build(LeanImplementation implementation, Subscription[] subscriptions)
    {
        var index = LeanBenchmarkData.CreateIndex(implementation, subscriptions);
        return index.FindSubscriptions(
            subscriptions[0].MessageTypeId,
            new MessageRoutingContent("999", "1234")).Count();
    }
}
