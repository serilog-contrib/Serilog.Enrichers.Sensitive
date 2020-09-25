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
		private Func<string, string> _onBeforeMask;

		public RegexMaskingOperator(Regex regex) : this(regex, "{0}", s => s)
		{
		}

		public RegexMaskingOperator(Regex regex, Func<string, string> onBeforeMask) : this(regex, "{0}", onBeforeMask)
		{
		}

		public RegexMaskingOperator(Regex regex, string replacementPattern) : this(regex, replacementPattern, s => s)
		{
		}

		public RegexMaskingOperator(Regex regex, string replacementPattern, Func<string, string> onBeforeMask)
		{
			_regex = regex ?? throw new ArgumentNullException(nameof(regex));
			_replacementPattern = replacementPattern ?? throw new ArgumentNullException(nameof(replacementPattern));
			_onBeforeMask = onBeforeMask ?? throw new ArgumentNullException(nameof(onBeforeMask));
		}

		public event EventHandler<IMaskingOperatorArgs> BeforeMask;

		public virtual MaskingResult Mask(string input, string mask)
		{
			input = _onBeforeMask(input);
			return new MaskingResult
			{
				Result = _regex.Replace(input, string.Format(_replacementPattern, mask)),
				Match = _regex.IsMatch(input)
			};
		}
	}
}
