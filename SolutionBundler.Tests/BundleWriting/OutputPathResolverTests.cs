using SolutionBundler.Core.Implementations.BundleWriting;
using SolutionBundler.Core.Models;
using System;
using System.IO;
using System.Linq;
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

    #region Group Tests

    [Fact]
    public void ResolveOutputPath_WithoutGroup_UsesBaseDirectory()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: null);

        // Assert
        Assert.Contains(Path.Combine("SolutionBundler", "Bundles"), result);
        Assert.DoesNotContain(Path.Combine("Bundles", "Apps"), result);
        Assert.DoesNotContain(Path.Combine("Bundles", "Libraries"), result);
    }

    [Fact]
    public void ResolveOutputPath_WithEmptyGroup_UsesBaseDirectory()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "");

        // Assert
        Assert.Contains(Path.Combine("SolutionBundler", "Bundles"), result);
        Assert.DoesNotContain(Path.Combine("Bundles", "Apps"), result);
    }

    [Fact]
    public void ResolveOutputPath_WithWhitespaceGroup_UsesBaseDirectory()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "   ");

        // Assert
        Assert.Contains(Path.Combine("SolutionBundler", "Bundles"), result);
    }

    [Fact]
    public void ResolveOutputPath_WithValidGroup_CreatesGroupSubdirectory()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "project.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "Apps");

        // Assert
        Assert.Contains(Path.Combine("Bundles", "Apps", "project.md"), result);
        var directory = Path.GetDirectoryName(result);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
    }

    [Theory]
    [InlineData("Apps", "Apps")]
    [InlineData("Libraries", "Libraries")]
    [InlineData("Core Components", "Core Components")]
    [InlineData("Test Projects", "Test Projects")]
    public void ResolveOutputPath_WithDifferentGroups_CreatesCorrectSubdirectories(string group, string expectedInPath)
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: group);

        // Assert
        Assert.Contains(Path.Combine("Bundles", expectedInPath), result);
        var directory = Path.GetDirectoryName(result);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void ResolveOutputPath_WithInvalidCharactersInGroup_SanitizesGroupName()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act - Gruppe mit ungültigen Zeichen: < > : " / \ | ? *
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "Apps<>:\"/\\|?*Test");

        // Assert
        // Ungültige Zeichen sollten durch _ ersetzt werden
        // Prüfe dass der Pfad existiert und einen bereinigten Namen enthält
        Assert.Contains(Path.Combine("Bundles"), result);
        var directory = Path.GetDirectoryName(result);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
        
        // Der bereinigte Name sollte keine ungültigen Zeichen mehr enthalten
        var groupDir = new DirectoryInfo(directory).Name;
        Assert.DoesNotContain("<", groupDir);
        Assert.DoesNotContain(">", groupDir);
        Assert.DoesNotContain(":", groupDir);
        Assert.DoesNotContain("\"", groupDir);
        Assert.DoesNotContain("/", groupDir);
        Assert.DoesNotContain("\\", groupDir);
        Assert.DoesNotContain("|", groupDir);
        Assert.DoesNotContain("?", groupDir);
        Assert.DoesNotContain("*", groupDir);
    }

    [Fact]
    public void ResolveOutputPath_WithGroupContainingOnlyInvalidChars_UsesDefaultName()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "<>:\"/\\|?*");

        // Assert
        // Wenn nach Bereinigung nur Whitespace/Underscores übrig sind, sollte ein Fallback verwendet werden
        Assert.Contains(Path.Combine("Bundles"), result);
        var directory = Path.GetDirectoryName(result);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
        
        // Der resultierende Pfad sollte einen gültigen Ordner haben (entweder Default oder bereinigter Name)
        var groupDir = new DirectoryInfo(directory).Name;
        Assert.NotEmpty(groupDir);
        Assert.True(groupDir == "Default" || groupDir.All(c => c == '_'), 
            $"Expected 'Default' or all underscores, but got: {groupDir}");
    }

    [Fact]
    public void ResolveOutputPath_WithGroupHavingLeadingTrailingSpaces_TrimsSpaces()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "  Apps  ");

        // Assert
        Assert.Contains(Path.Combine("Bundles", "Apps"), result);
        Assert.DoesNotContain("  Apps  ", result);
    }

    [Fact]
    public void ResolveOutputPath_GroupsAreCaseSensitiveInFileSystem()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act
        var resultLower = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "apps");
        var resultUpper = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "APPS");
        var resultMixed = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "Apps");

        // Assert - Auf Windows sind Pfade case-insensitive, aber Namen werden beibehalten
        Assert.Contains(Path.Combine("Bundles", "apps"), resultLower);
        Assert.Contains(Path.Combine("Bundles", "APPS"), resultUpper);
        Assert.Contains(Path.Combine("Bundles", "Apps"), resultMixed);
    }

    [Fact]
    public void ResolveOutputPath_WithNestedGroupName_CreatesNestedDirectory()
    {
        // Arrange
        var settings = new ScanSettings { OutputFileName = "test.md" };

        // Act - Gruppe enthält Backslash (wird zu Underscore)
        var result = OutputPathResolver.ResolveOutputPath(settings, "Project", group: "Apps\\Scriptum");

        // Assert
        // Backslash ist ungültiges Zeichen und wird zu_
        Assert.Contains(Path.Combine("Bundles", "Apps_Scriptum"), result);
        var directory = Path.GetDirectoryName(result);
        Assert.NotNull(directory);
        Assert.True(Directory.Exists(directory));
    }

    #endregion
}
