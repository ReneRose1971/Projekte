using SolutionBundler.Core.Implementations.BundleWriting;
using Xunit;

namespace SolutionBundler.Tests.BundleWriting;

public class MarkdownAnchorGeneratorTests
{
    [Theory]
    [InlineData("File.cs", "file-cs")]
    [InlineData("Folder/File.cs", "folder-file-cs")]
    [InlineData("Folder\\File.cs", "folder-file-cs")]
    [InlineData("My File.cs", "my-file-cs")]
    [InlineData("Folder/Sub Folder/File.cs", "folder-sub-folder-file-cs")]
    [InlineData("C:\\Path\\To\\File.cs", "c--path-to-file-cs")]
    [InlineData("File:Name.cs", "file-name-cs")]
    public void Generate_ReturnsCorrectAnchor(string relativePath, string expected)
    {
        var result = MarkdownAnchorGenerator.Generate(relativePath);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Generate_ConvertsToLowercase()
    {
        var result = MarkdownAnchorGenerator.Generate("UPPERCASE.CS");

        Assert.Equal("uppercase-cs", result);
    }

    [Fact]
    public void Generate_ReplacesBackslashesWithForwardSlashes()
    {
        var result = MarkdownAnchorGenerator.Generate("Folder\\SubFolder\\File.cs");

        Assert.Contains("folder-subfolder-file-cs", result);
    }

    [Fact]
    public void Generate_HandlesComplexPaths()
    {
        var result = MarkdownAnchorGenerator.Generate("Project/Src Folder/My File.Name:Version.cs");

        Assert.Equal("project-src-folder-my-file-name-version-cs", result);
    }

    [Fact]
    public void Generate_HandlesEmptyString()
    {
        var result = MarkdownAnchorGenerator.Generate("");

        Assert.Equal("", result);
    }

    [Fact]
    public void Generate_HandlesSingleCharacter()
    {
        var result = MarkdownAnchorGenerator.Generate("A");

        Assert.Equal("a", result);
    }

    [Theory]
    [InlineData("file.with.many.dots.cs", "file-with-many-dots-cs")]
    [InlineData("path/to/file.min.js", "path-to-file-min-js")]
    public void Generate_HandlesMultipleDots(string path, string expected)
    {
        var result = MarkdownAnchorGenerator.Generate(path);

        Assert.Equal(expected, result);
    }
}
