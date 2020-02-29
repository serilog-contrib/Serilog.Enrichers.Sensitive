using System;
using System.Threading;

namespace serilog_pii
{
    public class SensitiveArea : IDisposable
    {
        private static readonly AsyncLocal<SensitiveArea> InstanceLocal = new AsyncLocal<SensitiveArea>();

        public static SensitiveArea Instance
        {
            get => InstanceLocal.Value;
            set => InstanceLocal.Value = value;
        }

        public void Dispose()
        {
            Instance = null;
        }
    }
}