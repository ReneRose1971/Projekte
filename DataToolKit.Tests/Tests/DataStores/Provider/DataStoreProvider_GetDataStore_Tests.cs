using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using TestHelper.TestUtils;
using Xunit;

namespace DataToolKit.Tests.DataStores.Provider
{
    /// <summary>
    /// Tests für die GetDataStore<T>() Methode des DataStoreProviders.
    /// Prüft das Abrufen bereits registrierter DataStores und Fehlerbehandlung.
    /// </summary>
    public sealed class DataStoreProvider_GetDataStore_Tests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;
        private readonly IDataStoreFactory _factory;
        private readonly IDataStoreProvider _provider;
        private readonly IRepositoryFactory _repositoryFactory;

        public DataStoreProvider_GetDataStore_Tests()
        {
            _sandbox = new TestDirectorySandbox();
            _factory = new DataStoreFactory();
            _provider = new DataStoreProvider(_factory);
            _repositoryFactory = CreateRepositoryFactory();
        }

        public void Dispose()
        {
            (_provider as IDisposable)?.Dispose();
            _sandbox.Dispose();
        }

        private IRepositoryFactory CreateRepositoryFactory()
        {
            // Mock-Repository-Factory für Tests
            return new TestRepositoryFactory(_sandbox.Root);
        }

        #region GetDataStore - InMemory Store

        [Fact]
        public void GetDataStore_Returns_Previously_Registered_InMemory_Store()
        {
            // Arrange: Registriere InMemory Store
            var registeredStore = _provider.GetInMemory<TestEntity>(isSingleton: true);

            // Act: Hole Store via GetDataStore
            var retrievedStore = _provider.GetDataStore<TestEntity>();

            // Assert: Gleiche Instanz
            Assert.NotNull(retrievedStore);
            Assert.Same(registeredStore, retrievedStore);
        }

        [Fact]
        public void GetDataStore_Returns_InMemory_Store_After_Adding_Items()
        {
            // Arrange: Erstelle Store und füge Daten hinzu
            var registeredStore = _provider.GetInMemory<TestEntity>(isSingleton: true);
            var testEntity = new TestEntity { Id = 1, Name = "Test" };
            registeredStore.Add(testEntity);

            // Act: Hole Store via GetDataStore
            var retrievedStore = _provider.GetDataStore<TestEntity>();

            // Assert: Gleicher Store mit gleichen Daten
            Assert.Same(registeredStore, retrievedStore);
            Assert.Single(retrievedStore.Items);
            Assert.Equal(testEntity, retrievedStore.Items[0]);
        }

        #endregion

        #region GetDataStore - Persistent Store

        [Fact]
        public void GetDataStore_Returns_Previously_Registered_Persistent_Store()
        {
            // Arrange: Registriere Persistent Store
            var registeredStore = _provider.GetPersistent<TestEntityBase>(
                _repositoryFactory,
                isSingleton: true,
                autoLoad: false);

            // Act: Hole Store via GetDataStore
            var retrievedStore = _provider.GetDataStore<TestEntityBase>();

            // Assert: Gleiche Instanz
            Assert.NotNull(retrievedStore);
            Assert.Same(registeredStore, retrievedStore);
        }

        [Fact]
        public void GetDataStore_Returns_Persistent_Store_After_Adding_Items()
        {
            // Arrange: Erstelle Persistent Store und füge Daten hinzu
            var registeredStore = _provider.GetPersistent<TestEntityBase>(
                _repositoryFactory,
                isSingleton: true,
                autoLoad: false);

            var testEntity = new TestEntityBase { Id = 1, Name = "Persistent Test" };
            registeredStore.Add(testEntity);

            // Act: Hole Store via GetDataStore
            var retrievedStore = _provider.GetDataStore<TestEntityBase>();

            // Assert: Gleicher Store mit gleichen Daten
            Assert.Same(registeredStore, retrievedStore);
            Assert.Single(retrievedStore.Items);
            Assert.Equal(testEntity, retrievedStore.Items[0]);
        }

        #endregion

        #region GetDataStore - Priority (Persistent before InMemory)

        [Fact]
        public void GetDataStore_Returns_Persistent_Store_When_Only_Persistent_Registered()
        {
            // Arrange: Nur Persistent registrieren
            var persistentStore = _provider.GetPersistent<TestEntityBase>(
                _repositoryFactory,
                isSingleton: true,
                autoLoad: false);

            // Act
            var retrievedStore = _provider.GetDataStore<TestEntityBase>();

            // Assert
            Assert.Same(persistentStore, retrievedStore);
        }

        [Fact]
        public void GetDataStore_Returns_InMemory_Store_When_Only_InMemory_Registered()
        {
            // Arrange: Nur InMemory registrieren
            var inMemoryStore = _provider.GetInMemory<TestEntity>(isSingleton: true);

            // Act
            var retrievedStore = _provider.GetDataStore<TestEntity>();

            // Assert
            Assert.Same(inMemoryStore, retrievedStore);
        }

        #endregion

        #region GetDataStore - Exception Handling

        [Fact]
        public void GetDataStore_Throws_InvalidOperationException_When_Not_Registered()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _provider.GetDataStore<TestEntity>());

            // Verify Exception Message
            Assert.Contains("Kein DataStore für Typ 'TestEntity'", ex.Message);
            Assert.Contains("TestEntity", ex.Message);
            Assert.Contains("GetPersistent<TestEntity>", ex.Message);
            Assert.Contains("GetInMemory<TestEntity>", ex.Message);
        }

        [Fact]
        public void GetDataStore_Exception_Contains_Helpful_Instructions()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _provider.GetDataStore<TestEntity>());

            // Verify helpful instructions
            Assert.Contains("Mögliche Lösungen:", ex.Message);
            Assert.Contains("provider.GetPersistent", ex.Message);
            Assert.Contains("provider.GetInMemory", ex.Message);
            Assert.Contains("repositoryFactory", ex.Message);
            Assert.Contains("isSingleton: true", ex.Message);
        }

        [Fact]
        public void GetDataStore_Exception_Contains_TypeName_And_FullName()
        {
            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _provider.GetDataStore<TestEntity>());

            // Verify type information
            var typeName = typeof(TestEntity).Name;
            var typeFullName = typeof(TestEntity).FullName;

            Assert.Contains(typeName, ex.Message);
            Assert.Contains(typeFullName!, ex.Message);
        }

        #endregion

        #region GetDataStoreAsync - InMemory Store

        [Fact]
        public async Task GetDataStoreAsync_Returns_Previously_Registered_InMemory_Store()
        {
            // Arrange: Registriere InMemory Store
            var registeredStore = await _provider.GetInMemoryAsync<TestEntity>(isSingleton: true);

            // Act: Hole Store via GetDataStoreAsync
            var retrievedStore = await _provider.GetDataStoreAsync<TestEntity>();

            // Assert: Gleiche Instanz
            Assert.NotNull(retrievedStore);
            Assert.Same(registeredStore, retrievedStore);
        }

        [Fact]
        public async Task GetDataStoreAsync_Returns_InMemory_Store_After_Adding_Items()
        {
            // Arrange
            var registeredStore = await _provider.GetInMemoryAsync<TestEntity>(isSingleton: true);
            var testEntity = new TestEntity { Id = 1, Name = "Async Test" };
            registeredStore.Add(testEntity);

            // Act
            var retrievedStore = await _provider.GetDataStoreAsync<TestEntity>();

            // Assert
            Assert.Same(registeredStore, retrievedStore);
            Assert.Single(retrievedStore.Items);
        }

        #endregion

        #region GetDataStoreAsync - Persistent Store

        [Fact]
        public async Task GetDataStoreAsync_Returns_Previously_Registered_Persistent_Store()
        {
            // Arrange
            var registeredStore = await _provider.GetPersistentAsync<TestEntityBase>(
                _repositoryFactory,
                isSingleton: true,
                autoLoad: false);

            // Act
            var retrievedStore = await _provider.GetDataStoreAsync<TestEntityBase>();

            // Assert
            Assert.NotNull(retrievedStore);
            Assert.Same(registeredStore, retrievedStore);
        }

        #endregion

        #region GetDataStoreAsync - Exception Handling

        [Fact]
        public async Task GetDataStoreAsync_Throws_InvalidOperationException_When_Not_Registered()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _provider.GetDataStoreAsync<TestEntity>());

            // Verify Exception Message
            Assert.Contains("Kein DataStore für Typ 'TestEntity'", ex.Message);
            Assert.Contains("GetPersistentAsync<TestEntity>", ex.Message);
            Assert.Contains("GetInMemoryAsync<TestEntity>", ex.Message);
        }

        [Fact]
        public async Task GetDataStoreAsync_Exception_Contains_Async_Instructions()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _provider.GetDataStoreAsync<TestEntity>());

            // Verify async-specific instructions
            Assert.Contains("await provider.GetPersistentAsync", ex.Message);
            Assert.Contains("await provider.GetInMemoryAsync", ex.Message);
        }

        #endregion

        #region GetDataStore - After RemoveSingleton

        [Fact]
        public void GetDataStore_Throws_After_RemoveSingleton()
        {
            // Arrange: Registriere und entferne
            _provider.GetInMemory<TestEntity>(isSingleton: true);
            _provider.RemoveSingleton<TestEntity>();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _provider.GetDataStore<TestEntity>());

            Assert.Contains("Kein DataStore für Typ 'TestEntity'", ex.Message);
        }

        [Fact]
        public void GetDataStore_Throws_After_ClearAll()
        {
            // Arrange: Registriere und lösche alle
            _provider.GetInMemory<TestEntity>(isSingleton: true);
            _provider.ClearAll();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(
                () => _provider.GetDataStore<TestEntity>());

            Assert.Contains("Kein DataStore für Typ 'TestEntity'", ex.Message);
        }

        #endregion

        #region GetDataStore - Multiple Types

        [Fact]
        public void GetDataStore_Can_Retrieve_Multiple_Different_Types()
        {
            // Arrange: Registriere mehrere Typen
            var entityStore = _provider.GetInMemory<TestEntity>(isSingleton: true);
            var entityBaseStore = _provider.GetPersistent<TestEntityBase>(
                _repositoryFactory,
                isSingleton: true,
                autoLoad: false);

            // Act: Hole beide Stores
            var retrievedEntity = _provider.GetDataStore<TestEntity>();
            var retrievedEntityBase = _provider.GetDataStore<TestEntityBase>();

            // Assert: Richtige Stores zurückgegeben
            Assert.Same(entityStore, retrievedEntity);
            Assert.Same(entityBaseStore, retrievedEntityBase);
        }

        [Fact]
        public void GetDataStore_Returns_Correct_Store_For_Each_Type()
        {
            // Arrange: Registriere mehrere Typen mit unterschiedlichen Daten
            var store1 = _provider.GetInMemory<TestEntity>(isSingleton: true);
            store1.Add(new TestEntity { Id = 1, Name = "Type1" });

            var store2 = _provider.GetInMemory<TestEntity2>(isSingleton: true);
            store2.Add(new TestEntity2 { Id = 2, Value = "Type2" });

            // Act: Hole beide Stores
            var retrieved1 = _provider.GetDataStore<TestEntity>();
            var retrieved2 = _provider.GetDataStore<TestEntity2>();

            // Assert: Richtige Daten in richtigen Stores
            Assert.Single(retrieved1.Items);
            Assert.Equal("Type1", retrieved1.Items[0].Name);

            Assert.Single(retrieved2.Items);
            Assert.Equal("Type2", retrieved2.Items[0].Value);
        }

        #endregion

        #region GetDataStore - Thread-Safety

        [Fact]
        public async Task GetDataStore_Is_Thread_Safe()
        {
            // Arrange: Registriere Store
            _provider.GetInMemory<TestEntity>(isSingleton: true);

            // Act: Parallele Zugriffe
            var tasks = new Task<IDataStore<TestEntity>>[100];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => _provider.GetDataStore<TestEntity>());
            }

            var results = await Task.WhenAll(tasks);

            // Assert: Alle bekommen die gleiche Instanz
            var firstStore = results[0];
            Assert.All(results, store => Assert.Same(firstStore, store));
        }

        [Fact]
        public async Task GetDataStoreAsync_Is_Thread_Safe()
        {
            // Arrange: Registriere Store
            await _provider.GetInMemoryAsync<TestEntity>(isSingleton: true);

            // Act: Parallele Zugriffe
            var tasks = new Task<IDataStore<TestEntity>>[100];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = _provider.GetDataStoreAsync<TestEntity>();
            }

            var results = await Task.WhenAll(tasks);

            // Assert: Alle bekommen die gleiche Instanz
            var firstStore = results[0];
            Assert.All(results, store => Assert.Same(firstStore, store));
        }

        #endregion

        #region Test Helper Classes

        private class TestEntity
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        private class TestEntity2
        {
            public int Id { get; set; }
            public string Value { get; set; } = "";
        }

        private class TestEntityBase : EntityBase
        {
            public string Name { get; set; } = "";
        }

        private class TestRepositoryFactory : IRepositoryFactory
        {
            private readonly string _rootPath;

            public TestRepositoryFactory(string rootPath)
            {
                _rootPath = rootPath;
            }

            public IRepositoryBase<T> GetJsonRepository<T>()
            {
                var options = new JsonStorageOptions<T>("TestApp", "test", null, _rootPath);
                return new JsonRepository<T>(options);
            }

            public IRepository<T> GetLiteDbRepository<T>() where T : class
            {
                // Use reflection to bypass the EntityBase constraint for testing
                var options = new LiteDbStorageOptions<T>("TestApp", "test", null, _rootPath);
                var comparerType = typeof(TestComparer<>).MakeGenericType(typeof(T));
                var comparer = Activator.CreateInstance(comparerType);
                
                var repositoryType = typeof(LiteDbRepository<>).MakeGenericType(typeof(T));
                var repository = Activator.CreateInstance(repositoryType, options, comparer);
                return (IRepository<T>)repository!;
            }
        }

        private class TestComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T? x, T? y)
            {
                if (x is null && y is null) return true;
                if (x is null || y is null) return false;
                return x.Equals(y);
            }

            public int GetHashCode(T obj)
            {
                return obj?.GetHashCode() ?? 0;
            }
        }

        #endregion
    }
}
