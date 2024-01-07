namespace Serilog.Enrichers.Sensitive;

public class MaskOptions
{
    public static readonly MaskOptions Default= new();
    public const int NotSet = -1;
    public int ShowFirst { get; set; } = NotSet;
    public int ShowLast { get; set; } = NotSet;
    public bool PreserveLength { get; set; } = true;

}