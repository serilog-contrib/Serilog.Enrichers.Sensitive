using System.Collections.Generic;
using System.Text.RegularExpressions;
using Serilog.Enrichers.Sensitive.MaskTypes;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingWithInputFilter
    {
        [Fact]
        public void GivenTestPropertyHasTestValue_ValueIsMasked()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskingOperators = new List<IMaskingOperator>
                    {
                        new SpecificValueMaskingOperator("[A-Za-z]*")
                    };
                })
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("TestValue");
            
            inMemorySink
                .Should()
                .HaveMessage("***MASKED***")
                .Appearing().Once();
        }

        [Fact]
        public void GivenTestPropertyHasSomeOtherValue_ValueIsNotMasked()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskingOperators = new List<IMaskingOperator>
                    {
                        new SpecificValueMaskingOperator("[A-Za-z]*")
                    };
                })
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("SomeOtherValue");
            
            inMemorySink
                .Should()
                .HaveMessage("SomeOtherValue")
                .Appearing().Once();
        }
    }

    internal class SpecificValueMaskingOperator : RegexMaskingOperator
    {
        public SpecificValueMaskingOperator(string regexString) : base(regexString, new FixedValueMask())
        {
        }

        public SpecificValueMaskingOperator(string regexString, RegexOptions options, IMaskType maskType = null) : base(regexString, options, maskType)
        {
        }

        protected override bool ShouldMaskMatch(Match match)
        {
            return match.Value == "TestValue";
        }
    }
}
