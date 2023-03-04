using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Serilog.Enrichers.Sensitive;

/// <summary>
/// Represents a masking operator for path names.
/// <remarks>Supports path names for Windows, Linux, and macOS.</remarks>
/// </summary>
public class PathMaskingOperator : RegexMaskingOperator
{
    private readonly IPathWrapper _pathWrapper;
    private readonly bool _keepLastPartOfPath;
    private const string PathPattern =
        @"^(?:[a-zA-Z]\:|\\\\[\w-]+\\[\w-]+\$?|[\/][^\/\0]+)+(\\[^\\/:*?""<>|]*)*(\\?)?$";

    /// <summary>
    /// Initializes a new instance of the <see cref="PathMaskingOperator"/> class.
    /// </summary>
    /// <param name="pathWrapper">The path wrapper.</param>
    /// <param name="keepLastPartOfPath">This means if set to <see langword="true"/> then the mask will combine with the file name or its directory name.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public PathMaskingOperator(IPathWrapper pathWrapper, bool keepLastPartOfPath = true) : base(PathPattern)
    {
        _pathWrapper = pathWrapper;
        _keepLastPartOfPath = keepLastPartOfPath;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="PathMaskingOperator"/> class, with default <see cref="PathWrapper"/>.
    /// </summary>
    /// <param name="keepLastPartOfPath">This means if set to <see langword="true"/> then the mask will combine with the file name or its directory name.</param>
    public PathMaskingOperator(bool keepLastPartOfPath = true) : this(PathWrapper.Instance, keepLastPartOfPath)
    {
    }
    
    [SuppressMessage("ReSharper", "InvertIf")]
    protected override string PreprocessMask(string mask, Match match)
    {
        if (_keepLastPartOfPath)
        {
            var value = match.Value;
            return _pathWrapper.IsDirectory(value)
                ? mask + _pathWrapper.GetDirectoryName(value)
                : mask + _pathWrapper.GetFileName(value);
        }
        return mask;
    }
}