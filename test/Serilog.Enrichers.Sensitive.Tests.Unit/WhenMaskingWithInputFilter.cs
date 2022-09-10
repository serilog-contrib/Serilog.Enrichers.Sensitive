using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        public SpecificValueMaskingOperator(string regexString) : base(regexString)
        {
        }

        public SpecificValueMaskingOperator(string regexString, RegexOptions options) : base(regexString, options)
        {
        }

        protected override bool ShouldMaskMatch(Match match)
        {
            return match.Value == "TestValue";
        }
    }
}
