using System;
using System.Linq;
using DataToolKit.Abstractions.Repositories;
using TestHelper.DataToolKit.Fixtures;
using TestHelper.DataToolKit.Fakes.Providers;
using TestHelper.DataToolKit.Fakes.Repositories;
using Xunit;

namespace DataToolKit.Tests.Examples.Fakes
{
    /// <summary>
    /// Beispiel-Tests zur Demonstration des FakeDataStoreProvider.
    /// Zeigt die Verwendung von InMemory- und PersistentDataStores mit Fake-Repositories.
    /// </summary>
    public class FakeDataStoreProvider_Example_Tests
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
        public void Example1_InMemory_DataStore()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();

            // Act: InMemory-DataStore erstellen
            var store = provider.GetInMemory<TestEntity>();

            var entity1 = new TestEntity { Id = 1, Name = "Alice", Value = 100 };
            var entity2 = new TestEntity { Id = 2, Name = "Bob", Value = 200 };

            store.Add(entity1);
            store.Add(entity2);

            // Assert
            Assert.Equal(2, store.Count);
            Assert.Contains(store.Items, e => e.Name == "Alice");
        }

        [Fact]
        public void Example2_Persistent_DataStore_With_AutoLoad()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();

            // Daten im Repository seeden
            provider.RepositoryFactory
                .GetFakeLiteDbRepository<TestEntity>()
                .SeedData(new TestEntity { Id = 1, Name = "Seeded", Value = 100 });

            // Act: PersistentDataStore mit AutoLoad
            var store = provider.GetPersistent<TestEntity>(
                provider.RepositoryFactory,
                autoLoad: true
            );

            // Assert: Daten wurden automatisch geladen
            Assert.Equal(1, store.Count);
            Assert.Equal("Seeded", store.Items.First().Name);
        }

        [Fact]
        public void Example3_Persistent_DataStore_Persists_Changes()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();
            var store = provider.GetPersistent<TestEntity>(
                provider.RepositoryFactory,
                autoLoad: false
            );

            // Act: Daten hinzufügen
            store.Add(new TestEntity { Id = 0, Name = "Alice", Value = 100 });

            // Assert: Repository wurde aktualisiert
            var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<TestEntity>();
            Assert.Equal(1, repo.WriteCallCount);

            var persisted = repo.Load();
            Assert.Single(persisted);
            Assert.Equal("Alice", persisted.First().Name);
        }

        [Fact]
        public void Example4_Singleton_Management()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();

            // Act: Zwei Mal GetInMemory mit Singleton
            var store1 = provider.GetInMemory<TestEntity>(isSingleton: true);
            var store2 = provider.GetInMemory<TestEntity>(isSingleton: true);

            // Assert: Gleiche Instanz
            Assert.Same(store1, store2);

            // Act: Singleton entfernen
            provider.RemoveSingleton<TestEntity>();

            // Assert: GetDataStore wirft Exception
            Assert.Throws<InvalidOperationException>(() => provider.GetDataStore<TestEntity>());
        }

        [Fact]
        public void Example5_GetDataStore_Returns_Existing()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();
            var store = provider.GetInMemory<TestEntity>(isSingleton: true);

            // Act: GetDataStore gibt existierenden Store zurück
            var retrieved = provider.GetDataStore<TestEntity>();

            // Assert: Gleiche Instanz
            Assert.Same(store, retrieved);
        }

        [Fact]
        public void Example6_ClearAll_Disposes_Everything()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();

            var store1 = provider.GetInMemory<TestEntity>(isSingleton: true);
            store1.Add(new TestEntity { Id = 1, Name = "Alice" });

            var store2 = provider.GetPersistent<TestEntity>(
                provider.RepositoryFactory,
                isSingleton: true,
                autoLoad: false
            );
            store2.Add(new TestEntity { Id = 2, Name = "Bob" });

            // Act: Alles zurücksetzen
            provider.ClearAll();

            // Assert: Alle Singletons entfernt
            Assert.Throws<InvalidOperationException>(() => provider.GetDataStore<TestEntity>());

            // Assert: Repositories auch zurückgesetzt
            var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<TestEntity>();
            Assert.Empty(repo.Load());
        }

        [Fact]
        public void Example7_Using_DataStoreTestFixture()
        {
            // Arrange: Fixture mit InMemory-DataStore
            using var fixture = new DataStoreTestFixture<TestEntity>(
                usePersistent: false
            );

            // Act: Daten hinzufügen
            fixture.SeedData(
                new TestEntity { Id = 1, Name = "Alice" },
                new TestEntity { Id = 2, Name = "Bob" }
            );

            // Assert
            Assert.Equal(2, fixture.DataStore.Count);

            // Reset für nächsten Test
            fixture.Reset();
            Assert.Equal(0, fixture.DataStore.Count);
        }

        [Fact]
        public void Example8_Persistent_Fixture_With_AutoLoad()
        {
            // Arrange: Fixture mit PersistentDataStore
            using var fixture = new DataStoreTestFixture<TestEntity>(
                usePersistent: true,
                autoLoad: false
            );

            // Daten seeden
            fixture.RepositoryFactory
                .GetFakeLiteDbRepository<TestEntity>()
                .SeedData(new TestEntity { Id = 1, Name = "Preseeded" });

            // Act: Store neu erstellen mit AutoLoad
            var store = fixture.Provider.GetPersistent<TestEntity>(
                fixture.RepositoryFactory,
                isSingleton: false,
                autoLoad: true
            );

            // Assert: Daten wurden geladen
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void Example9_Access_Fake_Repository_For_Assertions()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();
            var store = provider.GetPersistent<TestEntity>(
                provider.RepositoryFactory,
                autoLoad: false
            );

            // Act: Mehrere Operationen
            store.Add(new TestEntity { Id = 0, Name = "Alice" });
            store.Add(new TestEntity { Id = 0, Name = "Bob" });
            store.Remove(store.Items.First());

            // Assert: Repository-History prüfen
            var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<TestEntity>();
            Assert.Equal(3, repo.History.Count); // 2x Write (Add), 1x Delete (Remove)
            Assert.Equal("Write", repo.History[0].Action);  // Add Alice
            Assert.Equal("Write", repo.History[1].Action);  // Add Bob
            Assert.Equal("Delete", repo.History[2].Action); // Remove Alice (granular Delete bei LiteDB)
        }

        [Fact]
        public void Example10_Simulate_Repository_Failure()
        {
            // Arrange
            var provider = new FakeDataStoreProvider();

            // Repository für Fehler konfigurieren
            var repo = provider.RepositoryFactory.GetFakeLiteDbRepository<TestEntity>();
            repo.ThrowOnWrite = true;

            // Act: PersistentDataStore erstellen
            var store = provider.GetPersistent<TestEntity>(
                provider.RepositoryFactory,
                autoLoad: false
            );

            // Assert: Add wirft Exception wegen Repository-Fehler
            var ex = Assert.Throws<InvalidOperationException>(
                () => store.Add(new TestEntity { Id = 0, Name = "Alice" })
            );
            Assert.Contains("Simulated write failure", ex.Message);
        }
    }
}
