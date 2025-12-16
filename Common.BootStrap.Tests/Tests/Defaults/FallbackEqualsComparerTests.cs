using Common.Bootstrap.Defaults;
using System;
using Xunit;

namespace Common.BootStrap.Tests.Defaults;

/// <summary>
/// Tests für den FallbackEqualsComparer&lt;T&gt;.
/// </summary>
public sealed class FallbackEqualsComparerTests
{
    [Fact]
    public void Equals_Returns_True_On_Both_Null()
    {
        var comparer = new FallbackEqualsComparer<string?>();
        Assert.True(comparer.Equals(null, null));
    }

    [Fact]
    public void Equals_Returns_False_On_One_Null()
    {
        var comparer = new FallbackEqualsComparer<string?>();
        Assert.False(comparer.Equals("test", null));
        Assert.False(comparer.Equals(null, "test"));
    }

    [Fact]
    public void Equals_Returns_True_For_Same_Reference()
    {
        var comparer = new FallbackEqualsComparer<object>();
        var obj = new object();
        Assert.True(comparer.Equals(obj, obj));
    }

    [Fact]
    public void Equals_Delegates_To_Object_Equals()
    {
        var comparer = new FallbackEqualsComparer<TestObject>();
        
        var obj1 = new TestObject("A", 1);
        var obj2 = new TestObject("A", 1);
        var obj3 = new TestObject("A", 2);

        Assert.True(comparer.Equals(obj1, obj2));   // same content per Equals
        Assert.False(comparer.Equals(obj1, obj3));  // different content
    }

    [Fact]
    public void GetHashCode_Delegates_To_Object_GetHashCode()
    {
        var comparer = new FallbackEqualsComparer<TestObject>();
        var obj = new TestObject("Test", 42);

        var hash = comparer.GetHashCode(obj);
        
        Assert.Equal(obj.GetHashCode(), hash);
    }

    [Fact]
    public void GetHashCode_Throws_On_Null()
    {
        var comparer = new FallbackEqualsComparer<TestObject>();
        Assert.Throws<ArgumentNullException>(() => comparer.GetHashCode(null!));
    }

    [Fact]
    public void Works_With_Value_Types()
    {
        var comparer = new FallbackEqualsComparer<int>();
        
        Assert.True(comparer.Equals(42, 42));
        Assert.False(comparer.Equals(42, 43));
        Assert.Equal(42.GetHashCode(), comparer.GetHashCode(42));
    }

    [Fact]
    public void Works_With_Strings()
    {
        var comparer = new FallbackEqualsComparer<string>();
        
        Assert.True(comparer.Equals("test", "test"));
        Assert.False(comparer.Equals("test", "TEST")); // Case-sensitive per default
        Assert.Equal("test".GetHashCode(), comparer.GetHashCode("test"));
    }

    private sealed class TestObject : IEquatable<TestObject>
    {
        public string Name { get; }
        public int Value { get; }

        public TestObject(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(TestObject? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.Ordinal) && Value == other.Value;
        }

        public override bool Equals(object? obj) => obj is TestObject o && Equals(o);
        public override int GetHashCode() => HashCode.Combine(Name, Value);
    }
}
