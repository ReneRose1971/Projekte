using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using TestHelper.DataToolKit.Fakes.Repositories;
using TestHelper.DataToolKit.Testing;
using Xunit;

namespace DataToolKit.Tests.DataStores.Provider
{
    /// <summary>
    /// Tests für DataStoreProvider - AutoLoad-Funktionalität.
    /// </summary>
    public class DataStoreProvider_AutoLoadTests
    {
        private readonly IDataStoreFactory _factory;

        public DataStoreProvider_AutoLoadTests()
        {
            _factory = new DataStoreFactory();
        }

        #region Synchronous AutoLoad Tests

        [Fact]
        public void GetPersistent_WithAutoLoadTrue_LoadsData()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[]
            {
                new TestEntity { Id = 1, Name = "Entity1" },
                new TestEntity { Id = 2, Name = "Entity2" }
            };

            var fakeFactory = ConfigurableFakeRepositoryFactory.WithData(testData);

            // Act
            var store = provider.GetPersistent<TestEntity>(
                fakeFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);

            // Assert
            Assert.Equal(2, store.Count);
            Assert.Contains(store.Items, e => e.Id == 1);
            Assert.Contains(store.Items, e => e.Id == 2);
        }

        [Fact]
        public void GetPersistent_WithAutoLoadFalse_DoesNotLoadData()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[]
            {
                new TestEntity { Id = 1, Name = "Entity1" },
                new TestEntity { Id = 2, Name = "Entity2" }
            };

            var fakeFactory = ConfigurableFakeRepositoryFactory.WithData(testData);

            // Act
            var store = provider.GetPersistent<TestEntity>(
                fakeFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: false);

            // Assert
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void GetPersistent_NonSingleton_WithAutoLoad_LoadsData()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[] { new TestEntity { Id = 1, Name = "Test" } };
            var fakeFactory = ConfigurableFakeRepositoryFactory.WithData(testData);

            // Act
            var store = provider.GetPersistent<TestEntity>(
                fakeFactory,
                isSingleton: false,
                trackPropertyChanges: false,
                autoLoad: true);

            // Assert
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void GetPersistent_Singleton_WithAutoLoad_LoadsOnlyOnce()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[] { new TestEntity { Id = 1, Name = "Test" } };
            
            int loadCallCount = 0;
            var countingRepo = new CountingRepository<TestEntity>(testData, () => loadCallCount++);
            var fakeFactory = ConfigurableFakeRepositoryFactory.WithRepository(countingRepo);

            // Act - Zwei Aufrufe
            var store1 = provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: true);
            var store2 = provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: true);

            // Assert - Load() wurde nur einmal aufgerufen (beim ersten Mal)
            Assert.Equal(1, loadCallCount);
            Assert.Same(store1, store2);
        }

        #endregion

        #region Asynchronous AutoLoad Tests

        [Fact]
        public async Task GetPersistentAsync_WithAutoLoadTrue_LoadsDataAsync()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[]
            {
                new TestEntity { Id = 1, Name = "Entity1" },
                new TestEntity { Id = 2, Name = "Entity2" }
            };

            var fakeFactory = ConfigurableFakeRepositoryFactory.WithData(testData);

            // Act
            var store = await provider.GetPersistentAsync<TestEntity>(
                fakeFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);

            // Assert
            Assert.Equal(2, store.Count);
        }

        [Fact]
        public async Task GetPersistentAsync_WithAutoLoadFalse_DoesNotLoadData()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[] { new TestEntity { Id = 1, Name = "Test" } };
            var fakeFactory = ConfigurableFakeRepositoryFactory.WithData(testData);

            // Act
            var store = await provider.GetPersistentAsync<TestEntity>(
                fakeFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: false);

            // Assert
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public async Task GetPersistentAsync_Singleton_LoadsOnlyOnce()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[] { new TestEntity { Id = 1, Name = "Test" } };
            
            int loadCallCount = 0;
            var countingRepo = new CountingRepository<TestEntity>(testData, () => loadCallCount++);
            var fakeFactory = ConfigurableFakeRepositoryFactory.WithRepository(countingRepo);

            // Act - Zwei Aufrufe
            var store1 = await provider.GetPersistentAsync<TestEntity>(fakeFactory, autoLoad: true);
            var store2 = await provider.GetPersistentAsync<TestEntity>(fakeFactory, autoLoad: true);

            // Assert
            Assert.Equal(1, loadCallCount);
            Assert.Same(store1, store2);
        }

        [Fact]
        public async Task GetPersistentAsync_NonSingleton_LoadsEachTime()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var testData = new[] { new TestEntity { Id = 1, Name = "Test" } };
            
            int loadCallCount = 0;
            var countingRepo = new CountingRepository<TestEntity>(testData, () => loadCallCount++);
            var fakeFactory = ConfigurableFakeRepositoryFactory.WithRepository(countingRepo);

            // Act - Zwei Aufrufe mit isSingleton: false
            var store1 = await provider.GetPersistentAsync<TestEntity>(fakeFactory, isSingleton: false, autoLoad: true);
            var store2 = await provider.GetPersistentAsync<TestEntity>(fakeFactory, isSingleton: false, autoLoad: true);

            // Assert - Load() wurde zweimal aufgerufen
            Assert.Equal(2, loadCallCount);
            Assert.NotSame(store1, store2);
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Wrapper für IRepository (LiteDB), der Load()-Aufrufe zählt.
        /// Verwendet Composition statt Vererbung.
        /// </summary>
        private class CountingRepository<T> : IRepository<T> where T : class, IEntity
        {
            private readonly FakeRepository<T> _inner;
            private readonly System.Action _onLoad;

            public CountingRepository(T[] initialData, System.Action onLoad)
            {
                _inner = new FakeRepository<T>();
                _inner.SetData(initialData);
                _onLoad = onLoad;
            }

            public IReadOnlyList<T> Load()
            {
                _onLoad();
                return _inner.Load();
            }

            public void Write(IEnumerable<T> items) => _inner.Write(items);
            public void Clear() => _inner.Clear();
            public int Update(T entity) => _inner.Update(entity);
            public int Delete(T entity) => _inner.Delete(entity);
            public void Dispose() => _inner.Dispose();
        }

        #endregion
    }
}
