using System;
using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
	/// <summary>
	/// A masking operator that uses regular expressions to match the value from the input to mask
	/// </summary>
	public abstract class RegexMaskingOperator : IMaskingOperator
	{
		private readonly Regex _regex;

		protected RegexMaskingOperator(string regexString) 
            : this(regexString, RegexOptions.Compiled)
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

            var maskedResult = _regex.Replace(preprocessedInput, match =>
            {
                if (ShouldMaskMatch(match))
                {
                    return match.Result(PreprocessMask(mask));
                }

                return match.Value;
            });
			
			var result = new MaskingResult
			{
				Result = maskedResult,
				Match = maskedResult != input
			};

			return result;
		}

		/// <summary>
		/// Indicate whether the operator should continue with masking the input
		/// </summary>
		/// <param name="input">The message template or the value of a property on the log event</param>
		/// <returns><c>true</c> when the input should be masked, otherwise <c>false</c>. Defaults to <c>true</c></returns>
		/// <remarks>This method provides an extension point to short-circuit the masking operation before the regular expression matching is performed</remarks>
		protected virtual bool ShouldMaskInput(string input) => true;

		/// <summary>
		/// Perform any operations on the input value before masking the input
		/// </summary>
        /// <param name="input">The message template or the value of a property on the log event</param>
		/// <returns>The processed input, defaults to no pre-processing and returns the input</returns>
		/// <remarks>Use this method if the input is encoded using URL encoding for example</remarks>
		protected virtual string PreprocessInput(string input) => input;

		/// <summary>
		/// Perform any operations on the mask before masking the matched value
		/// </summary>
		/// <param name="mask">The mask value as specified on the <see cref="SensitiveDataEnricherOptions"/></param>
        /// <returns>The processed mask, defaults to no pre-processing and returns the input</returns>
		protected virtual string PreprocessMask(string mask) => mask;

		/// <summary>
		/// Indicate whether the operator should continue with masking the matched value from the input
		/// </summary>
		/// <param name="match">The match found by the regular expression of this operator</param>
        /// <returns><c>true</c> when the match should be masked, otherwise <c>false</c>. Defaults to <c>true</c></returns>
        /// <remarks>This method provides an extension point to short-circuit the masking operation if the value matches the regular expression but does not satisfy some additional criteria</remarks>
        protected virtual bool ShouldMaskMatch(Match match) => true;
    }
}
