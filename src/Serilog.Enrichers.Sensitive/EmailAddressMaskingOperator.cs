using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
    public class EmailAddressMaskingOperator : RegexMaskingOperator
    {
        private static readonly Regex EmailReplaceRegex = new Regex(
            "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public EmailAddressMaskingOperator() : base(EmailReplaceRegex, s => (s.Contains("%40")) ? s.Replace("%40", "@") : s)
        {
        }

        //public MaskingResult Mask(string input, string mask)
        //{
        //    // Naive approach to deal with URL encoded values
        //    // most likely this should properly URL decode once it
        //    // finds this marker in the input string
        //    if (input.Contains("%40"))
        //    {
        //        input = input.Replace("%40", "@");
        //    }

        //    // Early exit so we avoid the regex.
        //    // Probably needs a benchmark to see
        //    // if this actually helps.
        //    if (!input.Contains("@"))
        //    {
        //        return MaskingResult.NoMatch;
        //    }

        //    // Note that if we get here we _always_ assume
        //    // a successful replacement.
        //    return new MaskingResult
        //    {
        //        Result = EmailReplaceRegex.Replace(input, mask),
        //        Match = true
        //    };
        //}
    }
}