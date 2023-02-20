using System.IO;

namespace Serilog.Enrichers.Sensitive;

public class PathWrapper : IPathWrapper
{
    public bool IsDirectory(string path)
    {
        return Path.GetExtension(path) == string.Empty;
    }
    public string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }
    public string GetDirectoryName(string path)
    {
        return new DirectoryInfo(path).Name;
    }
}