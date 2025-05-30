using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive;

/// <summary>
/// Represents a masking operator for path names.
/// <remarks>Supports path names for Windows, Linux, and macOS.</remarks>
/// </summary>
public class PathMaskingOperator : RegexMaskingOperator
{
    private readonly bool _keepLastPartOfPath;
    private const string PathPattern =
        @"^(?:[a-zA-Z]\:|\\\\[\w-]+\\[\w-]+\$?|[\/][^\/\0]+)+(\\[^\\/:*?""<>|]*)*(\\?)?$";

    /// <summary>
    /// Initializes a new instance of the <see cref="PathMaskingOperator"/> class.
    /// </summary>
    /// <param name="keepLastPartOfPath">If set to <see langword="true"/> then the mask will keep together with file name or its directory name.</param>
    public PathMaskingOperator(bool keepLastPartOfPath = true) : base(PathPattern)
    {
        _keepLastPartOfPath = keepLastPartOfPath;
    }
    
    [SuppressMessage("ReSharper", "InvertIf")]
    protected override string PreprocessMask(string mask, Match match)
    {
        if (_keepLastPartOfPath)
        {
            var value = match.Value;
            return Path.GetExtension(value) == string.Empty
                ? mask + new DirectoryInfo(value).Name
                : mask + Path.GetFileName(value);
        }
        return mask;
    }
}