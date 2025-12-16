using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class CollectionHelpersTests
    {
        private sealed class Mod10Comparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y) => (x % 10) == (y % 10);
            public int GetHashCode(int obj) => (obj % 10).GetHashCode();
        }

        // --------------------------------------------------------------------
        // ContainsWithComparer
        // --------------------------------------------------------------------

        [Fact]
        public void ContainsWithComparer_FindsMatchingItem()
        {
            var list = new List<int> { 1, 2, 3, 14 };
            var result = CollectionHelpers.ContainsWithComparer(list, 4, new Mod10Comparer());
            Assert.True(result); // 14 % 10 == 4 % 10
        }

        [Fact]
        public void ContainsWithComparer_ReturnsFalse_WhenNotFound()
        {
            var list = new List<int> { 1, 2, 3, 14 };
            var result = CollectionHelpers.ContainsWithComparer(list, 7, new Mod10Comparer());
            Assert.False(result);
        }

        [Fact]
        public void ContainsWithComparer_Throws_WhenSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CollectionHelpers.ContainsWithComparer<int>(null!, 5, EqualityComparer<int>.Default));
        }

        [Fact]
        public void ContainsWithComparer_Throws_WhenComparerIsNull()
        {
            var list = new List<int> { 1, 2, 3 };
            Assert.Throws<ArgumentNullException>(() =>
                CollectionHelpers.ContainsWithComparer(list, 1, null!));
        }


        // --------------------------------------------------------------------
        // IndexOfWithComparer
        // --------------------------------------------------------------------

        [Fact]
        public void IndexOfWithComparer_ReturnsCorrectIndex()
        {
            var list = new List<int> { 10, 21, 32, 43 };
            var index = CollectionHelpers.IndexOfWithComparer(list, 2, new Mod10Comparer());
            Assert.Equal(2, index); // 32 % 10 == 2 % 10
        }


        [Fact]
        public void IndexOfWithComparer_ReturnsMinusOne_WhenNotFound()
        {
            var list = new List<int> { 10, 21, 32, 43 };
            var index = CollectionHelpers.IndexOfWithComparer(list, 7, new Mod10Comparer());
            Assert.Equal(-1, index);
        }

        [Fact]
        public void IndexOfWithComparer_Throws_WhenListIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                CollectionHelpers.IndexOfWithComparer<int>(null!, 5, EqualityComparer<int>.Default));
        }

        [Fact]
        public void IndexOfWithComparer_Throws_WhenComparerIsNull()
        {
            var list = new List<int> { 10, 20, 30 };
            Assert.Throws<ArgumentNullException>(() =>
                CollectionHelpers.IndexOfWithComparer(list, 10, null!));
        }
    }
}
