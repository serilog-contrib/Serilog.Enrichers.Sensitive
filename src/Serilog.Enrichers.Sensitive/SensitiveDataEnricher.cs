using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string MaskValue = "***MASKED***";

        private static readonly MessageTemplateParser Parser = new MessageTemplateParser();
        private readonly FieldInfo _messageTemplateBackingField;
        private readonly List<IMaskingOperator> _maskingOperators;

        public SensitiveDataEnricher(MaskingMode maskingMode, IEnumerable<IMaskingOperator> maskingOperators)
        {
            _maskingMode = maskingMode;

            var fields = typeof(LogEvent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            _messageTemplateBackingField = fields.SingleOrDefault(f => f.Name.Contains("<MessageTemplate>"));

            _maskingOperators = maskingOperators.ToList();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_maskingMode == MaskingMode.Globally || SensitiveArea.Instance != null)
            {
                var messageTemplateText = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

                _messageTemplateBackingField.SetValue(logEvent, Parser.Parse(messageTemplateText));

                try
                {
                    foreach (var property in logEvent.Properties.ToList())
                    {
                        try
                        {
                            if (property.Value is ScalarValue scalar && scalar.Value is string stringValue)
                            {
                                logEvent.AddOrUpdateProperty(
                                    new LogEventProperty(
                                        property.Key,
                                        new ScalarValue(ReplaceSensitiveDataFromString(stringValue))));
                            }
                        }
                        catch (Exception e)
                        {
                            Debugger.Break();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    throw e;
                }
            }
        }

        private string ReplaceSensitiveDataFromString(string input)
        {
            foreach(var maskingOperator in _maskingOperators)
            {
                var maskResult = maskingOperator.Mask(input, MaskValue);

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