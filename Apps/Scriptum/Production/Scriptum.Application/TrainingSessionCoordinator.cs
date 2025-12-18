using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using Scriptum.Content.Data;
using Scriptum.Core;
using Scriptum.Engine;
using Scriptum.Progress;

namespace Scriptum.Application;

/// <summary>
/// Koordiniert Trainingssitzungen im Scriptum-System.
/// </summary>
/// <remarks>
/// <para>
/// Dieser Dienst verbindet alle Schichten:
/// </para>
/// <list type="bullet">
/// <item><b>Content</b>: Lädt Lektionen und Module</item>
/// <item><b>Engine</b>: Führt Training und Bewertung durch</item>
/// <item><b>Progress</b>: Speichert Sitzungsverlauf</item>
/// <item><b>Persistence</b>: Nutzt DataToolKit für alle Speicheroperationen</item>
/// </list>
/// </remarks>
public sealed class TrainingSessionCoordinator : ITrainingSessionCoordinator
{
    private readonly ITrainingEngine _engine;
    private readonly IInputInterpreter _interpreter;
    private readonly IClock _clock;
    private readonly IDataStoreProvider _dataStoreProvider;
    private readonly IRepositoryFactory _repositoryFactory;

    private PersistentDataStore<TrainingSession>? _sessionStore;
    private PersistentDataStore<LessonData>? _lessonStore;
    private PersistentDataStore<ModuleData>? _moduleStore;

    private TrainingSession? _currentSession;
    private TrainingState? _currentState;

    /// <inheritdoc />
    public TrainingSession? CurrentSession => _currentSession;

    /// <inheritdoc />
    public TrainingState? CurrentState => _currentState;

    /// <inheritdoc />
    public bool IsSessionRunning => _currentSession != null && !_currentSession.IsCompleted;

    /// <summary>
    /// Erstellt einen neuen TrainingSessionCoordinator.
    /// </summary>
    /// <param name="engine">Die Trainingsmaschine.</param>
    /// <param name="interpreter">Der Eingabe-Interpreter (z.B. DeQwertzInputInterpreter).</param>
    /// <param name="clock">Die Zeitsteuerung.</param>
    /// <param name="dataStoreProvider">Provider für DataStores.</param>
    /// <param name="repositoryFactory">Factory für Repositories.</param>
    public TrainingSessionCoordinator(
        ITrainingEngine engine,
        IInputInterpreter interpreter,
        IClock clock,
        IDataStoreProvider dataStoreProvider,
        IRepositoryFactory repositoryFactory)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _dataStoreProvider = dataStoreProvider ?? throw new ArgumentNullException(nameof(dataStoreProvider));
        _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
    }

    /// <inheritdoc />
    public void StartSession(string moduleId, string lessonId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
            throw new ArgumentException("ModuleId darf nicht leer sein.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(lessonId))
            throw new ArgumentException("LessonId darf nicht leer sein.", nameof(lessonId));

        if (IsSessionRunning)
            throw new InvalidOperationException("Es läuft bereits eine Sitzung.");

        EnsureDataStoresLoaded();

        var lesson = FindLesson(lessonId);
        var module = FindModule(moduleId);

        ValidateLessonBelongsToModule(lesson, module);

        var sequence = CreateTargetSequence(lesson.Uebungstext);
        var startTime = _clock.Now;

        _currentState = _engine.CreateInitialState(sequence, startTime);

        _currentSession = new TrainingSession
        {
            LessonId = lessonId,
            ModuleId = moduleId,
            StartedAt = DateTimeOffset.Now,
            IsCompleted = false,
            Inputs = new List<StoredInput>(),
            Evaluations = new List<StoredEvaluation>()
        };

        _sessionStore!.Add(_currentSession);
    }

    /// <inheritdoc />
    public EvaluationEvent? ProcessInput(KeyChord chord)
    {
        if (!IsSessionRunning)
            throw new InvalidOperationException("Keine aktive Sitzung.");

        var timestamp = _clock.Now;
        var inputEvent = _interpreter.Interpret(chord, timestamp);

        var storedInput = new StoredInput
        {
            Zeitpunkt = DateTimeOffset.Now,
            Taste = chord.Key,
            Umschalter = chord.Modifiers,
            Art = MapInputEventKind(inputEvent.Kind),
            ErzeugtesGraphem = inputEvent.Graphem ?? string.Empty
        };

        _currentSession!.Inputs.Add(storedInput);

        var (newState, evaluation) = _engine.ProcessInput(_currentState!, inputEvent);
        _currentState = newState;

        if (evaluation != null)
        {
            var storedEvaluation = new StoredEvaluation
            {
                TokenIndex = evaluation.TargetIndex,
                Erwartet = evaluation.ExpectedGraphem,
                Tatsaechlich = evaluation.ActualGraphem,
                Ergebnis = evaluation.Outcome
            };

            _currentSession.Evaluations.Add(storedEvaluation);
        }

        if (_currentState.IstAbgeschlossen && !_currentSession.IsCompleted)
        {
            _currentSession.IsCompleted = true;
            _currentSession.EndedAt = DateTimeOffset.Now;
        }

        return evaluation;
    }

    private void EnsureDataStoresLoaded()
    {
        if (_sessionStore == null)
        {
            _sessionStore = _dataStoreProvider.GetPersistent<TrainingSession>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: true,
                autoLoad: true);
        }

        if (_lessonStore == null)
        {
            _lessonStore = _dataStoreProvider.GetPersistent<LessonData>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
        }

        if (_moduleStore == null)
        {
            _moduleStore = _dataStoreProvider.GetPersistent<ModuleData>(
                _repositoryFactory,
                isSingleton: true,
                trackPropertyChanges: false,
                autoLoad: true);
        }
    }

    private LessonData FindLesson(string lessonId)
    {
        var lesson = _lessonStore!.Items.FirstOrDefault(l => l.LessonId == lessonId);
        if (lesson == null)
            throw new InvalidOperationException($"Lektion mit ID '{lessonId}' wurde nicht gefunden.");
        return lesson;
    }

    private ModuleData FindModule(string moduleId)
    {
        var module = _moduleStore!.Items.FirstOrDefault(m => m.ModuleId == moduleId);
        if (module == null)
            throw new InvalidOperationException($"Modul mit ID '{moduleId}' wurde nicht gefunden.");
        return module;
    }

    private static void ValidateLessonBelongsToModule(LessonData lesson, ModuleData module)
    {
        if (lesson.ModuleId != module.ModuleId)
        {
            throw new InvalidOperationException(
                $"Lektion '{lesson.LessonId}' gehört nicht zu Modul '{module.ModuleId}'.");
        }
    }

    private static TargetSequence CreateTargetSequence(string uebungstext)
    {
        var graphemes = uebungstext.Select(c => c.ToString());
        return new TargetSequence(graphemes);
    }

    private static StoredInputKind MapInputEventKind(InputEventKind kind)
    {
        return kind switch
        {
            InputEventKind.Zeichen => StoredInputKind.Zeichen,
            InputEventKind.Ruecktaste => StoredInputKind.Ruecktaste,
            InputEventKind.Ignoriert => StoredInputKind.Ignoriert,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unbekannte InputEventKind.")
        };
    }
}
