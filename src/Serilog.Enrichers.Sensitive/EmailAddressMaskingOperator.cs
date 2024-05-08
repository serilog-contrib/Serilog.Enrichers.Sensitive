using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
    public class EmailAddressMaskingOperator : RegexMaskingOperator
    {
	    private const string EmailPattern =
		    "(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])";

        public EmailAddressMaskingOperator() : base(EmailPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
        }

        protected override string PreprocessInput(string input, string? propertyName = null)
        {
	        if (input.Contains("%40"))
	        {
		        input = input.Replace("%40", "@");
	        }
	        return input;
        }

        protected override bool ShouldMaskInput(string input, string? propertyName = null)
        {
	        return input.Contains("@");
        }
    }
}