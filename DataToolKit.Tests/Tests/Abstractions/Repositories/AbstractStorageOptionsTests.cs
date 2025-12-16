using DataToolKit.Abstractions.Repositories;
using System;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für die Basisklasse <see cref="AbstractStorageOptions{T}"/>.
    /// Ziel: Konstruktor-Validierung, Pfadbildung, Verzeichnisanlage und Diagnoseausgabe verlässlich prüfen.
    /// Verwendet <see cref="TestDirectorySandbox"/> für saubere, isolierte Test-Umgebungen.
    /// </summary>
    public sealed class AbstractStorageOptionsTests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        public AbstractStorageOptionsTests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        // Dummy-Typ für generische Tests
        private sealed class TestEntity { }

        /// <summary>
        /// Interne Test-Ableitung mit fixer Dateiendung ".fake".
        /// AppSubFolder/SubFolder/FileNameBase werden wie in der echten Basisklasse verarbeitet.
        /// </summary>
        private sealed class FakeStorageOptions : AbstractStorageOptions<TestEntity>
        {
            protected override string FileExtension => ".fake";

            public FakeStorageOptions(string appSubFolder, string fileNameBase, string? subFolder = null, string? rootFolder = null)
                : base(appSubFolder, fileNameBase, subFolder, rootFolder)
            {
            }
        }

        #region Konstruktor-Validierung

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_Throws_OnEmptyAppSubFolder(string? appSubFolder)
        {
            Assert.Throws<ArgumentException>(() => new FakeStorageOptions(appSubFolder!, "data", null, _sandbox.Root));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_Throws_OnEmptyFileNameBase(string? fileNameBase)
        {
            Assert.Throws<ArgumentException>(() => new FakeStorageOptions("TestApp", fileNameBase!, null, _sandbox.Root));
        }

        [Theory]
        [InlineData("data.json", "data")]
        [InlineData("report.db", "report")]
        [InlineData("file", "file")]
        [InlineData("  name.txt  ", "name")]
        public void FileNameBase_Normalized_StripsExtension(string input, string expected)
        {
            // Act
            var sut = new FakeStorageOptions("TestApp", input, null, _sandbox.Root);

            // Assert
            Assert.Equal(expected, sut.FileNameBase);
        }

        #endregion

        #region Pfadbildung & Verzeichnisanlage

        [Fact]
        public void EffectiveRoot_Builds_Without_SubFolder_And_IsCreated()
        {
            // Act
            var sut = new FakeStorageOptions("TestApp", "data", null, _sandbox.Root);

            // Assert
            var expectedRoot = _sandbox.PathOf("TestApp");
            Assert.Equal(expectedRoot, sut.EffectiveRoot);
            Assert.True(System.IO.Directory.Exists(sut.EffectiveRoot));
        }

        [Fact]
        public void EffectiveRoot_Builds_With_SubFolder_And_IsCreated()
        {
            // Act
            var sut = new FakeStorageOptions("TestApp", "data", "Sub", _sandbox.Root);

            // Assert
            var expectedRoot = _sandbox.PathOf(System.IO.Path.Combine("TestApp", "Sub"));
            Assert.Equal(expectedRoot, sut.EffectiveRoot);
            Assert.True(System.IO.Directory.Exists(sut.EffectiveRoot));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Module")]
        public void FullPath_Is_Correct_With_FakeExtension(string? sub)
        {
            // Act
            var sut = new FakeStorageOptions("TestApp", "repo", sub, _sandbox.Root);

            // Assert
            Assert.EndsWith(".fake", sut.FullPath, StringComparison.OrdinalIgnoreCase);
            Assert.StartsWith(_sandbox.Root, sut.FullPath);

            var effectiveRoot = string.IsNullOrWhiteSpace(sub)
                ? _sandbox.PathOf("TestApp")
                : _sandbox.PathOf(System.IO.Path.Combine("TestApp", sub));

            var expected = System.IO.Path.Combine(effectiveRoot, "repo.fake");
            Assert.Equal(expected, sut.FullPath);
        }

        #endregion

        #region Diagnose

        [Fact]
        public void ToString_Contains_TypeName_And_FullPath()
        {
            // Arrange
            var sut = new FakeStorageOptions("TestApp", "data", null, _sandbox.Root);

            // Act
            var text = sut.ToString();

            // Assert
            Assert.Contains(nameof(FakeStorageOptions), text);
            Assert.Contains(".fake", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Path='", text);
        }

        #endregion
    }
}
