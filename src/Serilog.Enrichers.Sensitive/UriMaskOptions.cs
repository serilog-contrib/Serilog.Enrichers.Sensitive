namespace Serilog.Enrichers.Sensitive;

public class UriMaskOptions : MaskOptions
{
    public bool ShowScheme { get; set; } = true;
    public bool ShowHost { get; set; } = true;
    public bool ShowPath { get; set; } = false;
    public bool ShowQueryString { get; set; } = false;
}