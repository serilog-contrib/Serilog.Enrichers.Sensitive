using System.Collections.Generic;
using Serilog.Core;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingDestructuredObject
    {
        private readonly InMemorySink _sink;
        private readonly Logger _logger;

        public WhenMaskingDestructuredObject()
        {
            _sink = new InMemorySink();

            _logger = new LoggerConfiguration()
                .WriteTo.Sink(_sink)
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskProperties.Add("TestProperty");
                    options.MaskingOperators = new List<IMaskingOperator> { new EmailAddressMaskingOperator() };
                })
                .CreateLogger();
        }

        [Fact]
        public void GivenLogMessageWithDestructuredObjectPropertyThatHasSensitiveData_SensitiveDataIsMasked()
        {
            var testObject = new TestObject();
            
            _logger.Information("Test message {@TestObject}", testObject);
            
            _sink
                .Should()
                .HaveMessage("Test message {@TestObject}")
                .Appearing()
                .Once()
                .WithProperty("TestObject")
                .HavingADestructuredObject()
                .WithProperty("TestProperty")
                .WithValue("***MASKED***");
        }

        [Fact]
        public void GivenLogMessageWithDestructuredObjectPropertyThatHasSensitiveDataInNestedProperty_SensitiveDataIsMasked()
        {
            var testObject = new TestObject();
            
            _logger.Information("Test message {@TestObject}", testObject);

            _sink
                .Should()
                .HaveMessage("Test message {@TestObject}")
                .Appearing()
                .Once()
                .WithProperty("TestObject")
                .HavingADestructuredObject()
                .WithProperty("Nested")
                .HavingADestructuredObject()
                .WithProperty("TestProperty")
                .WithValue("***MASKED***");
        }
    }

    public class TestObject
    {
        public string TestProperty { get; set; } = "james.bond@universalexports.com";

        public NestedTestObject Nested { get; set; } = new NestedTestObject();
    }

    public class NestedTestObject
    {
        public string TestProperty { get; set; } = "joe.blogs@example.com";
    }
}
