using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
	public class CreditCardMaskingOperator : IMaskingOperator
	{
		private readonly bool _fullMask;

		public CreditCardMaskingOperator() : this(true)
		{
		}

		public CreditCardMaskingOperator(bool fullMask)
		{
			_fullMask = fullMask;
		}

		private static readonly Regex CreditCardPartialReplaceRegex = new Regex(
			@"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex CreditCardFullReplaceRegex = new Regex(
			@"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public MaskingResult Mask(string input, string mask)
		{
			var regex = _fullMask ? CreditCardFullReplaceRegex : CreditCardPartialReplaceRegex;
			var replacement = _fullMask ? mask : "${leading4}" + mask + "${trailing6}";
			return new MaskingResult
			{
				Result = regex.Replace(input, replacement),
				Match = regex.IsMatch(input)
			};
		}
	}
}
