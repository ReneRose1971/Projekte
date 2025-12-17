using FluentAssertions;
using Xunit;

namespace Scriptum.Core.Tests;

public class KeyChordTests
{
    [Fact]
    public void Constructor_With_Key_Sets_Key()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.Shift);
        chord.Key.Should().Be(KeyId.A);
    }
    
    [Fact]
    public void Constructor_With_Key_Sets_Modifiers()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.Shift);
        chord.Modifiers.Should().Be(ModifierSet.Shift);
    }
    
    [Fact]
    public void Constructor_With_Key_Only_Defaults_To_None()
    {
        var chord = new KeyChord(KeyId.B);
        chord.Modifiers.Should().Be(ModifierSet.None);
    }
    
    [Fact]
    public void Two_Identical_Chords_Are_Equal()
    {
        var chord1 = new KeyChord(KeyId.C, ModifierSet.AltGr);
        var chord2 = new KeyChord(KeyId.C, ModifierSet.AltGr);
        chord1.Should().Be(chord2);
    }
    
    [Fact]
    public void Two_Different_Keys_Are_Not_Equal()
    {
        var chord1 = new KeyChord(KeyId.A);
        var chord2 = new KeyChord(KeyId.B);
        chord1.Should().NotBe(chord2);
    }
    
    [Fact]
    public void Two_Different_Modifiers_Are_Not_Equal()
    {
        var chord1 = new KeyChord(KeyId.A, ModifierSet.None);
        var chord2 = new KeyChord(KeyId.A, ModifierSet.Shift);
        chord1.Should().NotBe(chord2);
    }
    
    [Fact]
    public void Identical_Chords_Have_Same_HashCode()
    {
        var chord1 = new KeyChord(KeyId.D, ModifierSet.Shift);
        var chord2 = new KeyChord(KeyId.D, ModifierSet.Shift);
        chord1.GetHashCode().Should().Be(chord2.GetHashCode());
    }
}
