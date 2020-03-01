using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingTheMessageTemplateInArea
    {
        [Fact]
        public void GivenNotInSensitiveArea_EmailAddressIsNotMasked()
        {
            _logger.Information("test@email.com");

            InMemorySink.Instance
                .Should()
                .HaveMessage("test@email.com");
        }

        [Fact]
        public void GivenNotInSensitiveArea_IbanIsNotMasked()
        {
            _logger.Information("NL02ABNA0123456789");

            InMemorySink.Instance
                .Should()
                .HaveMessage("NL02ABNA0123456789");
        }

        [Fact]
        public void GivenInSensitiveArea_EmailAddressIsMasked()
        {
            using (_logger.EnterSensitiveArea())
            {
                _logger.Information("test@email.com");
            }

            InMemorySink.Instance
                .Should()
                .HaveMessage("***MASKED***");
        }

        [Fact]
        public void GivenInSensitiveArea_IbanIsMasked()
        {
            using (_logger.EnterSensitiveArea())
            {
                _logger.Information("NL02ABNA0123456789");
            }

            InMemorySink.Instance
                .Should()
                .HaveMessage("***MASKED***");
        }

        private readonly ILogger _logger;

        public WhenMaskingTheMessageTemplateInArea()
        {
            _logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMaskingInArea()
                .WriteTo.InMemory()
                .CreateLogger();
        }
    }
}
