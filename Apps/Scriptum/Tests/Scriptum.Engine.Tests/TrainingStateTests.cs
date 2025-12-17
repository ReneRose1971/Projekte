using FluentAssertions;
using Scriptum.Core;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class TrainingStateTests
{
    private static TargetSequence CreateSequence(int length = 5)
    {
        var graphemes = Enumerable.Range(0, length).Select(i => $"g{i}").ToList();
        return new TargetSequence(graphemes);
    }
    
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 2;
        var startTime = DateTime.Now;
        
        var state = new TrainingState(sequence, currentTargetIndex, startTime);
        
        state.Sequence.Should().BeSameAs(sequence);
        state.CurrentTargetIndex.Should().Be(currentTargetIndex);
        state.StartTime.Should().Be(startTime);
        state.EndTime.Should().BeNull();
        state.IstFehlerAktiv.Should().BeFalse();
        state.FehlerPosition.Should().Be(0);
        state.GesamtEingaben.Should().Be(0);
        state.Fehler.Should().Be(0);
        state.Korrekturen.Should().Be(0);
        state.Ruecktasten.Should().Be(0);
        state.IstAbgeschlossen.Should().BeFalse();
    }
    
    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateInstance()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 3;
        var startTime = DateTime.Now.AddMinutes(-5);
        var endTime = DateTime.Now;
        var istFehlerAktiv = true;
        var fehlerPosition = 3;
        var gesamtEingaben = 10;
        var fehler = 2;
        var korrekturen = 1;
        var ruecktasten = 3;
        
        var state = new TrainingState(
            sequence, 
            currentTargetIndex, 
            startTime, 
            endTime, 
            istFehlerAktiv, 
            fehlerPosition,
            gesamtEingaben,
            fehler,
            korrekturen,
            ruecktasten);
        
        state.Sequence.Should().BeSameAs(sequence);
        state.CurrentTargetIndex.Should().Be(currentTargetIndex);
        state.StartTime.Should().Be(startTime);
        state.EndTime.Should().Be(endTime);
        state.IstFehlerAktiv.Should().BeTrue();
        state.FehlerPosition.Should().Be(fehlerPosition);
        state.GesamtEingaben.Should().Be(gesamtEingaben);
        state.Fehler.Should().Be(fehler);
        state.Korrekturen.Should().Be(korrekturen);
        state.Ruecktasten.Should().Be(ruecktasten);
        state.IstAbgeschlossen.Should().BeTrue();
    }
    
    [Fact]
    public void Constructor_WithNullSequence_ShouldThrowArgumentNullException()
    {
        TargetSequence? sequence = null;
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence!, currentTargetIndex, startTime);
        
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("sequence");
    }
    
    [Fact]
    public void Constructor_WithNegativeCurrentTargetIndex_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = -1;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("currentTargetIndex");
    }
    
    [Fact]
    public void Constructor_WithCurrentTargetIndexGreaterThanSequenceLength_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence(5);
        var currentTargetIndex = 6;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("currentTargetIndex");
    }
    
    [Fact]
    public void Constructor_WithCurrentTargetIndexEqualToSequenceLength_ShouldCreateInstance()
    {
        var sequence = CreateSequence(5);
        var currentTargetIndex = 5;
        var startTime = DateTime.Now;
        
        var state = new TrainingState(sequence, currentTargetIndex, startTime);
        
        state.CurrentTargetIndex.Should().Be(5);
    }
    
    [Fact]
    public void Constructor_WithNegativeGesamtEingaben_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime, gesamtEingaben: -1);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("gesamtEingaben");
    }
    
    [Fact]
    public void Constructor_WithNegativeFehler_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime, fehler: -1);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fehler");
    }
    
    [Fact]
    public void Constructor_WithNegativeKorrekturen_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime, korrekturen: -1);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("korrekturen");
    }
    
    [Fact]
    public void Constructor_WithNegativeRuecktasten_ShouldThrowArgumentOutOfRangeException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime, ruecktasten: -1);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("ruecktasten");
    }
    
    [Fact]
    public void Constructor_WithEndTimeBeforeStartTime_ShouldThrowArgumentException()
    {
        var sequence = CreateSequence();
        var currentTargetIndex = 0;
        var startTime = DateTime.Now;
        var endTime = startTime.AddMinutes(-1);
        
        var act = () => new TrainingState(sequence, currentTargetIndex, startTime, endTime);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("endTime");
    }
    
    [Fact]
    public void IstAbgeschlossen_WhenEndTimeIsNull_ShouldBeFalse()
    {
        var sequence = CreateSequence();
        var state = new TrainingState(sequence, 0, DateTime.Now);
        
        state.IstAbgeschlossen.Should().BeFalse();
    }
    
    [Fact]
    public void IstAbgeschlossen_WhenEndTimeIsSet_ShouldBeTrue()
    {
        var sequence = CreateSequence();
        var startTime = DateTime.Now.AddMinutes(-5);
        var endTime = DateTime.Now;
        var state = new TrainingState(sequence, 0, startTime, endTime);
        
        state.IstAbgeschlossen.Should().BeTrue();
    }
}
