using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Storage.DataStores;
using DataToolKit.Tests.Common;
using DataToolKit.Tests.Testing;
using Xunit;

namespace DataToolKit.Tests.DataStores.Provider
{
    /// <summary>
    /// Tests für DataStoreProvider - Thread-Safety.
    /// </summary>
    public class DataStoreProvider_ThreadSafetyTests
    {
        private readonly IDataStoreFactory _factory;

        public DataStoreProvider_ThreadSafetyTests()
        {
            _factory = new DataStoreFactory();
        }

        #region Concurrent Singleton Access Tests

        [Fact]
        public async Task GetInMemory_ConcurrentCalls_ReturnsSameSingleton()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int concurrentCalls = 100;

            // Act - 100 parallele Aufrufe
            var tasks = Enumerable.Range(0, concurrentCalls)
                .Select(_ => Task.Run(() => provider.GetInMemory<TestEntity>()))
                .ToArray();

            var stores = await Task.WhenAll(tasks);

            // Assert - Alle sollten die gleiche Instanz sein
            var distinctStores = stores.Distinct().Count();
            Assert.Equal(1, distinctStores);
        }

        [Fact]
        public async Task GetPersistent_ConcurrentCalls_ReturnsSameSingleton()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            const int concurrentCalls = 100;

            // Act - 100 parallele Aufrufe
            var tasks = Enumerable.Range(0, concurrentCalls)
                .Select(_ => Task.Run(() => 
                    provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: false)))
                .ToArray();

            var stores = await Task.WhenAll(tasks);

            // Assert - Alle sollten die gleiche Instanz sein
            var distinctStores = stores.Distinct().Count();
            Assert.Equal(1, distinctStores);
        }

        [Fact]
        public async Task GetInMemoryAsync_ConcurrentCalls_ReturnsSameSingleton()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int concurrentCalls = 100;

            // Act
            var tasks = Enumerable.Range(0, concurrentCalls)
                .Select(_ => provider.GetInMemoryAsync<TestEntity>())
                .ToArray();

            var stores = await Task.WhenAll(tasks);

            // Assert
            var distinctStores = stores.Distinct().Count();
            Assert.Equal(1, distinctStores);
        }

        [Fact]
        public async Task GetPersistentAsync_ConcurrentCalls_ReturnsSameSingleton()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            const int concurrentCalls = 100;

            // Act
            var tasks = Enumerable.Range(0, concurrentCalls)
                .Select(_ => provider.GetPersistentAsync<TestEntity>(fakeFactory, autoLoad: false))
                .ToArray();

            var stores = await Task.WhenAll(tasks);

            // Assert
            var distinctStores = stores.Distinct().Count();
            Assert.Equal(1, distinctStores);
        }

        #endregion

        #region Mixed Concurrent Operations Tests

        [Fact]
        public async Task ConcurrentGetAndRemove_ThreadSafe()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int iterations = 50;

            // Act - Paralleles Get und Remove
            var getTasks = Enumerable.Range(0, iterations)
                .Select(_ => Task.Run(() => provider.GetInMemory<TestEntity>()))
                .Cast<Task>()  // Cast to Task
                .ToArray();

            var removeTasks = Enumerable.Range(0, iterations)
                .Select(_ => Task.Run(() => provider.RemoveSingleton<TestEntity>()))
                .Cast<Task>()  // Cast to Task
                .ToArray();

            var allTasks = getTasks.Concat(removeTasks).ToArray();
            await Task.WhenAll(allTasks);

            // Assert - Kein Crash, Operation abgeschlossen
            Assert.True(true);
        }

        [Fact]
        public async Task ConcurrentGetAndClearAll_ThreadSafe()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int getCallCount = 50;

            // Act - Paralleles Get und ClearAll
            var getTasks = Enumerable.Range(0, getCallCount)
                .Select(_ => Task.Run(() => provider.GetInMemory<TestEntity>()))
                .Cast<Task>()
                .ToArray();

            var clearTask = Task.Run(async () =>
            {
                await Task.Delay(10); // Kleines Delay
                provider.ClearAll();
            });

            var allTasks = getTasks.Append(clearTask).ToArray();
            await Task.WhenAll(allTasks);

            // Assert - Kein Crash
            Assert.True(true);
        }

        [Fact]
        public async Task ConcurrentDifferentTypes_Independent()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int callsPerType = 50;

            // Act - Parallele Aufrufe für verschiedene Typen
            var customerTasks = Enumerable.Range(0, callsPerType)
                .Select(_ => Task.Run(() => provider.GetInMemory<Customer>()))
                .ToArray();

            var orderTasks = Enumerable.Range(0, callsPerType)
                .Select(_ => Task.Run(() => provider.GetInMemory<Order>()))
                .ToArray();

            var customerStores = await Task.WhenAll(customerTasks);
            var orderStores = await Task.WhenAll(orderTasks);

            // Assert - Jeder Typ hat genau eine Singleton-Instanz
            Assert.Equal(1, customerStores.Distinct().Count());
            Assert.Equal(1, orderStores.Distinct().Count());
            Assert.NotSame(customerStores[0], orderStores[0]);
        }

        [Fact]
        public async Task ConcurrentSingletonAndNonSingleton_Independent()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int calls = 50;

            // Act - Parallele Singleton und Non-Singleton Aufrufe
            var singletonTasks = Enumerable.Range(0, calls)
                .Select(_ => Task.Run(() => provider.GetInMemory<TestEntity>(isSingleton: true)))
                .ToArray();

            var nonSingletonTasks = Enumerable.Range(0, calls)
                .Select(_ => Task.Run(() => provider.GetInMemory<TestEntity>(isSingleton: false)))
                .ToArray();

            var singletons = await Task.WhenAll(singletonTasks);
            var nonSingletons = await Task.WhenAll(nonSingletonTasks);

            // Assert
            // Alle Singletons sind gleich
            Assert.Equal(1, singletons.Distinct().Count());
            
            // Alle Non-Singletons sind unterschiedlich
            Assert.Equal(calls, nonSingletons.Distinct().Count());
        }

        #endregion

        #region Stress Tests

        [Fact]
        public async Task StressTest_MixedOperations_NoRaceConditions()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            const int iterations = 100;

            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            var tasks = new List<Task>();

            // Act - Gemischte Operationen mit VERSCHIEDENEN Typen
            // (weil pro Typ nur eine Instanz existieren darf)
            for (int i = 0; i < iterations; i++)
            {
                var index = i; // Capture for closure
                
                // Abwechselnd GetInMemory (Customer) und GetPersistent (Order)
                if (index % 2 == 0)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            provider.GetInMemory<Customer>();
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(new Exception($"GetInMemory<Customer> failed at iteration {index}", ex));
                        }
                    }));
                }
                else
                {
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            var fakeFactory = new FakeRepositoryFactory();
                            provider.GetPersistent<Order>(fakeFactory, autoLoad: false);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Add(new Exception($"GetPersistent<Order> failed at iteration {index}", ex));
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);

            // Assert - Zeige erste Exception, falls vorhanden
            if (!exceptions.IsEmpty)
            {
                var firstException = exceptions.First();
                throw new Xunit.Sdk.XunitException(
                    $"StressTest failed with {exceptions.Count} exception(s). First: {firstException.Message}\n\nInner: {firstException.InnerException?.Message}\n\nStack:\n{firstException.InnerException?.StackTrace}", 
                    firstException);
            }

            // Zusätzliche Assertions - verifiziere beide Stores existieren
            var customerStore = provider.GetInMemory<Customer>();
            var orderStore = provider.GetPersistent<Order>(new FakeRepositoryFactory(), autoLoad: false);
            
            Assert.NotNull(customerStore);
            Assert.NotNull(orderStore);
            Assert.NotSame(customerStore, orderStore);
            
            Assert.True(true, "StressTest completed successfully");
        }

        #endregion

        #region Singleton Exclusivity Tests

        [Fact]
        public void GetInMemory_ThenGetPersistent_SameType_ThrowsException()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            
            // Act - Zuerst InMemory erstellen
            var inMemoryStore = provider.GetInMemory<TestEntity>();
            
            // Dann versuchen, Persistent für denselben Typ zu erstellen
            var exception = Assert.Throws<InvalidOperationException>(() =>
                provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: false));
            
            // Assert
            Assert.Contains("existiert bereits", exception.Message);
            Assert.Contains("InMemoryDataStore", exception.Message);
        }

        [Fact]
        public void GetPersistent_ThenGetInMemory_SameType_ReturnsPersistentStore()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            
            // Act - Zuerst Persistent erstellen
            var persistentStore = provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: false);
            
            // Dann GetInMemory aufrufen
            var inMemoryStore = provider.GetInMemory<TestEntity>();
            
            // Assert - Da PersistentDataStore von InMemoryDataStore erbt, wird der gleiche Store zurückgegeben
            Assert.Same(persistentStore, inMemoryStore);
            Assert.IsAssignableFrom<PersistentDataStore<TestEntity>>(inMemoryStore);
        }

        [Fact]
        public void GetDataStore_AfterGetInMemory_ReturnsSameInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            
            // Act
            var inMemoryStore = provider.GetInMemory<TestEntity>();
            var dataStore = provider.GetDataStore<TestEntity>();
            
            // Assert
            Assert.Same(inMemoryStore, dataStore);
        }

        [Fact]
        public void GetDataStore_AfterGetPersistent_ReturnsSameInstance()
        {
            // Arrange
            using var provider = new DataStoreProvider(_factory);
            var fakeFactory = new FakeRepositoryFactory();
            
            // Act
            var persistentStore = provider.GetPersistent<TestEntity>(fakeFactory, autoLoad: false);
            var dataStore = provider.GetDataStore<TestEntity>();
            
            // Assert
            Assert.Same(persistentStore, dataStore);
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
