using BenchmarkDotNet.Attributes;

namespace Pattrn.Benchmarks;

[MemoryDiagnoser]
public class BuilderBenchmarks
{
    private (PatternSegment<string>[] Pattern, int Value)[] _registrations = [];

    [Params(
        BuilderBenchmarkScenario.BuildLargeExact,
        BuilderBenchmarkScenario.BuildLargeParameters,
        BuilderBenchmarkScenario.CompileDiagnosticsClean,
        BuilderBenchmarkScenario.CompileDiagnosticsDuplicate)]
    public BuilderBenchmarkScenario Scenario { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _registrations = Scenario switch
        {
            BuilderBenchmarkScenario.BuildLargeParameters => CreateParameterRegistrations(4096),
            BuilderBenchmarkScenario.CompileDiagnosticsDuplicate => CreateAmbiguousRegistrations(2048),
            _ => CreateExactRegistrations(4096)
        };
    }

    [Benchmark]
    public PattrnIndex<string, int> Build()
    {
        var builder = CreateBuilder();
        return builder.Build();
    }

    [Benchmark]
    public PattrnDiagnosticReport CompileWithDiagnostics()
    {
        var registrations = _registrations
            .Select(registration => PattrnRegistration<string, int>.Create(registration.Pattern, registration.Value))
            .ToArray();
        return PattrnIndex<string, int>.CompileWithDiagnostics(
            registrations,
            new PattrnCompileOptions
            {
                DuplicatePatternPolicy = Scenario == BuilderBenchmarkScenario.CompileDiagnosticsDuplicate
                    ? DuplicatePatternPolicy.Warn
                    : DuplicatePatternPolicy.Allow
            }).Report;
    }

    private PattrnIndexBuilder<string, int> CreateBuilder()
    {
        var builder = PattrnIndex<string, int>.Builder("*");
        foreach (var (pattern, value) in _registrations)
        {
            builder.AddPattern(pattern, value);
        }

        return builder;
    }

    private static (PatternSegment<string>[] Pattern, int Value)[] CreateExactRegistrations(int count)
    {
        var registrations = new (PatternSegment<string>[], int)[count];
        for (var i = 0; i < count; i++)
        {
            registrations[i] = ([
                PatternSegment<string>.Literal("root"),
                PatternSegment<string>.Literal($"child-{i}"),
                PatternSegment<string>.Literal("leaf")], i);
        }

        return registrations;
    }

    private static (PatternSegment<string>[] Pattern, int Value)[] CreateParameterRegistrations(int count)
    {
        var registrations = new (PatternSegment<string>[], int)[count];
        for (var i = 0; i < count; i++)
        {
            registrations[i] = ([
                PatternSegment<string>.Literal("tenant"),
                PatternSegment<string>.Literal($"tenant-{i}"),
                PatternSegment<string>.Parameter("resource")], i);
        }

        return registrations;
    }

    private static (PatternSegment<string>[] Pattern, int Value)[] CreateAmbiguousRegistrations(int count)
    {
        var registrations = new (PatternSegment<string>[], int)[count * 2];
        for (var i = 0; i < count; i++)
        {
            registrations[i * 2] = ([
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Literal($"tenant-{i}"),
                PatternSegment<string>.Parameter("id")], i);
            registrations[(i * 2) + 1] = ([
                PatternSegment<string>.Literal("orders"),
                PatternSegment<string>.Literal($"tenant-{i}"),
                PatternSegment<string>.Parameter("name")], i);
        }

        return registrations;
    }
}
