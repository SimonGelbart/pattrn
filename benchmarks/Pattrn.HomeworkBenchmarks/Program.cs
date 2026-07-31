using BenchmarkDotNet.Running;

namespace Homework;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--retained-memory")
        {
            Environment.ExitCode = Benchmarks.RetainedMemoryProbe.Run(args);
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
