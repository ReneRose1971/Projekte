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
/// Verknüpfung zu LessonGuideData:
/// - Jede Lesson wird einem LessonGuide über das Feld ModuleId zugeordnet
/// - ModuleId entspricht dem Title-Feld in LessonGuideData
/// </summary>
public sealed record LessonData
{
    /// <summary>
    /// Titel der Lektion (eindeutiger Identifier).
    /// Dient als Primärschlüssel für Repository-Operationen.
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
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Modul-ID zur Zuordnung zu einem übergeordneten LessonGuide.
    /// Verknüpfung: LessonData.ModuleId == LessonGuideData.Title
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
    /// <param name="title">Titel der Lektion (erforderlich).</param>
    /// <param name="content">Inhalt der Lektion (erforderlich).</param>
    public LessonData(string title, string content)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }
}

/// <summary>
/// Root-Container für JSON-Dateien mit mehreren Lessons.
/// Struktur: { "Lessons": [ {...}, {...} ] }
/// 
/// Verwendung für Import/Export:
/// - TypeTutor_Import_LessonData.json
/// - Batch-Import von Lessons
/// </summary>
public sealed record LessonDataContainer
{
    /// <summary>
    /// Liste aller Lessons im Container.
    /// </summary>
    public List<LessonData> Lessons { get; init; } = new();

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public LessonDataContainer() { }
}
