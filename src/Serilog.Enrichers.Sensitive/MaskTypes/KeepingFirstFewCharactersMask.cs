using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog.Enrichers.Sensitive.MaskTypes
{
    public class KeepingFirstFewCharactersMask : IMaskType
    {
        private readonly int _numberOfCharactersToKeep;
        public KeepingFirstFewCharactersMask(UInt16 numberOfCharactersToKeep)
        {
            _numberOfCharactersToKeep = numberOfCharactersToKeep;
        }
        public string CreateMask(string orginalSource)
        {
            return orginalSource.Substring(0, Math.Min(orginalSource.Length, _numberOfCharactersToKeep)) + "***";
        }
    }
}
