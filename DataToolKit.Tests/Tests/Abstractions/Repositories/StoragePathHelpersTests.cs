using DataToolKit.Abstractions.Repositories;
using System;
using System.IO;
using TestHelper.TestUtils;               // TestDirectorySandbox
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für die Hilfsklasse <see cref="StoragePathHelpers"/>.
    /// Ziel: Die reine Pfadlogik (ohne Geschäftslogik) verlässlich und deterministisch prüfen.
    /// 
    /// Hinweise:
    /// - Für I/O-nahe Tests wird eine Sandbox verwendet, die sich automatisch aufräumt.
    /// - Die Tests machen keine Annahmen über das aktuelle Working Directory.
    /// - Pfad-Assertions vermeiden harte Separatoren und prüfen Endungen/Eigenschaften statt 1:1-Strings.
    /// </summary>
    public sealed class StoragePathHelpersTests
    {
        #region NormalizeFileNameBaseOrThrow

        /// <summary>
        /// Verifiziert, dass bei null/leer/Whitespace ein <see cref="ArgumentException"/> geworfen wird.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NormalizeFileNameBaseOrThrow_Throws_OnNullOrWhitespace(string? input)
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() => StoragePathHelpers.NormalizeFileNameBaseOrThrow(input!));
        }

        /// <summary>
        /// Verifiziert, dass eine versehentlich übergebene Dateierweiterung entfernt wird.
        /// Beispiel: "data.json" → "data".
        /// </summary>
        [Theory]
        [InlineData("data.json", "data")]
        [InlineData("report.db", "report")]
        [InlineData("name", "name")]
        [InlineData("  file.txt  ", "file")]
        public void NormalizeFileNameBaseOrThrow_StripsExtension_IfProvided(string input, string expected)
        {
            // Act
            var normalized = StoragePathHelpers.NormalizeFileNameBaseOrThrow(input);

            // Assert
            Assert.Equal(expected, normalized);
        }

        #endregion

        #region CombineRootWithOptionalSub

        /// <summary>
        /// Verifiziert, dass bei null/leerem Subfolder ausschließlich der Root verwendet wird.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CombineRootWithOptionalSub_IgnoresNullOrEmptySub(string? sub)
        {
            using var sandbox = new TestDirectorySandbox();
            var root = sandbox.Root;

            // Act
            var combined = StoragePathHelpers.CombineRootWithOptionalSub(root, sub);

            // Assert
            Assert.Equal(root, combined);
        }

        /// <summary>
        /// Verifiziert, dass ein gesetzter Subfolder korrekt mit dem Root kombiniert wird.
        /// </summary>
        [Theory]
        [InlineData("Sub")]
        [InlineData(" Nested ")]
        public void CombineRootWithOptionalSub_Combines_WhenSubIsGiven(string sub)
        {
            using var sandbox = new TestDirectorySandbox();
            var root = sandbox.Root;

            // Act
            var combined = StoragePathHelpers.CombineRootWithOptionalSub(root, sub);

            // Assert
            var expected = Path.Combine(root, sub.Trim());
            Assert.Equal(expected, combined);
        }

        #endregion

        #region BuildFullPath

        /// <summary>
        /// Verifiziert die vollständige Pfadbildung aus Root, optionalem Sub, Basisname und Erweiterung.
        /// </summary>
        [Theory]
        [InlineData("Sub", "data", ".json")]
        [InlineData(null, "repo", ".db")]
        public void BuildFullPath_Joins_Root_Sub_FileName_Extension(string? sub, string baseName, string ext)
        {
            using var sandbox = new TestDirectorySandbox();
            var root = sandbox.Root;

            // Act
            var full = StoragePathHelpers.BuildFullPath(root, sub, baseName, ext);

            // Assert
            var effectiveRoot = string.IsNullOrWhiteSpace(sub) ? root : Path.Combine(root, sub.Trim());
            var expected = Path.Combine(effectiveRoot, baseName + ext);
            Assert.Equal(expected, full);
            Assert.EndsWith(ext, full, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region EnsureDirectoryFor

        /// <summary>
        /// Verifiziert, dass das Zielverzeichnis angelegt wird und ein zweiter Aufruf idempotent ist.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("Sub")]
        public void EnsureDirectoryFor_CreatesDirectory_Idempotent(string? sub)
        {
            using var sandbox = new TestDirectorySandbox();
            var root = sandbox.Root;
            var effectiveRoot = string.IsNullOrWhiteSpace(sub) ? root : Path.Combine(root, sub?.Trim()!);

            // Pre-Assert: sicherstellen, dass das Ziel nicht existiert
            if (Directory.Exists(effectiveRoot))
                Directory.Delete(effectiveRoot, recursive: true);

            // Act
            StoragePathHelpers.EnsureDirectoryFor(root, sub);

            // Assert 1: Verzeichnis existiert nach erstem Aufruf
            Assert.True(Directory.Exists(effectiveRoot));

            // Act 2: Nochmals aufrufen (Idempotenz)
            StoragePathHelpers.EnsureDirectoryFor(root, sub);

            // Assert 2: Immer noch vorhanden, keine Ausnahme
            Assert.True(Directory.Exists(effectiveRoot));
        }

        #endregion

        #region GetMyDocuments

        /// <summary>
        /// Verifiziert, dass der MyDocuments-Pfad existiert.
        /// (Es wird kein Schreibvorgang ausgeführt.)
        /// </summary>
        [Fact]
        public void GetMyDocuments_Returns_ExistingPath()
        {
            // Act
            var myDocs = StoragePathHelpers.GetMyDocuments();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(myDocs));
            Assert.True(Directory.Exists(myDocs));
        }

        #endregion
    }
}
