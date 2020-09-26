using System;

namespace Serilog.Enrichers.Sensitive
{

	public class BeforeMaskingArgs : EventArgs
	{
		public string ValueToMask { get; set; }
		public bool Cancel { get; set; }
	}

	public class AfterMaskingArgs : EventArgs
	{
		public string MaskedValue { get; set; }
	}

	public interface IMaskingOperator
    {
	    event EventHandler<BeforeMaskingArgs> BeforeMask;
	    event EventHandler<AfterMaskingArgs> AfterMask;
        MaskingResult Mask(string input, string mask);
    }
}