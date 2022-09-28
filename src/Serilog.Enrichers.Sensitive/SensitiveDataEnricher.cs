using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
                    if (property.Value is StructureValue structureProperty)
                    {
                        // This is readonly collection in the original property
                        var newProperties = new List<LogEventProperty>();
                        foreach (var structureEventProperty in structureProperty.Properties.ToList())
                        {
                            if (_maskProperties.Contains(structureEventProperty.Name, StringComparer.InvariantCultureIgnoreCase))
                            {
                                newProperties.Add(
                                    new LogEventProperty(
                                        structureEventProperty.Name,
                                        new ScalarValue(_maskValue)));
                            }
                            else if (structureEventProperty.Value is ScalarValue structureScalar && structureScalar.Value is string structureString)
                            {
                                newProperties.Add(
                                    new LogEventProperty(
                                        structureEventProperty.Name,
                                        new ScalarValue(ReplaceSensitiveDataFromString(structureString))));
                            }
                            else // if not masked, put back as normal
                            {
                                newProperties.Add(
                                    new LogEventProperty(
                                        structureEventProperty.Name,
                                        new ScalarValue(structureEventProperty.Value)));
                            }
                        }

                        var newStructure = new StructureValue(newProperties);
                        logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, newStructure));
                    }

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
                    else if (property.Value is ScalarValue scalar && scalar.Value is string stringValue)
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