using BenchmarkDotNet.Running;

namespace Pattrn.LeanCoreBenchmarks;

internal static class LeanBenchmarkProgram
{
    public static async Task Main(string[] args)
    {
        if (args.Length > 0 && string.Equals(args[0], "--probe", StringComparison.OrdinalIgnoreCase))
        {
            Environment.ExitCode = await LeanCoreProbe.RunAsync(args[1..]);
        }
        else
        {
            BenchmarkSwitcher.FromAssembly(typeof(LeanBenchmarkProgram).Assembly).Run(args);
        }
    }
}
