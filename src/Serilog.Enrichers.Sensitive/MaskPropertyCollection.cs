using System.Collections.Generic;

namespace Serilog.Enrichers.Sensitive;

public class MaskPropertyCollection : List<MaskProperty>
{
    private readonly Dictionary<string, MaskOptions> _properties = new();

    public void Add(string propertyName)
    {
        _properties.Add(propertyName.ToLower(), MaskOptions.Default);
    }

    public void Add(string propertyName, MaskOptions maskOptions)
    {
        _properties.Add(propertyName.ToLower(), maskOptions);
    }

    public bool TryGetProperty(string propertyName, out MaskOptions options)
    {
        return _properties.TryGetValue(propertyName.ToLower(), out options);
    }

    public static MaskPropertyCollection From(IEnumerable<string> enricherOptionsMaskProperties)
    {
        var collection = new MaskPropertyCollection();

        foreach (var x in enricherOptionsMaskProperties)
        {
            collection.Add(x, MaskOptions.Default);
        }

        return collection;
    }
}