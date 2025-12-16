using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations.BundleWriting;
using SolutionBundler.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace SolutionBundler.Tests.BundleWriting;

public class MarkdownGeneratorTests : IDisposable
{
    private readonly string _tempDir;

    public MarkdownGeneratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "MarkdownGeneratorTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
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
    public void Generate_IncludesFrontMatter()
    {
        // Arrange
        var contentReader = CreateContentReader();
        var files = new List<FileEntry>();

        // Act
        var result = MarkdownGenerator.Generate("TestProject", files, contentReader, false);

        // Assert
        Assert.Contains("---", result);
        Assert.Contains("project_root: TestProject", result);
        Assert.Contains("generated_at:", result);
        Assert.Contains("tool: SolutionBundler v1", result);
    }

    [Fact]
    public void Generate_IncludesTableOfContents()
    {
        // Arrange
        var file = CreateFileEntry("Test.cs", "content");
        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("# Inhaltsverzeichnis", result);
        Assert.Contains("* [Test.cs](#test-cs)", result);
    }

    [Fact]
    public void Generate_IncludesFileSection()
    {
        // Arrange
        var file = CreateFileEntry("Program.cs", "// C# code");
        file.Language = "csharp";
        file.Size = 100;
        file.Sha1 = "abc123";
        file.Action = BuildAction.Compile;

        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("# Dateien", result);
        Assert.Contains("## Program.cs", result);
        Assert.Contains("_size_: 100 bytes - _sha1_: abc123 - _action_: Compile", result);
        Assert.Contains("--- FILE: Program.cs | HASH: abc123 | ACTION: Compile ---", result);
        Assert.Contains("```csharp", result);
        Assert.Contains("// C# code", result);
    }

    [Fact]
    public void Generate_WithMultipleFiles_IncludesAllFiles()
    {
        // Arrange
        var file1 = CreateFileEntry("File1.cs", "content1");
        var file2 = CreateFileEntry("File2.cs", "content2");
        
        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file1, file2 };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("File1.cs", result);
        Assert.Contains("File2.cs", result);
        Assert.Contains("content1", result);
        Assert.Contains("content2", result);
    }

    [Fact]
    public void Generate_WithNoLanguage_UsesPlainCodeFence()
    {
        // Arrange
        var file = CreateFileEntry("readme.txt", "plain text");
        file.Language = "";

        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        var lines = result.Split('\n');
        Assert.Contains(lines, l => l.Trim() == "```" && Array.IndexOf(lines, l) < Array.IndexOf(lines, lines.First(x => x.Contains("plain text"))));
    }

    [Fact]
    public void Generate_WithMaskSecretsEnabled_PassesToContentReader()
    {
        // Arrange
        var file = CreateFileEntry("config.json", "password123");
        
        var masker = new TestMasker();
        var contentReader = new FileContentReader(masker);
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, true);

        // Assert
        Assert.Contains("[MASKED]", result);
        Assert.DoesNotContain("password123", result);
    }

    [Fact]
    public void Generate_WithMaskSecretsDisabled_KeepsOriginalContent()
    {
        // Arrange
        var file = CreateFileEntry("config.json", "password123");
        
        var masker = new TestMasker();
        var contentReader = new FileContentReader(masker);
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("password123", result);
        Assert.DoesNotContain("[MASKED]", result);
    }

    [Fact]
    public void Generate_WithEmptyFileList_CreatesValidMarkdown()
    {
        // Arrange
        var contentReader = CreateContentReader();
        var files = new List<FileEntry>();

        // Act
        var result = MarkdownGenerator.Generate("EmptyProject", files, contentReader, false);

        // Assert
        Assert.Contains("project_root: EmptyProject", result);
        Assert.Contains("# Inhaltsverzeichnis", result);
        Assert.Contains("# Dateien", result);
    }

    [Fact]
    public void Generate_PreservesFileOrder()
    {
        // Arrange
        var file1 = CreateFileEntry("A.cs", "a");
        var file2 = CreateFileEntry("B.cs", "b");
        var file3 = CreateFileEntry("C.cs", "c");
        
        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file1, file2, file3 };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        var indexA = result.IndexOf("## A.cs");
        var indexB = result.IndexOf("## B.cs");
        var indexC = result.IndexOf("## C.cs");

        Assert.True(indexA < indexB);
        Assert.True(indexB < indexC);
    }

    [Fact]
    public void Generate_IncludesAllBuildActions()
    {
        // Arrange
        var files = new List<FileEntry>
        {
            CreateFileEntryWithAction("Compile.cs", BuildAction.Compile),
            CreateFileEntryWithAction("Page.xaml", BuildAction.Page),
            CreateFileEntryWithAction("Resource.resx", BuildAction.Resource),
            CreateFileEntryWithAction("Content.json", BuildAction.Content),
            CreateFileEntryWithAction("None.txt", BuildAction.None),
            CreateFileEntryWithAction("Unknown.dat", BuildAction.Unknown)
        };

        var contentReader = CreateContentReader();

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("_action_: Compile", result);
        Assert.Contains("_action_: Page", result);
        Assert.Contains("_action_: Resource", result);
        Assert.Contains("_action_: Content", result);
        Assert.Contains("_action_: None", result);
        Assert.Contains("_action_: Unknown", result);
    }

    [Fact]
    public void Generate_WithSpecialCharactersInPath_HandlesCorrectly()
    {
        // Arrange
        var file = CreateFileEntry("Folder/Sub Folder/File Name.cs", "content");
        var contentReader = CreateContentReader();
        var files = new List<FileEntry> { file };

        // Act
        var result = MarkdownGenerator.Generate("Project", files, contentReader, false);

        // Assert
        Assert.Contains("Folder/Sub Folder/File Name.cs", result);
        Assert.Contains("#folder-sub-folder-file-name-cs", result);
    }

    // Helper Methods

    private FileContentReader CreateContentReader()
    {
        return new FileContentReader(new NoOpMasker());
    }

    private FileEntry CreateFileEntry(string relativePath, string content)
    {
        var fullPath = Path.Combine(_tempDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content);

        return new FileEntry
        {
            RelativePath = relativePath,
            FullPath = fullPath,
            Size = content.Length,
            Sha1 = "hash",
            Language = "",
            Action = BuildAction.Unknown
        };
    }

    private FileEntry CreateFileEntryWithAction(string relativePath, BuildAction action)
    {
        var entry = CreateFileEntry(relativePath, "content");
        entry.Action = action;
        return entry;
    }

    // Test Doubles

    private class NoOpMasker : ISecretMasker
    {
        public string Process(string relativePath, string content) => content;
    }

    private class TestMasker : ISecretMasker
    {
        public string Process(string relativePath, string content)
        {
            return content.Replace("password123", "[MASKED]");
        }
    }
}
