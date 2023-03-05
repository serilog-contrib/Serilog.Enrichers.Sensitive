using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Serilog.Enrichers.Sensitive
{
    public class SensitiveDataEnricherOptions
    {
        private string[]? _operators;

        // ReSharper disable once UnusedMember.Global as this only exists to support JSON configuration. See issue #25
        public SensitiveDataEnricherOptions()
        {
        }

        public SensitiveDataEnricherOptions(
            MaskingMode mode = MaskingMode.Globally, 
            string maskValue = SensitiveDataEnricher.DefaultMaskValue, 
            IEnumerable<string>? maskingOperators = null,
            IEnumerable<string>? maskProperties = null, 
            IEnumerable<string>? excludeProperties = null,
            // ReSharper disable once UnusedParameter.Local as this only exists to support JSON configuration, see the Operators property below 
            IEnumerable<string>? operators = null)
        {
            Mode = mode;
            MaskValue = maskValue;
            MaskingOperators = maskingOperators == null ? new List<IMaskingOperator>() : ResolveMaskingOperators(maskingOperators);
            MaskProperties = maskProperties?.ToList() ?? new List<string>();
            ExcludeProperties = excludeProperties?.ToList() ?? new List<string>();
        }

        private static List<IMaskingOperator> ResolveMaskingOperators(IEnumerable<string> maskingOperatorTypeNames)
        {
            var maskingOperators = new List<IMaskingOperator>();

            foreach (var typeName in maskingOperatorTypeNames)
            {
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    continue;
                }

                var type = Type.GetType(
                    typeName,
                    Assembly.Load,
                    (assembly, fullTypeName, _) =>
                    {
                        if (assembly == null)
                        {
                            return null;
                        }

                        var singleOrDefault = assembly.GetTypes().SingleOrDefault(t => t.FullName == fullTypeName);
                        
                        return singleOrDefault;
                    });

                if (type == null)
                {
                    throw new Exception(
                        $"Could not find the masking operator type '{typeName}', check if the type name matches 'Assembly.Namespace.TypeName, Assembly' format and that the assembly can be resolved by the application (i.e. in the same folder)");
                }

                var instance = Activator.CreateInstance(type);

                if (instance != null)
                {
                    maskingOperators.Add((IMaskingOperator)instance);
                }
            }

            return maskingOperators;
        }

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

        /// <remarks>
        /// This property only exists to support JSON configuration of the enricher. If you are configuring the enricher from code you'll want <see cref="MaskingOperators"/> instead.
        /// </remarks>
        public string[]? Operators
        {
            get => _operators;
            set
            {
                _operators = value;
                if (value != null)
                {
                    MaskingOperators = ResolveMaskingOperators(value);
                }
            }
        }

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
            other.Operators = Operators;
        }
    }
}