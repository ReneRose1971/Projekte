using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Data Transfer Object (DTO) für JSON-Serialisierung eines LessonGuide.
/// Dient für Import/Export-Operationen und JSON-Persistierung.
/// 
/// Ein LessonGuide beschreibt detaillierte Anleitungen für eine einzelne Lesson
/// mit ausführlichen Informationen im Markdown-Format.
/// 
/// Verwendung:
/// - JSON-Import aus externen Dateien (z.B. TypeTutor_Import_LessonGuideData.json)
/// - JSON-Export für Backup/Transfer
/// - Persistierung über DataToolKit JSON-Repository
/// 
/// Verknüpfung zu LessonData:
/// - Ein LessonGuide gehört zu genau einer Lesson (1:1-Beziehung)
/// - Verknüpfung über LessonId (nicht über Title)
/// - Beispiel: LessonGuideData.LessonId = "L0001" ? LessonData.LessonId = "L0001"
/// </summary>
public sealed record LessonGuideData
{
    /// <summary>
    /// Referenz auf die zugehörige Lesson über deren LessonId.
    /// Verknüpfung: LessonGuideData.LessonId == LessonData.LessonId
    /// Beispiele: "L0001", "L0002", "L0003" …
    /// </summary>
    public string LessonId { get; init; } = string.Empty;

    /// <summary>
    /// Markdown-formatierte Beschreibung/Anleitung für die Lesson.
    /// Kann Überschriften, Listen, Code-Blöcke etc. enthalten.
    /// 
    /// Beispiel:
    /// # Lektion: Grundreihe
    /// 
    /// ## Lernziele
    /// - Buchstaben asdf jklö beherrschen
    /// - Fingerposition auf der Grundreihe
    /// 
    /// ## Übungen
    /// 1. Einzelne Buchstaben
    /// 2. Buchstabenkombinationen
    /// </summary>
    public string BodyMarkdown { get; init; } = string.Empty;

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public LessonGuideData() { }

    /// <summary>
    /// Konstruktor für programmatische Erstellung.
    /// </summary>
    /// <param name="lessonId">Referenz auf die Lesson-ID (erforderlich).</param>
    /// <param name="bodyMarkdown">Markdown-Beschreibung (optional).</param>
    public LessonGuideData(string lessonId, string bodyMarkdown = "")
    {
        LessonId = lessonId ?? throw new ArgumentNullException(nameof(lessonId));
        BodyMarkdown = bodyMarkdown ?? string.Empty;
    }
}
