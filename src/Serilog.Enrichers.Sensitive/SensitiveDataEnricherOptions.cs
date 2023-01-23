using System.Collections.Generic;
using System.Linq;

namespace Serilog.Enrichers.Sensitive
{
    public class SensitiveDataEnricherOptions
    {
        /// <summary>
        /// Sets whether masking should happen for all log messages ('Globally') or only in sensitive areas ('SensitiveArea')
        /// </summary>
        public MaskingMode Mode { get; set; }
        /// <summary>
        /// The string that replaces the sensitive value, defaults to '***MASKED***'
        /// </summary>
        public string MaskValue { get; set; } = SensitiveDataEnricher.DefaultMaskValue;
        /// <summary>
        /// The list of masking operators that are available
        /// </summary>
        /// <remarks>By default this list contains <see cref="SensitiveDataEnricher.DefaultOperators"/>, if you want to have only your specific enricher(s) supply a new list instead of calling <c>Add()</c></remarks>
        public List<IMaskingOperator> MaskingOperators { get; set; } = SensitiveDataEnricher.DefaultOperators.ToList();
        /// <summary>
        /// The list of properties that should always be masked regardless of whether they match the pattern of any of the masking operators
        /// </summary>
        /// <remarks>The property name is case-insensitive, when the property is present on the log message it will always be masked even if it is empty</remarks>
        public List<string> MaskProperties { get; set; } = new List<string>();
        /// <summary>
        /// The list of properties that should never be masked
        /// </summary>
        /// <remarks>
        /// <para>The property name is case-insensitive, when the property is present on the log message it will always be masked even if it is empty.</para>
        /// <para>This property takes precedence over <see cref="MaskProperties"/> and the masking operators.</para>
        /// </remarks>
        public List<string> ExcludeProperties { get; set; } = new List<string>();

        /// <summary>
        /// Applies the settings of this <c>SensitiveDataEnricherOptions</c> instance to another <c>SensitiveDataEnricherOptions</c> instance
        /// </summary>
        /// <param name="other">The <c>SensitiveDataEnricherOptions</c> to apply the options to</param>
        public void Apply(SensitiveDataEnricherOptions other)
        {
            other.Mode = Mode;
            other.MaskValue = MaskValue;
            other.MaskingOperators = MaskingOperators;
            other.MaskProperties = MaskProperties;
            other.ExcludeProperties = ExcludeProperties;
        }
    }
}