using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
	public class RegexMaskingOperator : IMaskingOperator
	{
		private readonly Regex _regex;
		private readonly string _replacementPattern;

		public RegexMaskingOperator(Regex regex) : this(regex, "{0}")
		{
		}

		public RegexMaskingOperator(Regex regex, string replacementPattern)
		{
			_regex = regex ?? throw new ArgumentNullException(nameof(regex));
			_replacementPattern = replacementPattern ?? throw new ArgumentNullException(nameof(replacementPattern));
		}

		public event EventHandler<BeforeMaskingArgs> BeforeMask;
		public event EventHandler<AfterMaskingArgs> AfterMask;

		public virtual MaskingResult Mask(string input, string mask)
		{
			var beforeArgs = new BeforeMaskingArgs {ValueToMask = input};
			BeforeMask?.Invoke(this, beforeArgs);
			if (beforeArgs.Cancel) return MaskingResult.NoMatch;
			var result = new MaskingResult
			{
				Result = _regex.Replace(beforeArgs.ValueToMask, string.Format(_replacementPattern, mask)),
				Match = _regex.IsMatch(beforeArgs.ValueToMask)
			};
			var afterArgs = new AfterMaskingArgs {MaskedValue = result.Result};
			AfterMask?.Invoke(this, afterArgs);
			result.Result = afterArgs.MaskedValue;
			return result;
		}
	}
}
