using System;
using System.Configuration;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingWithRegexOperator
    {
	    private const string Mask = "***MASK***";

	    private static Func<string, string> defaultBeforeMask = s => s;

	    [Theory]
        [ClassData(typeof(ConstructorParameterData))]
	    public void ConstructorParameterTests(Regex regex, string replacementPattern, Func<string, string> onBeforeMask, string missingParameter)
	    {
            var ex = Record.Exception(() => new RegexMaskingOperator(regex, replacementPattern, onBeforeMask));
	        ex
		        .Should()
	            .NotBeNull()
	            .And
	            .BeOfType<ArgumentNullException>();
			(ex as ArgumentNullException)?.ParamName
			    .Should()
			    .Be(missingParameter);
	    }


        [Theory]
        [InlineData("4111 1111 1111 1111", "***MASK***", true, false)]
        [InlineData("4111-1111-1111-1111", "***MASK***", true, false)]
        [InlineData("4111111111111111", "***MASK***", true, true)]
	    [InlineData("4111 1111 1111 1111", "4111 ***MASK***11 1111", false, false)]
	    [InlineData("4111-1111-1111-1111", "4111-***MASK***11-1111", false, false)]
	    [InlineData("4111111111111111", "4111***MASK***111111", false, false)]
	    [InlineData("4111 1111 1111 1111", "***MASK***", false, true)]
	    [InlineData("4111-1111-1111-1111", "***MASK***", false, true)]
	    [InlineData("4111111111111111", "***MASK***", false, true)]
        public void GivenCreditCard_ValuesAreMaskedFully(string cc, string result, bool fullMask, bool useDefaultConstructor)
        {
            TheMaskedResultOf(cc, fullMask, useDefaultConstructor)
                .Should()
                .Be(result);
        }

        private static string TheMaskedResultOf(string input, bool fullMask, bool useDefaultConstructor)
        {
            var maskingResult = (useDefaultConstructor ? new CreditCardMaskingOperator() : new CreditCardMaskingOperator(fullMask)).Mask(input, Mask);

            return maskingResult.Match 
                ? maskingResult.Result 
                : input;
        }

        private class ConstructorParameterData : TheoryData<Regex, string, Func<string, string>, string>
        {
	        public ConstructorParameterData()
	        {
		        Add(null, "replacementPattern", s => s, "regex");
		        Add(new Regex(".+"), null, s => s, "replacementPattern");
		        Add(new Regex(".+"), "replacementPattern", null, "onBeforeMask");
            }
        }
    }
}