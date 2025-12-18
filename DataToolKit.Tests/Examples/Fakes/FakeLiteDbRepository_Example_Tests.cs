using System;
using System.Linq;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.Repositories;
using TestHelper.DataToolKit.Builders;
using TestHelper.DataToolKit.Fakes.Repositories;
using Xunit;

namespace DataToolKit.Tests.Examples.Fakes
{
    /// <summary>
    /// Beispiel-Tests zur Demonstration des Fake-Frameworks mit LiteDB-Repository.
    /// Zeigt automatische ID-Vergabe, Delta-Synchronisierung und granulare Operationen.
    /// </summary>
    public class FakeLiteDbRepository_Example_Tests
    {
        // Test-Entität
        private sealed class TestEntity : EntityBase
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is not TestEntity other) return false;
                return Id == other.Id && Name == other.Name && Value == other.Value;
            }

            public override int GetHashCode() => HashCode.Combine(Id, Name, Value);
        }

        [Fact]
        public void Example1_AutomaticId_Assignment()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();

            var entity1 = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            var entity2 = new TestEntity { Id = 0, Name = "Bob", Value = 200 };

            // Act: Entities mit Id=0 einfügen
            repo.Write(new[] { entity1, entity2 });

            // Assert: IDs wurden automatisch vergeben
            Assert.Equal(1, entity1.Id);
            Assert.Equal(2, entity2.Id);
            Assert.Equal(2, repo.CurrentMaxId);
        }

        [Fact]
        public void Example2_Delta_Synchronization()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();

            // Seed: 3 Entities
            var entity1 = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            var entity2 = new TestEntity { Id = 0, Name = "Bob", Value = 200 };
            var entity3 = new TestEntity { Id = 0, Name = "Charlie", Value = 300 };
            repo.Write(new[] { entity1, entity2, entity3 });

            // Act: Delta-Update (Alice ändern, Charlie löschen, Dave hinzufügen)
            entity1.Value = 150; // Update
            var entity4 = new TestEntity { Id = 0, Name = "Dave", Value = 400 }; // Insert
            // Charlie fehlt ? Delete

            repo.Write(new[] { entity1, entity2, entity4 });

            // Assert
            var loaded = repo.Load().OrderBy(e => e.Id).ToList();
            Assert.Equal(3, loaded.Count);
            Assert.Equal(150, loaded[0].Value); // Alice updated
            Assert.Equal("Bob", loaded[1].Name); // Bob unverändert
            Assert.Equal("Dave", loaded[2].Name); // Dave inserted
            Assert.DoesNotContain(loaded, e => e.Name == "Charlie"); // Charlie deleted
        }

        [Fact]
        public void Example3_Update_Single_Entity()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();
            var entity = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            repo.Write(new[] { entity });

            // Act: Einzelnes Update
            entity.Value = 150;
            repo.Update(entity);

            // Assert
            var loaded = repo.Load().Single();
            Assert.Equal(150, loaded.Value);
            Assert.Equal(1, repo.History.Count(h => h.Action == "Update"));
        }

        [Fact]
        public void Example4_Delete_Single_Entity()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();
            var entity = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            repo.Write(new[] { entity });

            // Act: Einzelnes Delete
            repo.Delete(entity);

            // Assert
            Assert.Empty(repo.Load());
            Assert.Equal(1, repo.History.Count(h => h.Action == "Delete"));
        }

        [Fact]
        public void Example5_Using_TestEntityBuilder()
        {
            // Arrange: Builder für Test-Daten
            var repo = new FakeLiteDbRepository<TestEntity>();

            var entities = new TestEntityBuilder<TestEntity>()
                .WithId(0) // Auto-ID
                .BuildMany(5, (e, i) =>
                {
                    e.Name = $"Entity {i}";
                    e.Value = i * 100;
                });

            // Act
            repo.Write(entities);

            // Assert
            var loaded = repo.Load().OrderBy(e => e.Id).ToList();
            Assert.Equal(5, loaded.Count);
            Assert.All(loaded, e => Assert.True(e.Id > 0)); // Alle haben IDs
        }

        [Fact]
        public void Example6_GetById_Helper()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();
            var entity = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            repo.Write(new[] { entity });

            // Act: GetById verwenden
            var found = repo.GetById(entity.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Alice", found.Name);

            // Nicht existierende ID
            Assert.Null(repo.GetById(9999));
        }

        [Fact]
        public void Example7_Simulate_Update_Failure()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();
            var entity = new TestEntity { Id = 0, Name = "Alice", Value = 100 };
            repo.Write(new[] { entity });

            repo.ThrowOnUpdate = true;

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => repo.Update(entity));
            Assert.Contains("Simulated update failure", ex.Message);
        }

        [Fact]
        public void Example8_History_Tracking()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();

            // Act: Verschiedene Operationen
            repo.Write(new[] { new TestEntity { Id = 0, Name = "Alice" } });
            var entity = repo.Load().Single();
            entity.Value = 100;
            repo.Update(entity);
            repo.Delete(entity);

            // Assert: History enthält alle Operationen
            Assert.Equal(4, repo.History.Count); // Write, Load, Update, Delete
            Assert.Equal("Write", repo.History[0].Action);
            Assert.Equal("Load", repo.History[1].Action);
            Assert.Equal("Update", repo.History[2].Action);
            Assert.Equal("Delete", repo.History[3].Action);
        }

        [Fact]
        public void Example9_SeedData_With_Auto_Id()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();

            // Act: SeedData mit Id=0 (Auto-ID wird vergeben)
            repo.SeedData(
                new TestEntity { Id = 0, Name = "Alice" },   // Bekommt ID 1
                new TestEntity { Id = 0, Name = "Bob" },     // Bekommt ID 2
                new TestEntity { Id = 5, Name = "Charlie" }  // Behält ID 5
            );

            // Assert
            var loaded = repo.Load().OrderBy(e => e.Id).ToList();
            Assert.Equal(3, loaded.Count);
            Assert.Equal(1, loaded[0].Id); // Auto-ID
            Assert.Equal(2, loaded[1].Id); // Auto-ID
            Assert.Equal(5, loaded[2].Id); // Explizite ID
            Assert.Equal(5, repo.CurrentMaxId); // Höchste ID im Repository
        }

        [Fact]
        public void Example10_Using_RepositoryScenarioBuilder()
        {
            // Arrange: Komplexes Szenario mit Builder
            var factory = new FakeRepositoryFactory();
            var scenario = new RepositoryScenarioBuilder<TestEntity>(factory)
                .WithEntity(new TestEntity { Id = 1, Name = "Alice", Value = 100 })
                .WithEntity(new TestEntity { Id = 2, Name = "Bob", Value = 200 })
                .WithRandomEntities(8, i => new TestEntity
                {
                    Id = 0, // Auto-ID
                    Name = $"Random {i}",
                    Value = i * 10
                })
                .BuildFakeLiteDb();

            // Act
            var loaded = scenario.Load();

            // Assert
            Assert.Equal(10, loaded.Count);
            Assert.Contains(loaded, e => e.Name == "Alice");
            Assert.Contains(loaded, e => e.Name == "Bob");
            Assert.Equal(8, loaded.Count(e => e.Name.StartsWith("Random")));
        }

        [Fact]
        public void Example11_Reset_Clears_Everything()
        {
            // Arrange
            var repo = new FakeLiteDbRepository<TestEntity>();
            repo.Write(new[] { new TestEntity { Id = 0, Name = "Alice" } });

            // Act: Reset
            repo.Reset();

            // Assert: Alles zurückgesetzt
            Assert.Empty(repo.Load());
            Assert.Equal(0, repo.CurrentMaxId); // NextId = 1, CurrentMaxId = 0
            Assert.Equal(1, repo.History.Count); // Nur der neue Load nach Reset
        }
    }
}
