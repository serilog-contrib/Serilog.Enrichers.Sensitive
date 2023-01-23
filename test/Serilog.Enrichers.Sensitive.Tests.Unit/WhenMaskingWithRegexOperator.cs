using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
	public class WhenMaskingWithRegexOperator
	{
		private class RegexExtenderWithOptions : RegexMaskingOperator
        {
            private Func<string, Match, string> _preprocessMask;

			public RegexExtenderWithOptions(string regexPattern, Func<string, Match, string>? preprocessMask = null)
                : base(regexPattern)
            {
                _preprocessMask = preprocessMask ?? new Func<string,Match,string>((mask, _) => mask);
            }

            protected override string PreprocessMask(string mask, Match match)
            {
                return _preprocessMask(mask, match);
            }
        }


		[Fact]
		public void GivenConstructor_NullPatternThrowsException()
		{
			var ex = Record.Exception(() => new RegexExtenderWithOptions(null!));
			ex
				.Should()
				.NotBeNull()
				.And
				.BeOfType<ArgumentNullException>();
			(ex as ArgumentNullException)?.ParamName
				.Should()
				.Be("regexString");
		}

		[Theory]
		[InlineData("")]
		[InlineData("  ")]
		public void GivenConstructor_EmptyOrWhitespacePatternThrowsException(string regexPattern)
		{
			var ex = Record.Exception(() => new RegexExtenderWithOptions(regexPattern));
			ex
				.Should()
				.NotBeNull()
				.And
				.BeOfType<ArgumentOutOfRangeException>();
			(ex as ArgumentOutOfRangeException)?.ParamName
				.Should()
				.Be("regexString");
		}

		[Fact]
        public void GivenPreprocessMaskWithMatchIsUsed_MaskedValueIsModified()
        {
			// Regex matches any character and has a match group for the last character.
			// The mask provided to Mask() is ignored and instead it's set to mask all
			// characters with '*' except the last one.
            var result = new RegexExtenderWithOptions(
                    ".*([a-z])",
                    (mask, match) => match.Groups[1].Value.PadLeft(match.Value.Length, '*'))
                .Mask("abc", "**MASK**");

            result
                .Result
                .Should()
                .Be("**c");
        }
	}
}