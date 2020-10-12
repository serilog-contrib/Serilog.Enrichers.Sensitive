using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;

namespace Serilog.Enrichers.Sensitive.Demo
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var logger = new LoggerConfiguration()
				.Enrich.WithSensitiveDataMasking(MaskingMode.InArea, new IMaskingOperator[]
				{
					new EmailAddressMaskingOperator(),
					new IbanMaskingOperator(),
					new CreditCardMaskingOperator(false)
				})
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

				// Credit card numbers too
				logger.Information("Credit Card Number: 4111111111111111");

				// even works in an embedded XML string
				var x = new
				{
					Key = 12345, XmlValue = "<MyElement><CreditCard>4111111111111111</CreditCard></MyElement>"
				};
				logger.Information("Object dump with embedded credit card: {x}", x);

			}

			// But outside the sensitive area nothing is masked
			logger.Information("Felix can be reached at felix@cia.gov");


			// Now, show that this works for async contexts too
			logger.Information("Now, show the Async works");

			var t1 = LogAsSensitiveAsync(logger);
			var t2 = LogAsUnsensitiveAsync(logger);

			await Task.WhenAll(t1, t2).ConfigureAwait(false);

			Console.ReadLine();
		}

		private static async Task LogAsSensitiveAsync(Logger logger)
		{
			using (logger.EnterSensitiveArea())
			{
				await Task.Delay(new Random().Next(1000,2000)).ConfigureAwait(false);
				// Put in a delay, so we are sure these run basically simultaneiously 
				// An e-mail address in text
				logger.Information("Sensitive: This is a secret email address: james.bond@universal-exports.co.uk");

				// Works for properties too
				await Task.Delay(new Random().Next(1000, 2000)).ConfigureAwait(false);
				logger.Information("Sensitive: This is a secret email address: {Email}",
					"james.bond@universal-exports.co.uk");
			}
		}

		private static async Task LogAsUnsensitiveAsync(Logger logger)
		{
			// Put in a delay, so we are sure these run basically simultaneiously 
			// An e-mail address in text
			await Task.Delay(new Random().Next(1000, 2000)).ConfigureAwait(false);
			logger.Information("This is a secret email address: james.bond@universal-exports.co.uk");

			// Works for properties too
			await Task.Delay(new Random().Next(1000, 2000)).ConfigureAwait(false);
			logger.Information("This is a secret email address: {Email}", "james.bond@universal-exports.co.uk");
		}
	}
}
