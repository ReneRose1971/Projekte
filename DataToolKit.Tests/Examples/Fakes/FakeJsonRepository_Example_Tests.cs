using System;
using System.Linq;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Tests.Fakes.Builders;
using DataToolKit.Tests.Fakes.Repositories;
using Xunit;

namespace DataToolKit.Tests.Examples.Fakes
{
    /// <summary>
    /// Beispiel-Tests zur Demonstration des Fake-Frameworks.
    /// Zeigt die Verwendung von FakeJsonRepository mit verschiedenen Test-Szenarien.
    /// </summary>
    public class FakeJsonRepository_Example_Tests
    {
        // Test-Entität
        private sealed class TestEntity : EntityBase
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        [Fact]
        public void Example1_Basic_Usage()
        {
            // Arrange: FakeJsonRepository erstellen
            var repo = new FakeJsonRepository<TestEntity>();

            var entity1 = new TestEntity { Id = 1, Name = "Alice", Value = 100 };
            var entity2 = new TestEntity { Id = 2, Name = "Bob", Value = 200 };

            // Act: Daten schreiben und laden
            repo.Write(new[] { entity1, entity2 });
            var loaded = repo.Load();

            // Assert: Daten korrekt gespeichert
            Assert.Equal(2, loaded.Count);
            Assert.Contains(loaded, e => e.Name == "Alice");
            Assert.Contains(loaded, e => e.Name == "Bob");

            // Assert: History wurde aufgezeichnet
            Assert.Equal(1, repo.WriteCallCount);
            Assert.Equal(1, repo.LoadCallCount);
        }

        [Fact]
        public void Example2_Using_TestEntityBuilder()
        {
            // Arrange: Builder verwenden für Test-Daten
            var repo = new FakeJsonRepository<TestEntity>();

            var entities = new TestEntityBuilder<TestEntity>()
                .WithId(0)
                .With(e => e.Value = 100)
                .BuildMany(5, (e, i) => e.Name = $"Entity {i}");

            // Act
            repo.Write(entities);
            var loaded = repo.Load();

            // Assert
            Assert.Equal(5, loaded.Count);
            Assert.All(loaded, e => Assert.Equal(100, e.Value));
        }

        [Fact]
        public void Example3_Simulate_Load_Failure()
        {
            // Arrange: Fehler simulieren
            var repo = new FakeJsonRepository<TestEntity>();
            repo.ThrowOnLoad = true;

            // Act & Assert: IOException wird geworfen
            var ex = Assert.Throws<System.IO.IOException>(() => repo.Load());
            Assert.Contains("Simulated load failure", ex.Message);
        }

        [Fact]
        public void Example4_Simulate_Write_Failure()
        {
            // Arrange
            var repo = new FakeJsonRepository<TestEntity>();
            repo.ThrowOnWrite = true;

            var entity = new TestEntity { Id = 1, Name = "Alice" };

            // Act & Assert
            var ex = Assert.Throws<System.IO.IOException>(
                () => repo.Write(new[] { entity }));
            Assert.Contains("Simulated write failure", ex.Message);
        }

        [Fact]
        public void Example5_History_Tracking()
        {
            // Arrange
            var repo = new FakeJsonRepository<TestEntity>();

            // Act: Mehrere Operationen
            repo.Write(new[] { new TestEntity { Id = 1, Name = "Alice" } });
            repo.Load();
            repo.Clear();
            repo.Load();

            // Assert: History enthält alle Operationen
            Assert.Equal(4, repo.History.Count);
            Assert.Equal("Write", repo.History[0].Action);
            Assert.Equal("Load", repo.History[1].Action);
            Assert.Equal("Clear", repo.History[2].Action);
            Assert.Equal("Load", repo.History[3].Action);
        }

        [Fact]
        public void Example6_SeedData_And_Reset()
        {
            // Arrange
            var repo = new FakeJsonRepository<TestEntity>();

            // Act: Daten seeden (ohne History-Eintrag)
            repo.SeedData(
                new TestEntity { Id = 1, Name = "Seeded1" },
                new TestEntity { Id = 2, Name = "Seeded2" }
            );

            // Assert: Daten vorhanden, aber keine History
            Assert.Equal(2, repo.Load().Count);
            Assert.Equal(1, repo.History.Count); // Nur Load

            // Reset
            repo.Reset();

            // Assert: Alles zurückgesetzt
            Assert.Empty(repo.Load());
            Assert.Equal(1, repo.History.Count); // Nur neuer Load
        }

        [Fact]
        public void Example7_Simulated_Delay()
        {
            // Arrange: Verzögerung simulieren
            var repo = new FakeJsonRepository<TestEntity>();
            repo.SimulatedDelay = TimeSpan.FromMilliseconds(100);

            // Act & Measure
            var start = DateTime.UtcNow;
            repo.Write(new[] { new TestEntity { Id = 1, Name = "Alice" } });
            var elapsed = DateTime.UtcNow - start;

            // Assert: Verzögerung wurde angewendet
            Assert.True(elapsed >= TimeSpan.FromMilliseconds(100));
        }

        [Fact]
        public void Example8_Using_RepositoryScenarioBuilder()
        {
            // Arrange: Szenario mit Builder erstellen
            var factory = new FakeRepositoryFactory();
            var scenario = new RepositoryScenarioBuilder<TestEntity>(factory)
                .WithRandomEntities(10, i => new TestEntity
                {
                    Id = i + 1,
                    Name = $"Entity {i}",
                    Value = i * 10
                })
                .BuildFakeJson();

            // Act
            var loaded = scenario.Load();

            // Assert
            Assert.Equal(10, loaded.Count);
            Assert.Equal("Entity 0", loaded[0].Name);
            Assert.Equal("Entity 9", loaded[9].Name);
        }
    }
}
