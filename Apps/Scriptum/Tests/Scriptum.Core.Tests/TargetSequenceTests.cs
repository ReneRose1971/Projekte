using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class TargetSequenceTests
{
    [Fact]
    public void Constructor_With_Symbols_Sets_Symbols()
    {
        var symbols = new List<TargetSymbol>
        {
            new(0, "a"),
            new(1, "b")
        };
        var sequence = new TargetSequence(symbols);
        
        sequence.Symbols.Should().BeEquivalentTo(symbols);
    }
    
    [Fact]
    public void Constructor_With_Symbols_Sets_Length()
    {
        var symbols = new List<TargetSymbol>
        {
            new(0, "a"),
            new(1, "b"),
            new(2, "c")
        };
        var sequence = new TargetSequence(symbols);
        
        sequence.Length.Should().Be(3);
    }
    
    [Fact]
    public void Constructor_With_Graphemes_Creates_Symbols()
    {
        var graphemes = new[] { "x", "y", "z" };
        var sequence = new TargetSequence(graphemes);
        
        sequence.Symbols.Should().HaveCount(3);
    }
    
    [Fact]
    public void Constructor_With_Graphemes_Sets_Correct_Indices()
    {
        var graphemes = new[] { "a", "b" };
        var sequence = new TargetSequence(graphemes);
        
        sequence.Symbols[0].Index.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_With_Graphemes_Sets_Correct_Second_Index()
    {
        var graphemes = new[] { "a", "b" };
        var sequence = new TargetSequence(graphemes);
        
        sequence.Symbols[1].Index.Should().Be(1);
    }
    
    [Fact]
    public void Constructor_With_Graphemes_Sets_Correct_Graphem()
    {
        var graphemes = new[] { "m" };
        var sequence = new TargetSequence(graphemes);
        
        sequence.Symbols[0].Graphem.Should().Be("m");
    }
    
    [Fact]
    public void Constructor_Throws_For_Null_Symbols()
    {
        var act = () => new TargetSequence((IReadOnlyList<TargetSymbol>)null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("symbols");
    }
    
    [Fact]
    public void Constructor_Throws_For_Empty_Symbols_List()
    {
        var symbols = new List<TargetSymbol>();
        var act = () => new TargetSequence(symbols);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbols");
    }
    
    [Fact]
    public void Constructor_Throws_For_Null_Graphemes()
    {
        var act = () => new TargetSequence((IEnumerable<string>)null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("graphemes");
    }
    
    [Fact]
    public void Constructor_Throws_For_Empty_Graphemes_List()
    {
        var graphemes = Array.Empty<string>();
        var act = () => new TargetSequence(graphemes);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("graphemes");
    }
    
    [Fact]
    public void Constructor_Throws_For_Inconsistent_First_Index()
    {
        var symbols = new List<TargetSymbol>
        {
            new(1, "a"),
            new(1, "b")
        };
        var act = () => new TargetSequence(symbols);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbols");
    }
    
    [Fact]
    public void Constructor_Throws_For_Inconsistent_Second_Index()
    {
        var symbols = new List<TargetSymbol>
        {
            new(0, "a"),
            new(5, "b")
        };
        var act = () => new TargetSequence(symbols);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbols");
    }
    
    [Fact]
    public void Constructor_Throws_For_Gap_In_Indices()
    {
        var symbols = new List<TargetSymbol>
        {
            new(0, "a"),
            new(1, "b"),
            new(3, "c")
        };
        var act = () => new TargetSequence(symbols);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbols");
    }
    
    [Fact]
    public void Symbols_Property_Returns_ReadOnly_List()
    {
        var graphemes = new[] { "a", "b" };
        var sequence = new TargetSequence(graphemes);
        
        sequence.Symbols.Should().BeAssignableTo<IReadOnlyList<TargetSymbol>>();
    }
}
