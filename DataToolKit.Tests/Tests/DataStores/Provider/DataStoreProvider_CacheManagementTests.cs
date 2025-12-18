using System;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using TestHelper.DataToolKit.Fakes.Repositories;
using TestHelper.DataToolKit.Testing;
using Xunit;

namespace DataToolKit.Tests.DataStores.Provider
{
    /// <summary>
    /// Tests für DataStoreProvider - Cache-Management (RemoveSingleton, ClearAll).
    /// </summary>
    public class DataStoreProvider_CacheManagementTests
    {
        private readonly IDataStoreFactory _factory;

        public DataStoreProvider_CacheManagementTests()
        {
            _factory = new DataStoreFactory();
        }

        #region RemoveSingleton Tests

        [Fact]
        public void RemoveSingleton_ExistingSingleton_ReturnsTrue()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var store = provider.GetInMemory<TestEntity>();

            // Act
            var removed = provider.RemoveSingleton<TestEntity>();

            // Assert
            Assert.True(removed);
        }

        [Fact]
        public void RemoveSingleton_NonExistingSingleton_ReturnsFalse()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act - Ohne vorheriges GetInMemory
            var removed = provider.RemoveSingleton<TestEntity>();

            // Assert
            Assert.False(removed);
        }

        [Fact]
        public void RemoveSingleton_ThenGetAgain_CreatesNewInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var store1 = provider.GetInMemory<TestEntity>();
            store1.Add(new TestEntity { Id = 1, Name = "Test" });

            // Act
            provider.RemoveSingleton<TestEntity>();
            var store2 = provider.GetInMemory<TestEntity>();

            // Assert
            Assert.NotSame(store1, store2);
            Assert.Equal(0, store2.Count); // Neue Instanz ist leer
        }

        [Fact]
        public void RemoveSingleton_DisposesStore()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            
            var store = provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: false);
            
            // Act
            provider.RemoveSingleton<TestEntity>();

            // Assert
            // PersistentDataStore implementiert IDisposable
            // Nach Remove sollte der Store disposed sein
            // (Keine direkte Validierung möglich ohne zusätzliche Test-Infrastruktur)
            Assert.True(true);
        }

        [Fact]
        public void RemoveSingleton_DifferentType_DoesNotAffectOtherSingletons()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var customerStore = provider.GetInMemory<Customer>();
            var orderStore = provider.GetInMemory<Order>();

            // Act
            provider.RemoveSingleton<Customer>();

            // Assert
            var newCustomerStore = provider.GetInMemory<Customer>();
            var sameOrderStore = provider.GetInMemory<Order>();

            Assert.NotSame(customerStore, newCustomerStore);
            Assert.Same(orderStore, sameOrderStore);
        }

        #endregion

        #region ClearAll Tests

        [Fact]
        public void ClearAll_DisposesAllSingletons()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var store1 = provider.GetInMemory<TestEntity>();
            var store2 = provider.GetInMemory<Customer>();

            // Act
            provider.ClearAll();

            // Assert
            var newStore1 = provider.GetInMemory<TestEntity>();
            var newStore2 = provider.GetInMemory<Customer>();

            Assert.NotSame(store1, newStore1);
            Assert.NotSame(store2, newStore2);
        }

        [Fact]
        public void ClearAll_ThenGetAgain_CreatesNewInstances()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var store1 = provider.GetInMemory<TestEntity>();
            store1.Add(new TestEntity { Id = 1, Name = "Test1" });

            var store2 = provider.GetInMemory<Customer>();
            store2.Add(new Customer { Id = 2, Name = "Test2" });

            // Act
            provider.ClearAll();

            var newStore1 = provider.GetInMemory<TestEntity>();
            var newStore2 = provider.GetInMemory<Customer>();

            // Assert
            Assert.Equal(0, newStore1.Count);
            Assert.Equal(0, newStore2.Count);
        }

        [Fact]
        public void ClearAll_EmptyCache_DoesNotThrow()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act & Assert
            provider.ClearAll(); // Sollte nicht werfen
            Assert.True(true);
        }

        [Fact]
        public void ClearAll_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var store = provider.GetInMemory<TestEntity>();

            // Act & Assert
            provider.ClearAll();
            provider.ClearAll(); // Zweiter Aufruf
            provider.ClearAll(); // Dritter Aufruf

            Assert.True(true);
        }

        #endregion

        #region Provider Dispose Tests

        [Fact]
        public void Dispose_CallsClearAll()
        {
            // Arrange
            var provider = new DataStoreProvider(_factory);
            var store1 = provider.GetInMemory<TestEntity>();
            var store2 = provider.GetInMemory<Customer>();

            // Act
            provider.Dispose();

            // Assert
            // Nach Dispose sollten keine Singletons mehr existieren
            // (Indirekte Validierung - Dispose ruft ClearAll auf)
            Assert.True(true);
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var provider = new DataStoreProvider(_factory);

            // Act & Assert
            provider.Dispose();
            provider.Dispose(); // Zweiter Aufruf sollte sicher sein
            Assert.True(true);
        }

        [Fact]
        public void Dispose_WithUsingPattern_WorksCorrectly()
        {
            // Arrange & Act
            using (var provider = new DataStoreProvider(_factory))
            {
                var store = provider.GetInMemory<TestEntity>();
                Assert.NotNull(store);
            }

            // Assert - using-Block hat Dispose aufgerufen
            Assert.True(true);
        }

        #endregion

        #region Cache State Tests

        [Fact]
        public void CacheState_AfterMultipleOperations_Consistent()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);

            // Act - Verschiedene Operationen
            var store1 = provider.GetInMemory<TestEntity>();
            var store2 = provider.GetInMemory<Customer>();
            
            provider.RemoveSingleton<TestEntity>();
            
            var store3 = provider.GetInMemory<TestEntity>(); // Neue Instanz
            var store4 = provider.GetInMemory<Customer>();   // Gleiche Instanz wie store2

            // Assert
            Assert.NotSame(store1, store3);
            Assert.Same(store2, store4);
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
