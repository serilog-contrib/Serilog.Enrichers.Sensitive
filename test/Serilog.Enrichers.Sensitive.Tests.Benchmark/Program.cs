using BenchmarkDotNet.Running;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // BenchmarkRunner.Run<BenchmarkCompiledEmailRegex>();
            // BenchmarkRunner.Run<BenchmarkCompiledIbanRegex>();
            // BenchmarkRunner.Run<CreditCardMarkingBenchmarks>();
            BenchmarkRunner.Run<BenchmarkWildcardPropertyMatch>();
        }
    }
}
