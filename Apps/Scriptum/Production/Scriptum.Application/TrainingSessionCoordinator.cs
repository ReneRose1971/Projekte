using DataToolKit.Abstractions.DataStores;
using Scriptum.Application.Factories;
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
/// <para>
/// <b>DataStore-Verwaltung:</b> Die benötigten DataStores (<see cref="TrainingSession"/>, 
/// <see cref="LessonData"/>) werden vom 
/// <see cref="ScriptumDataStoreInitializer"/> beim Anwendungsstart erstellt und
/// hier im Konstruktor über <see cref="IDataStoreProvider.GetDataStore{T}"/> abgerufen.
/// </para>
/// <para>
/// <b>Hinweis:</b> Die <c>ModuleId</c> in <see cref="TrainingSession"/> dient nur als
/// UI-Navigations-Kontext. Es gibt keine fachliche Abhängigkeit zwischen Lektion und Modul;
/// Lektionen können unabhängig vom Modul geübt werden.
/// </para>
/// </remarks>
public sealed class TrainingSessionCoordinator : ITrainingSessionCoordinator
{
    private readonly ITrainingEngine _engine;
    private readonly IInputInterpreter _interpreter;
    private readonly IClock _clock;

    private readonly IDataStore<TrainingSession> _sessionStore;
    private readonly IDataStore<LessonData> _lessonStore;

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
    /// <exception cref="ArgumentNullException">Wenn einer der Parameter null ist.</exception>
    /// <exception cref="InvalidOperationException">
    /// Wenn ein benötigter DataStore nicht registriert wurde. Dies deutet darauf hin,
    /// dass <see cref="ScriptumDataStoreInitializer"/> nicht ausgeführt wurde.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Die DataStores müssen vor der Konstruktion dieses Services vom 
    /// <see cref="ScriptumDataStoreInitializer"/> initialisiert worden sein.
    /// </para>
    /// </remarks>
    public TrainingSessionCoordinator(
        ITrainingEngine engine,
        IInputInterpreter interpreter,
        IClock clock,
        IDataStoreProvider dataStoreProvider)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));

        if (dataStoreProvider == null)
            throw new ArgumentNullException(nameof(dataStoreProvider));

        _sessionStore = dataStoreProvider.GetDataStore<TrainingSession>();
        _lessonStore = dataStoreProvider.GetDataStore<LessonData>();
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

        var lesson = FindLesson(lessonId);

        var sequence = TargetSequence.FromText(lesson.Uebungstext);
        var startTime = _clock.Now;

        _currentState = _engine.CreateInitialState(sequence, startTime);

        _currentSession = TrainingSession.CreateNew(lessonId, moduleId, DateTimeOffset.Now);

        _sessionStore.Add(_currentSession);
    }

    /// <inheritdoc />
    public EvaluationEvent? ProcessInput(KeyChord chord)
    {
        if (!IsSessionRunning)
            throw new InvalidOperationException("Keine aktive Sitzung.");

        var timestamp = _clock.Now;
        var inputEvent = _interpreter.Interpret(chord, timestamp);

        var storedInput = StoredInputFactory.FromInputEvent(inputEvent, chord, DateTimeOffset.Now);
        _currentSession!.Inputs.Add(storedInput);

        var (newState, evaluation) = _engine.ProcessInput(_currentState!, inputEvent);
        _currentState = newState;

        if (evaluation != null)
        {
            var storedEvaluation = StoredEvaluationFactory.FromEvaluationEvent(evaluation);
            _currentSession.Evaluations.Add(storedEvaluation);
        }

        if (_currentState.IstAbgeschlossen && !_currentSession.IsCompleted)
        {
            _currentSession.IsCompleted = true;
            _currentSession.EndedAt = DateTimeOffset.Now;
        }

        return evaluation;
    }

    private LessonData FindLesson(string lessonId)
    {
        var lesson = _lessonStore.Items.FirstOrDefault(l => l.LessonId == lessonId);
        if (lesson == null)
            throw new InvalidOperationException($"Lektion mit ID '{lessonId}' wurde nicht gefunden.");
        return lesson;
    }
}
