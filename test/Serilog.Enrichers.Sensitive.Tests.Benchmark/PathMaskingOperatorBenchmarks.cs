using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark;

[SimpleJob(RunStrategy.Throughput, warmupCount: 1)]
public class PathMaskingOperatorBenchmarks
{
    private const string PathInput = "/home/me";
    private const string MaskValue = "***MASKED***";
    
    private readonly Regex PathRegex =
        new(@"^(?:[a-zA-Z]\:|\\\\[\w-]+\\[\w-]+\$?|[\/][^\/\0]+)+(\\[^\\/:*?""<>|]*)*(\\?)?$");
    
    private readonly Regex PathCompiledRegex =
        new(@"^(?:[a-zA-Z]\:|\\\\[\w-]+\\[\w-]+\$?|[\/][^\/\0]+)+(\\[^\\/:*?""<>|]*)*(\\?)?$",
            RegexOptions.Compiled);
    
    [Benchmark]
    public string PathRegexReplace()
    {
        string result = null;
        for (var i = 0; i < 10000; i++)
            result = PathRegex.Replace(PathInput, MaskValue);
        return result;
    }

    [Benchmark]
    public string PathRegexCompiledReplace()
    {
        string result = null;
        for (var i = 0; i < 10000; i++)
            result = PathCompiledRegex.Replace(PathInput, MaskValue);
        return result;
    }
}