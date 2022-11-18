using FluentAssertions;
using Serilog.Enrichers.Sensitive.MaskTypes;
using System;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
	public class WhenMaskingWithRegexOperator
	{
		private class RegexExtenderWithOptions : RegexMaskingOperator
		{
			public RegexExtenderWithOptions(string regexPattern) : base(regexPattern, new FixedValueMask())
			{
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
	}
}