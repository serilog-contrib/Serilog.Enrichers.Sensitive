using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingLogEventProperty
    {
        [Fact]
        public void GivenNotInSensitiveArea_EmailAddressIsNotMasked()
        {
            _logger.Information("{Prop}","test@email.com");

            InMemorySink.Instance
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue("test@email.com");
        }

        [Fact]
        public void GivenNotInSensitiveArea_IbanIsNotMasked()
        {
            _logger.Information("{Prop}", "NL02ABNA0123456789");

            InMemorySink.Instance
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue("NL02ABNA0123456789");
        }

        [Fact]
        public void GivenInSensitiveArea_EmailAddressIsMasked()
        {
            using (_logger.EnterSensitiveArea())
            {
                _logger.Information("{Prop}", "test@email.com");
            }

            InMemorySink.Instance
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue("***MASKED***");
        }

        [Fact]
        public void GivenInSensitiveArea_IbanIsMasked()
        {
            using (_logger.EnterSensitiveArea())
            {
                _logger.Information("{Prop}", "NL02ABNA0123456789");
            }

            InMemorySink.Instance
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue("***MASKED***");
        }

        private readonly ILogger _logger;

        public WhenMaskingLogEventProperty()
        {
            _logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking()
                .WriteTo.InMemory()
                .CreateLogger();
        }
    }
}
