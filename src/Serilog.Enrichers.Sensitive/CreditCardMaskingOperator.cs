using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
	public class CreditCardMaskingOperator : RegexMaskingOperator
	{
		//private static readonly Regex CreditCardPartialReplaceRegex = new Regex(
		//	@"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})",
		//	RegexOptions.IgnoreCase | RegexOptions.Compiled);

		//private static readonly Regex CreditCardFullReplaceRegex = new Regex(
		//	@"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})",
		//	RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private const string CreditCardPartialReplacePattern =
			@"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})";

		private const string CreditCardFullReplacePattern =
			@"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})";

		private readonly string _replacementPattern;

		public CreditCardMaskingOperator() : this(true)
		{
		}

		public CreditCardMaskingOperator(bool fullMask) 
			: base(fullMask ? CreditCardFullReplacePattern : CreditCardPartialReplacePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
		{
			_replacementPattern = fullMask ? "{0}" : "${{leading4}}{0}${{trailing6}}";
		}

		protected override string PreprocessMask(string mask) => string.Format(_replacementPattern, mask);
	}
}
