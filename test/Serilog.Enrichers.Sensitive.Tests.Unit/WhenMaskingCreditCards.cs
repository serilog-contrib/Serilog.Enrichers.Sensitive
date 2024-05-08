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
            ThePropertyMaskedResultOf("anyPropertyName", cc, fullMask, useDefaultConstructor)
                .Should()
                .Be(result);

            TheMessageMaskedResultOf(cc, fullMask, useDefaultConstructor)
                .Should()
                .Be(result);
        }

        private static string ThePropertyMaskedResultOf(string propertyName, string input, bool fullMask, bool useDefaultConstructor)
        {
            var maskingResult = (useDefaultConstructor ? new CreditCardMaskingOperator() : new CreditCardMaskingOperator(fullMask)).MaskProperty(propertyName, input, Mask);

            return maskingResult.Match
                ? maskingResult.Result
                : input;
        }



        private static string TheMessageMaskedResultOf(string input, bool fullMask, bool useDefaultConstructor)
        {
            var maskingResult = (useDefaultConstructor ? new CreditCardMaskingOperator() : new CreditCardMaskingOperator(fullMask)).MaskMessage(input, Mask);

            return maskingResult.Match
                ? maskingResult.Result
                : input;
        }
    }
}