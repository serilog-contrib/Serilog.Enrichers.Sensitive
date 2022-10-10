using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Enrichers.Sensitive
{
    internal class SensitiveDataEnricher : ILogEventEnricher
    {
        private readonly MaskingMode _maskingMode;
        public const string DefaultMaskValue = "***MASKED***";

        private static readonly MessageTemplateParser Parser = new MessageTemplateParser();
        private readonly FieldInfo _messageTemplateBackingField;
        private readonly List<IMaskingOperator> _maskingOperators;
        private readonly string _maskValue;
        private readonly List<string> _maskProperties;
        private readonly List<string> _excludeProperties;

        public SensitiveDataEnricher(
            Action<SensitiveDataEnricherOptions> options)
        {
            var enricherOptions = new SensitiveDataEnricherOptions();

            if (options != null)
            {
                options(enricherOptions);
            }

            if (string.IsNullOrEmpty(enricherOptions.MaskValue))
            {
                throw new ArgumentNullException("mask", "The mask must be a non-empty string");
            }

            _maskingMode = enricherOptions.Mode;
            _maskValue = enricherOptions.MaskValue;
            _maskProperties = enricherOptions.MaskProperties ?? new List<string>();
            _excludeProperties = enricherOptions.ExcludeProperties ?? new List<string>();

            var fields = typeof(LogEvent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            _messageTemplateBackingField = fields.SingleOrDefault(f => f.Name.Contains("<MessageTemplate>"));

            _maskingOperators = enricherOptions.MaskingOperators.ToList();
        }

        public SensitiveDataEnricher(
            MaskingMode maskingMode,
            IEnumerable<IMaskingOperator> maskingOperators,
            string mask = DefaultMaskValue)
        : this(options =>
        {
            options.MaskValue = mask;
            options.Mode = maskingMode;
            options.MaskingOperators = maskingOperators.ToList();
        })
        {
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_maskingMode == MaskingMode.Globally || SensitiveArea.Instance != null)
            {
                var messageTemplateText = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

                _messageTemplateBackingField.SetValue(logEvent, Parser.Parse(messageTemplateText));

                foreach (var property in logEvent.Properties.ToList())
                {
                    var maskedValue = MaskProperty(property);

                    logEvent
                        .AddOrUpdateProperty(
                            new LogEventProperty(
                                property.Key,
                                maskedValue));
                }
            }
        }

        private LogEventPropertyValue MaskProperty(KeyValuePair<string, LogEventPropertyValue> property)
        {
            if (_excludeProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
            {
                return property.Value;
            }

            if (_maskProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
            {
                return new ScalarValue(_maskValue);
            }

            if (property.Value is ScalarValue scalar && scalar.Value is string stringValue)
            {
                return new ScalarValue(ReplaceSensitiveDataFromString(stringValue));
            }
            if (property.Value is StructureValue structureValue)
            {
                var propList = new List<LogEventProperty>();

                foreach (var prop in structureValue.Properties)
                {
                    var maskedValue = MaskProperty(new KeyValuePair<string, LogEventPropertyValue>(prop.Name, prop.Value));

                    propList.Add(new LogEventProperty(prop.Name, maskedValue));
                }

                return new StructureValue(propList);
            }

            return property.Value;
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

        public static IEnumerable<IMaskingOperator> DefaultOperators => new List<IMaskingOperator>
        {
            new EmailAddressMaskingOperator(),
            new IbanMaskingOperator(),
            new CreditCardMaskingOperator()
        };

    }

    public enum MaskingMode
    {
        Globally,
        InArea
    }
}