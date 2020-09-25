using System;

namespace Serilog.Enrichers.Sensitive
{
	public interface IMaskingOperatorArgs
	{
		string ValueToMask { get; set; }
		bool Cancel { get; set; }
	}

    public interface IMaskingOperator
    {
	    event EventHandler<IMaskingOperatorArgs> BeforeMask;
        MaskingResult Mask(string input, string mask);
    }
}