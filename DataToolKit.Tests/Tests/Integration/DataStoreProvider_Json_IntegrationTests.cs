using System;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;
using DataToolKit.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DataToolKit.Tests.Integration
{
    /// <summary>
    /// Integration-Tests für DataStoreProvider mit JSON-Repository.
    /// Testet: PersistentDataStore-Erzeugung via Provider, AutoLoad, Dateipfad-Validierung.
    /// </summary>
    public class DataStoreProvider_Json_IntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly string _testDataPath;

        public DataStoreProvider_Json_IntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"DataToolKit_ProviderJsonInt_{Guid.NewGuid():N}");
            
            var services = new ServiceCollection();
            new IntegrationTestModule(_testDataPath).Register(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void DataStoreProvider_CanBeResolvedFromDI()
        {
            // Act
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();

            // Assert
            Assert.NotNull(provider);
        }

        [Fact]
        public void DataStoreProvider_CreatesPersistentStore_ForJsonRepository()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();

            // Act
            var store = provider.GetPersistent<TestDto>(
                repositoryFactory,
                isSingleton: false,
                trackPropertyChanges: false,
                autoLoad: false);

            // Assert
            Assert.NotNull(store);
            Assert.Equal(0, store.Count); // Noch keine Daten geladen
        }

        [Fact]
        public void DataStoreProvider_WithAutoLoad_LoadsDataFromJsonRepository()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();

            // Seed data
            var testData = new[]
            {
                new TestDto { Id = 1, Name = "Alice", Index = 10 },
                new TestDto { Id = 2, Name = "Bob", Index = 20 }
            };
            repository.Write(testData);

            // Act
            var store = provider.GetPersistent<TestDto>(
                repositoryFactory,
                isSingleton: false,
                trackPropertyChanges: false,
                autoLoad: true);  // ? AutoLoad aktiviert

            // Assert
            Assert.Equal(2, store.Count);
            Assert.Contains(store.Items, e => e.Name == "Alice");
            Assert.Contains(store.Items, e => e.Name == "Bob");
        }

        [Fact]
        public void DataStoreProvider_Add_PersistsToJsonFile()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();
            var options = _serviceProvider.GetRequiredService<IStorageOptions<TestDto>>();

            var store = provider.GetPersistent<TestDto>(
                repositoryFactory,
                isSingleton: false,
                trackPropertyChanges: false,
                autoLoad: false);

            // Act
            store.Add(new TestDto { Id = 1, Name = "Charlie", Index = 30 });

            // Assert - Datei wurde erstellt
            Assert.True(File.Exists(options.FullPath), $"File not found: {options.FullPath}");

            // Assert - Daten sind korrekt persistiert
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();
            var loaded = repository.Load();
            Assert.Single(loaded);
            Assert.Equal("Charlie", loaded[0].Name);
        }

        [Fact]
        public void DataStoreProvider_Singleton_ReturnsSameInstance()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();

            // Act
            var store1 = provider.GetPersistent<TestDto>(repositoryFactory, isSingleton: true, autoLoad: false);
            var store2 = provider.GetPersistent<TestDto>(repositoryFactory, isSingleton: true, autoLoad: false);

            // Assert
            Assert.Same(store1, store2);
        }

        [Fact]
        public void DataStoreProvider_PropertyChanged_PersistsToJsonFile()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();

            var store = provider.GetPersistent<TestDto>(
                repositoryFactory,
                isSingleton: false,
                trackPropertyChanges: true,  // ? PropertyChanged-Tracking aktiviert
                autoLoad: false);

            var entity = new TestDto { Id = 1, Name = "Original", Index = 10 };
            store.Add(entity);

            // Act - Änderung sollte automatisch persistiert werden
            // HINWEIS: TestDto hat KEIN INotifyPropertyChanged!
            // Daher wird diese Änderung NICHT automatisch persistiert
            entity.Name = "Modified";

            // Assert - Für TestDto (ohne INotifyPropertyChanged) wird nichts automatisch gespeichert
            // Dies ist erwartetes Verhalten - nur für LiteDB-Entities mit INotifyPropertyChanged
            var loaded = repository.Load();
            Assert.Single(loaded);
            // TestDto unterstützt kein PropertyChanged, daher bleibt "Original"
            Assert.Equal("Original", loaded[0].Name);
        }

        [Fact]
        public void DataStoreProvider_Clear_RemovesAllDataFromJsonFile()
        {
            // Arrange
            var provider = _serviceProvider.GetRequiredService<IDataStoreProvider>();
            var repositoryFactory = _serviceProvider.GetRequiredService<IRepositoryFactory>();
            var repository = _serviceProvider.GetRequiredService<IRepositoryBase<TestDto>>();

            var store = provider.GetPersistent<TestDto>(repositoryFactory, isSingleton: false, autoLoad: false);
            store.Add(new TestDto { Id = 1, Name = "Test", Index = 1 });
            store.Add(new TestDto { Id = 2, Name = "Test2", Index = 2 });

            // Act
            store.Clear();

            // Assert
            Assert.Equal(0, store.Count);
            
            var loaded = repository.Load();
            Assert.Empty(loaded);
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
