using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SolutionBundler.Tests.Integration;

/// <summary>
/// Integrationstests für BundleOrchestrator mit Group-Unterstützung.
/// Prüft, dass Bundles korrekt in Gruppen-Unterordner geschrieben werden.
/// </summary>
public class BundleOrchestratorGroupIntegrationTests : IDisposable
{
    private readonly string _tempRootDir;
    private readonly string _outputBaseDir;
    private readonly BundleOrchestrator _orchestrator;

    public BundleOrchestratorGroupIntegrationTests()
    {
        // Erstelle temporäres Test-Verzeichnis
        _tempRootDir = Path.Combine(Path.GetTempPath(), "BundleOrchestratorGroupTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRootDir);

        // Override Output-Base-Directory für Tests
        _outputBaseDir = Path.Combine(_tempRootDir, "Output");

        // Erstelle Orchestrator mit Mock-Dependencies
        var scanner = new MockFileScanner();
        var metadata = new MockProjectMetadataReader();
        var classifier = new MockContentClassifier();
        var hasher = new MockHashCalculator();
        var masker = new MockSecretMasker();
        var writer = new MarkdownBundleWriter(masker);

        _orchestrator = new BundleOrchestrator(scanner, metadata, classifier, hasher, writer);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRootDir))
                Directory.Delete(_tempRootDir, true);
        }
        catch
        {
            // Cleanup best effort
        }
    }

    [Fact]
    public void Run_WithoutGroup_CreatesFileInBaseDirectory()
    {
        // Arrange
        var projectDir = CreateTestProject("Project1");
        var settings = new ScanSettings { OutputFileName = "Project1.md" };

        // Act
        var outputPath = _orchestrator.Run(projectDir, settings, group: null);

        // Assert
        Assert.True(File.Exists(outputPath));
        Assert.Contains(Path.Combine("Bundles"), outputPath);
        Assert.DoesNotContain(Path.Combine("Bundles", "Apps"), outputPath);
        Assert.DoesNotContain(Path.Combine("Bundles", "Libraries"), outputPath);
    }

    [Fact]
    public void Run_WithGroup_CreatesFileInGroupSubdirectory()
    {
        // Arrange
        var projectDir = CreateTestProject("AppProject");
        var settings = new ScanSettings { OutputFileName = "AppProject.md" };

        // Act
        var outputPath = _orchestrator.Run(projectDir, settings, group: "Apps");

        // Assert
        Assert.True(File.Exists(outputPath));
        Assert.Contains(Path.Combine("Bundles", "Apps"), outputPath);
        Assert.EndsWith("AppProject.md", outputPath);
    }

    [Fact]
    public void Run_WithTwoProjectsInDifferentGroups_CreatesTwoFilesInSeparateDirectories()
    {
        // Arrange
        var appProject = CreateTestProject("Scriptum");
        var libProject = CreateTestProject("DataToolKit");

        var appSettings = new ScanSettings { OutputFileName = "Scriptum.md" };
        var libSettings = new ScanSettings { OutputFileName = "DataToolKit.md" };

        // Act
        var appOutputPath = _orchestrator.Run(appProject, appSettings, group: "Apps");
        var libOutputPath = _orchestrator.Run(libProject, libSettings, group: "Libraries");

        // Assert
        Assert.True(File.Exists(appOutputPath));
        Assert.True(File.Exists(libOutputPath));

        // Apps-Projekt im Apps-Ordner
        Assert.Contains(Path.Combine("Bundles", "Apps"), appOutputPath);
        Assert.EndsWith("Scriptum.md", appOutputPath);

        // Libraries-Projekt im Libraries-Ordner
        Assert.Contains(Path.Combine("Bundles", "Libraries"), libOutputPath);
        Assert.EndsWith("DataToolKit.md", libOutputPath);

        // Verschiedene Verzeichnisse
        var appDir = Path.GetDirectoryName(appOutputPath);
        var libDir = Path.GetDirectoryName(libOutputPath);
        Assert.NotEqual(appDir, libDir);
    }

    [Fact]
    public void Run_WithMultipleProjectsInSameGroup_CreatesFilesInSameDirectory()
    {
        // Arrange
        var project1 = CreateTestProject("Scriptum.Core");
        var project2 = CreateTestProject("Scriptum.Wpf");

        var settings1 = new ScanSettings { OutputFileName = "Scriptum.Core.md" };
        var settings2 = new ScanSettings { OutputFileName = "Scriptum.Wpf.md" };

        // Act
        var outputPath1 = _orchestrator.Run(project1, settings1, group: "Apps");
        var outputPath2 = _orchestrator.Run(project2, settings2, group: "Apps");

        // Assert
        Assert.True(File.Exists(outputPath1));
        Assert.True(File.Exists(outputPath2));

        var dir1 = Path.GetDirectoryName(outputPath1);
        var dir2 = Path.GetDirectoryName(outputPath2);

        // Beide im gleichen Verzeichnis
        Assert.Equal(dir1, dir2);
        Assert.Contains(Path.Combine("Bundles", "Apps"), dir1!);
    }

    [Fact]
    public void Run_WithInvalidGroupCharacters_SanitizesDirectoryName()
    {
        // Arrange
        var projectDir = CreateTestProject("TestProject");
        var settings = new ScanSettings { OutputFileName = "TestProject.md" };

        // Act
        var outputPath = _orchestrator.Run(projectDir, settings, group: "My<Group>");

        // Assert
        Assert.True(File.Exists(outputPath));
        // Ungültige Zeichen sollten durch _ ersetzt werden
        Assert.Contains("My_Group_", outputPath);
    }

    [Fact]
    public void Run_WithEmptyGroup_CreatesFileInBaseDirectory()
    {
        // Arrange
        var projectDir = CreateTestProject("Project");
        var settings = new ScanSettings { OutputFileName = "Project.md" };

        // Act
        var outputPath = _orchestrator.Run(projectDir, settings, group: "");

        // Assert
        Assert.True(File.Exists(outputPath));
        Assert.Contains(Path.Combine("Bundles"), outputPath);
        Assert.DoesNotContain(Path.Combine("Bundles", "Apps"), outputPath);
    }

    [Fact]
    public void Run_WithWhitespaceGroup_CreatesFileInBaseDirectory()
    {
        // Arrange
        var projectDir = CreateTestProject("Project");
        var settings = new ScanSettings { OutputFileName = "Project.md" };

        // Act
        var outputPath = _orchestrator.Run(projectDir, settings, group: "   ");

        // Assert
        Assert.True(File.Exists(outputPath));
        Assert.Contains(Path.Combine("Bundles"), outputPath);
    }

    [Fact]
    public void Run_MarkdownContentIsIdentical_RegardlessOfGroup()
    {
        // Arrange
        var project1 = CreateTestProject("SameProject");
        var project2 = CreateTestProject("SameProject");

        var settings = new ScanSettings { OutputFileName = "SameProject.md" };

        // Act
        var outputWithoutGroup = _orchestrator.Run(project1, settings, group: null);
        var outputWithGroup = _orchestrator.Run(project2, settings, group: "TestGroup");

        // Assert
        var content1 = File.ReadAllText(outputWithoutGroup);
        var content2 = File.ReadAllText(outputWithGroup);

        // Markdown-Inhalt sollte identisch sein, nur Pfade unterscheiden sich
        Assert.Contains("SameProject", content1);
        Assert.Contains("SameProject", content2);
    }

    #region Helper Methods

    private string CreateTestProject(string projectName)
    {
        var projectDir = Path.Combine(_tempRootDir, projectName);
        Directory.CreateDirectory(projectDir);

        // Erstelle minimale Test-Dateien
        var csprojPath = Path.Combine(projectDir, $"{projectName}.csproj");
        File.WriteAllText(csprojPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        var csPath = Path.Combine(projectDir, "Program.cs");
        File.WriteAllText(csPath, "// Test file");

        return projectDir;
    }

    #endregion

    #region Mock Implementations

    private class MockFileScanner : IFileScanner
    {
        public IReadOnlyList<FileEntry> Scan(string rootPath, ScanSettings settings)
        {
            var files = new List<FileEntry>();

            if (Directory.Exists(rootPath))
            {
                foreach (var file in Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories))
                {
                    files.Add(new FileEntry
                    {
                        FullPath = file,
                        RelativePath = Path.GetFileName(file),
                        Size = new FileInfo(file).Length,
                        Language = Path.GetExtension(file) == ".cs" ? "csharp" : "",
                        Action = BuildAction.Unknown
                    });
                }
            }

            return files;
        }
    }

    private class MockProjectMetadataReader : IProjectMetadataReader
    {
        public void EnrichBuildActions(IList<FileEntry> files, string rootPath)
        {
            // Nichts tun - für Tests nicht relevant
        }
    }

    private class MockContentClassifier : IContentClassifier
    {
        public string Classify(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension == ".cs" ? "csharp" : "";
        }
    }

    private class MockHashCalculator : IHashCalculator
    {
        public string Sha1(byte[] data)
        {
            return "mockhash";
        }
    }

    private class MockSecretMasker : ISecretMasker
    {
        public string Process(string relativePath, string content)
        {
            return content;
        }
    }

    #endregion
}
