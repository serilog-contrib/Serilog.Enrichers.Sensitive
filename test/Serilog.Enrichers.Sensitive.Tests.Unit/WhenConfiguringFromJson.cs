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
    public void GivenJsonConfigurationWithMaskingOperator_MaskingOperatorIsUsedAndFullMessageIsMasked()
    {
        var jsonConfiguration = @"
{
  ""Serilog"": {
    ""Using"": [ ""Serilog.Enrichers.Sensitive"" ],
    ""Enrich"": [ {
        ""Name"": ""WithSensitiveDataMasking"",
        ""Args"": {
            ""options"": {
                ""MaskValue"": ""MASK FROM JSON"",
                ""MaskingOperators"": [ ""Serilog.Enrichers.Sensitive.Tests.Unit.MyTestMaskingOperator, Serilog.Enrichers.Sensitive.Tests.Unit"" ]
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

        logger.Information("A test message");

        inMemorySink
            .Should()
            .HaveMessage("MASK FROM JSON", "the custom masking operator matches everything")
            .Appearing().Once();
    }
}

public class MyTestMaskingOperator : IMaskingOperator
{
    public static IMaskingOperator Instance = new MyTestMaskingOperator();
        
    public MaskingResult Mask(string input, string mask)
    {
        return new MaskingResult
        {
            Match = true,
            Result = mask
        };
    }
}