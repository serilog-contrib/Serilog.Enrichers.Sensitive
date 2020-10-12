using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
    public class IbanMaskingOperator : RegexMaskingOperator
    {
    //    private static readonly Regex IbanReplaceRegex = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}", RegexOptions.Compiled);

    //    public IbanMaskingOperator() : base(IbanReplaceRegex)
    //    {
    //    }
		private const string IbanReplacePattern = "[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}";

		public IbanMaskingOperator() : base(IbanReplacePattern)
		{
		}
    }
}