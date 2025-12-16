using SolutionBundler.Core.Implementations.MetadataReading;
using Xunit;

namespace SolutionBundler.Tests.MetadataReading;

public class PathNormalizerTests
{
    [Fact]
    public void NormalizeRelativePath_ConvertsBackslashesToForwardSlashes()
    {
        var rootPath = @"C:\Solution";
        var absolutePath = @"C:\Solution\Project\File.cs";

        var result = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

        Assert.Equal("Project/File.cs", result);
    }

    [Fact]
    public void NormalizeRelativePath_HandlesNestedDirectories()
    {
        var rootPath = @"C:\Root";
        var absolutePath = @"C:\Root\Folder1\Folder2\Folder3\File.txt";

        var result = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

        Assert.Equal("Folder1/Folder2/Folder3/File.txt", result);
    }

    [Fact]
    public void NormalizeRelativePath_HandlesFileInRootDirectory()
    {
        var rootPath = @"C:\Root";
        var absolutePath = @"C:\Root\File.cs";

        var result = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

        Assert.Equal("File.cs", result);
    }

    [Fact]
    public void NormalizeRelativePath_WorksWithUnixStylePaths()
    {
        var rootPath = "/home/user/solution";
        var absolutePath = "/home/user/solution/project/file.cs";

        var result = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

        Assert.Equal("project/file.cs", result);
    }

    [Theory]
    [InlineData(@"C:\Root", @"C:\Root\Project\src\File.cs", "Project/src/File.cs")]
    [InlineData(@"C:\Solution", @"C:\Solution\Core\Models\Entry.cs", "Core/Models/Entry.cs")]
    [InlineData(@"D:\Dev\App", @"D:\Dev\App\Tests\TestClass.cs", "Tests/TestClass.cs")]
    public void NormalizeRelativePath_ProducesCorrectRelativePaths(string rootPath, string absolutePath, string expected)
    {
        var result = PathNormalizer.NormalizeRelativePath(absolutePath, rootPath);

        Assert.Equal(expected, result);
    }
}
