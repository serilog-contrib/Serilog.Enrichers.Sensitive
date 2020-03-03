using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingEmailAddresses
    {
        [Fact]
        public void GivenSimpleMailAddress_AddressIsMasked()
        {
            TheMaskedResultOf("simple@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenSimpleMailAddressButUppercase_AddressIsMasked()
        {
            TheMaskedResultOf("SIMPLE@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressWithBoxQualifier_AddressIsMasked()
        {
            TheMaskedResultOf("test+spamfolder@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressWithSubdomains_AddressIsMasked()
        {
            TheMaskedResultOf("test@sub.sub.sub.email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressInUrl_EntireStringIsMasked()
        {
            TheMaskedResultOf("https://foo.com/api/1/some/endpoint?email=test@sub.sub.sub.email.com")
                .Should()
                .Be("https:" + Mask); // I don't even regex
        }

        [Fact]
        public void GivenEmailAddressUrlEncoded_AddressIsMasked()
        {
            TheMaskedResultOf("test%40email.com")
                .Should()
                .Be(Mask); // I don't even regex
        }

        private const string Mask = "***MASK***";

        private static string TheMaskedResultOf(string input)
        {
            var maskingResult = new EmailAddressMaskingOperator().Mask(input, Mask);

            return maskingResult.Match 
                ? maskingResult.Result 
                : input;
        }
    }
}