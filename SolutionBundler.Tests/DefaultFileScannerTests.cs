using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using Xunit;

namespace SolutionBundler.Tests;

public class DefaultFileScannerTests
{
    [Fact]
    public void Scan_Returns_ExpectedFiles_In_RepoRoot()
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
        Assert.Contains(files, f => f.RelativePath.EndsWith("SolutionBundler.sln", StringComparison.OrdinalIgnoreCase) || f.RelativePath.EndsWith("SolutionBundler.Core.csproj", StringComparison.OrdinalIgnoreCase));
    }
}