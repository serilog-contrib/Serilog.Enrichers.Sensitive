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

        private static readonly MessageTemplateParser Parser = new();
        private readonly FieldInfo _messageTemplateBackingField;
        private readonly List<IMaskingOperator> _maskingOperators;
        private readonly string _maskValue;
        private readonly List<string> _maskProperties;
        private readonly List<string> _excludeProperties;

        public SensitiveDataEnricher(SensitiveDataEnricherOptions options) 
            : this(options.Apply)
        {
        }

        public SensitiveDataEnricher(
            Action<SensitiveDataEnricherOptions>? options)
        {
            var enricherOptions = new SensitiveDataEnricherOptions(
                MaskingMode.Globally, 
                DefaultMaskValue, 
                DefaultOperators.Select(o => o.GetType().AssemblyQualifiedName),
                new List<string>(),
                new List<string>());

            if (options != null)
            {
                options(enricherOptions);
            }

            if (string.IsNullOrEmpty(enricherOptions.MaskValue))
            {
                throw new ArgumentNullException(nameof(enricherOptions.MaskValue), "The mask must be a non-empty string");
            }

            _maskingMode = enricherOptions.Mode;
            _maskValue = enricherOptions.MaskValue;
            _maskProperties = enricherOptions.MaskProperties;
            _excludeProperties = enricherOptions.ExcludeProperties;
            _maskingOperators = enricherOptions.MaskingOperators.ToList();

            var fields = typeof(LogEvent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            var backingField = fields.SingleOrDefault(f => f.Name.Contains("<MessageTemplate>"));
            
            if (backingField == null)
            {
                throw new InvalidOperationException(
                    "Could not find the backing field for the message template on the LogEvent type");
            }

            _messageTemplateBackingField = backingField;
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
                                    maskedValue!));
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
                case ScalarValue { Value: Uri uriValue }:
                    {
                        var (wasMasked, maskedValue) = ReplaceSensitiveDataFromString(uriValue.ToString());

                        if (wasMasked)
                        {
                            return (true, new ScalarValue(maskedValue));
                        }

                        return (false, null);
                    }
                case SequenceValue sequenceValue:
                    var resultElements = new List<LogEventPropertyValue>();
                    var anyElementMasked = false;
                    foreach (var element in sequenceValue.Elements)
                    {
                        var (wasElementMasked, elementResult) = MaskProperty(new KeyValuePair<string, LogEventPropertyValue>(property.Key, element));
                        
                        if (wasElementMasked)
                        {
                            resultElements.Add(elementResult!);
                            anyElementMasked = true;
                        }
                        else
                        {
                            resultElements.Add(element);
                        }
                    }

                    return (anyElementMasked, new SequenceValue(resultElements));
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
                                propList.Add(new LogEventProperty(prop.Name, maskedValue!));
                            }
                            else
                            {
                                propList.Add(prop);
                            }
                        }

                        return (anyMasked, new StructureValue(propList));
                    }
                case DictionaryValue dictionaryValue:
                {
                    var resultDictionary = new List<KeyValuePair<ScalarValue, LogEventPropertyValue>>();
                    var anyKeyMasked = false;

                    foreach (var pair in dictionaryValue.Elements)
                    {
                        var (wasPairMasked, pairResult) = MaskProperty(new KeyValuePair<string, LogEventPropertyValue>(pair.Key.Value as string, pair.Value));

                        if (wasPairMasked)
                        {
                            resultDictionary.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(pair.Key, pairResult));
                            anyKeyMasked = true;
                        }
                        else
                        {
                            resultDictionary.Add(new KeyValuePair<ScalarValue, LogEventPropertyValue>(pair.Key, pair.Value));
                        }
                    }

                    return (anyKeyMasked, new DictionaryValue(resultDictionary));
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