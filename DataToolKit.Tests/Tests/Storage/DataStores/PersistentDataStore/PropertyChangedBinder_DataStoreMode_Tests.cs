using System;
using System.Collections.Generic;
using Xunit;
using DataToolKit.Storage.Persistence;
using DataToolKit.Storage.DataStores;
using DataToolKit.Tests.Testing;

namespace DataToolKit.Tests.Storage.Persistence
{
    /// <summary>
    /// Tests für PropertyChangedBinder im DataStore-Modus (AttachToDataStore).
    /// </summary>
    public class PropertyChangedBinder_DataStoreMode_Tests
    {
        // ==================== AttachToDataStore - Basis ====================

        [Fact]
        public void AttachToDataStore_ThrowsArgumentNullException_WhenDataStoreIsNull()
        {
            // Arrange
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => { });

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => binder.AttachToDataStore(null!));
        }

        [Fact]
        public void AttachToDataStore_BindsExistingItems()
        {
            // Arrange
            var callbackItems = new List<TestEntity>();
            var binder = new PropertyChangedBinder<TestEntity>(true, e => callbackItems.Add(e));
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.Add(entity1);
            store.Add(entity2);

            // Act
            using var subscription = binder.AttachToDataStore(store);
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(2, callbackItems.Count);
            Assert.Contains(entity1, callbackItems);
            Assert.Contains(entity2, callbackItems);
        }

        [Fact]
        public void AttachToDataStore_Disabled_ReturnsEmptyDisposable()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(false, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);

            // Act
            using var subscription = binder.AttachToDataStore(store);
            entity.Name = "Changed";

            // Assert
            Assert.Equal(0, callbackCalls);
        }

        // ==================== CollectionChanged.Add ====================

        [Fact]
        public void AttachToDataStore_BindsNewItemsOnAdd()
        {
            // Arrange
            var callbackItems = new List<TestEntity>();
            var binder = new PropertyChangedBinder<TestEntity>(true, e => callbackItems.Add(e));
            var store = new InMemoryDataStore<TestEntity>();
            using var subscription = binder.AttachToDataStore(store);

            // Act
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            entity.Name = "Changed";

            // Assert
            Assert.Single(callbackItems);
            Assert.Contains(entity, callbackItems);
        }

        [Fact]
        public void AttachToDataStore_BindsMultipleItemsOnAddRange()
        {
            // Arrange
            var callbackItems = new List<TestEntity>();
            var binder = new PropertyChangedBinder<TestEntity>(true, e => callbackItems.Add(e));
            var store = new InMemoryDataStore<TestEntity>();
            using var subscription = binder.AttachToDataStore(store);

            // Act
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.AddRange(new[] { entity1, entity2 });
            
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(2, callbackItems.Count);
            Assert.Contains(entity1, callbackItems);
            Assert.Contains(entity2, callbackItems);
        }

        // ==================== CollectionChanged.Remove ====================

        [Fact]
        public void AttachToDataStore_UnbindsItemOnRemove()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act
            store.Remove(entity);
            entity.Name = "Changed";

            // Assert
            Assert.Equal(0, callbackCalls);
        }

        [Fact]
        public void AttachToDataStore_UnbindsMultipleItemsOnRemoveRange()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.AddRange(new[] { entity1, entity2 });
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act
            store.RemoveRange(new[] { entity1, entity2 });
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(0, callbackCalls);
        }

        // ==================== CollectionChanged.Reset ====================

        [Fact]
        public void AttachToDataStore_UnbindsAllItemsOnClear()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.AddRange(new[] { entity1, entity2 });
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act
            store.Clear();
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(0, callbackCalls);
        }

        // ==================== CollectionChanged.Replace ====================

        [Fact]
        public void AttachToDataStore_HandlesReplace()
        {
            // Arrange
            var callbackItems = new List<TestEntity>();
            var binder = new PropertyChangedBinder<TestEntity>(true, e => callbackItems.Add(e));
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity1);
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act - Simulate Replace (Remove old, Add new)
            store.Remove(entity1);
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.Add(entity2);
            
            entity1.Name = "Changed1"; // Should not trigger (unbound)
            entity2.Name = "Changed2"; // Should trigger (bound)

            // Assert
            Assert.Single(callbackItems);
            Assert.Contains(entity2, callbackItems);
            Assert.DoesNotContain(entity1, callbackItems);
        }

        // ==================== Dispose ====================

        [Fact]
        public void Dispose_UnbindsDataStoreSubscription()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            
            var subscription = binder.AttachToDataStore(store);
            
            // Act
            subscription.Dispose();
            
            var newEntity = new TestEntity { Id = 2, Name = "B" };
            store.Add(newEntity);
            newEntity.Name = "Changed";

            // Assert - New items after dispose should not trigger callback
            Assert.Equal(0, callbackCalls);
        }

        [Fact]
        public void Dispose_Binder_UnbindsAllItems()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            store.AddRange(new[] { entity1, entity2 });
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act
            binder.Dispose();
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(0, callbackCalls);
        }

        // ==================== Idempotenz ====================

        [Fact]
        public void AttachToDataStore_IdempotentAttach_NoDuplicateCallbacks()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            
            // Act - Attach to same store twice (items get attached twice internally)
            using var subscription1 = binder.AttachToDataStore(store);
            binder.Attach(entity); // Manual attach again
            
            entity.Name = "Changed";

            // Assert - Should only call once due to idempotent Attach
            Assert.Equal(1, callbackCalls);
        }

        // ==================== Callback wird aufgerufen ====================

        [Fact]
        public void AttachToDataStore_CallbackReceivesCorrectEntity()
        {
            // Arrange
            TestEntity? receivedEntity = null;
            string? receivedPropertyName = null;
            
            var binder = new PropertyChangedBinder<TestEntity>(true, e => receivedEntity = e);
            var store = new InMemoryDataStore<TestEntity>();
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            
            using var subscription = binder.AttachToDataStore(store);
            
            // Act
            entity.Name = "Changed";

            // Assert
            Assert.NotNull(receivedEntity);
            Assert.Same(entity, receivedEntity);
        }

        // ==================== Multiple Stores ====================

        [Fact]
        public void AttachToDataStore_CanAttachToMultipleStores()
        {
            // Arrange
            var callbackCalls = 0;
            var binder = new PropertyChangedBinder<TestEntity>(true, _ => callbackCalls++);
            
            var store1 = new InMemoryDataStore<TestEntity>();
            var store2 = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            
            store1.Add(entity1);
            store2.Add(entity2);
            
            // Act
            using var sub1 = binder.AttachToDataStore(store1);
            using var sub2 = binder.AttachToDataStore(store2);
            
            entity1.Name = "Changed1";
            entity2.Name = "Changed2";

            // Assert
            Assert.Equal(2, callbackCalls);
        }
    }
}
