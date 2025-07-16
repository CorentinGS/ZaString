using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace ZaString.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance;

        // Use this to select benchmarks from the console:
        var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
    }
}