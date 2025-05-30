using Serilog.Core;
using Serilog.Sinks.InMemory;
using System;
using System.Text.RegularExpressions;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingLogEventWithNonStringScalarValue
    {
        private readonly InMemorySink _inMemorySnk;
        private readonly Logger _logger;

        [Fact]
        public void GivenPropertyValueIsUri_ValueIsMasked()
        {
            _logger.Information("Message {Prop}", new Uri("https://tempuri.org?someparam=SENSITIVE"));

            _inMemorySnk
                .Snapshot()
                .Should()
                .HaveMessage("Message {Prop}")
                .Appearing()
                .Once()
                .WithProperty("Prop")
                .WithValue(new Uri("https://tempuri.org?***MASKED***"));
        }

        [Fact]
        public void GivenPropertyValueIsTypeWithToStringOverride_ValueIsMasked()
        {
            _logger.Information("Message {Prop}", new TypeWithToStringOverride("my SECRET message"));

            _inMemorySnk
                .Snapshot()
                .Should()
                .HaveMessage("Message {Prop}")
                .Appearing()
                .Once()
                .WithProperty("Prop")
                .WithValue("my ***MASKED*** message");
        }

        public WhenMaskingLogEventWithNonStringScalarValue()
        {
            _inMemorySnk = new InMemorySink();

            _logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskingOperators.Add(new UriMaskingOperator());
                    options.MaskingOperators.Add(new TestRegexMaskingOperator());
                })
                .WriteTo.Sink(_inMemorySnk)
                .CreateLogger();
        }
    }

    public class TestRegexMaskingOperator : RegexMaskingOperator
    {
        public TestRegexMaskingOperator() : base("SECRET", RegexOptions.Compiled)
        {
        }
    }

    public class TypeWithToStringOverride
    {
        private readonly string _value;

        public TypeWithToStringOverride(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }
    }

    public class UriMaskingOperator : RegexMaskingOperator
    {
        private const string SomePattern =
            "someparam=.*?(.(?:&|$))";

        public UriMaskingOperator() : base(SomePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
        }

        protected override string PreprocessInput(string input, string? logPropertyName = null)
        {
            return input;
        }
    }
}
