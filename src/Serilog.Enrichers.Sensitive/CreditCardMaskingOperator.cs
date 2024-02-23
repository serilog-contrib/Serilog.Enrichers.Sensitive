using System;
using System.Text.RegularExpressions;
namespace Serilog.Enrichers.Sensitive
{
public class CreditCardMaskingOperator : RegexMaskingOperator
{
    private const string CreditCardPartialReplacePattern =
        @"(?<leading4>\d{4}(?<sep>[ -]?))(?<toMask>\d{4}\k<sep>*\d{2})(?<trailing6>\d{2}\k<sep>*\d{4})";

    private const string CreditCardFullReplacePattern =
        @"(?<toMask>\d{4}(?<sep>[ -]?)\d{4}\k<sep>*\d{4}\k<sep>*\d{4})";

    private readonly string _replacementPattern;

    public CreditCardMaskingOperator(bool fullMask)
        : base(fullMask ? CreditCardFullReplacePattern : CreditCardPartialReplacePattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
    {
        _replacementPattern = fullMask ? "{0}" : "${{leading4}}{0}${{trailing6}}";
    }

    protected override string PreprocessMask(string mask)
    {
        return string.Format(_replacementPattern, mask);
    }

    public override string Mask(string input)
    {
        var matches = Regex.Matches(input, _regexPattern, _regexOptions);
        foreach (Match match in matches)
        {
            if (IsValidCreditCardNumber(match.Value))
            {
                // Perform masking
                input = Regex.Replace(input, match.Value, m => PreprocessMask("XXXX-XXXX-XXXX-XXXX"));
            }
        }
        return input;
    }

    private bool IsValidCreditCardNumber(string cardNumber)
{
    // Remove non-numeric characters
    var cleanCardNumber = Regex.Replace(cardNumber, "[^0-9]", "");

    int sum = 0;
    bool alternate = false;
    for (int i = cleanCardNumber.Length - 1; i >= 0; i--)
    {
        var digit = int.Parse(cleanCardNumber[i].ToString());

        if (alternate)
        {
            digit *= 2;
            if (digit > 9)
            {
                digit = (digit % 10) + 1;
            }
        }
        sum += digit;
        alternate = !alternate;
    }
    // If the sum modulo 10 is 0, the number is valid according to the Luhn formula
    return sum % 10 == 0;
}

}
}
