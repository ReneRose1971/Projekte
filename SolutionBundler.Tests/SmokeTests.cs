using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using System.IO;
using Xunit;

namespace SolutionBundler.Tests;

public class SmokeTests
{
    [Fact]
    public void Scanner_Returns_SomeFiles_On_Self()
    {
        var root = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(root, "SolutionBundler.sln")))
        {
            var parent = Directory.GetParent(root);
            if (parent is null) break;
            root = parent.FullName;
        }

        var scanner = new DefaultFileScanner();
        var settings = new ScanSettings();
        var files = scanner.Scan(root, settings);

        Assert.NotNull(files);
        Assert.True(files.Count > 0);
    }
}