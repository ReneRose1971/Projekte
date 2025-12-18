using FluentAssertions;
using Scriptum.Application.Tests.Fakes;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Engine;
using TestHelper.DataToolKit.Fakes.Providers;
using TestHelper.DataToolKit.Fakes.Repositories;

namespace Scriptum.Application.Tests;

/// <summary>
/// Tests für TrainingSessionCoordinator - StartSession.
/// </summary>
public sealed class TrainingSessionCoordinatorStartSessionTests
{
    private readonly FakeDataStoreProvider _dataStoreProvider;
    private readonly FakeRepositoryFactory _repositoryFactory;
    private readonly FakeClock _clock;
    private readonly ITrainingEngine _engine;
    private readonly IInputInterpreter _interpreter;
    private readonly TrainingSessionCoordinator _coordinator;

    public TrainingSessionCoordinatorStartSessionTests()
    {
        _repositoryFactory = new FakeRepositoryFactory();
        _dataStoreProvider = new FakeDataStoreProvider(_repositoryFactory);
        _clock = new FakeClock();
        _engine = new TrainingEngine();
        _interpreter = new DeQwertzInputInterpreter();

        _coordinator = new TrainingSessionCoordinator(
            _engine,
            _interpreter,
            _clock,
            _dataStoreProvider,
            _repositoryFactory);
    }

    [Fact]
    public void StartSession_ShouldCreateSessionWithCorrectLessonId()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession.Should().NotBeNull();
        _coordinator.CurrentSession!.LessonId.Should().Be("Lektion1");
    }

    [Fact]
    public void StartSession_ShouldCreateSessionWithCorrectModuleId()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession.Should().NotBeNull();
        _coordinator.CurrentSession!.ModuleId.Should().Be("Modul1");
    }

    [Fact]
    public void StartSession_ShouldSetStartedAtToCurrentTime()
    {
        PrepareTestData();
        var expectedTime = DateTime.Now;
        _clock.SetTime(expectedTime);

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession!.StartedAt.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void StartSession_ShouldSetIsCompletedToFalse()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void StartSession_ShouldInitializeEmptyInputsList()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession!.Inputs.Should().BeEmpty();
    }

    [Fact]
    public void StartSession_ShouldInitializeEmptyEvaluationsList()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentSession!.Evaluations.Should().BeEmpty();
    }

    [Fact]
    public void StartSession_ShouldSetCurrentStateToInitialState()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.CurrentState.Should().NotBeNull();
    }

    [Fact]
    public void StartSession_ShouldSetIsSessionRunningToTrue()
    {
        PrepareTestData();

        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.IsSessionRunning.Should().BeTrue();
    }

    [Fact]
    public void StartSession_WithEmptyModuleId_ShouldThrowArgumentException()
    {
        PrepareTestData();

        var act = () => _coordinator.StartSession("", "Lektion1");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("moduleId");
    }

    [Fact]
    public void StartSession_WithEmptyLessonId_ShouldThrowArgumentException()
    {
        PrepareTestData();

        var act = () => _coordinator.StartSession("Modul1", "");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("lessonId");
    }

    [Fact]
    public void StartSession_WithNonExistentLesson_ShouldThrowInvalidOperationException()
    {
        PrepareTestData();

        var act = () => _coordinator.StartSession("Modul1", "NichtExistent");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*nicht gefunden*");
    }

    [Fact]
    public void StartSession_WithNonExistentModule_ShouldThrowInvalidOperationException()
    {
        PrepareTestData();

        var act = () => _coordinator.StartSession("NichtExistent", "Lektion1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*nicht gefunden*");
    }

    [Fact]
    public void StartSession_WhenSessionAlreadyRunning_ShouldThrowInvalidOperationException()
    {
        PrepareTestData();
        _coordinator.StartSession("Modul1", "Lektion1");

        var act = () => _coordinator.StartSession("Modul1", "Lektion1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*bereits eine Sitzung*");
    }

    private void PrepareTestData()
    {
        var moduleStore = _dataStoreProvider.GetPersistent<ModuleData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);

        var lessonStore = _dataStoreProvider.GetPersistent<LessonData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);

        var module = new ModuleData("Modul1", "Test-Modul");
        var lesson = new LessonData("Lektion1", "Modul1", "Test-Lektion", uebungstext: "abc");

        moduleStore.Add(module);
        lessonStore.Add(lesson);
    }
}
