using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.Storage.Repositories
{
    /// <summary>
    /// Kleinteilige Unit-Tests für JsonRepository<T>:
    /// - Roundtrip (Write/Load)
    /// - Atomisches Ersetzen (2. Write -> .bak vorhanden, keine .tmp-Leichen)
    /// - Clear() (idempotent)
    /// - Load() bei fehlender Datei
    /// - Rückgabe ist ReadOnly
    /// - Fehlerfälle (Write null, null-Elemente, korrupte JSON-Datei)
    /// - Write mit leerer Eingabe -> leere Persistenz
    /// Verwendet <see cref="TestDirectorySandbox"/> für saubere Test-Umgebungen.
    /// </summary>
    public sealed class JsonRepository_Tests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        public JsonRepository_Tests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        private (JsonRepository<TestEntity> repo, string fullPath) CreateRepo()
        {
            var opts = new JsonStorageOptions<TestEntity>(
                appSubFolder: "TestApp",
                fileNameBase: "JsonRepoTests",
                subFolder: "data",
                rootFolder: _sandbox.Root
            );

            var repo = new JsonRepository<TestEntity>(opts);
            return (repo, opts.FullPath);
        }

        private static TestEntity E(int id, string name, int value) => new TestEntity { Id = id, Name = name, Value = value };

        // ----------------- Tests -----------------

        [Fact]
        public void Write_Then_Load_Should_Roundtrip_Data()
        {
            var (repo, path) = CreateRepo();

            var incoming = new[]
            {
                E(1, "Alpha", 10),
                E(2, "Beta",  20)
            };

            repo.Write(incoming);

            var loaded = repo.Load().OrderBy(x => x.Id).ToList();
            Assert.Equal(2, loaded.Count);
            Assert.Equal(new[] { 1, 2 }, loaded.Select(x => x.Id));
            Assert.Equal(new[] { "Alpha", "Beta" }, loaded.Select(x => x.Name));
            Assert.Equal(new[] { 10, 20 }, loaded.Select(x => x.Value));

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void Write_Should_Create_Directory_When_Not_Existing()
        {
            var (repo, path) = CreateRepo();

            // Verzeichnis existiert (noch) nicht
            var dir = Path.GetDirectoryName(path)!;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Assert.False(Directory.Exists(dir));

            repo.Write(new[] { E(1, "A", 1) });

            Assert.True(File.Exists(path));
        }

        [Fact]
        public void Second_Write_Should_Create_Bak_And_No_Tmp_Leftovers()
        {
            var (repo, path) = CreateRepo();
            var bak = path + ".bak";
            var tmp = path + ".tmp";

            // 1. Write
            repo.Write(new[] { E(1, "A", 1) });
            Assert.True(File.Exists(path));
            Assert.False(File.Exists(bak)); // nach erstem Write evtl. kein Backup
            Assert.False(File.Exists(tmp));

            // 2. Write (anderer Inhalt)
            repo.Write(new[] { E(1, "A*", 11), E(2, "B", 2) });

            Assert.True(File.Exists(path));
            Assert.True(File.Exists(bak));  // File.Replace sollte ein .bak angelegt haben
            Assert.False(File.Exists(tmp)); // keine .tmp-Leiche

            var loaded = repo.Load().OrderBy(x => x.Id).ToList();
            Assert.Equal(2, loaded.Count);
            Assert.Equal("A*", loaded[0].Name);
            Assert.Equal(11, loaded[0].Value);
        }

        [Fact]
        public void Clear_Should_Empty_File_And_Load_Returns_Empty()
        {
            var (repo, path) = CreateRepo();

            repo.Write(new[] { E(1, "A", 1), E(2, "B", 2) });
            Assert.NotEmpty(repo.Load());

            repo.Clear();

            var after = repo.Load();
            Assert.Empty(after);
            Assert.True(File.Exists(path)); // Datei existiert weiterhin (Implementierungsdetail)
        }

        [Fact]
        public void Clear_Should_Be_Idempotent()
        {
            var (repo, _) = CreateRepo();

            repo.Clear(); // kein Fehler, leer
            Assert.Empty(repo.Load());

            repo.Write(new[] { E(1, "A", 1) });
            Assert.NotEmpty(repo.Load());

            repo.Clear();
            Assert.Empty(repo.Load());

            repo.Clear();
            Assert.Empty(repo.Load());
        }

        [Fact]
        public void Load_On_Missing_File_Should_Return_Empty_List()
        {
            var (repo, path) = CreateRepo();

            // Noch nie geschrieben -> Datei fehlt
            var dir = Path.GetDirectoryName(path)!;
            if (Directory.Exists(dir)) Directory.Delete(dir, true);

            var list = repo.Load();
            Assert.Empty(list);
        }

        [Fact]
        public void Returned_List_Should_Be_ReadOnly()
        {
            var (repo, _) = CreateRepo();

            repo.Write(new[] { E(1, "A", 1) });
            var list = repo.Load();

            // IReadOnlyCollection<T> hat keine IsReadOnly-Property -> auf ICollection<T> casten
            var asCollection = Assert.IsAssignableFrom<ICollection<TestEntity>>(list);
            Assert.True(asCollection.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => asCollection.Add(E(2, "B", 2)));
            Assert.Throws<NotSupportedException>(() => asCollection.Remove(asCollection.First()));
        }

        [Fact]
        public void Write_Should_Throw_On_Null_Items()
        {
            var (repo, _) = CreateRepo();
            Assert.Throws<ArgumentNullException>(() => repo.Write(null!));
        }

        [Fact]
        public void Write_Should_Throw_On_Null_Element_In_Sequence()
        {
            var (repo, _) = CreateRepo();

            var incoming = new TestEntity?[]
            {
                E(1, "A", 1),
                null
            };

            Assert.ThrowsAny<Exception>(() => repo.Write(incoming! as IEnumerable<TestEntity>));
        }

        [Fact]
        public void Load_Should_Throw_On_Corrupt_Json()
        {
            var (repo, path) = CreateRepo();

            // Schreibe absichtlich ungültiges JSON
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, "{ this is not valid json }", Encoding.UTF8);

            Assert.ThrowsAny<JsonException>(() => repo.Load());
        }

        [Fact]
        public void Write_Empty_Collection_Should_Persist_Empty_And_Load_Empty()
        {
            var (repo, path) = CreateRepo();

            repo.Write(Array.Empty<TestEntity>());

            var loaded = repo.Load();
            Assert.Empty(loaded);
            Assert.True(File.Exists(path));
        }

        // ----------------- Hilfs-Typ -----------------

        private sealed class TestEntity : EntityBase, IEquatable<TestEntity>
        {
            public string? Name { get; set; }
            public int Value { get; set; }

            public bool Equals(TestEntity? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id
                    && string.Equals(Name, other.Name, StringComparison.Ordinal)
                    && Value == other.Value;
            }

            public override bool Equals(object? obj) => obj is TestEntity te && Equals(te);
            public override int GetHashCode() => HashCode.Combine(Id, Name, Value);
            public override string ToString() => $"#{Id}:{Name}:{Value}";
        }
    }
}
