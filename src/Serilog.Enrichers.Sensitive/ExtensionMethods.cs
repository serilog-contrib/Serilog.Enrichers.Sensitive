using Serilog;

namespace serilog_pii
{
    public static class ExtensionMethods
    {
        public static SensitiveArea Sensitive(this ILogger logger)
        {
            var sensitiveArea = new SensitiveArea();

            SensitiveArea.Instance = sensitiveArea;

            return sensitiveArea;
        }
    }
}