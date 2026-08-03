using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks.Core;

[MemoryDiagnoser]
[BenchmarkCategory("Compilation")]
public sealed class CompilerScalingBenchmarks
{
    [Params(1_000, 10_000, 50_000, 100_000)]
    public int RegistrationCount { get; set; }

    private PattrnRegistration<string, int>[] _registrations = [];

    [GlobalSetup]
    public void Setup()
    {
        _registrations = new PattrnRegistration<string, int>[RegistrationCount];
        for (var index = 0; index < _registrations.Length; index++)
        {
            _registrations[index] = PattrnRegistration<string, int>.Create(
                [PatternSegment<string>.Literal($"unique-{index}")],
                index);
        }
    }

    [Benchmark]
    public int CompileUniquePatterns() => PattrnIndex<string, int>.Compile(_registrations).PatternCount;
}
