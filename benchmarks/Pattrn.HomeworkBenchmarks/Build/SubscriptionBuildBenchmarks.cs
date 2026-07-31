using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Homework.Benchmarks.Scenarios;
using Homework.Routing;

namespace Homework.Benchmarks.Build;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[BenchmarkCategory("HomeworkBuildMatrix")]
public class SubscriptionBuildBenchmarks
{
    private Subscription[] _registrations = [];

    public IEnumerable<HomeworkBuildScenario> Scenarios() =>
        HomeworkBenchmarkScenarioCatalog.BuildScenarios();

    [ParamsSource(nameof(Scenarios))]
    public HomeworkBuildScenario Scenario { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        _registrations = HomeworkBenchmarkDataFactory.CreateBuild(Scenario);
        if (_registrations.Length != Scenario.RegistrationCount)
        {
            throw new InvalidOperationException("Build scenario registration count is invalid.");
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BuildFromScratch")]
    public ISubscriptionIndex CustomTree_BuildFromScratch() =>
        HomeworkBenchmarkDataFactory.CreateIndex(
            SubscriptionIndexImplementation.CustomTree,
            _registrations);

    [Benchmark]
    [BenchmarkCategory("BuildFromScratch")]
    public ISubscriptionIndex Pattrn_BuildFromScratch() =>
        HomeworkBenchmarkDataFactory.CreateIndex(
            SubscriptionIndexImplementation.Pattrn,
            _registrations);
}
