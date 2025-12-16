using System;
using System.Collections.Generic;
using DataToolKit.Storage.DataStores;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_RangeOperationsTests
    {
        [Fact]
        public void AddRange_AllNew_ReturnsAddedCount()
        {
            var store = new InMemoryDataStore<string>();
            var count = store.AddRange(new[] { "A", "B", "C" });
            Assert.Equal(3, count);
            Assert.Equal(3, store.Count);
        }

        [Fact]
        public void AddRange_SkipsDuplicates()
        {
            var store = new InMemoryDataStore<string>();
            store.Add("A");
            var count = store.AddRange(new[] { "A", "B", "B", "C" });
            Assert.Equal(2, count); // B, C
            Assert.Equal(3, store.Count); // A, B, C
        }

        [Fact]
        public void AddRange_IgnoresNulls()
        {
            var store = new InMemoryDataStore<string>();
            var list = new List<string?> { "A", null, "B", null, "B" };
            var count = store.AddRange(list!);
            Assert.Equal(2, count);
            Assert.Equal(2, store.Count);
        }

        [Fact]
        public void AddRange_ThrowsOnNullEnumerable()
        {
            var store = new InMemoryDataStore<string>();
            Assert.Throws<ArgumentNullException>(() => store.AddRange(null!));
        }

        [Fact]
        public void RemoveRange_RemovesPresent_SkipsMissing()
        {
            var store = new InMemoryDataStore<string>();
            store.AddRange(new[] { "A", "B", "C" });
            var removed = store.RemoveRange(new[] { "B", "X", "C" });
            Assert.Equal(2, removed);
            Assert.Equal(1, store.Count); // A
        }

        [Fact]
        public void RemoveRange_IgnoresNulls()
        {
            var store = new InMemoryDataStore<string>();
            store.AddRange(new[] { "A", "B" });
            var removed = store.RemoveRange(new string?[] { null, "A", null }!);
            Assert.Equal(1, removed);
            Assert.Equal(1, store.Count); // B
        }

        [Fact]
        public void RemoveRange_ThrowsOnNullEnumerable()
        {
            var store = new InMemoryDataStore<string>();
            Assert.Throws<ArgumentNullException>(() => store.RemoveRange(null!));
        }
    }
}
