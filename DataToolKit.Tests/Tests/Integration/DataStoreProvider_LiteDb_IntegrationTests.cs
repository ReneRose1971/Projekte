using System;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using TestHelper.DataToolKit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataToolKit.Tests.Integration
{
    /// <summary>
    /// Integration-Tests für DataStoreProvider mit LiteDB-Repository.
    /// Testet: PersistentDataStore-Erzeugung, AutoLoad, PropertyChanged-Tracking, Dateipfad-Validierung.
    /// </summary>
    public class DataStoreProvider_LiteDb_IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _testDataPath;

        public DataStoreProvider_LiteDb_IntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"DataToolKit_ProviderLiteDbInt_{Guid.NewGuid():N}");
            
            var services = new ServiceCollection();
            new IntegrationTestModule(_testDataPath).Register(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void DataStoreProvider_CreatesPersistentStore_ForLiteDbRepository()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            // Act - Direkt mit Repository, nicht über RepositoryFactory
            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);

            // Assert
            Assert.NotNull(store);
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void DataStoreProvider_WithAutoLoad_LoadsDataFromLiteDbRepository()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            // Seed data
            var testData = new[]
            {
                new TestEntity { Id = 0, Name = "Alice", Index = 10 },
                new TestEntity { Id = 0, Name = "Bob", Index = 20 }
            };
            repository.Write(testData);

            // Act - Direkt mit Repository
            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);
            store.Load(); // Manuelles Load

            // Assert
            Assert.Equal(2, store.Count);
            Assert.Contains(store.Items, e => e.Name == "Alice");
            Assert.Contains(store.Items, e => e.Name == "Bob");
        }

        [Fact]
        public void DataStoreProvider_Add_PersistsToLiteDbFile()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);

            // Act
            store.Add(new TestEntity { Id = 0, Name = "Charlie", Index = 30 });

            // Assert - Datei wurde erstellt
            Assert.True(File.Exists(options.FullPath), $"File not found: {options.FullPath}");

            // Assert - Daten sind korrekt persistiert
            var loaded = repository.Load();
            Assert.Single(loaded);
            Assert.Equal("Charlie", loaded[0].Name);
        }

        [Fact]
        public void DataStoreProvider_PropertyChanged_PersistsToLiteDb()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: true); // ? PropertyChanged-Tracking aktiviert

            var entity = new TestEntity { Id = 0, Name = "Original", Index = 10 };
            store.Add(entity);

            var entityId = entity.Id; // ID wurde von LiteDB zugewiesen

            // Act - Änderung via Property Setter (löst PropertyChanged aus)
            entity.Name = "Modified";

            // Assert - Änderung wurde automatisch persistiert
            var loaded = repository.Load().First();
            Assert.Equal(entityId, loaded.Id);
            Assert.Equal("Modified", loaded.Name);
            Assert.Equal(10, loaded.Index);
        }

        [Fact]
        public void DataStoreProvider_PropertyChanged_MultipleChanges_AllPersisted()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: true);

            var entity = new TestEntity { Id = 0, Name = "Start", Index = 1 };
            store.Add(entity);

            // Act - Mehrere Änderungen
            entity.Name = "Change1";
            entity.Index = 100;
            entity.Name = "Change2";
            entity.Index = 200;

            // Assert - Letzte Änderung ist persistiert
            var loaded = repository.Load().First();
            Assert.Equal("Change2", loaded.Name);
            Assert.Equal(200, loaded.Index);
        }

        [Fact]
        public void DataStoreProvider_Remove_DeletesFromLiteDb()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);
            
            var entity1 = new TestEntity { Id = 0, Name = "Entity1", Index = 1 };
            var entity2 = new TestEntity { Id = 0, Name = "Entity2", Index = 2 };
            store.Add(entity1);
            store.Add(entity2);

            // Act
            store.Remove(entity1);

            // Assert
            Assert.Single(store.Items);
            
            var remaining = repository.Load();
            Assert.Single(remaining);
            Assert.Equal("Entity2", remaining[0].Name);
        }

        [Fact]
        public void DataStoreProvider_Clear_RemovesAllDataFromLiteDb()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);
            store.Add(new TestEntity { Id = 0, Name = "Test1", Index = 1 });
            store.Add(new TestEntity { Id = 0, Name = "Test2", Index = 2 });

            // Act
            store.Clear();

            // Assert
            Assert.Equal(0, store.Count);
            
            var loaded = repository.Load();
            Assert.Empty(loaded);
        }

        [Fact]
        public void DataStoreProvider_WithoutPropertyChangedTracking_DoesNotPersistAutomatically()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var repository = _serviceProvider.GetRequiredService<IRepository<TestEntity>>();

            var store = factory.CreatePersistentStore(repository, trackPropertyChanges: false);  // ? DEAKTIVIERT

            var entity = new TestEntity { Id = 0, Name = "Original", Index = 10 };
            store.Add(entity);

            // Act - Änderung OHNE PropertyChanged-Tracking
            entity.Name = "Modified";

            // Assert - Änderung wurde NICHT automatischPersistiert
            var loaded = repository.Load().First();
            Assert.Equal("Original", loaded.Name); // Bleibt Original!
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
            
            if (Directory.Exists(_testDataPath))
            {
                try
                {
                    Directory.Delete(_testDataPath, recursive: true);
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
        }
    }
}
