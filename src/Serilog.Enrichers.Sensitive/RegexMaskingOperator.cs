using System;
using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
	public abstract class RegexMaskingOperator : IMaskingOperator
	{
		private readonly Regex _regex;

		protected RegexMaskingOperator(string regexString) : this(regexString, RegexOptions.Compiled)
		{
		}

		protected RegexMaskingOperator(string regexString, RegexOptions options)
		{
			_regex = new Regex(regexString ?? throw new ArgumentNullException(nameof(regexString)), options);
			if (string.IsNullOrWhiteSpace(regexString))
			{
				throw new ArgumentOutOfRangeException(nameof(regexString), "Regex pattern cannot be empty or whitespace.");
			}
		}

		public MaskingResult Mask(string input, string mask)
		{
			var preprocessedInput = PreprocessInput(input);
			if (!ShouldMaskInput(preprocessedInput))
			{
				return MaskingResult.NoMatch;
			}

			var maskedResult = _regex.Replace(preprocessedInput, PreprocessMask(mask));
			var result = new MaskingResult
			{
				Result = maskedResult,
				Match = maskedResult != input
			};

			return result;
		}

		protected virtual bool ShouldMaskInput(string input) => true;

		protected virtual string PreprocessInput(string input) => input;

		protected virtual string PreprocessMask(string mask) => mask;
	}
}
