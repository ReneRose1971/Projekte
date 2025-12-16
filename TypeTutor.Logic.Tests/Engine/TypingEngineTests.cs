using FluentAssertions;
using Moq;
using TypeTutor.Logic.Core;
using TypeTutor.Logic.Engine;
using TypeTutor.Logic.Tests.Helpers;
using Xunit;

namespace TypeTutor.Logic.Tests.Engine;

/// <summary>
/// Umfassende Tests für die TypingEngine.
/// Validiert Eingabeverarbeitung, Fehlerbehandlung, Case-Sensitivity und Event-Handling.
/// </summary>
public sealed class TypingEngineTests
{
    private readonly Mock<IKeyToCharMapper> _mapperMock;

    public TypingEngineTests()
    {
        _mapperMock = new Mock<IKeyToCharMapper>();
        SetupDefaultMapper();
    }

    private void SetupDefaultMapper()
    {
        // Standard-Mapper - nicht verwendet, da KeyStroke bereits das Char enthält
        _mapperMock.Setup(m => m.Map(It.IsAny<KeyStroke>()))
            .Returns((KeyStroke stroke) => stroke.Char);
    }

    [Fact]
    public void Constructor_WithValidMapper_ShouldCreateEngine()
    {
        // Act
        var engine = new TypingEngine(_mapperMock.Object);

        // Assert
        engine.Should().NotBeNull();
        engine.State.Should().NotBeNull();
        engine.State.TargetText.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullMapper_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new TypingEngine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mapper");
    }

    [Fact]
    public void Reset_WithValidText_ShouldInitializeState()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        var targetText = "test";

        // Act
        engine.Reset(targetText);

        // Assert
        engine.State.TargetText.Should().Be(targetText);
        engine.State.InputText.Should().BeEmpty();
        engine.State.CorrectPrefixLength.Should().Be(0);
        engine.State.ErrorCount.Should().Be(0);
        engine.State.NextIndex.Should().Be(0);
        engine.State.IsComplete.Should().BeFalse();
        engine.State.ExpectedNextChar.Should().Be('t');
    }

    [Fact]
    public void Reset_WithEmptyText_ShouldMarkAsComplete()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);

        // Act
        engine.Reset("");

        // Assert
        engine.State.IsComplete.Should().BeTrue();
        engine.State.ExpectedNextChar.Should().BeNull();
    }

    [Fact]
    public void Process_WithCorrectCharacter_ShouldIncreasePrefixLength()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("test");
        var stroke = TestDataBuilder.CreateKeyStroke('t');

        // Act
        engine.Process(stroke);

        // Assert
        engine.State.CorrectPrefixLength.Should().Be(1);
        engine.State.NextIndex.Should().Be(1);
        engine.State.ExpectedNextChar.Should().Be('e');
        engine.State.ErrorCount.Should().Be(0);
    }

    [Fact]
    public void Process_WithIncorrectCharacter_ShouldIncreaseErrorCount()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("test");
        var stroke = TestDataBuilder.CreateKeyStroke('x');

        // Act
        engine.Process(stroke);

        // Assert
        engine.State.ErrorCount.Should().Be(1);
        engine.State.CorrectPrefixLength.Should().Be(0);
        engine.State.NextIndex.Should().Be(1);
        engine.State.ErrorPositions.Should().Contain(0);
    }

    [Fact]
    public void Process_CompleteCorrectSequence_ShouldMarkAsComplete()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("abc");

        // Act
        foreach (var stroke in TestDataBuilder.CreateKeyStrokes("abc"))
        {
            engine.Process(stroke);
        }

        // Assert
        engine.State.IsComplete.Should().BeTrue();
        engine.State.ErrorCount.Should().Be(0);
        engine.State.CorrectPrefixLength.Should().Be(3);
        engine.State.NextIndex.Should().Be(3);
        engine.State.ExpectedNextChar.Should().BeNull();
    }

    [Fact]
    public void Process_CompleteWithErrors_ShouldMarkAsCompleteButTrackErrors()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("abc");

        // Act - Tippe "axc" statt "abc"
        engine.Process(TestDataBuilder.CreateKeyStroke('a'));
        engine.Process(TestDataBuilder.CreateKeyStroke('x')); // Fehler
        engine.Process(TestDataBuilder.CreateKeyStroke('c'));

        // Assert
        engine.State.IsComplete.Should().BeTrue();
        engine.State.ErrorCount.Should().Be(1);
        engine.State.ErrorPositions.Should().Contain(1);
    }

    [Fact]
    public void Process_AfterCompletion_ShouldIgnoreAdditionalInput()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("ab");
        engine.Process(TestDataBuilder.CreateKeyStroke('a'));
        engine.Process(TestDataBuilder.CreateKeyStroke('b'));

        // Act - Weitere Eingabe nach Completion
        engine.Process(TestDataBuilder.CreateKeyStroke('c'));

        // Assert
        engine.State.NextIndex.Should().Be(2);
        engine.State.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void LessonCompleted_WithSuccessfulCompletion_ShouldFireWithTrue()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("ab");
        bool? result = null;
        engine.LessonCompleted += (success) => result = success;

        // Act
        engine.Process(TestDataBuilder.CreateKeyStroke('a'));
        engine.Process(TestDataBuilder.CreateKeyStroke('b'));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LessonCompleted_WithErrors_ShouldFireWithFalse()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("ab");
        bool? result = null;
        engine.LessonCompleted += (success) => result = success;

        // Act
        engine.Process(TestDataBuilder.CreateKeyStroke('x')); // Fehler
        engine.Process(TestDataBuilder.CreateKeyStroke('b'));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LessonCompleted_ShouldOnlyFireOnce()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("a");
        int callCount = 0;
        engine.LessonCompleted += (success) => callCount++;

        // Act
        engine.Process(TestDataBuilder.CreateKeyStroke('a'));
        engine.Process(TestDataBuilder.CreateKeyStroke('b')); // Sollte ignoriert werden

        // Assert
        callCount.Should().Be(1);
    }

    [Theory]
    [InlineData(CaseSensitivity.Strict, 'A', 'a', false)]
    [InlineData(CaseSensitivity.IgnoreCase, 'A', 'a', true)]
    [InlineData(CaseSensitivity.Strict, 'a', 'a', true)]
    [InlineData(CaseSensitivity.IgnoreCase, 'a', 'A', true)]
    public void CaseSensitivity_ShouldAffectComparison(
        CaseSensitivity mode,
        char target,
        char input,
        bool shouldMatch)
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object, mode);
        engine.Reset(target.ToString());

        // Act
        engine.Process(TestDataBuilder.CreateKeyStroke(input));

        // Assert
        if (shouldMatch)
        {
            engine.State.CorrectPrefixLength.Should().Be(1);
            engine.State.ErrorCount.Should().Be(0);
        }
        else
        {
            engine.State.CorrectPrefixLength.Should().Be(0);
            engine.State.ErrorCount.Should().Be(1);
        }
    }

    [Fact]
    public void Process_WithNullCharacter_ShouldUpdateStateWithoutProcessing()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("test");
        var stroke = new KeyStroke(KeyCode.None, null, ModifierKeys.None);

        // Act
        engine.Process(stroke);

        // Assert
        engine.State.CorrectPrefixLength.Should().Be(0);
        engine.State.ErrorCount.Should().Be(0);
        engine.State.NextIndex.Should().Be(0);
        engine.State.LastInputChar.Should().BeNull();
    }

    [Fact]
    public void Reset_AfterCompletion_ShouldResetToInitialState()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("ab");
        engine.Process(TestDataBuilder.CreateKeyStroke('a'));
        engine.Process(TestDataBuilder.CreateKeyStroke('b'));

        // Act
        engine.Reset("xyz");

        // Assert
        engine.State.TargetText.Should().Be("xyz");
        engine.State.IsComplete.Should().BeFalse();
        engine.State.ErrorCount.Should().Be(0);
        engine.State.CorrectPrefixLength.Should().Be(0);
        engine.State.ExpectedNextChar.Should().Be('x');
    }

    [Fact]
    public void State_LastInputChar_ShouldTrackLastTypedCharacter()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("test");

        // Act
        engine.Process(TestDataBuilder.CreateKeyStroke('t'));
        engine.Process(TestDataBuilder.CreateKeyStroke('e'));

        // Assert
        engine.State.LastInputChar.Should().Be('e');
    }

    [Fact]
    public void ErrorPositions_ShouldAccumulateAllErrors()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        engine.Reset("abcd");

        // Act - Fehler an Position 1 und 3
        engine.Process(TestDataBuilder.CreateKeyStroke('a')); // Korrekt
        engine.Process(TestDataBuilder.CreateKeyStroke('x')); // Fehler an Position 1
        engine.Process(TestDataBuilder.CreateKeyStroke('c')); // Korrekt
        engine.Process(TestDataBuilder.CreateKeyStroke('y')); // Fehler an Position 3

        // Assert
        engine.State.ErrorPositions.Should().HaveCount(2);
        engine.State.ErrorPositions.Should().Contain(new[] { 1, 3 });
        engine.State.ErrorCount.Should().Be(2);
    }

    [Fact]
    public void Process_WithLongText_ShouldHandleCorrectly()
    {
        // Arrange
        var engine = new TypingEngine(_mapperMock.Object);
        var targetText = "The quick brown fox jumps over the lazy dog";
        engine.Reset(targetText);

        // Act
        foreach (var stroke in TestDataBuilder.CreateKeyStrokes(targetText))
        {
            engine.Process(stroke);
        }

        // Assert
        engine.State.IsComplete.Should().BeTrue();
        engine.State.ErrorCount.Should().Be(0);
        engine.State.CorrectPrefixLength.Should().Be(targetText.Length);
    }
}
