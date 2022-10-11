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
                var (wasTemplateMasked, messageTemplateText) = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

                // Only replace the template if it was actually masked
                if (wasTemplateMasked)
                {
                    _messageTemplateBackingField.SetValue(logEvent, Parser.Parse(messageTemplateText));
                }

                foreach (var property in logEvent.Properties.ToList())
                {
                    var (wasMasked, maskedValue) = MaskProperty(property);

                    // Only update the property if it was actually masked
                    if (wasMasked)
                    {
                        logEvent
                            .AddOrUpdateProperty(
                                new LogEventProperty(
                                    property.Key,
                                    maskedValue));
                    }
                }
            }
        }

        private (bool, LogEventPropertyValue?) MaskProperty(KeyValuePair<string, LogEventPropertyValue> property)
        {
            if (_excludeProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
            {
                return (false, null);
            }

            if (_maskProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
            {
                return (true, new ScalarValue(_maskValue));
            }

            switch (property.Value)
            {
                case ScalarValue { Value: string stringValue }:
                    {
                        var (wasMasked, maskedValue) = ReplaceSensitiveDataFromString(stringValue);

                        if (wasMasked)
                        {
                            return (true, new ScalarValue(maskedValue));
                        }

                        return (false, null);
                    }
                case StructureValue structureValue:
                    {
                        var propList = new List<LogEventProperty>();
                        var anyMasked = false;
                        foreach (var prop in structureValue.Properties)
                        {
                            var (wasMasked, maskedValue) = MaskProperty(new KeyValuePair<string, LogEventPropertyValue>(prop.Name, prop.Value));

                            if (wasMasked)
                            {
                                anyMasked = true;
                                propList.Add(new LogEventProperty(prop.Name, maskedValue));
                            }
                            else
                            {
                                propList.Add(prop);
                            }
                        }

                        return (anyMasked, new StructureValue(propList));
                    }
                default:
                    return (false, null);
            }
        }

        private (bool, string) ReplaceSensitiveDataFromString(string input)
        {
            var isMasked = false;

            foreach (var maskingOperator in _maskingOperators)
            {
                var maskResult = maskingOperator.Mask(input, _maskValue);

                if (maskResult.Match)
                {
                    isMasked = true;
                    input = maskResult.Result;
                }
            }

            return (isMasked, input);
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