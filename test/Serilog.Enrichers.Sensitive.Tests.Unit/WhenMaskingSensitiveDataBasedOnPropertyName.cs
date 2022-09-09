using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingSensitiveDataBasedOnPropertyName
    {
        [Fact]
        public void GivenLogMessageHasSpecificProperty_PropertyValueIsMasked()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskProperties.Add("Email");
                })
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("Example {Email}", "this doesn't match the e-mail regex");

            inMemorySink
                .Should()
                .HaveMessage("Example {Email}")
                .Appearing()
                .Once()
                .WithProperty("Email")
                .WithValue("***MASKED***");
        }

        [Fact]
        public void GivenLogMessageHasSpecificPropertyAndLogMessageHasPropertyButLowerCase_PropertyValueIsMasked()
        {
            var inMemorySink = new InMemorySink();

            var logger =  new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskProperties.Add("Email");
                })
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("Example {email}", "this doesn't match the e-mail regex");

            inMemorySink
                .Should()
                .HaveMessage("Example {email}")
                .Appearing()
                .Once()
                .WithProperty("email")
                .WithValue("***MASKED***");
        }
    }
}
