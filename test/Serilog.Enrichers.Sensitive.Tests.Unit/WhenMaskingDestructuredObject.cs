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
                    options.MaskingOperators = new List<IMaskingOperator> { new EmailAddressMaskingOperator() };
                    options.MaskProperties.Add("SensitiveProperty");
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

        [Fact]
        public void GivenLogMessageWithDestructuredObjectPropertyWithoutSensitiveDataInNestedProperty_StructureValueIsUnchanged()
        {
            var testObject = new TestObject
            {
                TestProperty = "not sensitive",
                Nested = new NestedTestObject
                {
                    TestProperty = "also not sensitive"
                }
            };
            
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
                .WithValue("also not sensitive");
        }

        [Fact]
        public void GivenConfigurationToMaskSpecificPropertyAndLoggingADestructuredObject_PropertyOnObjectIsMasked()
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
                .WithProperty("SensitiveProperty")
                .WithValue("***MASKED***");
        }
    }

    public class TestObject
    {
        public string TestProperty { get; set; } = "james.bond@universalexports.com";
        public string SensitiveProperty { get; set; } = "Super sensitive data";
        public NestedTestObject Nested { get; set; } = new NestedTestObject();
    }

    public class NestedTestObject
    {
        public string TestProperty { get; set; } = "joe.blogs@example.com";
    }
}
