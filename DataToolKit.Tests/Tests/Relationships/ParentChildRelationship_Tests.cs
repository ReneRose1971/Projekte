using System;
using System.Collections.Generic;
using System.Linq;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Relationships;
using DataToolKit.Storage.DataStores;
using TestHelper.DataToolKit.Testing;
using Xunit;

namespace DataToolKit.Tests.Relationships
{
    /// <summary>
    /// Tests für ParentChildRelationship - 1:n-Beziehungsverwaltung.
    /// </summary>
    public class ParentChildRelationship_Tests
    {
        // ==================== Test Entities ====================

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class Order : TestEntity
        {
            public int? CustomerId { get; set; }
        }

        // ==================== Setup Helper ====================

        private IDataStoreProvider CreateProviderWithOrderStore(out InMemoryDataStore<Order> orderStore)
        {
            var factory = new DataStoreFactory();
            var provider = new DataStoreProvider(factory);
            
            // Register Order store as singleton
            orderStore = provider.GetInMemory<Order>(isSingleton: true);
            
            return provider;
        }

        // ==================== Constructor ====================

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenProviderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new ParentChildRelationship<Customer, Order>(null!));
        }

        [Fact]
        public void Constructor_CreatesEmptyChildsCollection()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Assert
            Assert.NotNull(relationship.Childs);
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Parent Property ====================

        [Fact]
        public void Parent_CanBeSetOnce()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);
            var customer = new Customer { Id = 1, Name = "Alice" };

            // Act
            relationship.Parent = customer;

            // Assert
            Assert.Equal(customer, relationship.Parent);
        }

        [Fact]
        public void Parent_ThrowsInvalidOperationException_WhenSetTwice()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);
            var customer1 = new Customer { Id = 1, Name = "Alice" };
            var customer2 = new Customer { Id = 2, Name = "Bob" };

            relationship.Parent = customer1;

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
                relationship.Parent = customer2);
            
            Assert.Contains("nur einmal gesetzt werden", ex.Message);
        }

        [Fact]
        public void Parent_CanBeSetToNull()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Act
            relationship.Parent = null;

            // Assert
            Assert.Null(relationship.Parent);
        }

        // ==================== IsChildFilter Property ====================

        [Fact]
        public void IsChildFilter_CanBeSetMultipleTimes()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            Func<Customer, Order, bool> filter1 = (c, o) => o.CustomerId == c.Id;
            Func<Customer, Order, bool> filter2 = (c, o) => o.CustomerId == c.Id && o.Index > 10;

            // Act & Assert - Should not throw
            relationship.IsChildFilter = filter1;
            relationship.IsChildFilter = filter2;
        }

        [Fact]
        public void IsChildFilter_SettingSameFilter_DoesNotTriggerResync()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);
            
            Func<Customer, Order, bool> filter = (c, o) => o.CustomerId == c.Id;
            relationship.IsChildFilter = filter;
            relationship.Parent = new Customer { Id = 1 };
            
            var syncCount = relationship.Childs.Count;

            // Act
            relationship.IsChildFilter = filter; // Same reference

            // Assert - Should not have triggered resync
            Assert.Equal(syncCount, relationship.Childs.Count);
        }

        // ==================== DataSource Property - Lazy Loading ====================

        [Fact]
        public void DataSource_LazyInitialization_LoadsFromProvider()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);
            
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Act
            var dataSource = relationship.DataSource;

            // Assert
            Assert.NotNull(dataSource);
            Assert.Single(dataSource.Items);
            Assert.Contains(order, dataSource.Items);
        }

        [Fact]
        public void DataSource_LazyInitialization_HandlesUnregisteredStore()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var provider = new DataStoreProvider(factory);
            // Note: No Order store registered!
            
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Act
            var dataSource = relationship.DataSource;

            // Assert - Should be null (no exception thrown)
            Assert.Null(dataSource);
        }

        [Fact]
        public void DataSource_ExplicitSet_OverridesLazyLoading()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var defaultStore);
            var customStore = new InMemoryDataStore<Order>();
            
            var order = new Order { Id = 99, Name = "Custom", CustomerId = 1 };
            customStore.Add(order);
            
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Act
            relationship.DataSource = customStore;

            // Assert
            Assert.Same(customStore, relationship.DataSource);
            Assert.Single(relationship.DataSource.Items);
            Assert.Contains(order, relationship.DataSource.Items);
        }

        [Fact]
        public void DataSource_SetToNull_ClearsChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);
            
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1 },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            // Access DataSource to trigger lazy load
            _ = relationship.DataSource;
            
            Assert.Single(relationship.Childs.Items);

            // Act
            relationship.DataSource = null;

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Basic Filtering ====================

        [Fact]
        public void Childs_FiltersBasedOnIsChildFilter()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer1 = new Customer { Id = 1, Name = "Alice" };
            var customer2 = new Customer { Id = 2, Name = "Bob" };
            
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 };
            var order3 = new Order { Id = 3, Name = "Order3", CustomerId = 1 };
            
            orderStore.AddRange(new[] { order1, order2, order3 });

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer1,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Equal(2, relationship.Childs.Count);
            Assert.Contains(order1, relationship.Childs.Items);
            Assert.Contains(order3, relationship.Childs.Items);
            Assert.DoesNotContain(order2, relationship.Childs.Items);
        }

        [Fact]
        public void Childs_EmptyWhenNoMatches()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 999, Name = "NoOrders" };
            
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 };
            
            orderStore.AddRange(new[] { order1, order2 });

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_NullCustomerId_NotIncluded()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = null };
            
            orderStore.AddRange(new[] { order1, order2 });

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order1, relationship.Childs.Items);
            Assert.DoesNotContain(order2, relationship.Childs.Items);
        }

        // ==================== PropertyChanged - Item gains relationship ====================

        [Fact]
        public void Childs_PropertyChanged_ItemGainsRelationship_AddsToChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order = new Order { Id = 1, Name = "Order1", CustomerId = null };
            
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Empty(relationship.Childs.Items);

            // Act
            order.CustomerId = 1;

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order, relationship.Childs.Items);
        }

        [Fact]
        public void Childs_PropertyChanged_ItemLosesRelationship_RemovesFromChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);

            // Act
            order.CustomerId = 2; // Changes to different customer

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_PropertyChanged_IrrelevantProperty_NoChange()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);

            // Act
            order.Name = "Modified"; // Doesn't affect filter

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order, relationship.Childs.Items);
        }

        // ==================== CollectionChanged.Add ====================

        [Fact]
        public void Childs_DataSourceAdd_MatchingItem_AddsToChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Empty(relationship.Childs.Items);

            // Act
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order, relationship.Childs.Items);
        }

        [Fact]
        public void Childs_DataSourceAdd_NonMatchingItem_DoesNotAddToChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Act
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 2 };
            orderStore.Add(order);

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_DataSourceAddRange_MixedItems_AddsOnlyMatching()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Act
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 }; // Match
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 }; // No match
            var order3 = new Order { Id = 3, Name = "Order3", CustomerId = 1 }; // Match
            
            orderStore.AddRange(new[] { order1, order2, order3 });

            // Assert
            Assert.Equal(2, relationship.Childs.Count);
            Assert.Contains(order1, relationship.Childs.Items);
            Assert.Contains(order3, relationship.Childs.Items);
            Assert.DoesNotContain(order2, relationship.Childs.Items);
        }

        // ==================== CollectionChanged.Remove ====================

        [Fact]
        public void Childs_DataSourceRemove_RemovesFromChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);

            // Act
            orderStore.Remove(order);

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_DataSourceRemove_NonMatchingItem_NoEffect()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 };
            
            orderStore.AddRange(new[] { order1, order2 });

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);

            // Act
            orderStore.Remove(order2); // Was not in Childs anyway

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order1, relationship.Childs.Items);
        }

        // ==================== CollectionChanged.Clear ====================

        [Fact]
        public void Childs_DataSourceClear_ClearsChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 1 };
            
            orderStore.AddRange(new[] { order1, order2 });

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Equal(2, relationship.Childs.Count);

            // Act
            orderStore.Clear();

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_DataSourceClear_ThenAdd_ResynchronizesCorrectly()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            
            orderStore.Add(order1);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            orderStore.Clear();

            // Act
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 1 };
            orderStore.Add(order2);

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order2, relationship.Childs.Items);
            Assert.DoesNotContain(order1, relationship.Childs.Items);
        }

        // ==================== Dispose ====================

        [Fact]
        public void Dispose_StopsAllSynchronization()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            
            orderStore.Add(order1);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);
            var childCountBeforeDispose = relationship.Childs.Count;

            // Act
            relationship.Dispose();
            
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 1 };
            orderStore.Add(order2);

            // Assert - After dispose, Childs should be empty (cleared in Dispose)
            Assert.Empty(relationship.Childs.Items);
            
            // Verify order2 was actually added to orderStore
            Assert.Equal(2, orderStore.Count);
        }

        [Fact]
        public void Dispose_Idempotent_CanBeCalledMultipleTimes()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out _);
            var relationship = new ParentChildRelationship<Customer, Order>(provider);

            // Act & Assert - Should not throw
            relationship.Dispose();
            relationship.Dispose();
            relationship.Dispose();
        }

        // ==================== Complex Scenarios ====================

        [Fact]
        public void ComplexScenario_MultipleOperations_MaintainsCorrectState()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer1 = new Customer { Id = 1, Name = "Alice" };
            var customer2 = new Customer { Id = 2, Name = "Bob" };
            
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 };
            var order3 = new Order { Id = 3, Name = "Order3", CustomerId = 1 };
            
            orderStore.AddRange(new[] { order1, order2, order3 });

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer1,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            // Initial state: [order1, order3]
            Assert.Equal(2, relationship.Childs.Count);

            // Act 1: order2 changes to customer1
            order2.CustomerId = 1;
            Assert.Equal(3, relationship.Childs.Count);

            // Act 2: order1 changes to customer2
            order1.CustomerId = 2;
            Assert.Equal(2, relationship.Childs.Count);
            Assert.Contains(order2, relationship.Childs.Items);
            Assert.Contains(order3, relationship.Childs.Items);

            // Act 3: Add new order for customer1
            var order4 = new Order { Id = 4, Name = "Order4", CustomerId = 1 };
            orderStore.Add(order4);
            Assert.Equal(3, relationship.Childs.Count);

            // Act 4: Remove order3
            orderStore.Remove(order3);
            Assert.Equal(2, relationship.Childs.Count);
            Assert.Contains(order2, relationship.Childs.Items);
            Assert.Contains(order4, relationship.Childs.Items);
        }

        [Fact]
        public void ComplexScenario_FilterChange_ResynchronizesChilds()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            var customer = new Customer { Id = 1, Name = "Alice" };
            
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1, Index = 5 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 1, Index = 15 };
            var order3 = new Order { Id = 3, Name = "Order3", CustomerId = 1, Index = 25 };
            
            orderStore.AddRange(new[] { order1, order2, order3 });

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            // Initial: All 3 orders
            Assert.Equal(3, relationship.Childs.Count);

            // Act: Change filter to only include Index > 10
            relationship.IsChildFilter = (c, o) => o.CustomerId == c.Id && o.Index > 10;

            // Assert
            Assert.Equal(2, relationship.Childs.Count);
            Assert.DoesNotContain(order1, relationship.Childs.Items);
            Assert.Contains(order2, relationship.Childs.Items);
            Assert.Contains(order3, relationship.Childs.Items);
        }
    }
}
