using System;

namespace TypeTutor.Logic.Data;

/// <summary>
/// Data Transfer Object (DTO) für JSON-Serialisierung eines Moduls.
/// Dient für Import/Export-Operationen und JSON-Persistierung.
/// 
/// Ein Modul ist eine übergeordnete Lerneinheit, die mehrere Lessons gruppiert
/// und eine didaktische Reihenfolge definiert.
/// 
/// Verwendung:
/// - JSON-Import aus externen Dateien
/// - JSON-Export für Backup/Transfer
/// - Persistierung über DataToolKit JSON-Repository
/// 
/// Verknüpfung zu LessonData:
/// - Ein Modul kann mehrere Lessons enthalten (1:n-Beziehung)
/// - Lessons verweisen über das ModuleId-Feld auf ein Modul
/// - Beispiel: ModuleData.ModuleId = "M01" ? LessonData.ModuleId = "M01"
/// </summary>
public sealed record ModuleData
{
    /// <summary>
    /// Eindeutiger, stabiler Identifier des Moduls (z. B. "M01", "M02").
    /// Dient als Primärschlüssel für Repository-Operationen und als
    /// Verknüpfungspunkt für Lessons (über LessonData.ModuleId).
    /// 
    /// Dieser Identifier ist stabil und ändert sich nicht, auch wenn
    /// der Title bearbeitet wird.
    /// </summary>
    public string ModuleId { get; init; } = string.Empty;

    /// <summary>
    /// Anzeigename des Moduls (frei editierbar).
    /// Beispiel: "Grundreihe", "Obere Reihe", "Zahlen und Sonderzeichen"
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Markdown-formatierter Einführungstext für das Modul.
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
    public string IntroMarkdown { get; init; } = string.Empty;

    /// <summary>
    /// Didaktische Reihenfolge des Moduls (für Sortierung/Anzeige).
    /// Kleinere Werte erscheinen zuerst.
    /// Beispiel: 1, 2, 3, ...
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Parameterloser Konstruktor für JSON-Deserializer.
    /// </summary>
    public ModuleData() { }

    /// <summary>
    /// Konstruktor für programmatische Erstellung.
    /// </summary>
    /// <param name="moduleId">Eindeutiger Identifier des Moduls (erforderlich).</param>
    /// <param name="title">Anzeigename des Moduls (optional).</param>
    /// <param name="introMarkdown">Markdown-Einführungstext (optional).</param>
    /// <param name="order">Didaktische Reihenfolge (optional, Standard: 0).</param>
    public ModuleData(string moduleId, string title = "", string introMarkdown = "", int order = 0)
    {
        ModuleId = moduleId ?? throw new ArgumentNullException(nameof(moduleId));
        Title = title ?? string.Empty;
        IntroMarkdown = introMarkdown ?? string.Empty;
        Order = order;
    }
}
