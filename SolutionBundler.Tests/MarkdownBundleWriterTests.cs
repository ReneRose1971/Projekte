using System.Text;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations;
using SolutionBundler.Core.Models;
using Xunit;

namespace SolutionBundler.Tests;

public class MarkdownBundleWriterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _testOutputDir;

    public MarkdownBundleWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "MarkdownBundleWriterTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _testOutputDir = Path.Combine(_tempDir, "Output");
        Directory.CreateDirectory(_testOutputDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch
        {
            // Cleanup best effort
        }
    }

    [Fact]
    public void Write_WithEmptyFileList_CreatesValidMarkdown()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("empty-test.md");
        var files = Array.Empty<FileEntry>();

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        Assert.True(File.Exists(outputPath));
        var content = File.ReadAllText(outputPath);
        Assert.Contains("project_root:", content);
        Assert.Contains("generated_at:", content);
        Assert.Contains("tool: SolutionBundler v1", content);
        Assert.Contains("# Inhaltsverzeichnis", content);
        Assert.Contains("# Dateien", content);
    }

    [Fact]
    public void Write_WithSingleFile_CreatesCorrectStructure()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "Test.cs");
        File.WriteAllText(testFile, "// Test content\npublic class Test { }");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("single-file-test.md");

        var files = new[]
        {
            new FileEntry
            {
                RelativePath = "Test.cs",
                FullPath = testFile,
                Size = 100,
                Sha1 = "abc123",
                Language = "csharp",
                Action = BuildAction.Compile
            }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        Assert.True(File.Exists(outputPath));
        var content = File.ReadAllText(outputPath);

        // Check table of contents
        Assert.Contains("* [Test.cs](#test-cs)", content);

        // Check file section
        Assert.Contains("## Test.cs", content);
        Assert.Contains("_size_: 100 bytes - _sha1_: abc123 - _action_: Compile", content);
        Assert.Contains("--- FILE: Test.cs | HASH: abc123 | ACTION: Compile ---", content);
        Assert.Contains("```csharp", content);
        Assert.Contains("// Test content", content);
        Assert.Contains("public class Test { }", content);
    }

    [Fact]
    public void Write_WithMultipleFiles_CreatesCompleteTableOfContents()
    {
        // Arrange
        var file1 = Path.Combine(_tempDir, "File1.cs");
        var file2 = Path.Combine(_tempDir, "File2.xaml");
        File.WriteAllText(file1, "class File1 { }");
        File.WriteAllText(file2, "<Window />");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("multi-file-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = "File1.cs", FullPath = file1, Size = 10, Sha1 = "hash1", Language = "csharp", Action = BuildAction.Compile },
            new FileEntry { RelativePath = "File2.xaml", FullPath = file2, Size = 20, Sha1 = "hash2", Language = "xaml", Action = BuildAction.Page }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("* [File1.cs](#file1-cs)", content);
        Assert.Contains("* [File2.xaml](#file2-xaml)", content);
        Assert.Contains("## File1.cs", content);
        Assert.Contains("## File2.xaml", content);
    }

    [Fact]
    public void Write_WithMaskSecretsEnabled_CallsSecretMasker()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "Config.json");
        File.WriteAllText(testFile, "{ \"apiKey\": \"secret123\" }");

        var masker = new TestSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("masked-test.md", maskSecrets: true);

        var files = new[]
        {
            new FileEntry { RelativePath = "Config.json", FullPath = testFile, Size = 50, Sha1 = "hash", Language = "json", Action = BuildAction.Content }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("[MASKED]", content);
        Assert.DoesNotContain("secret123", content);
    }

    [Fact]
    public void Write_WithMaskSecretsDisabled_DoesNotCallSecretMasker()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "Config.json");
        File.WriteAllText(testFile, "{ \"apiKey\": \"secret123\" }");

        var masker = new TestSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("unmasked-test.md", maskSecrets: false);

        var files = new[]
        {
            new FileEntry { RelativePath = "Config.json", FullPath = testFile, Size = 50, Sha1 = "hash", Language = "json", Action = BuildAction.Content }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("secret123", content);
        Assert.DoesNotContain("[MASKED]", content);
    }

    [Fact]
    public void Write_WithUnreadableFile_ShowsErrorMessage()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("error-test.md");

        var files = new[]
        {
            new FileEntry
            {
                RelativePath = "NonExistent.cs",
                FullPath = Path.Combine(_tempDir, "DoesNotExist.cs"),
                Size = 0,
                Sha1 = "hash",
                Language = "csharp",
                Action = BuildAction.Compile
            }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("FEHLER: Datei konnte nicht gelesen werden", content);
        Assert.Contains("DoesNotExist.cs", content);
    }

    [Fact]
    public void Write_WithEmptyLanguage_UsesPlainCodeFence()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "NoLang.txt");
        File.WriteAllText(testFile, "Plain text content");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("plain-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = "NoLang.txt", FullPath = testFile, Size = 20, Sha1 = "hash", Language = "", Action = BuildAction.Unknown }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        var lines = content.Split('\n');
        var fenceLine = lines.FirstOrDefault(l => l.Trim() == "```" && lines[Array.IndexOf(lines, l) + 1].Contains("Plain text"));
        Assert.NotNull(fenceLine);
    }

    [Fact]
    public void Write_WithCustomOutputFileName_UsesSpecifiedName()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("custom-bundle-name.md");

        // Act
        var outputPath = writer.Write(_tempDir, Array.Empty<FileEntry>(), settings);

        // Assert
        Assert.EndsWith("custom-bundle-name.md", outputPath);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Write_WithoutMdExtension_AppendsMdExtension()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("without-extension");

        // Act
        var outputPath = writer.Write(_tempDir, Array.Empty<FileEntry>(), settings);

        // Assert
        Assert.EndsWith(".md", outputPath);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Write_WithNullOrEmptyFileName_UsesFallbackName()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = new ScanSettings { OutputFileName = "" };

        // Act
        var outputPath = writer.Write(_tempDir, Array.Empty<FileEntry>(), settings);

        // Assert
        var fileName = Path.GetFileName(outputPath);
        Assert.NotEmpty(fileName);
        Assert.EndsWith(".md", fileName);
        Assert.True(File.Exists(outputPath));
    }

    [Fact]
    public void Write_CreatesOutputDirectoryIfNotExists()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("dir-test.md");

        // Act
        var outputPath = writer.Write(_tempDir, Array.Empty<FileEntry>(), settings);

        // Assert
        Assert.True(File.Exists(outputPath));
        var directory = Path.GetDirectoryName(outputPath);
        Assert.True(Directory.Exists(directory));
    }

    [Fact]
    public void Write_GeneratesValidAnchorLinks()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "My File.cs");
        File.WriteAllText(testFile, "// Test");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("anchor-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = "Folder/Sub Folder/My File.cs", FullPath = testFile, Size = 10, Sha1 = "hash", Language = "csharp", Action = BuildAction.Compile }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        // Anchor should have: spaces?-, /?-, .?-, :?-, lowercase
        Assert.Contains("[Folder/Sub Folder/My File.cs](#folder-sub-folder-my-file-cs)", content);
    }

    [Fact]
    public void Write_IncludesProjectMetadataInFrontMatter()
    {
        // Arrange
        var projectRoot = Path.Combine(_tempDir, "MyProject");
        Directory.CreateDirectory(projectRoot);

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("frontmatter-test.md");

        // Act
        var outputPath = writer.Write(projectRoot, Array.Empty<FileEntry>(), settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        var lines = content.Split('\n').Select(l => l.Trim()).ToArray();

        Assert.Equal("---", lines[0]);
        Assert.Contains(lines, l => l.StartsWith("project_root:") && l.Contains("MyProject"));
        Assert.Contains(lines, l => l.StartsWith("generated_at:"));
        Assert.Contains(lines, l => l == "tool: SolutionBundler v1");
        Assert.Contains(lines, l => l == "---");
    }

    [Fact]
    public void Write_IncludesAllBuildActionTypes()
    {
        // Arrange
        var files = new[]
        {
            CreateFileEntry("Compile.cs", "content", BuildAction.Compile),
            CreateFileEntry("Page.xaml", "content", BuildAction.Page),
            CreateFileEntry("Resource.resx", "content", BuildAction.Resource),
            CreateFileEntry("Content.json", "content", BuildAction.Content),
            CreateFileEntry("None.txt", "content", BuildAction.None),
            CreateFileEntry("Unknown.dat", "content", BuildAction.Unknown)
        };

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("buildactions-test.md");

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("_action_: Compile", content);
        Assert.Contains("_action_: Page", content);
        Assert.Contains("_action_: Resource", content);
        Assert.Contains("_action_: Content", content);
        Assert.Contains("_action_: None", content);
        Assert.Contains("_action_: Unknown", content);
    }

    [Fact]
    public void Write_HandlesSpecialCharactersInPaths()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "special.cs");
        File.WriteAllText(testFile, "// Test");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("special-chars-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = "Folder\\Sub:Folder/File.Name.cs", FullPath = testFile, Size = 10, Sha1 = "hash", Language = "csharp", Action = BuildAction.Compile }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        Assert.Contains("Folder\\Sub:Folder/File.Name.cs", content); // Original path preserved
        Assert.Contains("#folder-sub-folder-file-name-cs", content); // Anchor normalized
    }

    [Fact]
    public void Write_PreservesUtf8Encoding()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "Utf8.cs");
        File.WriteAllText(testFile, "// Umlaute: äöü ÄÖÜ ß\n// Symbols: €™®", Encoding.UTF8);

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("utf8-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = "Utf8.cs", FullPath = testFile, Size = 50, Sha1 = "hash", Language = "csharp", Action = BuildAction.Compile }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath, Encoding.UTF8);
        Assert.Contains("äöü", content);
        Assert.Contains("ÄÖÜ", content);
        Assert.Contains("ß", content);
        Assert.Contains("€™®", content);
    }

    [Fact]
    public void Write_WithBackslashInRelativePath_ConvertsToForwardSlashInAnchor()
    {
        // Arrange
        var testFile = Path.Combine(_tempDir, "test.cs");
        File.WriteAllText(testFile, "// test");

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("backslash-test.md");

        var files = new[]
        {
            new FileEntry { RelativePath = @"Folder\File.cs", FullPath = testFile, Size = 10, Sha1 = "hash", Language = "csharp", Action = BuildAction.Compile }
        };

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        var content = File.ReadAllText(outputPath);
        // Anchor should use forward slashes
        Assert.Contains("#folder-file-cs", content);
    }

    [Fact]
    public void Write_ReturnsCorrectOutputPath()
    {
        // Arrange
        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("return-path-test.md");

        // Act
        var outputPath = writer.Write(_tempDir, Array.Empty<FileEntry>(), settings);

        // Assert
        Assert.NotNull(outputPath);
        Assert.NotEmpty(outputPath);
        Assert.True(Path.IsPathRooted(outputPath));
        Assert.EndsWith("return-path-test.md", outputPath);
        Assert.Contains("SolutionBundler", outputPath);
        Assert.Contains("Bundles", outputPath);
    }

    [Fact]
    public void Write_WithLargeFileList_CompletesSuccessfully()
    {
        // Arrange
        var files = Enumerable.Range(1, 100)
            .Select(i => CreateFileEntry($"File{i}.cs", $"// Content {i}", BuildAction.Compile))
            .ToArray();

        var masker = new NoOpSecretMasker();
        var writer = new MarkdownBundleWriter(masker);
        var settings = CreateSettings("large-test.md");

        // Act
        var outputPath = writer.Write(_tempDir, files, settings);

        // Assert
        Assert.True(File.Exists(outputPath));
        var content = File.ReadAllText(outputPath);
        Assert.Contains("File1.cs", content);
        Assert.Contains("File100.cs", content);
        Assert.Contains("// Content 1", content);
        Assert.Contains("// Content 100", content);
    }

    // Helper Methods

    private ScanSettings CreateSettings(string outputFileName, bool maskSecrets = false)
    {
        return new ScanSettings
        {
            OutputFileName = outputFileName,
            MaskSecrets = maskSecrets
        };
    }

    private FileEntry CreateFileEntry(string relativePath, string content, BuildAction action)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content);

        return new FileEntry
        {
            RelativePath = relativePath,
            FullPath = fullPath,
            Size = content.Length,
            Sha1 = "hash" + relativePath.GetHashCode(),
            Language = Path.GetExtension(relativePath) switch
            {
                ".cs" => "csharp",
                ".xaml" => "xaml",
                ".json" => "json",
                _ => ""
            },
            Action = action
        };
    }

    // Test Doubles

    private class NoOpSecretMasker : ISecretMasker
    {
        public string Process(string relativePath, string content) => content;
    }

    private class TestSecretMasker : ISecretMasker
    {
        public string Process(string relativePath, string content)
        {
            return content.Replace("secret123", "[MASKED]");
        }
    }
}
