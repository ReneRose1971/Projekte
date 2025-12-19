using System.Windows.Input;
using DataToolKit.Abstractions.DataStores;
using FluentAssertions;
using Moq;
using Scriptum.Application;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;
using Scriptum.Wpf.Keyboard;
using Scriptum.Wpf.Keyboard.ViewModels;
using Scriptum.Wpf.Navigation;
using Scriptum.Wpf.ViewModels;
using Xunit;

namespace Scriptum.Wpf.Tests.ViewModels;

/// <summary>
/// Characterization Tests für TrainingViewModel.
/// Diese Tests sichern das bestehende Verhalten vor dem Refactoring ab.
/// </summary>
public sealed class TrainingViewModelTests
{
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<ITrainingSessionCoordinator> _coordinatorMock;
    private readonly Mock<IKeyChordAdapter> _adapterMock;
    private readonly Mock<IKeyCodeMapper> _keyCodeMapperMock;
    private readonly VisualKeyboardViewModel _keyboardViewModel;
    private readonly Mock<IDataStoreProvider> _dataStoreProviderMock;
    private readonly Mock<IDataStore<LessonGuideData>> _guideDataStoreMock;
    private readonly TrainingViewModel _viewModel;

    public TrainingViewModelTests()
    {
        _navigationServiceMock = new Mock<INavigationService>();
        _coordinatorMock = new Mock<ITrainingSessionCoordinator>();
        _adapterMock = new Mock<IKeyChordAdapter>();
        _keyCodeMapperMock = new Mock<IKeyCodeMapper>();
        _keyboardViewModel = new VisualKeyboardViewModel();
        _dataStoreProviderMock = new Mock<IDataStoreProvider>();
        _guideDataStoreMock = new Mock<IDataStore<LessonGuideData>>();

        _dataStoreProviderMock
            .Setup(p => p.GetDataStore<LessonGuideData>())
            .Returns(_guideDataStoreMock.Object);

        _viewModel = new TrainingViewModel(
            _navigationServiceMock.Object,
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _dataStoreProviderMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        _viewModel.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullNavigationService_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            null!,
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _dataStoreProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("navigationService");
    }

    [Fact]
    public void Constructor_WithNullCoordinator_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            _navigationServiceMock.Object,
            null!,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _dataStoreProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("coordinator");
    }

    [Fact]
    public void Constructor_WithNullAdapter_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            _navigationServiceMock.Object,
            _coordinatorMock.Object,
            null!,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            _dataStoreProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("adapter");
    }

    [Fact]
    public void Constructor_WithNullKeyCodeMapper_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            _navigationServiceMock.Object,
            _coordinatorMock.Object,
            _adapterMock.Object,
            null!,
            _keyboardViewModel,
            _dataStoreProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("keyCodeMapper");
    }

    [Fact]
    public void Constructor_WithNullKeyboardViewModel_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            _navigationServiceMock.Object,
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            null!,
            _dataStoreProviderMock.Object);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("keyboardViewModel");
    }

    [Fact]
    public void Constructor_WithNullDataStoreProvider_ShouldThrowArgumentNullException()
    {
        var act = () => new TrainingViewModel(
            _navigationServiceMock.Object,
            _coordinatorMock.Object,
            _adapterMock.Object,
            _keyCodeMapperMock.Object,
            _keyboardViewModel,
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("dataStoreProvider");
    }

    #endregion

    #region Initialize Tests

    [Fact]
    public void Initialize_ShouldSetModuleId()
    {
        var moduleId = "module1";
        var lessonId = "lesson1";

        _viewModel.Initialize(moduleId, lessonId);

        _viewModel.ModuleId.Should().Be(moduleId);
    }

    [Fact]
    public void Initialize_ShouldSetLessonId()
    {
        var moduleId = "module1";
        var lessonId = "lesson1";

        _viewModel.Initialize(moduleId, lessonId);

        _viewModel.LessonId.Should().Be(lessonId);
    }

    [Fact]
    public void Initialize_ShouldCallCoordinatorStartSession()
    {
        var moduleId = "module1";
        var lessonId = "lesson1";

        _viewModel.Initialize(moduleId, lessonId);

        _coordinatorMock.Verify(
            c => c.StartSession(moduleId, lessonId),
            Times.Once);
    }

    #endregion

    #region ToggleGuide Tests

    [Fact]
    public void ToggleGuide_WhenFalse_ShouldSetToTrue()
    {
        _viewModel.IsGuideVisible = false;

        _viewModel.ToggleGuide();

        _viewModel.IsGuideVisible.Should().BeTrue();
    }

    [Fact]
    public void ToggleGuide_WhenTrue_ShouldSetToFalse()
    {
        _viewModel.IsGuideVisible = true;

        _viewModel.ToggleGuide();

        _viewModel.IsGuideVisible.Should().BeFalse();
    }

    #endregion

    #region NavigateBack Tests

    [Fact]
    public void NavigateBack_ShouldCallNavigationService()
    {
        var moduleId = "module1";
        var lessonId = "lesson1";
        _viewModel.Initialize(moduleId, lessonId);

        _viewModel.NavigateBack();

        _navigationServiceMock.Verify(
            n => n.NavigateToLessonDetails(moduleId, lessonId),
            Times.Once);
    }

    #endregion

    #region DisplayTarget Tests

    [Fact]
    public void DisplayTarget_WhenSequenceIsNull_ShouldReturnMessage()
    {
        _coordinatorMock.Setup(c => c.CurrentState).Returns((TrainingState?)null);

        var result = _viewModel.DisplayTarget;

        result.Should().Be("Keine Lektion geladen");
    }

    [Fact]
    public void DisplayTarget_WithValidSequence_ShouldReturnConcatenatedGraphemes()
    {
        var sequence = new TargetSequence(new[] { "a", "b", "c" });
        var state = new TrainingState(sequence, 0, DateTime.Now);
        _coordinatorMock.Setup(c => c.CurrentState).Returns(state);

        var result = _viewModel.DisplayTarget;

        result.Should().Be("abc");
    }

    #endregion

    #region DisplayInput Tests

    [Fact]
    public void DisplayInput_WhenNoInputs_ShouldReturnEmptyString()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.DisplayInput;

        result.Should().BeEmpty();
    }

    [Fact]
    public void DisplayInput_WithInputs_ShouldReturnConcatenatedGraphemes()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        session.Inputs.Add(new StoredInput
        {
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "a",
            Taste = KeyId.A,
            Umschalter = ModifierSet.None,
            Zeitpunkt = DateTimeOffset.Now
        });
        session.Inputs.Add(new StoredInput
        {
            Art = StoredInputKind.Zeichen,
            ErzeugtesGraphem = "b",
            Taste = KeyId.B,
            Umschalter = ModifierSet.None,
            Zeitpunkt = DateTimeOffset.Now
        });
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.DisplayInput;

        result.Should().Be("ab");
    }

    #endregion

    #region CurrentIndex Tests

    [Fact]
    public void CurrentIndex_WhenStateIsNull_ShouldReturnZero()
    {
        _coordinatorMock.Setup(c => c.CurrentState).Returns((TrainingState?)null);

        var result = _viewModel.CurrentIndex;

        result.Should().Be(0);
    }

    [Fact]
    public void CurrentIndex_WithValidState_ShouldReturnCurrentTargetIndex()
    {
        var sequence = new TargetSequence(new[] { "a", "b", "c" });
        var state = new TrainingState(sequence, 2, DateTime.Now);
        _coordinatorMock.Setup(c => c.CurrentState).Returns(state);

        var result = _viewModel.CurrentIndex;

        result.Should().Be(2);
    }

    #endregion

    #region IsCompleted Tests

    [Fact]
    public void IsCompleted_WhenSessionIsNull_ShouldReturnFalse()
    {
        _coordinatorMock.Setup(c => c.CurrentSession).Returns((TrainingSession?)null);

        var result = _viewModel.IsCompleted;

        result.Should().BeFalse();
    }

    [Fact]
    public void IsCompleted_WhenSessionIsNotCompleted_ShouldReturnFalse()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.IsCompleted;

        result.Should().BeFalse();
    }

    #endregion

    #region ErrorCount Tests

    [Fact]
    public void ErrorCount_WhenNoEvaluations_ShouldReturnZero()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.ErrorCount;

        result.Should().Be(0);
    }

    [Fact]
    public void ErrorCount_WithEvaluations_ShouldReturnCount()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        session.Evaluations.Add(new StoredEvaluation
        {
            TokenIndex = 0,
            Erwartet = "a",
            Tatsaechlich = "b",
            Ergebnis = EvaluationOutcome.Falsch
        });
        session.Evaluations.Add(new StoredEvaluation
        {
            TokenIndex = 1,
            Erwartet = "c",
            Tatsaechlich = "d",
            Ergebnis = EvaluationOutcome.Falsch
        });
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.ErrorCount;

        result.Should().Be(2);
    }

    #endregion

    #region StatusText Tests

    [Fact]
    public void StatusText_WhenCompleted_ShouldShowCompletionMessage()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        session.IsCompleted = true;
        session.EndedAt = DateTimeOffset.Now;
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);

        var result = _viewModel.StatusText;

        result.Should().Be("Lektion abgeschlossen!");
    }

    [Fact]
    public void StatusText_WhenNotCompleted_ShouldShowPositionAndErrors()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        var sequence = new TargetSequence(new[] { "a", "b", "c" });
        var state = new TrainingState(sequence, 1, DateTime.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);
        _coordinatorMock.Setup(c => c.CurrentState).Returns(state);

        var result = _viewModel.StatusText;

        result.Should().Be("Position: 1, Fehler: 0");
    }

    #endregion

    #region OnKeyDown Tests

    [Fact(Skip = "Requires WPF STA thread - test refactored input handling separately")]
    public void OnKeyDown_WhenSessionNotRunning_ShouldNotProcessInput()
    {
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(false);
        var keyArgs = CreateKeyEventArgs(Key.A);

        _viewModel.OnKeyDown(keyArgs);

        _coordinatorMock.Verify(
            c => c.ProcessInput(It.IsAny<KeyChord>()),
            Times.Never);
    }

    [Fact(Skip = "Requires WPF STA thread - test refactored input handling separately")]
    public void OnKeyDown_WhenAlreadyCompleted_ShouldNotProcessInput()
    {
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        session.IsCompleted = true;
        session.EndedAt = DateTimeOffset.Now;
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);
        var keyArgs = CreateKeyEventArgs(Key.A);

        _viewModel.OnKeyDown(keyArgs);

        _coordinatorMock.Verify(
            c => c.ProcessInput(It.IsAny<KeyChord>()),
            Times.Never);
    }

    [Fact(Skip = "Requires WPF STA thread - test refactored input handling separately")]
    public void OnKeyDown_WithValidInput_ShouldProcessInput()
    {
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);
        
        var keyArgs = CreateKeyEventArgs(Key.A);
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        _adapterMock.Setup(a => a.TryCreateChord(keyArgs, out chord)).Returns(true);

        _viewModel.OnKeyDown(keyArgs);

        _coordinatorMock.Verify(
            c => c.ProcessInput(It.IsAny<KeyChord>()),
            Times.Once);
    }

    [Fact(Skip = "Requires WPF STA thread - test refactored input handling separately")]
    public void OnKeyDown_WhenCompletedAfterInput_ShouldNavigateToSummary()
    {
        _coordinatorMock.Setup(c => c.IsSessionRunning).Returns(true);
        var session = TrainingSession.CreateNew("lesson1", "module1", DateTimeOffset.Now);
        _coordinatorMock.Setup(c => c.CurrentSession).Returns(session);
        
        var keyArgs = CreateKeyEventArgs(Key.A);
        var chord = new KeyChord(KeyId.A, ModifierSet.None);
        _adapterMock.Setup(a => a.TryCreateChord(keyArgs, out chord)).Returns(true);
        
        var evaluation = new EvaluationEvent(0, "a", "a", EvaluationOutcome.Richtig);
        _coordinatorMock.Setup(c => c.ProcessInput(It.IsAny<KeyChord>())).Returns(evaluation);
        
        session.IsCompleted = true;
        session.EndedAt = DateTimeOffset.Now;

        _viewModel.OnKeyDown(keyArgs);

        _navigationServiceMock.Verify(
            n => n.NavigateToTrainingSummary(It.IsAny<int?>()),
            Times.Once);
    }

    #endregion

    #region Command Tests

    [Fact]
    public void NavigateBackCommand_ShouldNotBeNull()
    {
        _viewModel.NavigateBackCommand.Should().NotBeNull();
    }

    [Fact]
    public void NavigateBackCommand_CanExecute_ShouldReturnTrue()
    {
        var result = _viewModel.NavigateBackCommand.CanExecute(null);

        result.Should().BeTrue();
    }

    [Fact]
    public void NavigateBackCommand_Execute_ShouldCallNavigationService()
    {
        var moduleId = "module1";
        var lessonId = "lesson1";
        _viewModel.Initialize(moduleId, lessonId);

        _viewModel.NavigateBackCommand.Execute(null);

        _navigationServiceMock.Verify(
            n => n.NavigateToLessonDetails(moduleId, lessonId),
            Times.Once);
    }

    [Fact]
    public void ToggleGuideCommand_ShouldNotBeNull()
    {
        _viewModel.ToggleGuideCommand.Should().NotBeNull();
    }

    [Fact]
    public void ToggleGuideCommand_CanExecute_ShouldReturnTrue()
    {
        var result = _viewModel.ToggleGuideCommand.CanExecute(null);

        result.Should().BeTrue();
    }

    [Fact]
    public void ToggleGuideCommand_Execute_WhenFalse_ShouldSetToTrue()
    {
        _viewModel.IsGuideVisible = false;

        _viewModel.ToggleGuideCommand.Execute(null);

        _viewModel.IsGuideVisible.Should().BeTrue();
    }

    [Fact]
    public void ToggleGuideCommand_Execute_WhenTrue_ShouldSetToFalse()
    {
        _viewModel.IsGuideVisible = true;

        _viewModel.ToggleGuideCommand.Execute(null);

        _viewModel.IsGuideVisible.Should().BeFalse();
    }

    #endregion

    #region GuideText Tests

    [Fact]
    public void GuideText_WhenLessonIdIsEmpty_ShouldReturnNoHelpMessage()
    {
        var result = _viewModel.GuideText;

        result.Should().Be("Keine Hilfe verfügbar");
    }

    [Fact]
    public void GuideText_WhenGuideNotFound_ShouldReturnNoGuideMessage()
    {
        _viewModel.Initialize("module1", "lesson1");
        _guideDataStoreMock.Setup(d => d.Items).Returns(new System.Collections.ObjectModel.ReadOnlyObservableCollection<LessonGuideData>(
            new System.Collections.ObjectModel.ObservableCollection<LessonGuideData>()));

        var result = _viewModel.GuideText;

        result.Should().Be("Keine Hilfe für diese Lektion vorhanden.");
    }

    #endregion

    #region Helper Methods

    private static KeyEventArgs CreateKeyEventArgs(Key key)
    {
        return new KeyEventArgs(
            System.Windows.Input.Keyboard.PrimaryDevice,
            new MockPresentationSource(),
            0,
            key)
        {
            RoutedEvent = System.Windows.Input.Keyboard.KeyDownEvent
        };
    }

    private class MockPresentationSource : System.Windows.PresentationSource
    {
        public override bool IsDisposed => false;
        public override System.Windows.Media.Visual RootVisual { get; set; } = null!;
        protected override System.Windows.Media.CompositionTarget GetCompositionTargetCore() => null!;
    }

    #endregion
}
