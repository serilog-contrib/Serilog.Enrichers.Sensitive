using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit
{
    public class WhenMaskingEmailAddresses
    {
        [Fact]
        public void GivenSimpleMailAddress_AddressIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "simple@email.com")
                .Should()
                .Be(Mask);

            TheMessageMaskedResultOf("simple@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenSimpleMailAddressButUppercase_AddressIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "SIMPLE@email.com")
                .Should()
                .Be(Mask);

            TheMessageMaskedResultOf("SIMPLE@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressWithBoxQualifier_AddressIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "test+spamfolder@email.com")
                .Should()
                .Be(Mask);

            TheMessageMaskedResultOf("test+spamfolder@email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressWithSubdomains_AddressIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "test@sub.sub.sub.email.com")
                .Should()
                .Be(Mask);

            TheMessageMaskedResultOf("test@sub.sub.sub.email.com")
                .Should()
                .Be(Mask);
        }

        [Fact]
        public void GivenEmailAddressInUrl_EntireStringIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "https://foo.com/api/1/some/endpoint?email=test@sub.sub.sub.email.com")
                .Should()
                .Be("https:" + Mask); // I don't even regex

            TheMessageMaskedResultOf("https://foo.com/api/1/some/endpoint?email=test@sub.sub.sub.email.com")
                .Should()
                .Be("https:" + Mask); // I don't even regex
        }

        [Fact]
        public void GivenEmailAddressUrlEncoded_AddressIsMasked()
        {
            ThePropertyMaskedResultOf("anyPropertyName", "test%40email.com")
                .Should()
                .Be(Mask); // I don't even regex

            TheMessageMaskedResultOf("test%40email.com")
                .Should()
                .Be(Mask); // I don't even regex
        }

        private const string Mask = "***MASK***";

        [Theory]
        [InlineData("email@example.com")]
        [InlineData(@"firstname.lastname@example.com")]
        [InlineData(@"email@subdomain.example.com")]
        [InlineData(@"firstname+lastname@example.com")]
        [InlineData(@"email@123.123.123.123")]
        [InlineData(@"email@[123.123.123.123]")]
        [InlineData(@"""email""@example.com")]
        [InlineData(@"1234567890@example.com")]
        [InlineData(@"email@example-one.com")]
        [InlineData(@"_______@example.com")]
        [InlineData(@"email@example.name")]
        [InlineData(@"email@example.museum")]
        [InlineData(@"email@example.co.jp")]
        [InlineData(@"firstname-lastname@example.com")]
        public void GivenValidEmailAddress_AddressIsMasked(string email)
        {
            ThePropertyMaskedResultOf("anyPropertyName", email)
                .Should()
                .Be(Mask);

            TheMessageMaskedResultOf(email)
                .Should()
                .Be(Mask);
        }

        [Theory]
        [InlineData("PlainAddress")]
        [InlineData("#@%^%#$@#$@#.com")]
        [InlineData("@example.com")]
        [InlineData("email.example.com")]
        [InlineData("email.@example.com")]
        [InlineData("あいうえお@example.com")]
        [InlineData("email@example")]
        [InlineData("email@-example.com")]
        [InlineData("email@example..com")]
        [InlineData("(),:;<>[\\]@example.com")]
        public void GivenInvalidEmailAddress_StringIsNotMasked(string toTest)
        {
            ThePropertyMaskedResultOf("anyPropertyName", toTest)
                .Should()
                .Be(toTest);

            TheMessageMaskedResultOf(toTest)
                .Should()
                .Be(toTest);
        }

        [Theory]
        [InlineData(".email@example.com", ".{0}")]
        [InlineData("Abc..123@example.com", "Abc..{0}")]
        [InlineData("Joe Smith <email@example.com>", "Joe Smith <{0}>")]
        [InlineData("email..email@example.com", "email..{0}")]
        [InlineData("email@example.com (Joe Smith)", "{0} (Joe Smith)")]
        [InlineData("email@example@example.com", "email@{0}")]
        [InlineData("just”not”right@example.com", "just”not”{0}")]
        [InlineData("this\\ is\"really\"not\\allowed@example.com", "this\\ is\"really\"not\\{0}")]
        public void GivenInvalidEmailAddress_StringIsStillMasked(string toTest, string expectedMask)
        {
            ThePropertyMaskedResultOf("anyPropertyName", toTest)
                .Should()
                .Be(string.Format(expectedMask, Mask));

            TheMessageMaskedResultOf(toTest)
                .Should()
                .Be(string.Format(expectedMask, Mask));
        }

        private static string TheMessageMaskedResultOf(string input)
        {
            var maskingResult = new EmailAddressMaskingOperator().MaskMessage(input, Mask);

            return maskingResult.Match
                ? maskingResult.Result
                : input;
        }

        private static string ThePropertyMaskedResultOf(string propertyName, string input)
        {
            var maskingResult = new EmailAddressMaskingOperator().MaskProperty(propertyName, input, Mask);

            return maskingResult.Match
                ? maskingResult.Result
                : input;
        }
    }
}