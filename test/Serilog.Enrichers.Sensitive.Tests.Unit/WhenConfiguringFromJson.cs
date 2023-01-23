using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit;

public class WhenConfiguringFromJson
{
    [Fact]
    public void GivenJsonConfigurationWithMaskValue_EnricherIsConfiguredWithTheCorrectMaskValue()
    {
        var jsonConfiguration = @"
{
  ""Serilog"": {
    ""Using"": [ ""Serilog.Enrichers.Sensitive"" ],
    ""Enrich"": [ {
        ""Name"": ""WithSensitiveDataMasking"",
        ""Args"": {
            ""options"": {
                ""MaskValue"": ""^^"",
                ""ExcludeProperties"": [ ""email"" ],
                ""Mode"": ""Globally""
            }
        }
    }]
  }
}
";
        var memoryStream = new MemoryStream();
        memoryStream.Write(Encoding.UTF8.GetBytes(jsonConfiguration));
        memoryStream.Seek(0, SeekOrigin.Begin);

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(memoryStream)
            .Build();

        var inMemorySink = new InMemorySink();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(inMemorySink)
            .CreateLogger();

        logger.Information("Test value foo@bar.net");

        inMemorySink
            .Should()
            .HaveMessage("Test value ^^", "the e-mail address is replaced with the configured masking value")
            .Appearing().Once();
    }
}