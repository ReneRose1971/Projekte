using System;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Tests.Common;
using DataToolKit.Tests.Testing;
using Xunit;

namespace DataToolKit.Tests.DataStores.Provider
{
    /// <summary>
    /// Tests für DataStoreProvider - Singleton-Management.
    /// </summary>
    public class DataStoreProvider_SingletonTests
    {
        private readonly IDataStoreFactory _factory;

        public DataStoreProvider_SingletonTests()
        {
            _factory = new DataStoreFactory();
        }

        #region InMemoryDataStore Singleton Tests

        [Fact]
        public void GetInMemory_WithSingletonTrue_ReturnsSameInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act
            var store1 = provider.GetInMemory<TestEntity>(isSingleton: true);
            var store2 = provider.GetInMemory<TestEntity>(isSingleton: true);

            // Assert
            Assert.Same(store1, store2);
        }

        [Fact]
        public void GetInMemory_WithSingletonFalse_ReturnsNewInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act
            var store1 = provider.GetInMemory<TestEntity>(isSingleton: false);
            var store2 = provider.GetInMemory<TestEntity>(isSingleton: false);

            // Assert
            Assert.NotSame(store1, store2);
        }

        [Fact]
        public void GetInMemory_TwoCallsWithSingleton_ReturnsSameReference()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act
            var store1 = provider.GetInMemory<TestEntity>();
            store1.Add(new TestEntity { Id = 1, Name = "Test" });

            var store2 = provider.GetInMemory<TestEntity>();

            // Assert
            Assert.Same(store1, store2);
            Assert.Equal(1, store2.Count);
        }

        [Fact]
        public void GetInMemory_DifferentTypes_ReturnsDifferentInstances()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act
            var customerStore = provider.GetInMemory<Customer>();
            var orderStore = provider.GetInMemory<Order>();

            // Assert
            Assert.NotSame(customerStore, orderStore);
        }

        [Fact]
        public void GetInMemory_NonSingleton_DoesNotAffectSingleton()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act
            var singleton = provider.GetInMemory<TestEntity>(isSingleton: true);
            singleton.Add(new TestEntity { Id = 1, Name = "Singleton" });

            var nonSingleton = provider.GetInMemory<TestEntity>(isSingleton: false);
            nonSingleton.Add(new TestEntity { Id = 2, Name = "NonSingleton" });

            // Assert
            Assert.Equal(1, singleton.Count);
            Assert.Equal(1, nonSingleton.Count);
        }

        #endregion

        #region PersistentDataStore Singleton Tests

        [Fact]
        public void GetPersistent_WithSingletonTrue_ReturnsSameInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();

            // Act
            var store1 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: true, autoLoad: false);
            var store2 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: true, autoLoad: false);

            // Assert
            Assert.Same(store1, store2);
        }

        [Fact]
        public void GetPersistent_WithSingletonFalse_ReturnsNewInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();

            // Act
            var store1 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: false, autoLoad: false);
            var store2 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: false, autoLoad: false);

            // Assert
            Assert.NotSame(store1, store2);
        }

        [Fact]
        public void GetPersistent_TwoCallsWithSingleton_ReturnsSameReference()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();

            // Act
            var store1 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: true, autoLoad: false);
            store1.Add(new TestEntity { Id = 1, Name = "Test" });

            var store2 = provider.GetPersistent<TestEntity>(fakeFactory, isSingleton: true, autoLoad: false);

            // Assert
            Assert.Same(store1, store2);
            Assert.Equal(1, store2.Count);
        }

        [Fact]
        public void GetPersistent_DifferentTypes_ReturnsDifferentInstances()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();

            // Act
            var customerStore = provider.GetPersistent<Customer>(fakeFactory, autoLoad: false);
            var orderStore = provider.GetPersistent<Order>(fakeFactory, autoLoad: false);

            // Assert
            Assert.NotSame(customerStore, orderStore);
        }

        [Fact]
        public void GetPersistent_NullRepositoryFactory_ThrowsArgumentNullException()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                provider.GetPersistent<TestEntity>(null!, isSingleton: true, autoLoad: false));
        }

        #endregion

        #region Helper Classes

        private class Customer : TestEntity
        {
            public string Email { get; set; } = "";
        }

        private class Order : TestEntity
        {
            public decimal Total { get; set; }
        }

        #endregion
    }
}
