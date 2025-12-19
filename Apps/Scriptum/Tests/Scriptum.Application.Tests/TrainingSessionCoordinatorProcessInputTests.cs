using FluentAssertions;
using Scriptum.Application.Tests.Fakes;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;
using TestHelper.DataToolKit.Fakes.Providers;
using TestHelper.DataToolKit.Fakes.Repositories;

namespace Scriptum.Application.Tests;

/// <summary>
/// Tests für TrainingSessionCoordinator - ProcessInput.
/// </summary>
public sealed class TrainingSessionCoordinatorProcessInputTests
{
    private readonly FakeDataStoreProvider _dataStoreProvider;
    private readonly FakeRepositoryFactory _repositoryFactory;
    private readonly FakeClock _clock;
    private readonly ITrainingEngine _engine;
    private readonly IInputInterpreter _interpreter;
    private readonly TrainingSessionCoordinator _coordinator;

    public TrainingSessionCoordinatorProcessInputTests()
    {
        _repositoryFactory = new FakeRepositoryFactory();
        _dataStoreProvider = new FakeDataStoreProvider(_repositoryFactory);
        _clock = new FakeClock();
        _engine = new TrainingEngine();
        _interpreter = new DeQwertzInputInterpreter();

        PrepareDataStores();

        _coordinator = new TrainingSessionCoordinator(
            _engine,
            _interpreter,
            _clock,
            _dataStoreProvider);

        PrepareTestData();
    }

    [Fact]
    public void ProcessInput_ShouldCreateStoredInputEntry()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        _coordinator.ProcessInput(chord);

        _coordinator.CurrentSession!.Inputs.Should().HaveCount(1);
    }

    [Fact]
    public void ProcessInput_ShouldStoreCorrectKeyInStoredInput()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        _coordinator.ProcessInput(chord);

        _coordinator.CurrentSession!.Inputs[0].Taste.Should().Be(KeyId.A);
    }

    [Fact]
    public void ProcessInput_ShouldStoreCorrectModifiersInStoredInput()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.A, ModifierSet.Shift);

        _coordinator.ProcessInput(chord);

        _coordinator.CurrentSession!.Inputs[0].Umschalter.Should().Be(ModifierSet.Shift);
    }

    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldCreateEvaluationEntry()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        _coordinator.ProcessInput(chord);

        _coordinator.CurrentSession!.Evaluations.Should().HaveCount(1);
    }

    [Fact]
    public void ProcessInput_WithCorrectCharacter_ShouldReturnRichtigEvaluation()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        var result = _coordinator.ProcessInput(chord);

        result.Should().NotBeNull();
        result!.Outcome.Should().Be(EvaluationOutcome.Richtig);
    }

    [Fact]
    public void ProcessInput_WithIncorrectCharacter_ShouldReturnFalschEvaluation()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        var chord = new KeyChord(KeyId.X, ModifierSet.None);

        var result = _coordinator.ProcessInput(chord);

        result.Should().NotBeNull();
        result!.Outcome.Should().Be(EvaluationOutcome.Falsch);
    }

    [Fact]
    public void ProcessInput_WithBackspaceAfterError_ShouldReturnKorrigiertEvaluation()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.X, ModifierSet.None));

        var result = _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));

        result.Should().NotBeNull();
        result!.Outcome.Should().Be(EvaluationOutcome.Korrigiert);
    }

    [Fact]
    public void ProcessInput_WithBackspaceWithoutError_ShouldReturnNull()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        var result = _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));

        result.Should().BeNull();
    }

    [Fact]
    public void ProcessInput_WhenCompletingLesson_ShouldSetIsCompletedToTrue()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));

        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));

        _coordinator.CurrentSession!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void ProcessInput_WhenCompletingLesson_ShouldSetEndedAt()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
        _coordinator.ProcessInput(new KeyChord(KeyId.B, ModifierSet.None));

        _coordinator.ProcessInput(new KeyChord(KeyId.C, ModifierSet.None));

        _coordinator.CurrentSession!.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void ProcessInput_WithNoActiveSession_ShouldThrowInvalidOperationException()
    {
        var chord = new KeyChord(KeyId.A, ModifierSet.None);

        var act = () => _coordinator.ProcessInput(chord);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*keine aktive Sitzung*");
    }

    [Fact]
    public void ProcessInput_ShouldStoreInputKindZeichen()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));

        _coordinator.CurrentSession!.Inputs[0].Art.Should().Be(StoredInputKind.Zeichen);
    }

    [Fact]
    public void ProcessInput_WithBackspace_ShouldStoreInputKindRuecktaste()
    {
        _coordinator.StartSession("Modul1", "Lektion1");
        _coordinator.ProcessInput(new KeyChord(KeyId.X, ModifierSet.None));

        _coordinator.ProcessInput(new KeyChord(KeyId.Backspace, ModifierSet.None));

        _coordinator.CurrentSession!.Inputs[1].Art.Should().Be(StoredInputKind.Ruecktaste);
    }

    [Fact]
    public void ProcessInput_WithIgnoredKey_ShouldStoreInputKindIgnoriert()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.Escape, ModifierSet.None));

        _coordinator.CurrentSession!.Inputs[0].Art.Should().Be(StoredInputKind.Ignoriert);
    }

    [Fact]
    public void ProcessInput_ShouldStoreCorrectGraphem()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));

        _coordinator.CurrentSession!.Inputs[0].ErzeugtesGraphem.Should().Be("a");
    }

    [Fact]
    public void ProcessInput_ShouldStoreEvaluationWithCorrectTokenIndex()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));

        _coordinator.CurrentSession!.Evaluations[0].TokenIndex.Should().Be(0);
    }

    [Fact]
    public void ProcessInput_ShouldStoreEvaluationWithCorrectErwartet()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));

        _coordinator.CurrentSession!.Evaluations[0].Erwartet.Should().Be("a");
    }

    [Fact]
    public void ProcessInput_ShouldStoreEvaluationWithCorrectTatsaechlich()
    {
        _coordinator.StartSession("Modul1", "Lektion1");

        _coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));

        _coordinator.CurrentSession!.Evaluations[0].Tatsaechlich.Should().Be("a");
    }

    private void PrepareDataStores()
    {
        _dataStoreProvider.GetPersistent<TrainingSession>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: true,
            autoLoad: false);

        _dataStoreProvider.GetPersistent<ModuleData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);

        _dataStoreProvider.GetPersistent<LessonData>(
            _repositoryFactory,
            isSingleton: true,
            trackPropertyChanges: false,
            autoLoad: false);
    }

    private void PrepareTestData()
    {
        var moduleStore = _dataStoreProvider.GetDataStore<ModuleData>();
        var lessonStore = _dataStoreProvider.GetDataStore<LessonData>();

        var module = new ModuleData("Modul1", "Test-Modul");
        var lesson = new LessonData("Lektion1", "Modul1", "Test-Lektion", uebungstext: "abc");

        moduleStore.Add(module);
        lessonStore.Add(lesson);
    }
}
