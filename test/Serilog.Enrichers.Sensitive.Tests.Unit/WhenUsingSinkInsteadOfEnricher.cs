using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenUsingSinkInsteadOfEnricher
    {
        [Fact]
        public void Dwim()
        {
            var inMemorySink = new InMemorySink();

            var logger = new LoggerConfiguration()
                .WriteTo.Sink(inMemorySink)
                .MaskSensitiveData()
                .Enrich.FromLogContext()
                .CreateLogger();

            logger.Information("Test message {Email}", "joe.blogs@example.com");
            
            inMemorySink
                .Should()
                .HaveMessage("Test message {Email}")
                .Appearing().Once()
                .WithProperty("Email")
                .WithValue("***MASKED***");
        }
    }

    public static class ExtensionMethods
    {
        public static LoggerConfiguration MaskSensitiveData(this LoggerConfiguration configuration)
        {
            var maskingSink = new MaskingSink(configuration);

            return configuration
                .WriteTo.Sink(maskingSink);
        }
    }

    public class MaskingSink : ILogEventSink
    {
        public MaskingSink(LoggerConfiguration configuration)
        {
            
        }

        public void Emit(LogEvent logEvent)
        {
        }
    }
}