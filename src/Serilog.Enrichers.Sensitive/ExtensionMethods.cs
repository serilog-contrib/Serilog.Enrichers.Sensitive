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
            return loggerConfiguration
                .With(new SensitiveDataEnricher());
        }

        public static LoggerConfiguration WithSensitiveDataMaskingInArea(this LoggerEnrichmentConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .With(new SensitiveDataEnricher(MaskingMode.InArea));
        }
    }
}