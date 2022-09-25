using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;
using Serilog.Parsing;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenUsingSinkInsteadOfEnricher
    {
        [Fact]
        public void ShowItWorks()
        {
            var inMemorySink = new InMemorySink();

            var logger = new LoggerConfiguration()
                .WriteTo.Masked(options =>
                    {
                        options.Mode = MaskingMode.Globally;
                        options.MaskingOperators = new List<IMaskingOperator> { new EmailAddressMaskingOperator() };
                    },
                    writeTo =>
                    {
                        // Add a bunch of sinks for demonstration purposes
                        writeTo.Sink(inMemorySink);
                        writeTo.Console();
                        writeTo.Debug();
                    })
                .Enrich.FromLogContext()
                .CreateLogger();

            // Log a message with sensitive data
            logger.Information("Test message {Email}", "joe.blogs@example.com");

            // Assert that the e-mail address has been masked
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
        public static LoggerConfiguration Masked(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            Action<SensitiveDataEnricherOptions> options, 
            Action<LoggerSinkConfiguration> writeTo)
        {
            return LoggerSinkConfiguration.Wrap(
                loggerSinkConfiguration,
                sink => new MaskingSink(sink, options),
                writeTo,
                LevelAlias.Minimum,
                null
            );
        }
    }

    // The masking sink is basically a copy of the enricher, only it uses the Emit()
    // method instead of Enrich(). This isn't really the interesting part.
    public class MaskingSink : ILogEventSink
    {
        private readonly MaskingMode _maskingMode;

        private static readonly MessageTemplateParser Parser = new MessageTemplateParser();
        private readonly FieldInfo _messageTemplateBackingField;
        private readonly List<IMaskingOperator> _maskingOperators;
        private readonly string _maskValue;
        private readonly List<string> _maskProperties;
        private readonly List<string> _excludeProperties;
        private readonly ILogEventSink _aggregateSink;

        public MaskingSink(
            ILogEventSink aggregateSink,
            Action<SensitiveDataEnricherOptions> options)
        {
            _aggregateSink = aggregateSink;
            
            var enricherOptions = new SensitiveDataEnricherOptions();

            if (options != null)
            {
                options(enricherOptions);
            }

            if (string.IsNullOrEmpty(enricherOptions.MaskValue))
            {
                throw new Exception("The mask must be a non-empty string");
            }
            
            _maskingMode = enricherOptions.Mode;
            _maskValue = enricherOptions.MaskValue;
            _maskProperties = enricherOptions.MaskProperties ?? new List<string>();
            _excludeProperties = enricherOptions.ExcludeProperties ?? new List<string>();

            var fields = typeof(LogEvent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            _messageTemplateBackingField = fields.SingleOrDefault(f => f.Name.Contains("<MessageTemplate>"));

            _maskingOperators = enricherOptions.MaskingOperators.ToList();
        }

        public void Emit(LogEvent logEvent)
        {
            Mask(logEvent);

            _aggregateSink.Emit(logEvent);
        }

        private void Mask(LogEvent logEvent)
        {
            if (_maskingMode == MaskingMode.Globally || SensitiveArea.Instance != null)
            {
                var messageTemplateText = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

                _messageTemplateBackingField.SetValue(logEvent, Parser.Parse(messageTemplateText));

                foreach (var property in logEvent.Properties.ToList())
                {
                    if (_excludeProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (_maskProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
                    {
                        logEvent.AddOrUpdateProperty(
                            new LogEventProperty(
                                property.Key,
                                new ScalarValue(_maskValue)));
                    }
                    else if (property.Value is ScalarValue { Value: string stringValue })
                    {
                        logEvent.AddOrUpdateProperty(
                            new LogEventProperty(
                                property.Key,
                                new ScalarValue(ReplaceSensitiveDataFromString(stringValue))));
                    }
                }
            }
        }

        private string ReplaceSensitiveDataFromString(string input)
        {
            foreach (var maskingOperator in _maskingOperators)
            {
                var maskResult = maskingOperator.Mask(input, _maskValue);

                if (maskResult.Match)
                {
                    input = maskResult.Result;
                }
            }

            return input;
        }
    }
}