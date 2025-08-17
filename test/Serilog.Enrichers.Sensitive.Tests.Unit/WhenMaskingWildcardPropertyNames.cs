using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit;

public class WhenMaskingWildcardPropertyNames
{
    [Fact]
    public void GivenWildcardAtStartOfPropertyNameAndLogMessagePropertyNameMatches_PropertyValueIsMasked()
    {
        var inMemorySink = new InMemorySink();
        var logger = new LoggerConfiguration()
            .Enrich.WithSensitiveDataMasking(
                options =>
                {
                    options.MaskProperties.Add(new MaskProperty { Name = "*Prop", Options = new MaskOptions { WildcardMatch = true }});
                    options.MaskValue = "**MASKED**";
                })
            .WriteTo.Sink(inMemorySink)
            .CreateLogger();

        logger.Information("{SomeProp}", "UNMASKED");

        inMemorySink
            .Should()
            .HaveMessage("{SomeProp}")
            .Appearing().Once()
            .WithProperty("SomeProp")
            .WithValue("**MASKED**");
    }
    
    [Fact]
    public void GivenWildcardAtEndOfPropertyNameAndLogMessagePropertyNameMatches_PropertyValueIsMasked()
    {
        var inMemorySink = new InMemorySink();
        var logger = new LoggerConfiguration()
            .Enrich.WithSensitiveDataMasking(
                options =>
                {
                    options.MaskProperties.Add(new MaskProperty { Name = "Prop*", Options = new MaskOptions { WildcardMatch = true }});
                    options.MaskValue = "**MASKED**";
                })
            .WriteTo.Sink(inMemorySink)
            .CreateLogger();

        logger.Information("{PropTest}", "UNMASKED");

        inMemorySink
            .Should()
            .HaveMessage("{PropTest}")
            .Appearing().Once()
            .WithProperty("PropTest")
            .WithValue("**MASKED**");
    }
    
    [Fact]
    public void GivenWildcardAtStartAndEndOfPropertyNameAndLogMessagePropertyNameMatches_PropertyValueIsMasked()
    {
        var inMemorySink = new InMemorySink();
        var logger = new LoggerConfiguration()
            .Enrich.WithSensitiveDataMasking(
                options =>
                {
                    options.MaskProperties.Add(new MaskProperty { Name = "*Prop*", Options = new MaskOptions { WildcardMatch = true }});
                    options.MaskValue = "**MASKED**";
                })
            .WriteTo.Sink(inMemorySink)
            .CreateLogger();

        logger.Information("{SomePropTest}", "UNMASKED");

        inMemorySink
            .Should()
            .HaveMessage("{SomePropTest}")
            .Appearing().Once()
            .WithProperty("SomePropTest")
            .WithValue("**MASKED**");
    }
}