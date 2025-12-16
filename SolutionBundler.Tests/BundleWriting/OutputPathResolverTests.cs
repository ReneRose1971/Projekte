using SolutionBundler.Core.Implementations.BundleWriting;
using SolutionBundler.Core.Models;
using System;
using System.IO;
using Xunit;

namespace SolutionBundler.Tests.BundleWriting;

public class OutputPathResolverTests
{
    [Fact]
    public void ResolveOutputPath_WithCustomFileName_UsesProvidedName()
    {
        var settings = new ScanSettings { OutputFileName = "custom-bundle.md" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "ProjectName");

        Assert.EndsWith("custom-bundle.md", result);
    }

    [Fact]
    public void ResolveOutputPath_WithoutExtension_AppendsMdExtension()
    {
        var settings = new ScanSettings { OutputFileName = "bundle-without-extension" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "ProjectName");

        Assert.EndsWith("bundle-without-extension.md", result);
    }

    [Fact]
    public void ResolveOutputPath_WithEmptyFileName_UsesProjectNameAsFallback()
    {
        var settings = new ScanSettings { OutputFileName = "" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "MyProject");

        Assert.EndsWith("MyProject.md", result);
    }

    [Fact]
    public void ResolveOutputPath_WithNullFileName_UsesProjectNameAsFallback()
    {
        var settings = new ScanSettings { OutputFileName = null! };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "TestProject");

        Assert.EndsWith("TestProject.md", result);
    }

    [Fact]
    public void ResolveOutputPath_CreatesFullPath()
    {
        var settings = new ScanSettings { OutputFileName = "test.md" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project");

        Assert.True(Path.IsPathRooted(result));
        Assert.Contains("SolutionBundler", result);
        Assert.Contains("Bundles", result);
    }

    [Fact]
    public void ResolveOutputPath_WithCaseInsensitiveMdExtension_DoesNotDuplicate()
    {
        var settings1 = new ScanSettings { OutputFileName = "file.MD" };
        var settings2 = new ScanSettings { OutputFileName = "file.Md" };
        var settings3 = new ScanSettings { OutputFileName = "file.mD" };

        var result1 = OutputPathResolver.ResolveOutputPath(settings1, "Project");
        var result2 = OutputPathResolver.ResolveOutputPath(settings2, "Project");
        var result3 = OutputPathResolver.ResolveOutputPath(settings3, "Project");

        Assert.EndsWith("file.MD", result1);
        Assert.EndsWith("file.Md", result2);
        Assert.EndsWith("file.mD", result3);
        Assert.DoesNotMatch(@"\.md\.md$", result1.ToLower());
    }

    [Fact]
    public void ResolveOutputPath_PointsToDocumentsDirectory()
    {
        var settings = new ScanSettings { OutputFileName = "test.md" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project");

        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Assert.StartsWith(documentsPath, result);
    }

    [Fact]
    public void ResolveOutputPath_CreatesOutputDirectoryIfNotExists()
    {
        var settings = new ScanSettings { OutputFileName = "test.md" };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project");
        var directory = Path.GetDirectoryName(result);

        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
    }

    [Theory]
    [InlineData("bundle.md", "bundle.md")]
    [InlineData("bundle", "bundle.md")]
    [InlineData("my-bundle.markdown", "my-bundle.markdown.md")]
    [InlineData("", "Fallback.md")]
    public void ResolveOutputPath_HandlesVariousFileNames(string inputFileName, string expectedEnding)
    {
        var settings = new ScanSettings { OutputFileName = inputFileName };
        
        var result = OutputPathResolver.ResolveOutputPath(settings, "Fallback");

        Assert.EndsWith(expectedEnding, result);
    }
}
