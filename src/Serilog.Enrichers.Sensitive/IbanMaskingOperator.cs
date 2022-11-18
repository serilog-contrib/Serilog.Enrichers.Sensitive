namespace Serilog.Enrichers.Sensitive
{
    public class IbanMaskingOperator : RegexMaskingOperator
    {
		private const string IbanReplacePattern = "[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}";

		public IbanMaskingOperator(IMaskType maskType = null) : base(IbanReplacePattern, maskType)
		{
		}
    }
}