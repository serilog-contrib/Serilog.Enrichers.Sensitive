using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Serilog.Core;
using Serilog.Events;
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

        [Fact]
        public void GivenDestructuredObjectHasListOfNestedObjects()
        {
            var testObject = new TestObject();

            _logger.Information("Test message {@TestObject}", testObject);

            var elements = _sink
                .Should()
                .HaveMessage("Test message {@TestObject}")
                .Appearing()
                .Once()
                .WithProperty("TestObject")
                .HavingADestructuredObject()
                .WithProperty("NestedList")
                .Subject
                .As<SequenceValue>()
                .Elements;

            foreach (var structureValue in elements.OfType<StructureValue>())
            {
                var testProperty = structureValue.Properties.Single(p => p.Name == "TestProperty");
                testProperty.Value.ToString().Should().Be("\"***MASKED***\"");
                var sensitiveProperty = structureValue.Properties.Single(p => p.Name == "SensitiveProperty");
                sensitiveProperty.Value.ToString().Should().Be("\"***MASKED***\"");
            }
        }

        [Fact]
        public void GivenDestructuredObjectHasArrayOfNestedObjects()
        {
            var testObject = new TestObject();

            _logger.Information("Test message {@TestObject}", testObject);

            var elements = _sink
                .Should()
                .HaveMessage("Test message {@TestObject}")
                .Appearing()
                .Once()
                .WithProperty("TestObject")
                .HavingADestructuredObject()
                .WithProperty("NestedArray")
                .Subject
                .As<SequenceValue>()
                .Elements;

            foreach (var structureValue in elements.OfType<StructureValue>())
            {
                var testProperty = structureValue.Properties.Single(p => p.Name == "TestProperty");
                testProperty.Value.ToString().Should().Be("\"***MASKED***\"");
                var sensitiveProperty = structureValue.Properties.Single(p => p.Name == "SensitiveProperty");
                sensitiveProperty.Value.ToString().Should().Be("\"***MASKED***\"");
            }
        }

        [Fact]
        public void GivenDestructuredObjectHasCollectionOfNestedObjects()
        {
            var testObject = new TestObject();

            _logger.Information("Test message {@TestObject}", testObject);

            var elements = _sink
                .Should()
                .HaveMessage("Test message {@TestObject}")
                .Appearing()
                .Once()
                .WithProperty("TestObject")
                .HavingADestructuredObject()
                .WithProperty("NestedCollection")
                .Subject
                .As<SequenceValue>()
                .Elements;

            foreach (var structureValue in elements.OfType<StructureValue>())
            {
                var testProperty = structureValue.Properties.Single(p => p.Name == "TestProperty");
                testProperty.Value.ToString().Should().Be("\"***MASKED***\"");
                var sensitiveProperty = structureValue.Properties.Single(p => p.Name == "SensitiveProperty");
                sensitiveProperty.Value.ToString().Should().Be("\"***MASKED***\"");
            }
        }

        [Fact]
        public void GivenDestructuredObjectIsCollectionOfObjects()
        {
            var collection = new[] { new TestObject(), new TestObject() };

            _logger.Information("Test message {@Collection}", collection);

            var elements = _sink
                .Should()
                .HaveMessage("Test message {@Collection}")
                .Appearing()
                .Once()
                .WithProperty("Collection")
                .Subject
                .As<SequenceValue>()
                .Elements;

            foreach (var structureValue in elements.OfType<StructureValue>())
            {
                var testProperty = structureValue.Properties.Single(p => p.Name == "TestProperty");
                testProperty.Value.ToString().Should().Be("\"***MASKED***\"");
                var sensitiveProperty = structureValue.Properties.Single(p => p.Name == "SensitiveProperty");
                sensitiveProperty.Value.ToString().Should().Be("\"***MASKED***\"");
            }
        }
    }

    public class TestObject
    {
        public string TestProperty { get; set; } = "james.bond@universalexports.com";
        public string SensitiveProperty { get; set; } = "Super sensitive data";
        public NestedTestObject Nested { get; set; } = new NestedTestObject();

        public List<NestedTestObject> NestedList { get; set; } = new() { new NestedTestObject(), new NestedTestObject() };

        public NestedTestObject[] NestedArray { get; set; } = { new NestedTestObject(), new NestedTestObject() };
        public Collection<NestedTestObject> NestedCollection { get; set; } = new() { new NestedTestObject(), new NestedTestObject() };
    }

    public class NestedTestObject
    {
        public string TestProperty { get; set; } = "joe.blogs@example.com";
        public string SensitiveProperty { get; set; } = "Super sensitive data";
    }
}
