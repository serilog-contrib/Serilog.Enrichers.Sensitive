using System;

namespace Serilog.Enrichers.Sensitive
{

	public interface IMaskingOperator
    {
        MaskingResult Mask(string input, string mask);
    }
}