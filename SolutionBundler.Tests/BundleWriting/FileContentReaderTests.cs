using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations.BundleWriting;
using System;
using System.IO;
using Xunit;

namespace SolutionBundler.Tests.BundleWriting;

public class FileContentReaderTests : IDisposable
{
    private readonly string _tempDir;

    public FileContentReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FileContentReaderTests_" + Guid.NewGuid().ToString("N"));
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
    public void ReadContent_WithValidFile_ReturnsFileContent()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "Hello World");
        
        var masker = new NoOpMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "test.txt", false);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ReadContent_WithMaskSecretsEnabled_AppliesMasking()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "secrets.txt");
        File.WriteAllText(filePath, "Secret: password123");
        
        var masker = new TestMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "secrets.txt", true);

        // Assert
        Assert.Equal("Secret: [MASKED]", result);
    }

    [Fact]
    public void ReadContent_WithMaskSecretsDisabled_DoesNotApplyMasking()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "secrets.txt");
        File.WriteAllText(filePath, "Secret: password123");
        
        var masker = new TestMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "secrets.txt", false);

        // Assert
        Assert.Equal("Secret: password123", result);
    }

    [Fact]
    public void ReadContent_WithNonExistentFile_ReturnsErrorMessage()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDir, "does-not-exist.txt");
        
        var masker = new NoOpMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(nonExistentPath, "does-not-exist.txt", false);

        // Assert
        Assert.Contains("FEHLER", result);
        Assert.Contains("konnte nicht gelesen werden", result);
        Assert.Contains(nonExistentPath, result);
    }

    [Fact]
    public void ReadContent_WithEmptyFile_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "empty.txt");
        File.WriteAllText(filePath, "");
        
        var masker = new NoOpMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "empty.txt", false);

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public void ReadContent_WithMultilineContent_PreservesNewlines()
    {
        // Arrange
        var content = "Line 1\nLine 2\nLine 3";
        var filePath = Path.Combine(_tempDir, "multiline.txt");
        File.WriteAllText(filePath, content);
        
        var masker = new NoOpMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "multiline.txt", false);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadContent_WithUtf8Content_PreservesEncoding()
    {
        // Arrange
        var content = "Umlaute: äöü ÄÖÜ ß";
        var filePath = Path.Combine(_tempDir, "utf8.txt");
        File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
        
        var masker = new NoOpMasker();
        var reader = new FileContentReader(masker);

        // Act
        var result = reader.ReadContent(filePath, "utf8.txt", false);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ReadContent_PassesRelativePathToMasker()
    {
        // Arrange
        var filePath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(filePath, "content");
        
        var masker = new PathCapturingMasker();
        var reader = new FileContentReader(masker);

        // Act
        reader.ReadContent(filePath, "relative/path/test.txt", true);

        // Assert
        Assert.Equal("relative/path/test.txt", masker.CapturedPath);
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

    private class PathCapturingMasker : ISecretMasker
    {
        public string? CapturedPath { get; private set; }

        public string Process(string relativePath, string content)
        {
            CapturedPath = relativePath;
            return content;
        }
    }
}
