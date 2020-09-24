using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BenchmarkCompiledRegex>();
        }
    }

    public class BenchmarkCompiledRegex
    {
        private readonly Regex EmailRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
        private readonly Regex EmailRegexCompiled = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])", RegexOptions.Compiled);
        private readonly Regex IbanRegex = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");
        private readonly Regex IbanRegexCompiled = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}", RegexOptions.Compiled);
        private const string MaskValue = "***MASKED***";
        private const string EmailInput = "test@email.com";
        private const string IbanInput = "NL02ABNA0123456789";

        [Benchmark]
        public string EmailRegexReplace()
        {
            string result = null;
            for (int i = 0; i < 10000; i++)
                result = EmailRegex.Replace(EmailInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string EmailRegexCompiledReplace()
        {
            string result = null;
            for (int i = 0; i < 10000; i++)
                result = EmailRegexCompiled.Replace(EmailInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string IbanRegexReplace()
        {
            string result = null;
            for (int i = 0; i < 10000; i++)
                result = IbanRegex.Replace(IbanInput, MaskValue);

            return result;
        }

        [Benchmark]
        public string IbanRegexCompiledReplace()
        {
            string result = null;
            for (int i = 0; i < 10000; i++)
                result = IbanRegexCompiled.Replace(IbanInput, MaskValue);

            return result;
        }
    }
}
