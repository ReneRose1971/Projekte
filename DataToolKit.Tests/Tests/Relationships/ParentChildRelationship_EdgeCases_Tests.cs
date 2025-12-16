using System;
using System.Collections.Generic;
using Xunit;
using DataToolKit.Relationships;
using DataToolKit.Storage.DataStores;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Tests.Testing;

namespace DataToolKit.Tests.Relationships
{
    /// <summary>
    /// Edge Cases und Fehlerszenarien für ParentChildRelationship.
    /// </summary>
    public class ParentChildRelationship_EdgeCases_Tests
    {
        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class Order : TestEntity
        {
            public int? CustomerId { get; set; }
        }

        private IDataStoreProvider CreateProviderWithOrderStore(out InMemoryDataStore<Order> orderStore)
        {
            var factory = new DataStoreFactory();
            var provider = new DataStoreProvider(factory);
            orderStore = provider.GetInMemory<Order>(isSingleton: true);
            return provider;
        }

        // ==================== Null Parent ====================

        [Fact]
        public void Childs_NullParent_RemainsEmpty()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = null,
                IsChildFilter = (c, o) => o.CustomerId == c?.Id
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Null Filter ====================

        [Fact]
        public void Childs_NullFilter_RemainsEmpty()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = null
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        [Fact]
        public void Childs_NullFilter_ThenSetFilter_Synchronizes()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = null
            };
            
            Assert.Empty(relationship.Childs.Items);

            // Act
            relationship.IsChildFilter = (c, o) => o.CustomerId == c.Id;

            // Assert
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order, relationship.Childs.Items);
        }

        // ==================== No DataSource ====================

        [Fact]
        public void Childs_NoDataSource_RemainsEmpty()
        {
            // Arrange
            var factory = new DataStoreFactory();
            var provider = new DataStoreProvider(factory);
            // No Order store registered!

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Empty DataSource ====================

        [Fact]
        public void Childs_EmptyDataSource_RemainsEmpty()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            // orderStore is empty

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Filter Always Returns False ====================

        [Fact]
        public void Childs_FilterAlwaysFalse_RemainsEmpty()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => false // Always false
            };

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== Filter Always Returns True ====================

        [Fact]
        public void Childs_FilterAlwaysTrue_ContainsAllItems()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 2 };
            var order3 = new Order { Id = 3, Name = "Order3", CustomerId = 3 };
            orderStore.AddRange(new[] { order1, order2, order3 });

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => true // Always true
            };

            // Assert
            Assert.Equal(3, relationship.Childs.Count);
        }

        // ==================== Filter Throws Exception ====================

        [Fact]
        public void Childs_FilterThrowsException_ExceptionPropagates()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var relationship = new ParentChildRelationship<Customer, Order>(provider)
                {
                    Parent = new Customer { Id = 1, Name = "Alice" },
                    IsChildFilter = (c, o) => throw new InvalidOperationException("Filter error")
                };
            });
        }

        // ==================== Large Dataset ====================

        [Fact]
        public void Childs_LargeDataset_PerformanceAcceptable()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            
            // Add 1000 orders: 500 for customer 1, 500 for customer 2
            for (int i = 1; i <= 1000; i++)
            {
                orderStore.Add(new Order 
                { 
                    Id = i, 
                    Name = $"Order{i}", 
                    CustomerId = i % 2 == 0 ? 1 : 2 
                });
            }

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Equal(500, relationship.Childs.Count);
        }

        // ==================== Rapid Property Changes ====================

        [Fact]
        public void Childs_RapidPropertyChanges_HandlesCorrectly()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = null };
            orderStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Act - Rapid changes
            order.CustomerId = 1;   // Should be added
            order.CustomerId = 2;   // Should be removed
            order.CustomerId = 1;   // Should be added again
            order.CustomerId = null; // Should be removed

            // Assert
            Assert.Empty(relationship.Childs.Items);
        }

        // ==================== PropertyChanged from Non-Entity ====================

        [Fact]
        public void Childs_NonINotifyPropertyChanged_NoAutoUpdate()
        {
            // Arrange - Using a simple class without INotifyPropertyChanged
            var factory = new DataStoreFactory();
            var provider = new DataStoreProvider(factory);
            var simpleStore = provider.GetInMemory<SimpleOrder>(isSingleton: true);

            var customer = new Customer { Id = 1 };
            var order = new SimpleOrder { Id = 1, CustomerId = null };
            simpleStore.Add(order);

            var relationship = new ParentChildRelationship<Customer, SimpleOrder>(provider)
            {
                Parent = customer,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            Assert.Empty(relationship.Childs.Items);

            // Act - Property change without PropertyChanged event
            order.CustomerId = 1;

            // Assert - Should NOT be added (no PropertyChanged notification)
            Assert.Empty(relationship.Childs.Items);
        }

        // Simple class without INotifyPropertyChanged
        private class SimpleOrder
        {
            public int Id { get; set; }
            public int? CustomerId { get; set; }
        }

        // ==================== Multiple Relationships Same DataSource ====================

        [Fact]
        public void MultipleRelationships_SameDataSource_IndependentFiltering()
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
            var relationship1 = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer1,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            var relationship2 = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = customer2,
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Equal(2, relationship1.Childs.Count);
            Assert.Contains(order1, relationship1.Childs.Items);
            Assert.Contains(order3, relationship1.Childs.Items);

            Assert.Single(relationship2.Childs.Items);
            Assert.Contains(order2, relationship2.Childs.Items);
        }

        // ==================== DataSource Changes After Relationship Created ====================

        [Fact]
        public void DataSource_ChangedAfterCreation_NewDataSourceSynchronizes()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var defaultStore);
            var order1 = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            defaultStore.Add(order1);

            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };
            
            Assert.Single(relationship.Childs.Items);

            // Act - Set new DataSource
            var newStore = new InMemoryDataStore<Order>();
            var order2 = new Order { Id = 2, Name = "Order2", CustomerId = 1 };
            newStore.Add(order2);
            
            relationship.DataSource = newStore;

            // Assert - Should now show order2 instead of order1
            Assert.Single(relationship.Childs.Items);
            Assert.Contains(order2, relationship.Childs.Items);
            Assert.DoesNotContain(order1, relationship.Childs.Items);
        }

        // ==================== Initialization Order ====================

        [Fact]
        public void InitializationOrder_FilterBeforeParent_Works()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                IsChildFilter = (c, o) => o.CustomerId == c.Id, // Set filter first
                Parent = new Customer { Id = 1, Name = "Alice" }  // Then parent
            };

            // Assert
            Assert.Single(relationship.Childs.Items);
        }

        [Fact]
        public void InitializationOrder_ParentBeforeFilter_Works()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                Parent = new Customer { Id = 1, Name = "Alice" },  // Set parent first
                IsChildFilter = (c, o) => o.CustomerId == c.Id     // Then filter
            };

            // Assert
            Assert.Single(relationship.Childs.Items);
        }

        [Fact]
        public void InitializationOrder_DataSourceBeforeParentAndFilter_Works()
        {
            // Arrange
            var provider = CreateProviderWithOrderStore(out var orderStore);
            var order = new Order { Id = 1, Name = "Order1", CustomerId = 1 };
            orderStore.Add(order);

            // Act
            var relationship = new ParentChildRelationship<Customer, Order>(provider)
            {
                DataSource = orderStore,  // Explicit DataSource first
                Parent = new Customer { Id = 1, Name = "Alice" },
                IsChildFilter = (c, o) => o.CustomerId == c.Id
            };

            // Assert
            Assert.Single(relationship.Childs.Items);
        }
    }
}
