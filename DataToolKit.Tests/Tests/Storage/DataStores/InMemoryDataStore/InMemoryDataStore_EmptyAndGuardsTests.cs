using System;
using System.Collections.Generic;
using DataToolKit.Storage.DataStores;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_EmptyAndGuardsTests
    {
        [Fact]
        public void Remove_OnEmpty_ReturnsFalse()
        {
            var store = new InMemoryDataStore<string>();
            var result = store.Remove("X");
            Assert.False(result);
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void RemoveRange_OnEmpty_ReturnsZero()
        {
            var store = new InMemoryDataStore<string>();
            var removed = store.RemoveRange(new[] { "A", "B" });
            Assert.Equal(0, removed);
        }

        [Fact]
        public void RemoveWhere_OnEmpty_ReturnsZero()
        {
            var store = new InMemoryDataStore<int>();
            var removed = store.RemoveWhere(_ => true);
            Assert.Equal(0, removed);
        }

        [Fact]
        public void Clear_OnEmpty_NoThrow()
        {
            var store = new InMemoryDataStore<string>();
            store.Clear();
            Assert.Equal(0, store.Count);
        }

        [Fact]
        public void Items_ReflectsAfterMixedOperations()
        {
            var store = new InMemoryDataStore<string>();
            store.AddRange(new[] { "A", "B", "C" });
            store.Remove("B");
            store.Add("D");
            store.RemoveWhere(x => x == "A");

            Assert.Equal(2, store.Count); // C, D
            Assert.Contains("C", store.Items);
            Assert.Contains("D", store.Items);
        }

        [Fact]
        public void AddRange_WithMany_DeduplicatesByDefaultComparer()
        {
            var store = new InMemoryDataStore<string>();
            var data = new List<string>();
            for (int i = 0; i < 100; i++) data.Add("X"); // 100 gleiche Einträge

            var added = store.AddRange(data);
            Assert.Equal(1, added);
            Assert.Equal(1, store.Count);
        }
    }
}
