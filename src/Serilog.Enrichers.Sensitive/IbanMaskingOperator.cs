using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive
{
    public class IbanMaskingOperator : IMaskingOperator
    {
        private static readonly Regex IbanReplaceRegex = new Regex("[a-zA-Z]{2}[0-9]{2}[a-zA-Z0-9]{4}[0-9]{7}([a-zA-Z0-9]?){0,16}");

        public MaskingResult Mask(string input, string mask)
        {
            return new MaskingResult
            {
                Match = true,
                Result = IbanReplaceRegex.Replace(input, mask)
            };
        }
    }
}