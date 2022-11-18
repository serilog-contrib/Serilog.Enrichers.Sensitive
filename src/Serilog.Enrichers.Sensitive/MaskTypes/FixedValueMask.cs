using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Enrichers.Sensitive.MaskTypes
{
    public class FixedValueMask : IMaskType
    {
        private readonly string _value;
        public FixedValueMask(string value = "***MASKED***")
        {
            _value = value;
        }
        public string CreateMask(string? orginalSource)
        {
            return _value;
        }
    }
}
