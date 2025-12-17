using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class TargetSymbolTests
{
    [Fact]
    public void Constructor_Sets_Index()
    {
        var symbol = new TargetSymbol(5, "a");
        symbol.Index.Should().Be(5);
    }
    
    [Fact]
    public void Constructor_Sets_Graphem()
    {
        var symbol = new TargetSymbol(0, "x");
        symbol.Graphem.Should().Be("x");
    }
    
    [Fact]
    public void Constructor_Accepts_Newline_Graphem()
    {
        var symbol = new TargetSymbol(0, "\n");
        symbol.Graphem.Should().Be("\n");
    }
    
    [Fact]
    public void Constructor_Throws_For_Negative_Index()
    {
        var act = () => new TargetSymbol(-1, "a");
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("index");
    }
    
    [Fact]
    public void Constructor_Throws_For_Null_Graphem()
    {
        var act = () => new TargetSymbol(0, null!);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("graphem");
    }
    
    [Fact]
    public void Constructor_Throws_For_Empty_Graphem()
    {
        var act = () => new TargetSymbol(0, string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("graphem");
    }
    
    [Fact]
    public void Two_Identical_Symbols_Are_Equal()
    {
        var symbol1 = new TargetSymbol(2, "b");
        var symbol2 = new TargetSymbol(2, "b");
        symbol1.Should().Be(symbol2);
    }
    
    [Fact]
    public void Two_Different_Indices_Are_Not_Equal()
    {
        var symbol1 = new TargetSymbol(1, "a");
        var symbol2 = new TargetSymbol(2, "a");
        symbol1.Should().NotBe(symbol2);
    }
    
    [Fact]
    public void Two_Different_Graphems_Are_Not_Equal()
    {
        var symbol1 = new TargetSymbol(0, "a");
        var symbol2 = new TargetSymbol(0, "b");
        symbol1.Should().NotBe(symbol2);
    }
    
    [Fact]
    public void Identical_Symbols_Have_Same_HashCode()
    {
        var symbol1 = new TargetSymbol(3, "c");
        var symbol2 = new TargetSymbol(3, "c");
        symbol1.GetHashCode().Should().Be(symbol2.GetHashCode());
    }
}
