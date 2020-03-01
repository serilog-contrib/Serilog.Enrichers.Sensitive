using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingTheMessageTemplateGlobally
    {
        [Fact]
        public void GivenNotInSensitiveArea_EmailAddressIsMasked()
        {
            _logger.Information("test@email.com");

            InMemorySink.Instance
                .Should()
                .HaveMessage("***MASKED***");
        }

        [Fact]
        public void GivenNotInSensitiveArea_IbanIsMasked()
        {
            _logger.Information("NL02ABNA0123456789");

            InMemorySink.Instance
                .Should()
                .HaveMessage("***MASKED***");
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

        public WhenMaskingTheMessageTemplateGlobally()
        {
            _logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking()
                .WriteTo.InMemory()
                .CreateLogger();
        }
    }
}
