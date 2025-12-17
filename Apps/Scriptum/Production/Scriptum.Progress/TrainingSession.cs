using DataToolKit.Abstractions.Repositories;

namespace Scriptum.Progress;

/// <summary>
/// Repräsentiert eine Trainingssession eines Benutzers.
/// </summary>
public sealed class TrainingSession : EntityBase
{
    private string _lessonId = string.Empty;
    private string _moduleId = string.Empty;
    private DateTimeOffset _startedAt;
    private DateTimeOffset? _endedAt;
    private bool _isCompleted;
    private List<StoredInput> _inputs = new();
    private List<StoredEvaluation> _evaluations = new();
    
    /// <summary>
    /// Die ID der Lektion.
    /// </summary>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
    public string LessonId
    {
        get => _lessonId;
        set => _lessonId = value ?? throw new ArgumentNullException(nameof(value), "LessonId darf nicht null sein.");
    }
    
    /// <summary>
    /// Die ID des Moduls (optional).
    /// </summary>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
    public string ModuleId
    {
        get => _moduleId;
        set => _moduleId = value ?? throw new ArgumentNullException(nameof(value), "ModuleId darf nicht null sein.");
    }
    
    /// <summary>
    /// Der Startzeitpunkt der Session.
    /// </summary>
    public DateTimeOffset StartedAt
    {
        get => _startedAt;
        set => _startedAt = value;
    }
    
    /// <summary>
    /// Der Endzeitpunkt der Session (nur gesetzt, wenn <see cref="IsCompleted"/> true ist).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Wird ausgelöst, wenn versucht wird, <see cref="EndedAt"/> zu setzen, während <see cref="IsCompleted"/> false ist.
    /// </exception>
    public DateTimeOffset? EndedAt
    {
        get => _endedAt;
        set
        {
            if (value.HasValue && !_isCompleted)
                throw new InvalidOperationException("EndedAt darf nur gesetzt werden, wenn IsCompleted true ist.");
            _endedAt = value;
        }
    }
    
    /// <summary>
    /// Gibt an, ob die Session abgeschlossen ist.
    /// </summary>
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            // Wenn IsCompleted auf false gesetzt wird, EndedAt zurücksetzen
            if (!value && _endedAt.HasValue)
                _endedAt = null;
            _isCompleted = value;
        }
    }
    
    /// <summary>
    /// Die Liste aller Eingaben während der Session.
    /// </summary>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn versucht wird, die Liste auf null zu setzen.</exception>
    public List<StoredInput> Inputs
    {
        get => _inputs;
        set => _inputs = value ?? throw new ArgumentNullException(nameof(value), "Inputs darf nicht null sein.");
    }
    
    /// <summary>
    /// Die Liste aller Bewertungen während der Session.
    /// </summary>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn versucht wird, die Liste auf null zu setzen.</exception>
    public List<StoredEvaluation> Evaluations
    {
        get => _evaluations;
        set => _evaluations = value ?? throw new ArgumentNullException(nameof(value), "Evaluations darf nicht null sein.");
    }
    
    /// <summary>
    /// Erstellt eine neue Trainingssession mit Standardwerten.
    /// </summary>
    public TrainingSession()
    {
        _lessonId = string.Empty;
        _moduleId = string.Empty;
        _startedAt = default;
        _endedAt = null;
        _isCompleted = false;
        _inputs = new List<StoredInput>();
        _evaluations = new List<StoredEvaluation>();
    }
}
