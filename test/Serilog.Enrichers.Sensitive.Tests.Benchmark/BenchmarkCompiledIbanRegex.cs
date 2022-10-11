using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput, warmupCount: 1)]
public class BenchmarkCompiledIbanRegex
{
    private const string MaskValue = "***MASKED***";
    private const string IbanInput = "NL02ABNA0123456789";

    private readonly Regex IbanRegex = new("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");

    private readonly Regex IbanRegexCompiled =
        new("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}", RegexOptions.Compiled);


    [Params(10000)] public int N;

    [Benchmark(Baseline = true)]
    public string IbanRegexReplace()
    {
        var result = IbanRegex.Replace(IbanInput, MaskValue);

        return result;
    }

    [Benchmark(Baseline = false)]
    public string IbanRegexCompiledReplace()
    {
        var result = IbanRegexCompiled.Replace(IbanInput, MaskValue);

        return result;
    }
}