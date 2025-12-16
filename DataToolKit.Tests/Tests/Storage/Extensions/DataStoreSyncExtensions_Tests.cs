using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Extensions;
using DataToolKit.Tests.Testing;

namespace DataToolKit.Tests.Storage.Extensions
{
    /// <summary>
    /// Tests für DataStoreSyncExtensions.SyncWith - Vollständige Collection-Synchronisation.
    /// </summary>
    public class DataStoreSyncExtensions_Tests
    {
        // ==================== Parameter Validation ====================

        [Fact]
        public void SyncWith_ThrowsArgumentNullException_WhenTargetIsNull()
        {
            // Arrange
            InMemoryDataStore<TestEntity>? target = null;
            var source = new InMemoryDataStore<TestEntity>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => target!.SyncWith(source));
        }

        [Fact]
        public void SyncWith_ThrowsArgumentNullException_WhenSourceIsNull()
        {
            // Arrange
            var target = new InMemoryDataStore<TestEntity>();
            InMemoryDataStore<TestEntity>? source = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => target.SyncWith(source!));
        }

        // ==================== Initial Synchronization ====================

        [Fact]
        public void SyncWith_InitialSync_CopiesAllItemsFromSource()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            var entity3 = new TestEntity { Id = 3, Name = "C" };
            
            source.AddRange(new[] { entity1, entity2, entity3 });

            // Act
            using var sync = target.SyncWith(source);

            // Assert
            Assert.Equal(3, target.Count);
            Assert.Contains(entity1, target.Items);
            Assert.Contains(entity2, target.Items);
            Assert.Contains(entity3, target.Items);
        }

        [Fact]
        public void SyncWith_InitialSync_EmptySource_TargetRemainsEmpty()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();

            // Act
            using var sync = target.SyncWith(source);

            // Assert
            Assert.Equal(0, target.Count);
        }

        [Fact]
        public void SyncWith_InitialSync_TargetWithExistingItems_AddsSourceItems()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var existing = new TestEntity { Id = 99, Name = "Existing" };
            target.Add(existing);
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity1);

            // Act
            using var sync = target.SyncWith(source);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Contains(existing, target.Items);
            Assert.Contains(entity1, target.Items);
        }

        // ==================== CollectionChanged.Add ====================

        [Fact]
        public void SyncWith_Add_SingleItem_AddsToTarget()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            using var sync = target.SyncWith(source);

            // Act
            var entity = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity);

            // Assert
            Assert.Single(target.Items);
            Assert.Contains(entity, target.Items);
        }

        [Fact]
        public void SyncWith_AddRange_MultipleItems_AddsAllToTarget()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            using var sync = target.SyncWith(source);

            // Act
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            var entity3 = new TestEntity { Id = 3, Name = "C" };
            source.AddRange(new[] { entity1, entity2, entity3 });

            // Assert
            Assert.Equal(3, target.Count);
            Assert.Contains(entity1, target.Items);
            Assert.Contains(entity2, target.Items);
            Assert.Contains(entity3, target.Items);
        }

        [Fact]
        public void SyncWith_Add_DuplicateItem_TargetSkipsDuplicate()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            using var sync = target.SyncWith(source);
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity);

            // Act - Try to add same item again
            source.Add(entity); // Should be skipped by source (no duplicate)

            // Assert
            Assert.Single(target.Items);
        }

        // ==================== CollectionChanged.Remove ====================

        [Fact]
        public void SyncWith_Remove_SingleItem_RemovesFromTarget()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity);
            
            using var sync = target.SyncWith(source);

            // Act
            source.Remove(entity);

            // Assert
            Assert.Empty(target.Items);
        }

        [Fact]
        public void SyncWith_RemoveRange_MultipleItems_RemovesAllFromTarget()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            var entity3 = new TestEntity { Id = 3, Name = "C" };
            source.AddRange(new[] { entity1, entity2, entity3 });
            
            using var sync = target.SyncWith(source);

            // Act
            source.RemoveRange(new[] { entity1, entity3 });

            // Assert
            Assert.Single(target.Items);
            Assert.Contains(entity2, target.Items);
            Assert.DoesNotContain(entity1, target.Items);
            Assert.DoesNotContain(entity3, target.Items);
        }

        [Fact]
        public void SyncWith_Remove_NonExistentItem_TargetUnchanged()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            
            source.Add(entity1);
            using var sync = target.SyncWith(source);

            // Act - Try to remove item not in source
            source.Remove(entity2); // Does nothing

            // Assert
            Assert.Single(target.Items);
            Assert.Contains(entity1, target.Items);
        }

        // ==================== CollectionChanged.Reset (Clear) ====================

        [Fact]
        public void SyncWith_Clear_RemovesAllFromTarget()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            source.AddRange(new[] { entity1, entity2 });
            
            using var sync = target.SyncWith(source);

            // Act
            source.Clear();

            // Assert
            Assert.Empty(target.Items);
        }

        [Fact]
        public void SyncWith_Clear_ThenAdd_ResynchronizesCorrectly()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity1);
            
            using var sync = target.SyncWith(source);
            source.Clear();

            // Act
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            source.Add(entity2);

            // Assert
            Assert.Single(target.Items);
            Assert.Contains(entity2, target.Items);
            Assert.DoesNotContain(entity1, target.Items);
        }

        // ==================== CollectionChanged.Replace ====================

        [Fact]
        public void SyncWith_Replace_RemovesOldAndAddsNew()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity1);
            
            using var sync = target.SyncWith(source);

            // Act - Simulate Replace (Remove + Add)
            source.Remove(entity1);
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            source.Add(entity2);

            // Assert
            Assert.Single(target.Items);
            Assert.Contains(entity2, target.Items);
            Assert.DoesNotContain(entity1, target.Items);
        }

        // ==================== Multiple Operations ====================

        [Fact]
        public void SyncWith_MultipleOperations_MaintainsSynchronization()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            using var sync = target.SyncWith(source);

            // Act - Complex sequence
            var e1 = new TestEntity { Id = 1, Name = "A" };
            var e2 = new TestEntity { Id = 2, Name = "B" };
            var e3 = new TestEntity { Id = 3, Name = "C" };
            
            source.Add(e1);                           // [1]
            source.AddRange(new[] { e2, e3 });        // [1,2,3]
            source.Remove(e2);                        // [1,3]
            var e4 = new TestEntity { Id = 4, Name = "D" };
            source.Add(e4);                           // [1,3,4]
            source.Remove(e1);                        // [3,4]

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Contains(e3, target.Items);
            Assert.Contains(e4, target.Items);
            Assert.DoesNotContain(e1, target.Items);
            Assert.DoesNotContain(e2, target.Items);
        }

        // ==================== Dispose ====================

        [Fact]
        public void Dispose_StopsSynchronization()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entity1 = new TestEntity { Id = 1, Name = "A" };
            source.Add(entity1);
            
            var sync = target.SyncWith(source);
            
            // Act
            sync.Dispose();
            
            var entity2 = new TestEntity { Id = 2, Name = "B" };
            source.Add(entity2);

            // Assert - Target should not receive new item after dispose
            Assert.Single(target.Items);
            Assert.Contains(entity1, target.Items);
            Assert.DoesNotContain(entity2, target.Items);
        }

        [Fact]
        public void Dispose_Idempotent_CanBeCalledMultipleTimes()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            var sync = target.SyncWith(source);

            // Act & Assert - Should not throw
            sync.Dispose();
            sync.Dispose();
            sync.Dispose();
        }

        // ==================== Custom Comparer ====================

        [Fact]
        public void SyncWith_WithComparer_UsesComparerForEquality()
        {
            // Arrange
            var comparer = new TestEntityIdComparer();
            var source = new InMemoryDataStore<TestEntity>(comparer);
            var target = new InMemoryDataStore<TestEntity>(comparer);
            
            var entity1 = new TestEntity { Id = 1, Name = "Original" };
            source.Add(entity1);
            
            using var sync = target.SyncWith(source, comparer);

            // Act - Add entity with same Id but different Name
            var entity2 = new TestEntity { Id = 1, Name = "Modified" };
            source.Add(entity2); // Should be treated as duplicate by comparer

            // Assert - Target should only have one item (comparer treats same Id as equal)
            Assert.Single(target.Items);
        }

        // ==================== Edge Cases ====================

        [Fact]
        public void SyncWith_SourceAndTargetAreSame_WorksCorrectly()
        {
            // Arrange
            var store = new InMemoryDataStore<TestEntity>();
            
            // Act & Assert - Should not throw, but this is a strange use case
            using var sync = store.SyncWith(store);
            
            var entity = new TestEntity { Id = 1, Name = "A" };
            store.Add(entity);
            
            // Should only appear once (not duplicated)
            Assert.Single(store.Items);
        }

        [Fact]
        public void SyncWith_LargeDataSet_SynchronizesCorrectly()
        {
            // Arrange
            var source = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var entities = Enumerable.Range(1, 100)
                .Select(i => new TestEntity { Id = i, Name = $"Entity{i}" })
                .ToList();
            
            source.AddRange(entities);
            
            // Act
            using var sync = target.SyncWith(source);

            // Assert
            Assert.Equal(100, target.Count);
            Assert.All(entities, e => Assert.Contains(e, target.Items));
        }

        [Fact]
        public void SyncWith_MultipleSyncsToSameTarget_AllWorkIndependently()
        {
            // Arrange
            var source1 = new InMemoryDataStore<TestEntity>();
            var source2 = new InMemoryDataStore<TestEntity>();
            var target = new InMemoryDataStore<TestEntity>();
            
            var e1 = new TestEntity { Id = 1, Name = "A" };
            var e2 = new TestEntity { Id = 2, Name = "B" };
            
            source1.Add(e1);
            source2.Add(e2);
            
            // Act
            using var sync1 = target.SyncWith(source1);
            using var sync2 = target.SyncWith(source2);

            // Assert
            Assert.Equal(2, target.Count);
            Assert.Contains(e1, target.Items);
            Assert.Contains(e2, target.Items);
        }
    }

    /// <summary>
    /// Comparer that considers TestEntities equal if they have the same Id.
    /// </summary>
    internal class TestEntityIdComparer : IEqualityComparer<TestEntity>
    {
        public bool Equals(TestEntity? x, TestEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(TestEntity obj) => obj.Id.GetHashCode();
    }
}
