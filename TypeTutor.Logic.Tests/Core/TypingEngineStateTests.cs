using FluentAssertions;
using TypeTutor.Logic.Core;
using Xunit;

namespace TypeTutor.Logic.Tests.Core;

/// <summary>
/// Tests für TypingEngineState.
/// Validiert State-Konstruktion, Validierung und Factory-Methoden.
/// </summary>
public sealed class TypingEngineStateTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateState()
    {
        // Act
        var state = new TypingEngineState(
            targetText: "test",
            inputText: "te",
            correctPrefixLength: 2,
            errorCount: 0,
            nextIndex: 2,
            isComplete: false,
            errorPositions: Array.Empty<int>(),
            expectedNextChar: 's',
            lastInputChar: 'e'
        );

        // Assert
        state.Should().NotBeNull();
        state.TargetText.Should().Be("test");
        state.InputText.Should().Be("te");
        state.CorrectPrefixLength.Should().Be(2);
        state.ErrorCount.Should().Be(0);
        state.NextIndex.Should().Be(2);
        state.IsComplete.Should().BeFalse();
        state.ExpectedNextChar.Should().Be('s');
        state.LastInputChar.Should().Be('e');
    }

    [Fact]
    public void Constructor_WithNullTargetText_ShouldUseEmptyString()
    {
        // Act
        var state = new TypingEngineState(
            targetText: null,
            inputText: "test",
            correctPrefixLength: 0,
            errorCount: 0,
            nextIndex: 0,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        state.TargetText.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullInputText_ShouldUseEmptyString()
    {
        // Act
        var state = new TypingEngineState(
            targetText: "test",
            inputText: null,
            correctPrefixLength: 0,
            errorCount: 0,
            nextIndex: 0,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: 't',
            lastInputChar: null
        );

        // Assert
        state.InputText.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNegativeCorrectPrefixLength_ShouldThrow()
    {
        // Act
        Action act = () => new TypingEngineState(
            targetText: "test",
            inputText: "",
            correctPrefixLength: -1,
            errorCount: 0,
            nextIndex: 0,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("correctPrefixLength");
    }

    [Fact]
    public void Constructor_WithNegativeErrorCount_ShouldThrow()
    {
        // Act
        Action act = () => new TypingEngineState(
            targetText: "test",
            inputText: "",
            correctPrefixLength: 0,
            errorCount: -1,
            nextIndex: 0,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("errorCount");
    }

    [Fact]
    public void Constructor_WithNegativeNextIndex_ShouldThrow()
    {
        // Act
        Action act = () => new TypingEngineState(
            targetText: "test",
            inputText: "",
            correctPrefixLength: 0,
            errorCount: 0,
            nextIndex: -1,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("nextIndex");
    }

    [Fact]
    public void Constructor_WithNextIndexBeyondTarget_ShouldThrow()
    {
        // Act
        Action act = () => new TypingEngineState(
            targetText: "test",
            inputText: "",
            correctPrefixLength: 0,
            errorCount: 0,
            nextIndex: 5,
            isComplete: false,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("nextIndex");
    }

    [Fact]
    public void Start_WithValidText_ShouldCreateInitialState()
    {
        // Act
        var state = TypingEngineState.Start("test");

        // Assert
        state.TargetText.Should().Be("test");
        state.InputText.Should().BeEmpty();
        state.CorrectPrefixLength.Should().Be(0);
        state.ErrorCount.Should().Be(0);
        state.NextIndex.Should().Be(0);
        state.IsComplete.Should().BeFalse();
        state.ExpectedNextChar.Should().Be('t');
        state.LastInputChar.Should().BeNull();
        state.ErrorPositions.Should().BeEmpty();
    }

    [Fact]
    public void Start_WithEmptyText_ShouldMarkAsComplete()
    {
        // Act
        var state = TypingEngineState.Start("");

        // Assert
        state.IsComplete.Should().BeTrue();
        state.ExpectedNextChar.Should().BeNull();
    }

    [Fact]
    public void Start_WithNullText_ShouldTreatAsEmpty()
    {
        // Act
        var state = TypingEngineState.Start(null);

        // Assert
        state.TargetText.Should().BeEmpty();
        state.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void ErrorPositions_ShouldBeSorted()
    {
        // Act
        var state = new TypingEngineState(
            targetText: "test",
            inputText: "test",
            correctPrefixLength: 0,
            errorCount: 3,
            nextIndex: 4,
            isComplete: true,
            errorPositions: new[] { 3, 1, 2 },
            expectedNextChar: null,
            lastInputChar: 't'
        );

        // Assert
        state.ErrorPositions.Should().BeInAscendingOrder();
        state.ErrorPositions.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void ErrorPositions_WithNull_ShouldBeEmptyArray()
    {
        // Act
        var state = new TypingEngineState(
            targetText: "test",
            inputText: "test",
            correctPrefixLength: 4,
            errorCount: 0,
            nextIndex: 4,
            isComplete: true,
            errorPositions: null,
            expectedNextChar: null,
            lastInputChar: 't'
        );

        // Assert
        state.ErrorPositions.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnInformativeString()
    {
        // Arrange
        var state = TypingEngineState.Start("test");

        // Act
        var result = state.ToString();

        // Assert
        result.Should().Contain("TypingState");
        result.Should().Contain("len(Target)=4");
    }

    [Fact]
    public void Record_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var state1 = TypingEngineState.Start("test");
        var state2 = TypingEngineState.Start("test");

        // Act & Assert
        state1.Should().Be(state2);
    }

    [Fact]
    public void Record_Immutability_ShouldSupportWith()
    {
        // Arrange
        var original = TypingEngineState.Start("test");

        // Act
        var modified = original with { NextIndex = 1, ExpectedNextChar = 'e' };

        // Assert
        original.NextIndex.Should().Be(0);
        modified.NextIndex.Should().Be(1);
        modified.TargetText.Should().Be("test");
    }

    [Theory]
    [InlineData("", 0, 0, 0, true)]
    [InlineData("test", 0, 0, 0, false)]
    [InlineData("abc", 3, 0, 3, true)]
    public void Constructor_WithVariousStates_ShouldValidateCorrectly(
        string target,
        int prefixLen,
        int errorCount,
        int nextIndex,
        bool isComplete)
    {
        // Act
        var state = new TypingEngineState(
            targetText: target,
            inputText: "",
            correctPrefixLength: prefixLen,
            errorCount: errorCount,
            nextIndex: nextIndex,
            isComplete: isComplete,
            errorPositions: Array.Empty<int>(),
            expectedNextChar: null,
            lastInputChar: null
        );

        // Assert
        state.TargetText.Should().Be(target);
        state.CorrectPrefixLength.Should().Be(prefixLen);
        state.ErrorCount.Should().Be(errorCount);
        state.NextIndex.Should().Be(nextIndex);
        state.IsComplete.Should().Be(isComplete);
    }
}
