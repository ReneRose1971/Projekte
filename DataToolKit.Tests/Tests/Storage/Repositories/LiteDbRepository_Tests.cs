using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Equality;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using Xunit;

namespace DataToolKit.Tests.Storage.Repositories
{
    /// <summary>
    /// Kleinteilige Unit-Tests für LiteDbRepository&lt;T&gt; (v5):
    /// - Delta-Write (Insert/Update/Delete, Missing-IDs-Policy)
    /// - Clear()
    /// - Einzeloperationen Update(T), Delete(T)
    /// - Rollback bei Fehlern in Write()
    /// 
    /// Jeder Test arbeitet mit einer eigenen DB-Datei in einem temp. Verzeichnis.
    /// </summary>
    public sealed class LiteDbRepository_Tests : IDisposable
    {
        private readonly string _root;
        public LiteDbRepository_Tests()
        {
            _root = Path.Combine(Path.GetTempPath(), "LiteDbRepoTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, recursive: true); } catch { /* best-effort */ }
        }

        // ----------------- Hilfsfunktionen -----------------

        private LiteDbRepository<TestEntity> CreateRepo(IEqualityComparer<TestEntity>? comparer = null)
        {
            var dbName = Guid.NewGuid().ToString("N") + ".db";
            var opts = new LiteDbStorageOptions<TestEntity>(
                appSubFolder: "DataToolKit.Tests",
                fileNameBase: dbName,
                subFolder: _root
            );

            return new LiteDbRepository<TestEntity>(opts, comparer ?? new EqualsComparer<TestEntity>());
        }

        private static TestEntity E(int id, string name, int value) => new TestEntity { Id = id, Name = name, Value = value };

        // ----------------- Tests: Insert / Update / Delete / MissingIds -----------------

        [Fact]
        public void Write_Should_Insert_New_Items_And_Set_Ids()
        {
            using var repo = CreateRepo();

            // Arrange: 2 neue Datensätze (Id==0)
            var incoming = new[]
            {
                E(0, "A", 1),
                E(0, "B", 2)
            };

            // Act
            repo.Write(incoming);

            // Assert: Ids wurden gesetzt und Daten sind vorhanden
            var after = repo.Load().OrderBy(x => x.Name).ToList();
            Assert.Equal(2, after.Count);
            Assert.All(after, x => Assert.True(x.Id > 0));
            Assert.Equal(new[] { "A", "B" }, after.Select(x => x.Name));
            Assert.Equal(new[] { 1, 2 }, after.Select(x => x.Value));
        }

        [Fact]
        public void Write_Should_Update_Changed_Items_By_Id()
        {
            using var repo = CreateRepo();

            // Seed
            repo.Write(new[] { E(0, "A", 1), E(0, "B", 2) });
            var seeded = repo.Load().OrderBy(x => x.Name).ToList();
            var a = seeded.First(x => x.Name == "A");
            var b = seeded.First(x => x.Name == "B");

            // Ändere A (gleiche Id, geänderte Werte), B bleibt gleich
            var incoming = new[]
            {
                E(a.Id, "A*", 11),
                E(b.Id, "B", 2)
            };

            repo.Write(incoming);

            var after = repo.Load().OrderBy(x => x.Name).ToList();
            Assert.Equal(2, after.Count);

            var a2 = after.First(x => x.Name == "A*");
            var b2 = after.First(x => x.Name == "B");

            Assert.Equal(a.Id, a2.Id);   // gleiche Id, aktualisierte Werte
            Assert.Equal(11, a2.Value);
            Assert.Equal(b.Id, b2.Id);   // unverändert
            Assert.Equal(2, b2.Value);
        }

        [Fact]
        public void Write_Should_Delete_Items_That_Are_Missing_In_Incoming()
        {
            using var repo = CreateRepo();

            // Seed: A, B, C
            repo.Write(new[] { E(0, "A", 1), E(0, "B", 2), E(0, "C", 3) });
            var seeded = repo.Load().ToList();

            // Incoming: nur A bleibt, B & C fehlen => müssen gelöscht werden
            var a = seeded.First(x => x.Name == "A");
            repo.Write(new[] { E(a.Id, "A", 1) });

            var after = repo.Load().OrderBy(x => x.Name).ToList();
            Assert.Single(after);
            Assert.Equal("A", after[0].Name);
        }

        [Fact]
        public void Write_Should_Treat_Missing_Ids_As_Insert()
        {
            using var repo = CreateRepo();

            // Seed: A (Id wird vergeben)
            repo.Write(new[] { E(0, "A", 1) });
            var a = repo.Load().Single();

            // Incoming: A bleibt; X kommt mit Id>0, existiert aber nicht in DB => Insert
            var missingExternalId = a.Id + 999; // garantiert nicht vorhanden
            var incoming = new[]
            {
                E(a.Id, "A", 1),
                E(missingExternalId, "X", 99) // MissingId-Policy => als Insert behandeln
            };

            repo.Write(incoming);

            var after = repo.Load().OrderBy(x => x.Name).ToList();
            Assert.Equal(2, after.Count);
            Assert.Contains(after, x => x.Name == "A");
            Assert.Contains(after, x => x.Name == "X" && x.Value == 99);
        }

        // ----------------- Tests: Clear() -----------------

        [Fact]
        public void Clear_Should_Remove_All_Documents()
        {
            using var repo = CreateRepo();

            repo.Write(new[] { E(0, "A", 1), E(0, "B", 2) });
            Assert.NotEmpty(repo.Load());

            repo.Clear();
            Assert.Empty(repo.Load());
        }

        // ----------------- Tests: Update(T) / Delete(T) -----------------

        [Fact]
        public void Update_Should_Update_Single_Item_When_Id_Is_Valid()
        {
            using var repo = CreateRepo();

            repo.Write(new[] { E(0, "A", 1) });
            var a = repo.Load().Single();

            a.Name = "A+";
            a.Value = 10;

            var count = repo.Update(a);
            Assert.Equal(1, count);

            var after = repo.Load().Single();
            Assert.Equal("A+", after.Name);
            Assert.Equal(10, after.Value);
        }

        [Fact]
        public void Update_Should_Throw_When_Id_Is_Invalid()
        {
            using var repo = CreateRepo();
            var ex = Assert.Throws<ArgumentException>(() => repo.Update(E(0, "X", 1)));
            Assert.Contains("Id (>0)", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Update_Should_Throw_When_NotFound()
        {
            using var repo = CreateRepo();

            // Seed: A
            repo.Write(new[] { E(0, "A", 1) });
            var notExisting = E(9999, "Z", 9);

            var ex = Assert.Throws<InvalidOperationException>(() => repo.Update(notExisting));
            Assert.Contains("nicht gefunden", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Delete_Should_Delete_Single_Item_When_Id_Is_Valid()
        {
            using var repo = CreateRepo();

            repo.Write(new[] { E(0, "A", 1) });
            var a = repo.Load().Single();

            var count = repo.Delete(a);
            Assert.Equal(1, count);
            Assert.Empty(repo.Load());
        }

        [Fact]
        public void Delete_Should_Return_Zero_When_Id_Is_Invalid()
        {
            using var repo = CreateRepo();

            // Id==0 => kein Delete, kein Fehler
            var zero = E(0, "Z", 0);
            var c = repo.Delete(zero);
            Assert.Equal(0, c);
        }

        [Fact]
        public void Delete_Should_Throw_When_NotFound()
        {
            using var repo = CreateRepo();

            // Seed: A
            repo.Write(new[] { E(0, "A", 1) });
            var fake = E(123456, "fake", 0);

            var ex = Assert.Throws<InvalidOperationException>(() => repo.Delete(fake));
            Assert.Contains("nicht gefunden", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ----------------- Test: Rollback-Szenario -----------------

        [Fact]
        public void Write_Should_Rollback_All_When_Exception_Occurs_During_Batch()
        {
            using var repo = CreateRepo();

            // Seed: A, B
            repo.Write(new[] { E(0, "A", 1), E(0, "B", 2) });
            var before = repo.Load().OrderBy(x => x.Name).ToList();

            // Incoming:
            // - A bleibt gleich (kein Update)
            // - C ist neu (Id==0) -> Insert
            // - NULL provoziert eine Exception innerhalb der Transaktion
            var a = before.First(x => x.Name == "A");
            var incoming = new List<TestEntity?>
            {
                E(a.Id, "A", 1),
                E(0, "C", 3),
                null // Insert(null) -> wirft -> Rollback
            };

            // Cast notwendig, da Signatur IEnumerable<T> erwartet:
            Assert.ThrowsAny<Exception>(() => repo.Write(incoming! as IEnumerable<TestEntity>));

            // Erwartung: nichts hat sich geändert
            var after = repo.Load().OrderBy(x => x.Name).ToList();
            Assert.Equal(before.Count, after.Count);
            Assert.Equal(before.Select(x => x.Name), after.Select(x => x.Name));
            Assert.Equal(before.Select(x => x.Value), after.Select(x => x.Value));
        }

        // ----------------- Hilfstyp: TestEntity -----------------

        /// <summary>
        /// Testentität: vergleicht in Equals Id, Name und Value.
        /// So erkennt der EqualsComparer&lt;T&gt; (Fallback) „geändert“ korrekt.
        /// </summary>
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
