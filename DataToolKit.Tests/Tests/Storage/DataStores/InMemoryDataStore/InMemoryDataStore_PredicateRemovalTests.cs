using System;
using DataToolKit.Storage.DataStores;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_PredicateRemovalTests
    {
        [Fact]
        public void RemoveWhere_RemovesMatching()
        {
            var store = new InMemoryDataStore<int>();
            store.AddRange(new[] { 1, 2, 3, 4, 5, 6 });
            var removed = store.RemoveWhere(x => x % 2 == 0);
            Assert.Equal(3, removed); // 2,4,6
            Assert.Equal(3, store.Count); // 1,3,5
        }

        [Fact]
        public void RemoveWhere_NoMatches_ReturnsZero()
        {
            var store = new InMemoryDataStore<int>();
            store.AddRange(new[] { 1, 3, 5 });
            var removed = store.RemoveWhere(x => x % 2 == 0);
            Assert.Equal(0, removed);
            Assert.Equal(3, store.Count);
        }

        [Fact]
        public void RemoveWhere_ThrowsOnNullPredicate()
        {
            var store = new InMemoryDataStore<int>();
            Assert.Throws<ArgumentNullException>(() => store.RemoveWhere(null!));
        }
    }
}
