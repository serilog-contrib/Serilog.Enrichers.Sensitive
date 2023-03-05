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
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("enricher-operator-config.json")
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

    [Fact]
    public void ReproCaseIssue25()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("enricher-config.json")
            .Build();

        var inMemorySink = new InMemorySink();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Sink(inMemorySink)
            .CreateLogger();

        logger.Information("A test message {secret}", "this is secret");

        inMemorySink
            .Should()
            .HaveMessage("A test message {secret}")
            .Appearing().Once()
            .WithProperty("secret")
            .WithValue("**SECRET**");
    }
}

public class MyTestMaskingOperator : IMaskingOperator
{
    private readonly bool _flip;

    public static IMaskingOperator Instance = new MyTestMaskingOperator();

    public MyTestMaskingOperator()
    {
    }

    public MaskingResult Mask(string input, string mask)
    {
        return new MaskingResult
        {
            Match = true,
            Result = mask
        };
    }
}