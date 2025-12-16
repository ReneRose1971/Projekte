using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Data Transfer Object (DTO) für JSON-Serialisierung eines LessonGuide/Moduls.
/// Dient für Import/Export-Operationen und JSON-Persistierung.
/// 
/// Ein LessonGuide beschreibt ein übergeordnetes Modul (z.B. "M01: Grundreihe")
/// mit einer ausführlichen Anleitung im Markdown-Format.
/// 
/// Verwendung:
/// - JSON-Import aus externen Dateien (z.B. TypeTutor_Import_LessonGuideData.json)
/// - JSON-Export für Backup/Transfer
/// - Persistierung über DataToolKit JSON-Repository
/// 
/// Verknüpfung zu LessonData:
/// - Der Title eines LessonGuide entspricht dem ModuleId-Feld in LessonData
/// - Ein LessonGuide kann mehrere Lessons enthalten (1:n-Beziehung)
/// - Beispiel: LessonGuideData.Title = "M01" ? LessonData.ModuleId = "M01"
/// </summary>
public sealed record LessonGuideData
{
    /// <summary>
    /// Titel des Moduls (eindeutiger Identifier).
    /// Dient als Primärschlüssel für Repository-Operationen und
    /// als Verknüpfungspunkt für Lessons (über LessonData.ModuleId).
    /// 
    /// Beispiele: "M01", "M02", "M03" … "M07"
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Markdown-formatierte Beschreibung/Anleitung des Moduls.
    /// Kann Überschriften, Listen, Code-Blöcke etc. enthalten.
    /// 
    /// Beispiel:
    /// # Modul 01: Grundreihe
    /// 
    /// ## Lernziele
    /// - Buchstaben asdf jklö beherrschen
    /// - Fingerposition auf der Grundreihe
    /// 
    /// ## Übungen
    /// 1. Einzelne Buchstaben
    /// 2. Buchstabenkombinationen
    /// </summary>
    public string BodyMarkDown { get; init; } = string.Empty;

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public LessonGuideData() { }

    /// <summary>
    /// Konstruktor für programmatische Erstellung.
    /// </summary>
    /// <param name="title">Titel des Moduls (erforderlich).</param>
    /// <param name="bodyMarkDown">Markdown-Beschreibung (optional).</param>
    public LessonGuideData(string title, string bodyMarkDown = "")
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        BodyMarkDown = bodyMarkDown ?? string.Empty;
    }
}

/// <summary>
/// Root-Container für JSON-Dateien mit mehreren LessonGuides/Modulen.
/// Struktur: { "ModuleGuides": [ {...}, {...} ] }
/// 
/// Verwendung für Import/Export:
/// - TypeTutor_Import_LessonGuideData.json
/// - Batch-Import von Modulen
/// </summary>
public sealed record LessonGuideDataContainer
{
    /// <summary>
    /// Liste aller Module/LessonGuides im Container.
    /// </summary>
    public List<LessonGuideData> ModuleGuides { get; init; } = new();

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public LessonGuideDataContainer() { }
}
