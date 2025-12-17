using FluentAssertions;
using Scriptum.Core;
using Scriptum.Engine;
using Xunit;

namespace Scriptum.Engine.Tests;

public class EvaluationEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var targetIndex = 5;
        var expectedGraphem = "a";
        var actualGraphem = "a";
        var outcome = EvaluationOutcome.Richtig;
        
        var evaluationEvent = new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, outcome);
        
        evaluationEvent.TargetIndex.Should().Be(targetIndex);
        evaluationEvent.ExpectedGraphem.Should().Be(expectedGraphem);
        evaluationEvent.ActualGraphem.Should().Be(actualGraphem);
        evaluationEvent.Outcome.Should().Be(outcome);
    }
    
    [Fact]
    public void Constructor_WithEmptyActualGraphem_ShouldCreateInstance()
    {
        var targetIndex = 5;
        var expectedGraphem = "a";
        var actualGraphem = string.Empty;
        var outcome = EvaluationOutcome.Korrigiert;
        
        var evaluationEvent = new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, outcome);
        
        evaluationEvent.ActualGraphem.Should().BeEmpty();
    }
    
    [Fact]
    public void Constructor_WithNegativeTargetIndex_ShouldThrowArgumentOutOfRangeException()
    {
        var targetIndex = -1;
        var expectedGraphem = "a";
        var actualGraphem = "a";
        var outcome = EvaluationOutcome.Richtig;
        
        var act = () => new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, outcome);
        
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetIndex");
    }
    
    [Fact]
    public void Constructor_WithNullExpectedGraphem_ShouldThrowArgumentException()
    {
        var targetIndex = 0;
        string? expectedGraphem = null;
        var actualGraphem = "a";
        var outcome = EvaluationOutcome.Richtig;
        
        var act = () => new EvaluationEvent(targetIndex, expectedGraphem!, actualGraphem, outcome);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("expectedGraphem");
    }
    
    [Fact]
    public void Constructor_WithEmptyExpectedGraphem_ShouldThrowArgumentException()
    {
        var targetIndex = 0;
        var expectedGraphem = string.Empty;
        var actualGraphem = "a";
        var outcome = EvaluationOutcome.Richtig;
        
        var act = () => new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem, outcome);
        
        act.Should().Throw<ArgumentException>()
            .WithParameterName("expectedGraphem");
    }
    
    [Fact]
    public void Constructor_WithNullActualGraphem_ShouldThrowArgumentNullException()
    {
        var targetIndex = 0;
        var expectedGraphem = "a";
        string? actualGraphem = null;
        var outcome = EvaluationOutcome.Richtig;
        
        var act = () => new EvaluationEvent(targetIndex, expectedGraphem, actualGraphem!, outcome);
        
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("actualGraphem");
    }
}
