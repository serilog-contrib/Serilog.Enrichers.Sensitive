using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Serilog.Enrichers.Sensitive.Tests.Benchmark
{
    [SimpleJob(RunStrategy.Throughput, warmupCount: 1)]
    public class BenchmarkWildcardPropertyMatch
    {
        private readonly MaskPropertyForTest _maskProperty;
        
        public BenchmarkWildcardPropertyMatch()
        {
            _maskProperty = new MaskPropertyForTest
            {
                Name = "*Prop",
                Options = new MaskOptions
                {
                    WildcardMatch = true
                }
            };
        }
        
        [Params(10000)] public int N;

        [Benchmark(Baseline = true)]
        public bool Baseline()
        {
            var result = _maskProperty.Invoke("SomeProp");

            return result;
        }

        [Benchmark(Baseline = false)]
        public bool WithCaching()
        {
            var result = _maskProperty.Invoke("SomeProp");

            return result;
        }
    }

    public class MaskPropertyForTest : MaskProperty
    {
        public bool Invoke(string propertyName)
        {
            return PropertyNameMatchesWildcard(propertyName);
        }

        public bool InvokeOriginal(string propertyName)
        {
            if (Name[0] == '*')
            {
                if (Name[Name.Length - 1] == '*')
                {
                    var match = Name.Substring(1, Name.Length - 2);
                    return propertyName.IndexOf(match, StringComparison.OrdinalIgnoreCase) > 0;
                }
                else
                {
                    var match = Name.Substring(1);
                    return propertyName.EndsWith(match, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (Name[Name.Length - 1] == '*')
            {
                var match = Name.Substring(0, Name.Length - 2);
                return propertyName.StartsWith(match, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}