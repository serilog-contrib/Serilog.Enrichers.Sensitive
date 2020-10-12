using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingCreditCards
    {
	    private const string Mask = "***MASK***";

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
    }
}