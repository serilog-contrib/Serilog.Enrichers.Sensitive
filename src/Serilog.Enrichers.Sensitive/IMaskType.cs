using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Enrichers.Sensitive
{
    public interface IMaskType
    {
        string CreateMask(string orginalSource);
    }
}
