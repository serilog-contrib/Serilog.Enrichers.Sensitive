using FluentAssertions;
using Serilog.Enrichers.Sensitive.MaskTypes;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingCreditCards
    {
	    private const string Mask = "***MASKED***";

	    [Theory]
        [InlineData("4111 1111 1111 1111", "***MASKED***", true, false)]
        [InlineData("4111-1111-1111-1111", "***MASKED***", true, false)]
        [InlineData("4111111111111111", "***MASKED***", true, true)]
	    [InlineData("4111 1111 1111 1111", "4111 ***MASKED***11 1111", false, false)]
	    [InlineData("4111-1111-1111-1111", "4111-***MASKED***11-1111", false, false)]
	    [InlineData("4111111111111111", "4111***MASKED***111111", false, false)]
	    [InlineData("4111 1111 1111 1111", "***MASKED***", false, true)]
	    [InlineData("4111-1111-1111-1111", "***MASKED***", false, true)]
	    [InlineData("4111111111111111", "***MASKED***", false, true)]
        public void GivenCreditCard_ValuesAreMaskedFully(string cc, string result, bool fullMask, bool useDefaultConstructor)
        {
            TheMaskedResultOf(cc, fullMask, useDefaultConstructor)
                .Should()
                .Be(result);
        }

        private static string TheMaskedResultOf(string input, bool fullMask, bool useDefaultConstructor)
        {
            var maskingResult = (useDefaultConstructor ? new CreditCardMaskingOperator() : new CreditCardMaskingOperator(fullMask, new FixedValueMask("***MASKED***"))).Mask(input);

            return maskingResult.Match 
                ? maskingResult.Result 
                : input;
        }
    }
}