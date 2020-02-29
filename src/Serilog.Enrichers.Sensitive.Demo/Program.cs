using System;

namespace Serilog.Enrichers.Sensitive.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .Enrich.WithSensitiveDataMasking()
                .WriteTo.Console()
                .CreateLogger();
            
            logger.Information("Hello, world");

            using (logger.EnterSensitiveArea())
            {
                // An e-mail address in text
                logger.Information("This is a secret email address: james.bond@universal-exports.co.uk");

                // Works for properties too
                logger.Information("This is a secret email address: {Email}", "james.bond@universal-exports.co.uk");

                // IBANs are also masked
                logger.Information("Bank transfer from Felix Leiter on NL02ABNA0123456789");
                
                // IBANs are also masked
                logger.Information("Bank transfer from Felix Leiter on {BankAccount}", "NL02ABNA0123456789");
            }
            
            // But outside the sensitive area nothing is masked
            logger.Information("Felix can be reached at felix@cia.gov");

            Console.ReadLine();
        }
    }
}
