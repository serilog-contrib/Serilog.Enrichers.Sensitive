using System;
using FluentAssertions;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingWithCustomMaskValue
    {
        [Fact]
        public void GivenMaskValueIsNull_ArgumentNullExceptionIsThrown()
        {
            Action action = () => new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options => options.MaskValue = null!)
                .CreateLogger();

            action
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenMaskValueIsEmptyString_ArgumentNullExceptionIsThrown()
        {
            Action action = () => new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(string.Empty)
                .CreateLogger();

            action
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenMaskValue_MaskedLogEntryContainsMaskValue()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking("SPECIFIC VALUE")
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("Example foo@bar.net");

            inMemorySink
                .Should()
                .HaveMessage("Example SPECIFIC VALUE");
        }

        [Fact]
        public void GivenMaskValueAndLogMessageHasSensitiveDataInProperty_PropertyContainsMaskValue()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking("SPECIFIC VALUE")
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("Example {Email}", "foo@bar.net");

            inMemorySink
                .Should()
                .HaveMessage("Example {Email}")
                .Appearing()
                .Once()
                .WithProperty("Email")
                .WithValue("SPECIFIC VALUE");
        }
    }
}
