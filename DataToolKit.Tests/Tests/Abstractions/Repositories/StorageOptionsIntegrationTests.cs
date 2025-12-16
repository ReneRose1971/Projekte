using System;
using System.IO;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.Storage.Repositories
{
    /// <summary>
    /// End-to-End-Tests über beide Optionen (JSON & LiteDB).
    /// Ziel:
    /// - Unabhängige, kollisionsfreie FullPaths
    /// - Verzeichnisse werden korrekt angelegt
    /// - Wiederholte Erzeugung ist idempotent
    /// - FullPaths enthalten keine doppelten Separatoren
    /// Verwendet <see cref="TestDirectorySandbox"/> für saubere Test-Umgebungen.
    /// </summary>
    public sealed class StorageOptionsIntegrationTests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        // Dummy-Typen für generische Tests
        private sealed class JsonTestEntity { }
        private sealed class LiteDbTestEntity { }

        public StorageOptionsIntegrationTests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        [Fact]
        public void Json_And_LiteDb_Produce_Independent_FullPaths_And_CreateDirectories()
        {
            // Act
            var json = new JsonStorageOptions<JsonTestEntity>("AppJson", "settings", "Config", _sandbox.Root);
            var db = new LiteDbStorageOptions<LiteDbTestEntity>("AppDb", "data", "Store", _sandbox.Root);

            // Assert
            Assert.True(Directory.Exists(json.EffectiveRoot));
            Assert.True(Directory.Exists(db.EffectiveRoot));

            Assert.EndsWith(Path.Combine("Config", "settings.json"), json.FullPath, StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith(Path.Combine("Store", "data.db"), db.FullPath, StringComparison.OrdinalIgnoreCase);

            Assert.NotEqual(json.FullPath, db.FullPath);
            Assert.NotEqual(json.EffectiveRoot, db.EffectiveRoot);
        }

        [Theory]
        [InlineData("Alpha", "SubA", "fileA")]
        [InlineData("Beta", null, "fileB")]
        [InlineData("Gamma", "Deep", "fileC")]
        public void Reconstruction_Is_Idempotent_Same_FullPath_NoExceptions(string appName, string? sub, string baseName)
        {
            // Act
            var first = new JsonStorageOptions<JsonTestEntity>(appName, baseName, sub, _sandbox.Root);
            var second = new JsonStorageOptions<JsonTestEntity>(appName, baseName, sub, _sandbox.Root);

            // Assert
            Assert.Equal(first.FullPath, second.FullPath);
            Assert.True(Directory.Exists(first.EffectiveRoot));
            Assert.True(Directory.Exists(second.EffectiveRoot));
        }

        [Theory]
        [InlineData("Cfg", "Sub", "settings")]
        [InlineData("Cfg2", null, "repo")]
        public void FullPath_Has_No_Duplicate_Separators(string appName, string? sub, string baseName)
        {
            // Act
            var json = new JsonStorageOptions<JsonTestEntity>(appName, baseName, sub, _sandbox.Root);
            var path = json.FullPath;

            // Assert
            Assert.DoesNotContain(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar, path);
            Assert.EndsWith(".json", path, StringComparison.OrdinalIgnoreCase);
            Assert.True(FileNameSegment(path).EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        }

        private static string FileNameSegment(string fullPath)
            => Path.GetFileName(fullPath);
    }
}
