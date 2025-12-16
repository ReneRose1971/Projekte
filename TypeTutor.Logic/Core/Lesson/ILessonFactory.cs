namespace TypeTutor.Logic.Core;

/// <summary>
/// Erzeugt gültige Lesson-Instanzen aus Rohdaten.
/// Verantwortlich für Validierung, Trimmen, Whitespace-Normalisierung
/// und optionale Blockbildung (Word-Wrap).
/// </summary>
public interface ILessonFactory
{
    /// <summary>
    /// Erzeugt eine Lesson aus vorhandenen Blöcken.
    /// - Whitespace wird in jedem Block kollabiert (\\s+ → " ") und getrimmt.
    /// - Leere/Whitespace-Blöcke werden entfernt.
    /// - Danach wird eine gültige Lesson konstruiert.
    /// </summary>
    Lesson Create(LessonMetaData meta, IEnumerable<string?> blocks);

    /// <summary>
    /// Erzeugt eine Lesson aus einem Rohtext:
    /// - Whitespace im Text wird kollabiert/trimmt (\\s+ → " ")
    /// - Soft Word-Wrap in Blöcke bis maxBlockLen
    /// - Extrem lange Wörter werden hart umgebrochen
    /// - Danach wird eine gültige Lesson konstruiert.
    /// </summary>
    Lesson FromText(LessonMetaData meta, string text, int maxBlockLen = 24);
}
