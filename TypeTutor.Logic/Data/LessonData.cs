using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Data Transfer Object (DTO) für JSON-Serialisierung einer Lesson.
/// Dient für Import/Export-Operationen und JSON-Persistierung.
/// 
/// Diese Klasse ist ein flaches, serialisierbares Objekt, das alle
/// erforderlichen Daten einer Lesson enthält, ohne die komplexe
/// Struktur der Domain-Klasse <see cref="TypeTutor.Logic.Core.Lesson"/>.
/// 
/// Verwendung:
/// - JSON-Import aus externen Dateien (z.B. TypeTutor_Import_LessonData.json)
/// - JSON-Export für Backup/Transfer
/// - Persistierung über DataToolKit JSON-Repository
/// 
/// Verknüpfung zu ModuleData:
/// - Jede Lesson wird einem Modul über das Feld ModuleId zugeordnet
/// - ModuleId entspricht dem ModuleId-Feld in ModuleData
/// - LessonId ist der eindeutige, stabile Identifier der Lesson
/// </summary>
public sealed record LessonData
{
    /// <summary>
    /// Eindeutiger, stabiler Identifier der Lektion (z. B. "L0001", "L0002").
    /// Dient als Primärschlüssel für Repository-Operationen und Referenzierung.
    /// 
    /// Dieser Identifier ist stabil und ändert sich nicht, auch wenn
    /// der Title bearbeitet wird.
    /// </summary>
    public string LessonId { get; init; } = string.Empty;

    /// <summary>
    /// Titel der Lektion (Anzeigename, frei editierbar).
    /// Wird dem Benutzer im UI angezeigt.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Beschreibung oder Zusammenfassung der Lektion.
    /// Wird dem Benutzer im UI angezeigt (z.B. im Trainingsmenü).
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Schwierigkeitsgrad der Lektion (1-5).
    /// - 1 = Einsteiger
    /// - 3 = Standard
    /// - 5 = Experte
    /// </summary>
    public int Difficulty { get; init; } = 1;

    /// <summary>
    /// Tags/Kategorien für Filterung und Gruppierung.
    /// Beispiele: "Grundlagen", "Home Row", "Deutsch"
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Modul-ID zur Zuordnung zu einem übergeordneten Modul.
    /// Verknüpfung: LessonData.ModuleId == ModuleData.ModuleId
    /// Beispiele: "M01", "M02", "M03" … "M07"
    /// </summary>
    public string ModuleId { get; init; } = string.Empty;

    /// <summary>
    /// Der zu tippende Text/Inhalt der Lektion.
    /// Wird von der ILessonFactory normalisiert und in Blöcke aufgeteilt.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public LessonData() { }

    /// <summary>
    /// Konstruktor für programmatische Erstellung.
    /// </summary>
    /// <param name="lessonId">Eindeutiger Identifier der Lektion (erforderlich).</param>
    /// <param name="title">Titel der Lektion (erforderlich).</param>
    /// <param name="content">Inhalt der Lektion (erforderlich).</param>
    public LessonData(string lessonId, string title, string content)
    {
        LessonId = lessonId ?? throw new ArgumentNullException(nameof(lessonId));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}
