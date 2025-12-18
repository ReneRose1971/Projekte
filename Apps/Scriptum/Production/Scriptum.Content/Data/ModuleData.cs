namespace Scriptum.Content.Data;

/// <summary>
/// Repräsentiert ein Lernmodul als DTO.
/// </summary>
/// <param name="ModuleId">Die eindeutige ID des Moduls (Pflicht, nicht leer).</param>
/// <param name="Titel">Der Titel des Moduls (Pflicht, nicht leer).</param>
/// <param name="Beschreibung">Die Beschreibung des Moduls (optional).</param>
/// <param name="Order">Die Anzeigereihenfolge des Moduls (Standard: 0).</param>
/// <remarks>
/// <para>
/// Die 1:n Beziehung zu Lektionen wird über <see cref="LessonData.ModuleId"/> hergestellt
/// und nicht im Modul selbst gespeichert.
/// </para>
/// </remarks>
/// <exception cref="ArgumentException">Wird ausgelöst, wenn ModuleId oder Titel leer sind.</exception>
public sealed record ModuleData
{
    public string ModuleId { get; init; }
    public string Titel { get; init; }
    public string Beschreibung { get; init; }
    public int Order { get; init; }

    /// <summary>
    /// Erstellt eine neue Instanz von <see cref="ModuleData"/>.
    /// </summary>
    /// <param name="moduleId">Die eindeutige ID des Moduls.</param>
    /// <param name="titel">Der Titel des Moduls.</param>
    /// <param name="beschreibung">Die Beschreibung des Moduls.</param>
    /// <param name="order">Die Anzeigereihenfolge.</param>
    /// <exception cref="ArgumentException">Wird ausgelöst, wenn moduleId oder titel leer sind.</exception>
    public ModuleData(
        string moduleId,
        string titel,
        string beschreibung = "",
        int order = 0)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
            throw new ArgumentException("ModuleId darf nicht leer sein.", nameof(moduleId));

        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Titel darf nicht leer sein.", nameof(titel));

        ModuleId = moduleId;
        Titel = titel;
        Beschreibung = beschreibung ?? string.Empty;
        Order = order;
    }
}
