using System;
using System.Collections.Generic;
using System.Data;
using FluentAssertions;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;
using Xunit;

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
                        // Add sinks here instead of directly on LoggerConfiguration.WriteTo
                        writeTo.Console();
                        writeTo.Debug();
                        writeTo.Sink(inMemorySink);
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

        [Fact]
        public void ShowItWorksWithException()
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
                        // Add sinks here instead of directly on LoggerConfiguration.WriteTo
                        writeTo.Console();
                        writeTo.Debug();
                        writeTo.Sink(inMemorySink);
                    })
                .Enrich.FromLogContext()
                .CreateLogger();

            // Log a message with sensitive data in an inner exception
            var exception = new Exception("BANG!", new Exception("KAPUT!")
            {
                Data =
                {
                    { "Source", "joe.blogs@example.com" }
                }
            });
            logger.Error(exception, "Something bad happened");

            // Assert that the e-mail address has been masked
            var exceptionInnerException = inMemorySink
                .Should()
                .HaveMessage("Something bad happened")
                .Appearing().Once()
                .Subject
                .Exception
                .InnerException;

            ((string)exceptionInnerException.Data["Source"])
                .Should()
                .Be("***MASKED***");
        }

        [Fact]
        public void ShowItWorksWithAggregateException()
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
                        // Add sinks here instead of directly on LoggerConfiguration.WriteTo
                        writeTo.Console();
                        writeTo.Debug();
                        writeTo.Sink(inMemorySink);
                    })
                .Enrich.FromLogContext()
                .CreateLogger();

            // Log a message with sensitive data in an inner exception
            var exception = new AggregateException(
                "BANG!",
                new Exception("KAPUT!")
                {
                    Data =
                {
                    { "Source", "joe.blogs@example.com" }
                }
                },
                new Exception("KAPUT!")
                {
                    Data =
                    {
                        { "Source", "agent.smith@example.com" }
                    }
                });
            logger.Error(exception, "Something bad happened");

            // Assert that the e-mail address has been masked
            var exceptionInnerException = inMemorySink
                .Should()
                .HaveMessage("Something bad happened")
                .Appearing().Once()
                .Subject
                .Exception as AggregateException;

            exceptionInnerException
                .InnerExceptions
                .Should()
                .AllSatisfy(exception =>
                    ((string)exception.Data["Source"])
                    .Should()
                    .Be("***MASKED***"));
        }

        [Fact]
        public void ShowItWorksWithExceptionMessage()
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
                        // Add sinks here instead of directly on LoggerConfiguration.WriteTo
                        writeTo.Console();
                        writeTo.Debug();
                        writeTo.Sink(inMemorySink);
                    })
                .Enrich.FromLogContext()
                .CreateLogger();

            // Log a message with sensitive data in an inner exception
            var exception = new Exception("BANG joe.blogs@example.com!");
            logger.Error(exception, "Something bad happened");

            // Assert that the e-mail address has been masked
            var exceptionInnerException = inMemorySink
                .Should()
                .HaveMessage("Something bad happened")
                .Appearing().Once()
                .Subject
                .Exception
                .Message
                .Should()
                .Be("BANG ***MASKED***!");
        }

        [Fact]
        public void ShowItWorksWithEntityFrameworkException()
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
                        // Add sinks here instead of directly on LoggerConfiguration.WriteTo
                        writeTo.Console();
                        writeTo.Debug();
                        writeTo.Sink(inMemorySink);
                    })
                .Enrich.FromLogContext()
                .CreateLogger();

            // Log a message with sensitive data in an inner exception
            var exception =
                new ConstraintException("Unique constraint UQ_Username violated with value 'joe.blogs@example.com'");

            logger.Error(exception, "Something bad happened");

            // Assert that the e-mail address has been masked
            inMemorySink
                .Should()
                .HaveMessage("Something bad happened")
                .Appearing().Once()
                .Subject
                .Exception
                .Message
                .Should()
                .Be("Unique constraint UQ_Username violated with value ***MASKED***'");
        }
    }

    public static class ExtensionMethods
    {
        public static LoggerConfiguration Masked(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            Action<SensitiveDataEnricherOptions> options,
            Action<LoggerSinkConfiguration> writeTo)
        {
            return LoggerSinkConfiguration
                .Wrap(
                    loggerSinkConfiguration,
                    sink => new MaskingSink(sink, options),
                    writeTo,
                    LevelAlias.Minimum,
                    null
                );
        }
    }
}