using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class MaskingSink : ILogEventSink
    {
        private readonly MaskingMode _maskingMode;

        private static readonly MessageTemplateParser Parser = new MessageTemplateParser();
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

            _maskingOperators = enricherOptions.MaskingOperators.ToList();
        }

        public void Emit(LogEvent logEvent)
        {
            if (_maskingMode == MaskingMode.Globally || SensitiveArea.Instance != null)
            {
                _aggregateSink.Emit(Mask(logEvent));
            }
            else
            {
                _aggregateSink.Emit(logEvent);
            }
        }

        private Exception MaskException(Exception exception)
        {
            // Masking exceptions presents an interesting challenge.
            // In many cases exceptions are meant to be immutable so
            // the exposed properties will not have public accessible 
            // setters. Which is a good thing, but that makes our life
            // more difficult.
            //
            // Fortunately, exceptions are _also_ meant to be serializable.
            // That gives us an opportunity to serialize the exception,
            // perform the masking on the serialized data, and then
            // deserialize that into the original exception type.
            //
            // Of course it really depends on the implementor of the 
            // exception whether that is all fully implemented but
            // that's not really our problem. Better to lose some
            // information than having sensitive info in the logs...

            var exceptionType = exception.GetType();
            var serializationInfo = new SerializationInfo(exceptionType, new FormatterConverter());
            var context = new StreamingContext();
            var maskedSerializationInfo = new SerializationInfo(exceptionType, new FormatterConverter());

            exception.GetObjectData(serializationInfo, context);

            var enumerator = serializationInfo.GetEnumerator();

            // Loop through all the serialized items
            while (enumerator.MoveNext())
            {
                // Bit of ergonomics...
                var entry = enumerator.Current;

                if (entry.Value is string stringValue)
                {
                    var maskedValue = ReplaceSensitiveDataFromString(stringValue);

                    maskedSerializationInfo.AddValue(entry.Name, maskedValue, entry.ObjectType);
                }
                // This deals with the Data property of exceptions
                else if (entry.Value is IDictionary dictionary)
                {
                    // Construct a new dictionary to take the values
                    // because trying to modify an IDictionary in-place
                    // while enumerating will fail with a "collection modified"
                    // exception (as it's a hash table really)
                    var replacementDictionary = new Dictionary<object, object>();

                    foreach (var key in dictionary.Keys)
                    {
                        if (dictionary[key] is string dictionaryStringValue)
                        {
                            replacementDictionary.Add(
                                key,
                                ReplaceSensitiveDataFromString(dictionaryStringValue));
                        }
                        else
                        {
                            // TODO: check if the dictionary contains an exception
                            replacementDictionary.Add(key, dictionary[key]);
                        }
                    }

                    maskedSerializationInfo.AddValue(entry.Name, replacementDictionary, entry.ObjectType);
                }
                // This handles the AggregateException. We need to do this before
                // handling InnerException because on AggregateException that points
                // to the first exception of the InnerExceptions collection...
                else if (entry.Value is Exception[] innerExceptions)
                {
                    var replacementInnerExceptions = new List<Exception>();

                    foreach (var ex in innerExceptions)
                    {
                        replacementInnerExceptions.Add(MaskException(ex));
                    }

                    maskedSerializationInfo.AddValue(entry.Name, replacementInnerExceptions.ToArray(), entry.ObjectType);
                }
                #if NET6_0_OR_GREATER
                else if (exception is not AggregateException && entry.Value is Exception innerException)
                #else
                else if (!typeof(AggregateException).IsAssignableFrom(exceptionType) && entry.Value is Exception innerException)
                #endif
                {
                    // TODO: Be smart here with recursion
                    maskedSerializationInfo.AddValue(
                        entry.Name, 
                        MaskException(innerException), 
                        entry.ObjectType);
                }
                else
                {
                    // Current entry contains a type that we don't know how to
                    // handle, so add it to the modified serialization data.
                    maskedSerializationInfo.AddValue(entry.Name, entry.Value, entry.ObjectType);
                }
            }

            // Obtain the protected constructor on the exception to deserialize.
            // We need to do this on the original exception type instead of Exception
            // because it can (and should) be overridden in derived exceptions.
            var deserializingConstructor = exceptionType
                .GetConstructor(BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    CallingConventions.Any,
                    new[] { typeof(SerializationInfo), typeof(StreamingContext) },
                    Array.Empty<ParameterModifier>());

            if (deserializingConstructor == null)
            {
                throw new Exception($"Unable to find deserializing constructor on exception type '{exceptionType.Name}'");
            }

            // Create an instance of the input exception type using deserialization
            return deserializingConstructor
                .Invoke(
                    new object[]
                    {
                        maskedSerializationInfo,
                        context
                    }) as Exception;
        }

        private LogEvent Mask(LogEvent logEvent)
        {
            var messageTemplateText = ReplaceSensitiveDataFromString(logEvent.MessageTemplate.Text);

            var maskedProperties = new List<LogEventProperty>();

            foreach (var property in logEvent.Properties.ToList())
            {
                if (_excludeProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
                {
                    maskedProperties.Add(new LogEventProperty(property.Key, property.Value));
                }
                else if (_maskProperties.Contains(property.Key, StringComparer.InvariantCultureIgnoreCase))
                {
                    maskedProperties.Add(
                        new LogEventProperty(
                            property.Key,
                            new ScalarValue(_maskValue)));
                }
                else if (property.Value is ScalarValue { Value: string stringValue })
                {
                    maskedProperties.Add(
                        new LogEventProperty(
                            property.Key,
                            new ScalarValue(ReplaceSensitiveDataFromString(stringValue))));
                }
            }

            var exception = logEvent.Exception;

            if (exception != null)
            {
                exception = MaskException(exception);
            }

            return new LogEvent(
                logEvent.Timestamp,
                logEvent.Level,
                exception,
                Parser.Parse(messageTemplateText),
                maskedProperties);
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