using System;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.Storage.Repositories
{
    /// <summary>
    /// Tests für <see cref="LiteDbStorageOptions{T}"/>.
    /// Ziel: korrekte Endung (.db), Pfadbildung, automatische Verzeichnisanlage,
    /// Connection-String ohne "shared".
    /// Verwendet <see cref="TestDirectorySandbox"/> für saubere Test-Umgebungen.
    /// </summary>
    public sealed class LiteDbStorageOptionsTests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        // Dummy-Typ für generische Tests
        private sealed class TestEntity { }

        public LiteDbStorageOptionsTests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Module")]
        public void FullPath_EndsWith_DotDb(string? subFolder)
        {
            // Act
            var sut = new LiteDbStorageOptions<TestEntity>("TestApp", "repo", subFolder, _sandbox.Root);

            // Assert
            Assert.EndsWith(".db", sut.FullPath, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith(_sandbox.Root, sut.FullPath);
            
            var effectiveRoot = string.IsNullOrWhiteSpace(subFolder)
                ? _sandbox.PathOf("TestApp")
                : _sandbox.PathOf(System.IO.Path.Combine("TestApp", subFolder!.Trim()));
            var expected = System.IO.Path.Combine(effectiveRoot, "repo.db");
            Assert.Equal(expected, sut.FullPath);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Sub")]
        public void Ctor_Creates_Directory_With_And_Without_SubFolder(string? subFolder)
        {
            // Act
            var sut = new LiteDbStorageOptions<TestEntity>("TestApp", "data", subFolder, _sandbox.Root);

            // Assert
            Assert.True(System.IO.Directory.Exists(sut.EffectiveRoot));
            Assert.StartsWith(sut.EffectiveRoot, sut.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Nested")]
        public void GetConnectionString_Returns_Filename_Only_NoShared(string? subFolder)
        {
            // Act
            var sut = new LiteDbStorageOptions<TestEntity>("TestApp", "config", subFolder, _sandbox.Root);
            var cs = sut.GetConnectionString();

            // Assert
            Assert.StartsWith("Filename=", cs, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(".db", cs, StringComparison.OrdinalIgnoreCase);
            Assert.Equal($"Filename={sut.FullPath}", cs);
            Assert.DoesNotContain("shared", cs, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("=", cs.Substring("Filename=".Length));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Nested")]
        public void MultipleOptions_ForSameType_HaveSamePath_WhenConfiguredIdentically(string? subFolder)
        {
            // Act
            var options1 = new LiteDbStorageOptions<TestEntity>("TestApp", "config", subFolder, _sandbox.Root);
            var options2 = new LiteDbStorageOptions<TestEntity>("TestApp", "config", subFolder, _sandbox.Root);

            // Assert: Eigenschaften identisch
            Assert.Equal(options1.AppSubFolder, options2.AppSubFolder);
            Assert.Equal(options1.SubFolder, options2.SubFolder);
            Assert.Equal(options1.FileNameBase, options2.FileNameBase);
            Assert.Equal(options1.RootFolder, options2.RootFolder);
            Assert.Equal(options1.EffectiveRoot, options2.EffectiveRoot);
            Assert.Equal(options1.FullPath, options2.FullPath);

            // Zusatz: Endung und Verzeichnisvorhandensein
            Assert.EndsWith(".db", options2.FullPath, StringComparison.OrdinalIgnoreCase);
            Assert.True(System.IO.Directory.Exists(options2.EffectiveRoot));
        }
    }
}
