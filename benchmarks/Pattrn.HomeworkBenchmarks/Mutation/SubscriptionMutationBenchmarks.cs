using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Homework.Benchmarks.Scenarios;
using Homework.Routing;

namespace Homework.Benchmarks.Mutation;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[BenchmarkCategory("HomeworkMutationMatrix")]
public class SubscriptionMutationBenchmarks
{
    private HomeworkMutationData _data = null!;
    private SubscriptionIndex _custom = null!;
    private PattrnSubscriptionIndex _pattrn = null!;
    private object _publicationCurrent = null!;
    private object _publicationNext = null!;

    public IEnumerable<HomeworkMutationScenario> Scenarios() =>
        HomeworkBenchmarkScenarioCatalog.MutationScenarios();

    [ParamsSource(nameof(Scenarios))]
    public HomeworkMutationScenario Scenario { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _data = HomeworkBenchmarkDataFactory.CreateMutation(Scenario);
        var first = new PattrnSubscriptionIndex();
        first.AddSubscriptions(_data.Registrations);
        var second = new PattrnSubscriptionIndex();
        second.AddSubscriptions(_data.Registrations);
        second.AddSubscriptions(_data.Additions.AsSpan(0, 1).ToArray());
        _publicationCurrent = first.CaptureSnapshot();
        _publicationNext = second.CaptureSnapshot();
    }

    [IterationSetup(Targets =
    [
        nameof(CustomTree_Add1),
        nameof(Pattrn_Add1),
        nameof(CustomTree_Add100),
        nameof(Pattrn_Add100),
        nameof(CustomTree_Add10000),
        nameof(Pattrn_Add10000),
        nameof(CustomTree_Remove1),
        nameof(Pattrn_Remove1),
        nameof(CustomTree_Remove100),
        nameof(Pattrn_Remove100),
        nameof(CustomTree_RemoveConsumer),
        nameof(Pattrn_RemoveConsumer),
    ])]
    public void ResetIndexes()
    {
        _custom = new SubscriptionIndex();
        _custom.AddSubscriptions(_data.Registrations);
        _pattrn = new PattrnSubscriptionIndex();
        _pattrn.AddSubscriptions(_data.Registrations);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Add1")]
    public void CustomTree_Add1() => _custom.AddSubscriptions(_data.Additions.AsSpan(0, 1).ToArray());

    [Benchmark]
    [BenchmarkCategory("Add1")]
    public void Pattrn_Add1() => _pattrn.AddSubscriptions(_data.Additions.AsSpan(0, 1).ToArray());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Add100")]
    public void CustomTree_Add100() => _custom.AddSubscriptions(_data.Additions.AsSpan(0, 100).ToArray());

    [Benchmark]
    [BenchmarkCategory("Add100")]
    public void Pattrn_Add100() => _pattrn.AddSubscriptions(_data.Additions.AsSpan(0, 100).ToArray());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Add10000")]
    public void CustomTree_Add10000() => _custom.AddSubscriptions(_data.Additions);

    [Benchmark]
    [BenchmarkCategory("Add10000")]
    public void Pattrn_Add10000() => _pattrn.AddSubscriptions(_data.Additions);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Remove1")]
    public void CustomTree_Remove1() => _custom.RemoveSubscriptions(_data.Removals.AsSpan(0, 1).ToArray());

    [Benchmark]
    [BenchmarkCategory("Remove1")]
    public void Pattrn_Remove1() => _pattrn.RemoveSubscriptions(_data.Removals.AsSpan(0, 1).ToArray());

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Remove100")]
    public void CustomTree_Remove100() => _custom.RemoveSubscriptions(_data.Removals);

    [Benchmark]
    [BenchmarkCategory("Remove100")]
    public void Pattrn_Remove100() => _pattrn.RemoveSubscriptions(_data.Removals);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("RemoveConsumer")]
    public void CustomTree_RemoveConsumer() => _custom.RemoveSubscriptionsForConsumer(_data.HeavyConsumer);

    [Benchmark]
    [BenchmarkCategory("RemoveConsumer")]
    public void Pattrn_RemoveConsumer() => _pattrn.RemoveSubscriptionsForConsumer(_data.HeavyConsumer);

    [Benchmark]
    [BenchmarkCategory("PublishSnapshot")]
    public object Pattrn_PublishSnapshot()
    {
        var next = _publicationNext;
        _publicationNext = Volatile.Read(ref _publicationCurrent);
        Volatile.Write(ref _publicationCurrent, next);
        return next;
    }
}
