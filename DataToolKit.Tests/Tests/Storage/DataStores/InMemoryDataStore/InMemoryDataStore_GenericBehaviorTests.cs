using System;
using System.Collections.ObjectModel;
using DataToolKit.Storage.DataStores; // InMemoryDataStore<T>
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_GenericBehaviorTests
    {
        [Fact]
        public void Items_IsReadOnlyView()
        {
            var store = new InMemoryDataStore<string>();
            Assert.IsType<ReadOnlyObservableCollection<string>>(store.Items);
        }

        [Fact]
        public void Count_InitialIsZero()
        {
            var store = new InMemoryDataStore<string>();
            Assert.Equal(0, store.Count);
            Assert.Empty(store.Items);
        }

        [Fact]
        public void Add_AddsWhenNotPresent()
        {
            var store = new InMemoryDataStore<string>();
            var added = store.Add("A");
            Assert.True(added);
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void Add_SameElementTwice_ReturnsFalse_DoesNotDuplicate()
        {
            var store = new InMemoryDataStore<string>();
            Assert.True(store.Add("A"));
            Assert.False(store.Add("A"));
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void Add_ThrowsOnNull()
        {
            var store = new InMemoryDataStore<string>();
            Assert.Throws<ArgumentNullException>(() => store.Add(null!));
        }

        [Fact]
        public void Remove_RemovesWhenPresent()
        {
            var store = new InMemoryDataStore<string>();
            store.Add("A");
            var removed = store.Remove("A");
            Assert.True(removed);
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void Remove_ReturnsFalseWhenMissing()
        {
            var store = new InMemoryDataStore<string>();
            store.Add("A");
            var removed = store.Remove("B");
            Assert.False(removed);
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void Remove_ThrowsOnNull()
        {
            var store = new InMemoryDataStore<string>();
            Assert.Throws<ArgumentNullException>(() => store.Remove(null!));
        }

        [Fact]
        public void Clear_EmptiesCollection()
        {
            var store = new InMemoryDataStore<string>();
            store.Add("A");
            store.Add("B");
            store.Clear();
            Assert.Equal(0, store.Count);
            Assert.Empty(store.Items);
        }
    }
}
