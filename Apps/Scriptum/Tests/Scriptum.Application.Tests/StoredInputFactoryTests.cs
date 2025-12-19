using FluentAssertions;
using Scriptum.Application.Factories;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;
using Xunit;

namespace Scriptum.Application.Tests;

public sealed class StoredInputFactoryTests
{
    [Fact]
    public void FromInputEvent_ShouldCreateStoredInputWithCorrectZeitpunkt()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Zeichen, "a");

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Zeitpunkt.Should().Be(timestamp);
    }

    [Fact]
    public void FromInputEvent_ShouldCreateStoredInputWithCorrectTaste()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Zeichen, "a");

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Taste.Should().Be(KeyId.A);
    }

    [Fact]
    public void FromInputEvent_ShouldCreateStoredInputWithCorrectUmschalter()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.Shift);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Zeichen, "A");

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Umschalter.Should().Be(ModifierSet.Shift);
    }

    [Fact]
    public void FromInputEvent_WithZeichen_ShouldMapToZeichen()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Zeichen, "a");

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Art.Should().Be(StoredInputKind.Zeichen);
    }

    [Fact]
    public void FromInputEvent_WithRuecktaste_ShouldMapToRuecktaste()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.Backspace, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Ruecktaste);

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Art.Should().Be(StoredInputKind.Ruecktaste);
    }

    [Fact]
    public void FromInputEvent_WithIgnoriert_ShouldMapToIgnoriert()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.Escape, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Ignoriert);

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.Art.Should().Be(StoredInputKind.Ignoriert);
    }

    [Fact]
    public void FromInputEvent_WithGraphem_ShouldStoreGraphem()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Zeichen, "a");

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.ErzeugtesGraphem.Should().Be("a");
    }

    [Fact]
    public void FromInputEvent_WithNullGraphem_ShouldStoreEmptyString()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.Backspace, ModifierSet.None);
        var inputEvent = new InputEvent(DateTime.Now, chord, InputEventKind.Ruecktaste);

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, timestamp);

        storedInput.ErzeugtesGraphem.Should().BeEmpty();
    }

    [Fact]
    public void FromInputEvent_WithNullInputEvent_ShouldThrowArgumentNullException()
    {
        var timestamp = DateTimeOffset.Now;
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        var act = () => StoredInputFactory.FromInputEvent(null!, chord, timestamp);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("inputEvent");
    }
}
