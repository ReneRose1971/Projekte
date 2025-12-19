using System;
using System.Windows.Input;
using FluentAssertions;
using Moq;
using Scriptum.Application;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;
using Scriptum.Wpf.Keyboard;
using Scriptum.Wpf.Keyboard.ViewModels;
using Xunit;

namespace Scriptum.Wpf.Tests.Keyboard;

/// <summary>
/// Tests für TrainingKeyboardInputHandler.
/// </summary>
public sealed class TrainingKeyboardInputHandlerTests
{
    private readonly Mock<ITrainingSessionCoordinator> _coordinatorMock;
    private readonly Mock<IKeyChordAdapter> _adapterMock;
    private readonly Mock<IKeyCodeMapper> _keyCodeMapperMock;
    private readonly VisualKeyboardViewModel _keyboardViewModel;
    private readonly Mock<Action> _onInputProcessedMock;
    private readonly TrainingKeyboardInputHandler _handler;

    public TrainingKeyboardInputHandlerTests()
    {
        _coordinatorMock = new Mock<ITrainingSessionCoordinator>();
        _adapterMock = new Mock<IKeyChordAdapter>();
        _keyCodeMapperMock = new Mock<IKeyCodeMapper>();
        _keyboardViewModel = new VisualKeyboardViewModel();
        _onInputProcessedMock = new Mock<Action>();

        _handler = new TrainingKeyboardInputHandler(
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _onInputProcessedMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        _handler.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullCoordinator_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingKeyboardInputHandler(
            null!,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _onInputProcessedMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("coordinator");
    }

    [Fact]
    public void Constructor_WithNullAdapter_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingKeyboardInputHandler(
            _coordinatorMock.Object,
            null!,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _onInputProcessedMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("adapter");
    }

    [Fact]
    public void Constructor_WithNullKeyCodeMapper_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingKeyboardInputHandler(
            _coordinatorMock.Object,
            _adapterMock.Object,
            null!,
            _keyboardViewModel,
            _onInputProcessedMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("keyCodeMapper");
    }

    [Fact]
    public void Constructor_WithNullKeyboardViewModel_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingKeyboardInputHandler(
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            null!,
            _onInputProcessedMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("keyboardViewModel");
    }

    [Fact]
    public void Constructor_WithNullOnInputProcessed_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingKeyboardInputHandler(
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("onInputProcessed");
    }

    #endregion

    #region HandleKeyDown Tests

    [Fact]
    public void HandleKeyDown_WithNullKeyEventArgs_ShouldThrowArgumentNullException()
    {
        var act = () => _handler.HandleKeyDown(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("e");
    }

    [Fact]
    public void HandleKeyDown_WhenSessionNotRunning_ShouldReturnFalse()
    {
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(false);
        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);

        var result = _handler.HandleKeyDown(keyArgs.Object);

        result.Should().BeFalse();
        _coordinatorMock.Verify(c => c.ProcessInput(It.IsAny<KeyChord>()), Times.Never);
    }

    [Fact]
    public void HandleKeyDown_WhenSessionCompleted_ShouldReturnFalse()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        session.IsCompleted = true;
        session.EndedAt = DateTimeOffset.Now;

        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);

        var result = _handler.HandleKeyDown(keyArgs.Object);

        result.Should().BeFalse();
        _coordinatorMock.Verify(c => c.ProcessInput(It.IsAny<KeyChord>()), Times.Never);
    }

    [Fact]
    public void HandleKeyDown_WithValidInput_ShouldProcessInput()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        _adapterMock.Setup(a => a.TryCreateChord(keyArgs.Object, out chord)).Returns(true);

        var evaluation = new EvaluationEvent(0, "a", "a", EvaluationOutcome.Richtig);
        _coordinatorMock.Setup(c => c.ProcessInput(It.IsAny<KeyChord>())).Returns(evaluation);

        var result = _handler.HandleKeyDown(keyArgs.Object);

        result.Should().BeTrue();
        _coordinatorMock.Verify(c => c.ProcessInput(It.IsAny<KeyChord>()), Times.Once);
        _onInputProcessedMock.Verify(a => a(), Times.Once);
    }

    [Fact]
    public void HandleKeyDown_WhenAdapterCannotCreateChord_ShouldReturnFalse()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);
        KeyChord chord;
        _adapterMock.Setup(a => a.TryCreateChord(keyArgs.Object, out chord)).Returns(false);

        var result = _handler.HandleKeyDown(keyArgs.Object);

        result.Should().BeFalse();
        _coordinatorMock.Verify(c => c.ProcessInput(It.IsAny<KeyChord>()), Times.Never);
    }

    [Fact]
    public void HandleKeyDown_ShouldUpdateKeyboardVisualization()
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(Key.A)).Returns("A");

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);
        _handler.HandleKeyDown(keyArgs.Object);

        _keyCodeMapperMock.Verify(m => m.MapToLabel(Key.A), Times.Once);
    }

    #endregion

    #region HandleKeyUp Tests

    [Fact]
    public void HandleKeyUp_WithNullKeyEventArgs_ShouldThrowArgumentNullException()
    {
        var act = () => _handler.HandleKeyUp(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("e");
    }

    [Fact]
    public void HandleKeyUp_ShouldUpdateKeyboardVisualization()
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(Key.A)).Returns("A");

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.A);
        _handler.HandleKeyUp(keyArgs.Object);

        _keyCodeMapperMock.Verify(m => m.MapToLabel(Key.A), Times.Once);
    }

    #endregion

    #region Modifier Key Tests

    [Theory]
    [InlineData(Key.LeftShift)]
    [InlineData(Key.RightShift)]
    public void HandleKeyDown_WithShiftKey_ShouldSetIsShiftActive(Key shiftKey)
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(shiftKey)).Returns("Shift");
        _keyboardViewModel.IsShiftActive.Should().BeFalse();

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, shiftKey);
        _handler.HandleKeyDown(keyArgs.Object);

        _keyboardViewModel.IsShiftActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(Key.LeftShift)]
    [InlineData(Key.RightShift)]
    public void HandleKeyUp_WithShiftKey_ShouldClearIsShiftActive(Key shiftKey)
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(shiftKey)).Returns("Shift");
        _keyboardViewModel.IsShiftActive = true;

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, shiftKey);
        _handler.HandleKeyUp(keyArgs.Object);

        _keyboardViewModel.IsShiftActive.Should().BeFalse();
    }

    [Fact]
    public void HandleKeyDown_WithAltGrKey_ShouldSetIsAltGrActive()
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(Key.RightAlt)).Returns("AltGr");
        _keyboardViewModel.IsAltGrActive.Should().BeFalse();

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.RightAlt);
        _handler.HandleKeyDown(keyArgs.Object);

        _keyboardViewModel.IsAltGrActive.Should().BeTrue();
    }

    [Fact]
    public void HandleKeyUp_WithAltGrKey_ShouldClearIsAltGrActive()
    {
        _keyCodeMapperMock.Setup(m => m.MapToLabel(Key.RightAlt)).Returns("AltGr");
        _keyboardViewModel.IsAltGrActive = true;

        var keyArgs = new Mock<KeyEventArgs>(MockBehavior.Loose, System.Windows.Input.Keyboard.PrimaryDevice, new MockPresentationSource(), 0, Key.RightAlt);
        _handler.HandleKeyUp(keyArgs.Object);

        _keyboardViewModel.IsAltGrActive.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private class MockPresentationSource : System.Windows.PresentationSource
    {
        public override bool IsDisposed => false;
        public override System.Windows.Media.Visual RootVisual { get; set; } = null!;
        protected override System.Windows.Media.CompositionTarget GetCompositionTargetCore() => null!;
    }

    #endregion
}
