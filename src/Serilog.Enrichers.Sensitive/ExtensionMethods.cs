using System;
using System.Collections.Generic;
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

        public static LoggerConfiguration WithSensitiveDataMasking(this LoggerEnrichmentConfiguration loggerConfiguration)
        {
            return loggerConfiguration.WithSensitiveDataMasking(MaskingMode.Globally, SensitiveDataEnricher.DefaultOperators);
        }

        public static LoggerConfiguration WithSensitiveDataMasking(this LoggerEnrichmentConfiguration loggerConfiguration, string mask)
        {
            return loggerConfiguration.WithSensitiveDataMasking(MaskingMode.Globally, SensitiveDataEnricher.DefaultOperators, mask);
        }

        public static LoggerConfiguration WithSensitiveDataMaskingInArea(this LoggerEnrichmentConfiguration loggerConfiguration)
        {
	        return loggerConfiguration.WithSensitiveDataMasking(MaskingMode.InArea, SensitiveDataEnricher.DefaultOperators);
        }

        public static LoggerConfiguration WithSensitiveDataMaskingInArea(this LoggerEnrichmentConfiguration loggerConfiguration, string mask)
        {
            return loggerConfiguration.WithSensitiveDataMasking(MaskingMode.InArea, SensitiveDataEnricher.DefaultOperators, mask);
        }

        public static LoggerConfiguration WithSensitiveDataMasking(
	        this LoggerEnrichmentConfiguration loggerConfiguration, MaskingMode mode,
	        IEnumerable<IMaskingOperator> operators,
            string mask = SensitiveDataEnricher.DefaultMaskValue)
        {
	        return loggerConfiguration
		        .With(new SensitiveDataEnricher(mode, operators, mask));
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