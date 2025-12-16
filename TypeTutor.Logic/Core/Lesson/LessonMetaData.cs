using System;
using System.Collections.Generic;

namespace TypeTutor.Logic.Core;

/// <summary>
/// Repräsentiert alle Metadaten einer Lesson (Lerneinheit).
/// 
/// Dieses Objekt enthält ausschließlich beschreibende Informationen
/// über eine Lesson – jedoch keinerlei eigentlichen Übungsinhalt.
/// Es handelt sich um ein unveränderliches Value-Object
/// (sealed record), dessen Aufgabe die semantische Beschreibung
/// einer Lektion ist.
///
/// Typische Verwendung:
/// - Anzeigename (Title)
/// - Beschreibender Text (Description)
/// - Klassifikation nach Schwierigkeit (Difficulty)
/// - Zuordnung zu Kategorien/Themen (Tags)
/// - Optional: Zuordnung zu einem Modul (ModuleId)
///
/// Die tatsächlichen Blöcke und der zu tippende Text gehören NICHT
/// in diese Klasse. Dafür existiert separat die Klasse <see cref="Lesson"/>.
/// 
/// Warum sealed record?
/// ---------------------
/// - Ein record bietet Value-Equality: Zwei Metaobjekte mit denselben Werten
///   gelten als gleich.
/// - sealed verhindert Vererbung. Das ist wichtig, weil Metadaten ein
///   abgeschlossenes, wohldefiniertes Value-Object sind, das nicht
///   erweitert oder polymorph verwendet werden soll.
/// - Dieses Objekt ist stabil, sicher zu serialisieren und ideal für
///   JSON/Persistenz.
///
/// ModuleId
/// --------
/// Optionales Feld zur Zuordnung dieser Lesson zu einem übergeordneten
/// Modul. Kein Geschäftsverhalten wird hier implementiert; es dient
/// ausschließlich der semantischen Gruppierung und Anzeige.
/// Beispiele: "M01", "M02", "M03" … (z. B. "M01" bis "M07").
/// </summary>
public sealed record LessonMetaData(string Title)
{
    /// <summary>
    /// Eine frei formulierte Beschreibung oder Zusammenfassung.
    /// Sie kann dem Benutzer angezeigt werden (z. B. im Trainingsmenü).
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Schwierigkeitsgrad der Lesson. Die Bedeutung dieses Wertes ist
    /// domänenspezifisch und kann später erweitert werden
    /// (z. B. 1 = Einsteiger, 3 = Standard, 5 = Experte).
    /// </summary>
    public int Difficulty { get; init; }

    /// <summary>
    /// Freie Schlüsselwörter/Kategorien, die das Auffinden, Filtern oder
    /// Gruppieren von Lessons erleichtern. Beispiel:
    /// ["Grundlagen", "Home Row", "Deutsch"].
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Optionaler Identifier des übergeordneten Moduls.
    /// Dieses Feld ist init-only; Standard ist ein leerer String.
    /// Beispiele: "M01", "M02", "M03" … "M07".
    /// Keine Logik oder Validierung wird in dieser Klasse durchgeführt.
    /// </summary>
    public string ModuleId { get; init; } = string.Empty;
}
