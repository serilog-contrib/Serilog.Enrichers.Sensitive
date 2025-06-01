namespace Serilog.Enrichers.Sensitive;

public class MaskProperty
{
    public string Name { get; set; }
    public MaskOptions Options { get; set; } = new();

    public static MaskProperty WithDefaults(string propertyName)
    {
        return new MaskProperty { Name = propertyName };
    }
}