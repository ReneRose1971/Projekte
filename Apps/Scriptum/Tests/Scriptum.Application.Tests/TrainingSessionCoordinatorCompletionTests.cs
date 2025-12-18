using FluentAssertions;
using Scriptum.Application.Tests.Fakes;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Engine;
using TestHelper.DataToolKit.Fakes.Providers;
using TestHelper.DataToolKit.Fakes.Repositories;

namespace Scriptum.Application.Tests;

/// <summary>
/// Tests für TrainingSessionCoordinator - Session-Abschluss.
/// </summary>
public sealed class TrainingSessionCoordinatorCompletionTests
{
    private readonly FakeDataStoreProvider _dataStoreProvider;
    private readonly FakeRepositoryFactory _repositoryFactory;
    private readonly FakeClock _clock;
    private readonly ITrainingEngine _engine;
    private readonly IInputInterpreter _interpreter;
    private readonly TrainingSessionCoordinator _coordinator;

    public TrainingSessionCoordinatorCompletionTests()
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

        PrepareTestData();
    }

    [Fact]
    public void WhenSessionCompleted_IsCompletedShouldBeTrue()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        CompleteSession();

        _coordinator.CurrentSession!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void WhenSessionCompleted_EndedAtShouldBeSet()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        CompleteSession();

        _coordinator.CurrentSession!.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void WhenSessionCompleted_IsSessionRunningShouldBeFalse()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        CompleteSession();

        _coordinator.IsSessionRunning.Should().BeFalse();
    }

    [Fact]
    public void WhenSessionCompleted_AllInputsShouldBePersisted()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        CompleteSession();

        _coordinator.CurrentSession!.Inputs.Should().HaveCount(3);
    }

    [Fact]
    public void WhenSessionCompleted_AllEvaluationsShouldBePersisted()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        CompleteSession();

        _coordinator.CurrentSession!.Evaluations.Should().HaveCount(3);
    }

    [Fact]
    public void WhenSessionCompletedWithErrors_ErrorsShouldBePersisted()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.X, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));

        _coordinator.CurrentSession!.Inputs.Should().HaveCount(5);
    }

    [Fact]
    public void WhenSessionCompletedWithErrors_EvaluationsWithFalschAndKorrigiertShouldBePersisted()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.X, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));

        _coordinator.CurrentSession!.Evaluations.Should().Contain(e => e.Ergebnis == EvaluationOutcome.Falsch);
    }

    [Fact]
    public void WhenSessionCompletedWithErrors_KorrigiertEvaluationShouldBePersisted()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.X, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));

        _coordinator.CurrentSession!.Evaluations.Should().Contain(e => e.Ergebnis == EvaluationOutcome.Korrigiert);
    }

    private void CompleteSession()
    {
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));
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
