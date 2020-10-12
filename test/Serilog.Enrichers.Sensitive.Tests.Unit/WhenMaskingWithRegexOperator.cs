using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
	public class WhenMaskingWithRegexOperator
	{
		private const string Mask = "***MASK***";

		private class RegexExtenderWithOptions : RegexMaskingOperator
		{
			public RegexExtenderWithOptions(string regexPattern) : base(regexPattern)
			{
			}
			public RegexExtenderWithOptions(string regexPattern, RegexOptions options) : base(regexPattern, options)
			{
			}
		}


		[Fact]
		public void GivenConstructor_NullPatternThrowsException()
		{
			var ex = Record.Exception(() => new RegexExtenderWithOptions(null));
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

		//public void ConstructorParameterTests(Regex regex, string replacementPattern, string missingParameter)
		//{
		//	var ex = Record.Exception(() => new RegexMaskingOperator(regex, replacementPattern));
		//	ex
		//		.Should()
		//		.NotBeNull()
		//		.And
		//		.BeOfType<ArgumentNullException>();
		//	(ex as ArgumentNullException)?.ParamName
		//		.Should()
		//		.Be(missingParameter);
		//}

		//	    [Fact]
		//	    public void GivennRegex_OutputMasked()
		//	    {
		//		    // Arrange
		//		    var op = new RegexMaskingOperator(new Regex(".+"));

		//		    // Act
		//		    var result = op.Mask("TEST", Mask);

		//			// Assert
		//		    result.Match.Should().BeTrue();
		//		    result.Result.Should().Be(Mask);
		//	    }

		//		[Fact]
		//	    public void GivenBeforeMask_CancelsOperation()
		//	    {
		//			// Arrange
		//		    var op = new RegexMaskingOperator(new Regex(".+"));
		//		    op.BeforeMask += (sender, args) => args.Cancel = true;

		//			// Act
		//		    var result = op.Mask("TEST", Mask);

		//			// Assert
		//		    result.Should().Be(MaskingResult.NoMatch);
		//	    }

		//	    [Fact]
		//	    public void GivenBeforeMask_AltersInputString()
		//	    {
		//		    // Arrange
		//		    var op = new RegexMaskingOperator(new Regex("00"));
		//		    op.BeforeMask += (sender, args) => args.ValueToMask = "Changed value";

		//		    // Act
		//		    var result = op.Mask("TEST", Mask);

		//			// Assert
		//		    result.Match.Should().BeFalse();
		//		    result.Result.Should().Be("Changed value");
		//	    }

		//	    [Fact]
		//	    public void GivenAfterMask_ModifiesResult()
		//	    {
		//		    // Arrange
		//		    var op = new RegexMaskingOperator(new Regex(".+"));
		//		    op.AfterMask += (sender, args) =>
		//		    {
		//			    args.MaskedValue = "Trash";
		//		    };

		//			// Act
		//			var result = op.Mask("TEST", Mask);

		//		    // Assert
		//		    result.Match.Should().BeTrue();
		//		    result.Result.Should().Be("Trash");
		//	    }

		//		private class ConstructorParameterData : TheoryData<Regex, string, string>
		//        {
		//	        public ConstructorParameterData()
		//	        {
		//		        Add(null, "replacementPattern", "regex");
		//		        Add(new Regex(".+"), null, "replacementPattern");
		//            }
		//        }
	}
}