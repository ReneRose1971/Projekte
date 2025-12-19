using DataToolKit.Abstractions.Repositories;

namespace Scriptum.Progress;

/// <summary>
/// Repräsentiert eine Trainingssession eines Benutzers.
/// </summary>
/// <remarks>
/// <para>
/// Diese Klasse speichert alle relevanten Daten einer einzelnen Übungssitzung,
/// einschließlich der Eingaben (<see cref="StoredInput"/>) und Bewertungen 
/// (<see cref="StoredEvaluation"/>). Jede Session ist eindeutig über die <see cref="EntityBase.Id"/> 
/// identifizierbar.
/// </para>
/// <para>
/// <b>Persistierung:</b> Diese Entität wird mittels LiteDB persistiert und durch 
/// <see cref="IRepository{T}"/> verwaltet. Die Repository-Registrierung erfolgt in 
/// <c>Scriptum.Persistence</c> über <c>AddLiteDbRepository</c>.
/// </para>
/// <para>
/// <b>Session-Lebenszyklus:</b>
/// <list type="number">
/// <item>Session wird erstellt mit <see cref="StartedAt"/> = aktueller Zeitpunkt</item>
/// <item>Während des Trainings werden <see cref="Inputs"/> und <see cref="Evaluations"/> hinzugefügt</item>
/// <item>Bei Abschluss wird <see cref="IsCompleted"/> = true und <see cref="EndedAt"/> gesetzt</item>
/// </list>
/// </para>
/// </remarks>
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
    /// Die ID der Lektion, die in dieser Session geübt wird.
    /// </summary>
    /// <remarks>
    /// Verweist auf eine Lektion aus <c>Scriptum.Content</c>. Die ID muss mit einer 
    /// existierenden <c>LessonData.Id</c> übereinstimmen.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
    public string LessonId
    {
        get => _lessonId;
        set => _lessonId = value ?? throw new ArgumentNullException(nameof(value), "LessonId darf nicht null sein.");
    }
    
    /// <summary>
    /// Die ID des übergeordneten Moduls (optional).
    /// </summary>
    /// <remarks>
    /// Verweist auf ein Modul aus <c>Scriptum.Content</c>. Die ID muss mit einer 
    /// existierenden <c>ModuleData.Id</c> übereinstimmen. Ein leerer String bedeutet, 
    /// dass die Lektion keinem Modul zugeordnet ist.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn der Wert null ist.</exception>
    public string ModuleId
    {
        get => _moduleId;
        set => _moduleId = value ?? throw new ArgumentNullException(nameof(value), "ModuleId darf nicht null sein.");
    }
    
    /// <summary>
    /// Der Startzeitpunkt der Session.
    /// </summary>
    /// <remarks>
    /// Wird beim Erstellen der Session automatisch auf den aktuellen Zeitpunkt gesetzt.
    /// Dient zur Berechnung der Sessiondauer und zur chronologischen Sortierung.
    /// </remarks>
    public DateTimeOffset StartedAt
    {
        get => _startedAt;
        set => _startedAt = value;
    }
    
    /// <summary>
    /// Der Endzeitpunkt der Session (nur gesetzt, wenn <see cref="IsCompleted"/> true ist).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Darf nur gesetzt werden, wenn <see cref="IsCompleted"/> true ist. Die Differenz 
    /// zwischen <see cref="StartedAt"/> und <see cref="EndedAt"/> ergibt die Gesamtdauer 
    /// der Session.
    /// </para>
    /// <para>
    /// Ein <c>null</c>-Wert bedeutet, dass die Session noch nicht abgeschlossen ist oder 
    /// abgebrochen wurde.
    /// </para>
    /// </remarks>
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
    /// Gibt an, ob die Session erfolgreich abgeschlossen wurde.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Wird auf <c>true</c> gesetzt, wenn der Benutzer die Lektion vollständig durchlaufen hat.
    /// </para>
    /// <para>
    /// Beim Setzen auf <c>false</c> wird <see cref="EndedAt"/> automatisch auf <c>null</c> 
    /// zurückgesetzt, um Inkonsistenzen zu vermeiden.
    /// </para>
    /// </remarks>
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (!value && _endedAt.HasValue)
                _endedAt = null;
            _isCompleted = value;
        }
    }
    
    /// <summary>
    /// Die Liste aller Eingaben während der Session.
    /// </summary>
    /// <remarks>
    /// Enthält chronologisch alle Tastatureingaben des Benutzers während der Übung.
    /// Jedes <see cref="StoredInput"/>-Element speichert die tatsächliche Eingabe, 
    /// den Zeitstempel und die erwartete Eingabe für spätere Analysen.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn versucht wird, die Liste auf null zu setzen.</exception>
    public List<StoredInput> Inputs
    {
        get => _inputs;
        set => _inputs = value ?? throw new ArgumentNullException(nameof(value), "Inputs darf nicht null sein.");
    }
    
    /// <summary>
    /// Die Liste aller Bewertungen während der Session.
    /// </summary>
    /// <remarks>
    /// Enthält die Evaluierungsergebnisse für jeden Eingabeschritt. Jedes 
    /// <see cref="StoredEvaluation"/>-Element korreliert mit einem <see cref="StoredInput"/> 
    /// und speichert, ob die Eingabe korrekt war sowie weitere Metriken.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Wird ausgelöst, wenn versucht wird, die Liste auf null zu setzen.</exception>
    public List<StoredEvaluation> Evaluations
    {
        get => _evaluations;
        set => _evaluations = value ?? throw new ArgumentNullException(nameof(value), "Evaluations darf nicht null sein.");
    }
    
    /// <summary>
    /// Erstellt eine neue Trainingssession mit Standardwerten.
    /// </summary>
    /// <remarks>
    /// Initialisiert alle Felder mit sicheren Standardwerten. <see cref="StartedAt"/> 
    /// sollte nach der Erstellung auf den tatsächlichen Startzeitpunkt gesetzt werden.
    /// </remarks>
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
    
    /// <summary>
    /// Erstellt eine neue TrainingSession für eine Lektion.
    /// </summary>
    /// <param name="lessonId">Die ID der Lektion.</param>
    /// <param name="moduleId">Die ID des Moduls (UI-Navigations-Kontext).</param>
    /// <param name="startedAt">Der Startzeitpunkt der Session.</param>
    /// <returns>Eine neue, nicht abgeschlossene TrainingSession.</returns>
    /// <exception cref="ArgumentException">Wenn lessonId oder moduleId leer sind.</exception>
    public static TrainingSession CreateNew(string lessonId, string moduleId, DateTimeOffset startedAt)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            throw new ArgumentException("LessonId darf nicht leer sein.", nameof(lessonId));
        
        if (string.IsNullOrWhiteSpace(moduleId))
            throw new ArgumentException("ModuleId darf nicht leer sein.", nameof(moduleId));
        
        return new TrainingSession
        {
            LessonId = lessonId,
            ModuleId = moduleId,
            StartedAt = startedAt,
            IsCompleted = false,
            Inputs = new List<StoredInput>(),
            Evaluations = new List<StoredEvaluation>()
        };
    }
}
