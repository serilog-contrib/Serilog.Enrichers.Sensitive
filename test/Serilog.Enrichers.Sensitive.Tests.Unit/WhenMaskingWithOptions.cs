using System;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingWithOptions
    {
        [Theory]
        [InlineData("1234567890", 2, 2, true, "12******90")]
        [InlineData("1234567890", 2, 2, false, "12***90")]
        [InlineData("1234567890", 2, -5, false, "12***")]
        [InlineData("1234567890", 2, -5, true, "12********")]
        [InlineData("1234567890", -5, 2, false, "***90")]
        [InlineData("1234567890", -5, 2, true, "********90")]
        [InlineData("1234", 2, 2, true, "1***4")]
        [InlineData("1234", 2, 3, true, "1***4")]
        [InlineData("124", 2, 2, true, "1***")]
        [InlineData("12", 2, 2, true, SensitiveDataEnricher.DefaultMaskValue)]
        [InlineData("1234", 5,  MaskOptions.NotSet, true, "1***")]
        [InlineData("1234", MaskOptions.NotSet, 5, true, "***4")]
        public void GivenMaskOptionsWithInputShorterThanNumberOfCharactersThatShouldBeShown(string input, int showFirst, int showLast, bool preserveLength, string expectedValue)
        {
            var inMemorySink = new InMemorySink();
            var logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(
                    options => options.MaskProperties.Add(new MaskProperty { Name = "Prop", Options = new MaskOptions{ ShowFirst = showFirst, ShowLast = showLast, PreserveLength = preserveLength}}))
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("{Prop}", input);

            inMemorySink
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue(expectedValue);
        }

        [Theory]
        [InlineData(true, false ,false, false, "https://***/***?***")]
        [InlineData(true, true ,false, false, "https://example.com/***?***")]
        [InlineData(true, true,true, false, "https://example.com/some/sensitive/path?***")]
        [InlineData(true, false,true, true, "https://***/some/sensitive/path?foo=bar")]
        [InlineData(false, false,true, true, "***://***/some/sensitive/path?foo=bar")]
        [InlineData(false, false,true, false, "***://***/some/sensitive/path?***")]
        public void GivenUriMaskOptions(bool showScheme, bool showHost, bool showPath, bool showQuery,
            string expectedValue)
        {
            var inMemorySink = new InMemorySink();
            var logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking(
                    options => options.MaskProperties.Add(new MaskProperty { Name ="Prop", Options = new UriMaskOptions
                    {
                        ShowScheme  = showScheme,
                        ShowHost = showHost,
                        ShowPath = showPath,
                        ShowQueryString = showQuery
                    }}))
                .WriteTo.Sink(inMemorySink)
                .CreateLogger();

            logger.Information("{Prop}", new Uri("https://example.com/some/sensitive/path?foo=bar"));

            inMemorySink
                .Should()
                .HaveMessage("{Prop}")
                .Appearing().Once()
                .WithProperty("Prop")
                .WithValue(expectedValue);
        }
    }
}