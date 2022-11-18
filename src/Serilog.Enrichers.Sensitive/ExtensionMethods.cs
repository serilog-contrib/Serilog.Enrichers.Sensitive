using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Configuration;

namespace Serilog.Enrichers.Sensitive
{
    public static class ExtensionMethods
    {
        public static SensitiveArea EnterSensitiveArea(this ILogger logger)
        {
            var sensitiveArea = new SensitiveArea();

            SensitiveArea.Instance = sensitiveArea;

            return sensitiveArea;
        }

        [Obsolete("Use WithSensitiveDataMasking with the options argument instead")]
        public static LoggerConfiguration WithSensitiveDataMasking(this LoggerEnrichmentConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.Globally;
                    options.MaskingOperators = SensitiveDataEnricher.DefaultOperators.ToList();
                });
        }
        
        [Obsolete("Use WithSensitiveDataMasking with the options argument instead")]
        public static LoggerConfiguration WithSensitiveDataMasking(this LoggerEnrichmentConfiguration loggerConfiguration, string mask)
        {
            return loggerConfiguration
                .WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.Globally;
                    options.MaskingOperators = SensitiveDataEnricher.DefaultOperators.ToList();
                    options.MaskValue = mask;
                });
        }
        
        [Obsolete("Use WithSensitiveDataMasking with the options argument instead")]
        public static LoggerConfiguration WithSensitiveDataMaskingInArea(this LoggerEnrichmentConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.InArea;
                    options.MaskingOperators = SensitiveDataEnricher.DefaultOperators.ToList();
                });
        }
        
        [Obsolete("Use WithSensitiveDataMasking with the options argument instead")]
        public static LoggerConfiguration WithSensitiveDataMaskingInArea(this LoggerEnrichmentConfiguration loggerConfiguration, string mask)
        {
            return loggerConfiguration
                .WithSensitiveDataMasking(options =>
                {
                    options.Mode = MaskingMode.InArea;
                    options.MaskingOperators = SensitiveDataEnricher.DefaultOperators.ToList();
                    options.MaskValue = mask;
                });
        }
        
        public static LoggerConfiguration WithSensitiveDataMasking(
            this LoggerEnrichmentConfiguration loggerConfiguration, 
            Action<SensitiveDataEnricherOptions> options)
        {
            return loggerConfiguration
                .With(new SensitiveDataEnricher(options));
        }
    }
}