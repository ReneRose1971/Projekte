using FluentAssertions;
using Scriptum.Core;
using Xunit;

namespace Scriptum.Core.Tests;

public sealed class TargetSequenceFactoryTests
{
    [Fact]
    public void FromText_ShouldCreateSequenceWithCorrectLength()
    {
        var text = "abc";

        var sequence = TargetSequence.FromText(text);

        sequence.Length.Should().Be(3);
    }

    [Fact]
    public void FromText_ShouldCreateSymbolsForEachCharacter()
    {
        var text = "abc";

        var sequence = TargetSequence.FromText(text);

        sequence.Symbols.Should().HaveCount(3);
        sequence.Symbols[0].Graphem.Should().Be("a");
        sequence.Symbols[1].Graphem.Should().Be("b");
        sequence.Symbols[2].Graphem.Should().Be("c");
    }

    [Fact]
    public void FromText_ShouldAssignCorrectIndices()
    {
        var text = "abc";

        var sequence = TargetSequence.FromText(text);

        sequence.Symbols[0].Index.Should().Be(0);
        sequence.Symbols[1].Index.Should().Be(1);
        sequence.Symbols[2].Index.Should().Be(2);
    }

    [Fact]
    public void FromText_WithSpecialCharacters_ShouldCreateCorrectSymbols()
    {
        var text = "ä ö ü";

        var sequence = TargetSequence.FromText(text);

        sequence.Length.Should().Be(5);
        sequence.Symbols[0].Graphem.Should().Be("ä");
        sequence.Symbols[1].Graphem.Should().Be(" ");
        sequence.Symbols[2].Graphem.Should().Be("ö");
        sequence.Symbols[3].Graphem.Should().Be(" ");
        sequence.Symbols[4].Graphem.Should().Be("ü");
    }

    [Fact]
    public void FromText_WithSingleCharacter_ShouldCreateSingleSymbol()
    {
        var text = "x";

        var sequence = TargetSequence.FromText(text);

        sequence.Length.Should().Be(1);
        sequence.Symbols[0].Graphem.Should().Be("x");
        sequence.Symbols[0].Index.Should().Be(0);
    }

    [Fact]
    public void FromText_WithEmptyString_ShouldThrowArgumentException()
    {
        var text = "";

        var act = () => TargetSequence.FromText(text);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("text")
            .WithMessage("*darf nicht leer sein*");
    }

    [Fact]
    public void FromText_WithWhitespaceOnly_ShouldThrowArgumentException()
    {
        var text = "   ";

        var act = () => TargetSequence.FromText(text);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("text")
            .WithMessage("*darf nicht leer sein*");
    }

    [Fact]
    public void FromText_WithNull_ShouldThrowArgumentException()
    {
        string? text = null;

        var act = () => TargetSequence.FromText(text!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("text")
            .WithMessage("*darf nicht leer sein*");
    }

    [Fact]
    public void FromText_WithNewlineCharacters_ShouldIncludeNewlines()
    {
        var text = "a\nb";

        var sequence = TargetSequence.FromText(text);

        sequence.Length.Should().Be(3);
        sequence.Symbols[0].Graphem.Should().Be("a");
        sequence.Symbols[1].Graphem.Should().Be("\n");
        sequence.Symbols[2].Graphem.Should().Be("b");
    }

    [Fact]
    public void FromText_WithTabCharacters_ShouldIncludeTabs()
    {
        var text = "a\tb";

        var sequence = TargetSequence.FromText(text);

        sequence.Length.Should().Be(3);
        sequence.Symbols[0].Graphem.Should().Be("a");
        sequence.Symbols[1].Graphem.Should().Be("\t");
        sequence.Symbols[2].Graphem.Should().Be("b");
    }
}
