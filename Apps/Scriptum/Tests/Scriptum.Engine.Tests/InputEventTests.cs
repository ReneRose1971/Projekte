using FluentAssertions;
using Scriptum.Core;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class InputEventTests
{
    [Fact]
    public void Constructor_WithValidZeichenParameters_ShouldCreateInstance()
    {
        var timestamp = DateTime.Now;
        var chord = new KeyChord(KeyId.A);
        var graphem = "a";
        
        var inputEvent = new InputEvent(timestamp, chord, InputEventKind.Zeichen, graphem);
        
        inputEvent.Timestamp.Should().Be(timestamp);
        inputEvent.Chord.Should().Be(chord);
        inputEvent.Kind.Should().Be(InputEventKind.Zeichen);
        inputEvent.Graphem.Should().Be(graphem);
    }
    
    [Fact]
    public void Constructor_WithValidRuecktasteParameters_ShouldCreateInstance()
    {
        var timestamp = DateTime.Now;
        var chord = new KeyChord(KeyId.Backspace);
        
        var inputEvent = new InputEvent(timestamp, chord, InputEventKind.Ruecktaste);
        
        inputEvent.Timestamp.Should().Be(timestamp);
        inputEvent.Chord.Should().Be(chord);
        inputEvent.Kind.Should().Be(InputEventKind.Ruecktaste);
        inputEvent.Graphem.Should().BeNull();
    }
    
    [Fact]
    public void Constructor_WithValidIgnoriertParameters_ShouldCreateInstance()
    {
        var timestamp = DateTime.Now;
        var chord = new KeyChord(KeyId.LeftCtrl);
        
        var inputEvent = new InputEvent(timestamp, chord, InputEventKind.Ignoriert);
        
        inputEvent.Timestamp.Should().Be(timestamp);
        inputEvent.Chord.Should().Be(chord);
        inputEvent.Kind.Should().Be(InputEventKind.Ignoriert);
        inputEvent.Graphem.Should().BeNull();
    }
    
    [Fact]
    public void Constructor_WithKindZeichenAndNullGraphem_ShouldThrowArgumentException()
    {
        var timestamp = DateTime.Now;
        var chord = new KeyChord(KeyId.A);
        
        var act = () => new InputEvent(timestamp, chord, InputEventKind.Zeichen, null);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("graphem");
    }
    
    [Fact]
    public void Constructor_WithKindZeichenAndEmptyGraphem_ShouldThrowArgumentException()
    {
        var timestamp = DateTime.Now;
        var chord = new KeyChord(KeyId.A);
        
        var act = () => new InputEvent(timestamp, chord, InputEventKind.Zeichen, string.Empty);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("graphem");
    }
}
