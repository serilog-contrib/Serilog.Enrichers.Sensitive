using FluentAssertions;
using Xunit;

namespace Serilog.Enrichers.Sensitive.Tests.Unit;

public class WhenMaskingPaths
{
    private const string Mask = @"***\";

    [Theory]
    [InlineData(@"C:\Users\Admin\Secret\File.dll", @"***\", false)]
    [InlineData(@"C:\Users\Admin\Secret\File.dll", @"***\File.dll", true)]
    [InlineData(@"C:\Users\Admin\Secret\Hidden\File.dll", @"***\File.dll", true)]
    [InlineData(@"C:\Users\Admin\Secret\Hidden", @"***\", false)]
    [InlineData(@"C:\Users\Admin\Secret\Hidden", @"***\Hidden", true)]
    [InlineData(@"C:\Users\Admin\Secret", @"***\Secret", true)]
    [InlineData(@"C:\Users\", @"***\Users", true)]
    [InlineData(@"/home/i_use_arch_linux_btw", @"***\i_use_arch_linux_btw", true)]
    [InlineData(@"/home/i_use_arch_linux_btw", @"***\", false)]
    [InlineData(@"C:\", @"***\C:\", true)]
    [InlineData(@"C:\", @"***\", false)]
    [InlineData("File.txt", "File.txt", false)]
    [InlineData(@"This is not a path", "This is not a path", false)]
    public void GivenPaths_ReturnsExpectedResult(string path, string result, bool combineMaskWithPath)
    {
        TheMaskedResultOf(path, combineMaskWithPath)
            .Should()
            .Be(result);
    }

    private static string TheMaskedResultOf(string input, bool combineMaskWithPath)
    {
        var maskingResult = new PathMaskingOperator(combineMaskWithPath)
            .Mask(input, Mask);
        return maskingResult.Match
            ? maskingResult.Result
            : input;
    }
}