namespace Serilog.Enrichers.Sensitive
{

	public interface IMaskingOperator
    {
        MaskingResult MaskProperty(string propertyName, string input, string mask);
        MaskingResult MaskMessage(string input, string mask);
    }
}