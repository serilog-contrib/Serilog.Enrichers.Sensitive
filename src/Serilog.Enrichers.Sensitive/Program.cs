using System;
using Serilog;

namespace serilog_pii
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveAreEnricher())
                .WriteTo.Console()
                .CreateLogger();

            logger.Information("Hello, world");

            logger.Information("Entering sensitive area");

            using (logger.Sensitive())
            {
                logger.Information("Inside sensitive area");
                logger.Information("My email is: sander@codenizer.nl");
                logger.Information("My email is: {Email}", "sander@codenizer.nl");
                logger.Information("My back account is NL02ABNA0123456789");
                logger.Information("My back account is {PropertyNameDoesntMatter}", "NL02ABNA0123456789");
            }

            logger.Information("After sensitive area");

            logger.Information("My email is: support@jedlix.com");

            logger.Dispose();

            Console.ReadLine();
        }
    }
}
