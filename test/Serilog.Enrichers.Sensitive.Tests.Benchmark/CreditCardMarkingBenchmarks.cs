using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark
{
    [SimpleJob(RunStrategy.Throughput, warmupCount: 1)]
    public class CreditCardMarkingBenchmarks
    {
        private const string CreditCardInput = "4111111111111111";
        private const string MaskValue = "***MASKED***";

        private readonly Regex CreditCardFullReplaceRegex =
            new(@"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})",
                RegexOptions.IgnoreCase);

        private readonly Regex CreditCardFullReplaceRegexCompiled =
            new(@"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Regex CreditCardPartialReplaceRegex =
            new(@"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})",
                RegexOptions.IgnoreCase);

        private readonly Regex CreditCardPartialReplaceRegexCompiled =
            new(@"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        [Params(10000)] public int N;

        [Benchmark]
        public string CreditCardPartialRegexReplace()
        {
            string result = null;
            for (var i = 0; i < 10000; i++)
                result = CreditCardPartialReplaceRegex.Replace(CreditCardInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string CreditCardRegexPartialCompiledReplace()
        {
            string result = null;
            for (var i = 0; i < 10000; i++)
                result = CreditCardPartialReplaceRegex.Replace(CreditCardInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string CreditCardFullRegexReplace()
        {
            string result = null;
            for (var i = 0; i < 10000; i++)
                result = CreditCardFullReplaceRegex.Replace(CreditCardInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string CreditCardRegexFullCompiledReplace()
        {
            string result = null;
            for (var i = 0; i < 10000; i++)
                result = CreditCardFullReplaceRegex.Replace(CreditCardInput, MaskValue);

            return result;
        }
    }
}