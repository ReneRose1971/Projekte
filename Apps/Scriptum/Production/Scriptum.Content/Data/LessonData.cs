namespace Scriptum.Content.Data;

/// <summary>
/// Repräsentiert eine Lektion als DTO.
/// </summary>
/// <param name="LessonId">Die eindeutige ID der Lektion (Pflicht, nicht leer).</param>
/// <param name="ModuleId">Die ID des zugehörigen Moduls (Pflicht, nicht leer).</param>
/// <param name="Titel">Der Titel der Lektion (Pflicht, nicht leer).</param>
/// <param name="Beschreibung">Die Beschreibung der Lektion (optional).</param>
/// <param name="Schwierigkeit">Der Schwierigkeitsgrad der Lektion (Standard: 0).</param>
/// <param name="Tags">Liste von Tags für die Lektion (niemals null).</param>
/// <param name="Uebungstext">Der Übungstext der Lektion mit \n als Enter-Zielzeichen (Pflicht, nicht leer).</param>
/// <remarks>
/// <para>
/// Die Lektion ist über <see cref="ModuleId"/> mit einem <see cref="ModuleData"/> verknüpft (1:n).
/// Die 1:1 Beziehung zu <see cref="LessonGuideData"/> wird über <see cref="LessonId"/> hergestellt.
/// </para>
/// </remarks>
/// <exception cref="ArgumentException">
/// Wird ausgelöst, wenn LessonId, ModuleId, Titel oder Uebungstext leer sind.
/// </exception>
public sealed record LessonData
{
    public string LessonId { get; init; }
    public string ModuleId { get; init; }
    public string Titel { get; init; }
    public string Beschreibung { get; init; }
    public int Schwierigkeit { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
    public string Uebungstext { get; init; }

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="LessonData"/>.
    /// </summary>
    /// <param name="lessonId">Die eindeutige ID der Lektion.</param>
    /// <param name="moduleId">Die ID des zugehörigen Moduls.</param>
    /// <param name="titel">Der Titel der Lektion.</param>
    /// <param name="beschreibung">Die Beschreibung der Lektion.</param>
    /// <param name="schwierigkeit">Der Schwierigkeitsgrad.</param>
    /// <param name="tags">Liste von Tags.</param>
    /// <param name="uebungstext">Der Übungstext mit \n als Enter-Zielzeichen.</param>
    /// <exception cref="ArgumentException">
    /// Wird ausgelöst, wenn lessonId, moduleId, titel oder uebungstext leer sind.
    /// </exception>
    public LessonData(
        string lessonId,
        string moduleId,
        string titel,
        string beschreibung = "",
        int schwierigkeit = 0,
        IReadOnlyList<string>? tags = null,
        string? uebungstext = null)
    {
        if (string.IsNullOrWhiteSpace(lessonId))
            throw new ArgumentException("LessonId darf nicht leer sein.", nameof(lessonId));

        if (string.IsNullOrWhiteSpace(moduleId))
            throw new ArgumentException("ModuleId darf nicht leer sein.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Titel darf nicht leer sein.", nameof(titel));

        if (string.IsNullOrWhiteSpace(uebungstext))
            throw new ArgumentException("Uebungstext darf nicht leer sein.", nameof(uebungstext));

        LessonId = lessonId;
        ModuleId = moduleId;
        Titel = titel;
        Beschreibung = beschreibung ?? string.Empty;
        Schwierigkeit = schwierigkeit;
        Tags = tags ?? Array.Empty<string>();
        Uebungstext = uebungstext;
    }
}
