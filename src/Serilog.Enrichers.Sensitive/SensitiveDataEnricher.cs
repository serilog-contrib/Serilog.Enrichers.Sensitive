using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Enrichers.Sensitive
{
    internal class SensitiveDataEnricher : ILogEventEnricher
    {
        private readonly MaskingMode _maskingMode;
        private static readonly Regex EmailReplaceRegex = new Regex("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
        private static readonly Regex IbanReplaceRegex = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");
        private const string MaskValue = "***MASKED***";

        private static readonly MessageTemplateParser Parser = new MessageTemplateParser();
        private readonly FieldInfo _messageTemplateBackingField;

        public SensitiveDataEnricher(MaskingMode maskingMode = MaskingMode.Globally)
        {
            _maskingMode = maskingMode;

            var fields = typeof(LogEvent).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            _messageTemplateBackingField = fields.SingleOrDefault(f => f.Name.Contains("<MessageTemplate>"));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (_maskingMode == MaskingMode.Globally|| SensitiveArea.Instance != null)
            {
                var messageTemplateText = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

                _messageTemplateBackingField.SetValue(logEvent, Parser.Parse(messageTemplateText));
                
                foreach (var property in logEvent.Properties)
                {
                    if (property.Value is ScalarValue scalar)
                    {
                        if (scalar.Value is string stringValue)
                        {
                            logEvent.AddOrUpdateProperty(
                                new LogEventProperty(
                                    property.Key,
                                    new ScalarValue(ReplaceSensitiveDataFromString(stringValue))));
                        }
                    }
                }
            }
        }

        private static string ReplaceSensitiveDataFromString(string input)
        {
            var messageTemplateText = EmailReplaceRegex.Replace(input, MaskValue);

            messageTemplateText = IbanReplaceRegex.Replace(messageTemplateText, MaskValue);

            return messageTemplateText;
        }
    }

    public enum MaskingMode
    {
        Globally,
        InArea
    }
}