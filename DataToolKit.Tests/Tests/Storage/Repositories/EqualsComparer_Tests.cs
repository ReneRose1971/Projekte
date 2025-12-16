using Common.Bootstrap.Defaults;
using System;
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für den FallbackEqualsComparer&lt;T&gt; (Fallback-Comparer).
    /// </summary>
    public sealed class FallbackEqualsComparer_Tests
    {
        [Fact]
        public void FallbackEqualsComparer_Should_Return_True_On_Both_Null()
        {
            var cmp = new FallbackEqualsComparer<string?>();
            Assert.True(cmp.Equals(null, null));
        }

        [Fact]
        public void FallbackEqualsComparer_Should_Return_False_On_One_Null()
        {
            var cmp = new FallbackEqualsComparer<string?>();
            Assert.False(cmp.Equals("x", null));
            Assert.False(cmp.Equals(null, "x"));
        }

        [Fact]
        public void FallbackEqualsComparer_Should_Delegate_To_Equals()
        {
            var a = new Obj("A", 1);
            var b = new Obj("A", 1);
            var c = new Obj("A", 2);

            var cmp = new FallbackEqualsComparer<Obj>();
            Assert.True(cmp.Equals(a, b));   // same content per Equals
            Assert.False(cmp.Equals(a, c));  // different content per Equals
        }

        [Fact]
        public void FallbackEqualsComparer_Should_Delegate_GetHashCode_And_Throw_On_Null()
        {
            var cmp = new FallbackEqualsComparer<Obj>();
            var a = new Obj("A", 1);

            var _ = cmp.GetHashCode(a);    // delegiert an a.GetHashCode()

            Assert.Throws<ArgumentNullException>(() => cmp.GetHashCode(null!));
        }

        private sealed class Obj : IEquatable<Obj>
        {
            public string Name { get; }
            public int Value { get; }

            public Obj(string name, int value)
            {
                Name = name;
                Value = value;
            }

            public bool Equals(Obj? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name, StringComparison.Ordinal) && Value == other.Value;
            }

            public override bool Equals(object? obj) => obj is Obj o && Equals(o);
            public override int GetHashCode() => HashCode.Combine(Name, Value);
        }
    }
}
