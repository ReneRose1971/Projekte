using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Tests.Common;
using DataToolKit.Tests.Testing;
using Xunit;

namespace DataToolKit.Tests.DataStores.Factory
{
    /// <summary>
    /// Tests für DataStoreFactory - Validierung der Factory-Erzeugungslogik.
    /// </summary>
    public class DataStoreFactoryTests
    {
        private readonly IDataStoreFactory _factory;

        public DataStoreFactoryTests()
        {
            _factory = new DataStoreFactory();
        }

        #region InMemoryDataStore Creation Tests

        [Fact]
        public void CreateInMemoryStore_Returns_NewInstance()
        {
            // Act
            var store = _factory.CreateInMemoryStore<TestEntity>();

            // Assert
            Assert.NotNull(store);
            Assert.IsType<InMemoryDataStore<TestEntity>>(store);
        }

        [Fact]
        public void CreateInMemoryStore_WithComparer_UsesComparer()
        {
            // Arrange
            var comparer = EqualityComparer<TestEntity>.Default;

            // Act
            var store = _factory.CreateInMemoryStore<TestEntity>(comparer);

            // Assert
            Assert.NotNull(store);
            // Comparer wird intern verwendet - keine direkte Validierung möglich,
            // aber Store sollte erfolgreich erstellt werden
        }

        [Fact]
        public void CreateInMemoryStore_WithoutComparer_UsesDefault()
        {
            // Act
            var store = _factory.CreateInMemoryStore<TestEntity>();

            // Assert
            Assert.NotNull(store);
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void CreateInMemoryStore_Multiple_ReturnsDistinctInstances()
        {
            // Act
            var store1 = _factory.CreateInMemoryStore<TestEntity>();
            var store2 = _factory.CreateInMemoryStore<TestEntity>();

            // Assert
            Assert.NotSame(store1, store2);
        }

        [Fact]
        public void CreateInMemoryStore_DifferentTypes_ReturnsCorrectTypes()
        {
            // Act
            var customerStore = _factory.CreateInMemoryStore<Customer>();
            var orderStore = _factory.CreateInMemoryStore<Order>();

            // Assert
            Assert.IsType<InMemoryDataStore<Customer>>(customerStore);
            Assert.IsType<InMemoryDataStore<Order>>(orderStore);
            Assert.NotSame(customerStore, orderStore);
        }

        #endregion

        #region PersistentDataStore Creation Tests

        [Fact]
        public void CreatePersistentStore_Returns_NewInstance()
        {
            // Arrange
            var repository = new FakeRepositoryBase<TestEntity>();

            // Act
            var store = _factory.CreatePersistentStore(repository);

            // Assert
            Assert.NotNull(store);
            Assert.IsType<PersistentDataStore<TestEntity>>(store);
        }

        [Fact]
        public void CreatePersistentStore_WithTrackingEnabled_DoesNotThrow()
        {
            // Arrange
            var repository = new FakeRepositoryBase<TestEntity>();

            // Act & Assert
            var store = _factory.CreatePersistentStore(repository, trackPropertyChanges: true);
            Assert.NotNull(store);
        }

        [Fact]
        public void CreatePersistentStore_WithTrackingDisabled_DoesNotThrow()
        {
            // Arrange
            var repository = new FakeRepositoryBase<TestEntity>();

            // Act & Assert
            var store = _factory.CreatePersistentStore(repository, trackPropertyChanges: false);
            Assert.NotNull(store);
        }

        [Fact]
        public void CreatePersistentStore_DoesNotAutoLoad()
        {
            // Arrange
            var repository = new FakeRepositoryBase<TestEntity>();
            repository.SetData(new[] { new TestEntity { Id = 1, Name = "Test" } });

            // Act
            var store = _factory.CreatePersistentStore(repository);

            // Assert - Factory lädt NICHT automatisch
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void CreatePersistentStore_Multiple_ReturnsDistinctInstances()
        {
            // Arrange
            var repository = new FakeRepositoryBase<TestEntity>();

            // Act
            var store1 = _factory.CreatePersistentStore(repository);
            var store2 = _factory.CreatePersistentStore(repository);

            // Assert
            Assert.NotSame(store1, store2);
        }

        [Fact]
        public void CreatePersistentStore_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<System.ArgumentNullException>(() =>
                _factory.CreatePersistentStore<TestEntity>(null!));
        }

        #endregion

        #region Helper Classes

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        private class Order
        {
            public int Id { get; set; }
            public decimal Total { get; set; }
        }

        #endregion
    }
}
