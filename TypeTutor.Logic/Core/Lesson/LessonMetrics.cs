using System.Linq;

namespace TypeTutor.Logic.Core;

/// <summary>
/// Beschreibt die dauerhaften, aus den Textblöcken einer Lesson
/// abgeleiteten Metriken. Diese Kennzahlen dienen dazu, Eigenschaften
/// der Lesson präzise und stabil zu quantifizieren.
/// 
/// <para>
/// Im Gegensatz zu temporären „Stats“ oder Session-Daten werden
/// <see cref="LessonMetrics"/> vollständig aus den normalisierten
/// Blöcken berechnet, die eine Lesson definieren. Die Metriken sind
/// also eine unveränderliche, deterministische Eigenschaft der Lesson
/// selbst – unabhängig vom Benutzer, der Tippgeschwindigkeit oder vom
/// Engine-Zustand. Sie eignen sich perfekt für:
/// </para>
/// 
/// <list type="bullet">
///   <item><description>Analyse und strukturelle Bewertung von Lessons</description></item>
///   <item><description>Klassifikation (z. B. komplex vs. einfach)</description></item>
///   <item><description>Trainingslogik (z. B. Auswahl nächster Lessons)</description></item>
///   <item><description>Reporting (z. B. Zeichenanzahl, Blockanzahl)</description></item>
///   <item><description>Schwierigkeitsscores, Progression, Auto-Generierung</description></item>
/// </list>
/// 
/// <h3>Berechnungsregeln</h3>
/// <para>
/// Die Metriken werden streng nach der Logik der Lesson berechnet.
/// Eine Lesson besteht aus <i>normalisierten Blöcken</i>. Der Engine-
/// freundliche Zieltext (<see cref="Lesson.TargetText"/>) entsteht
/// dabei durch das Verbinden dieser Blöcke mit genau einem Leerzeichen.
/// </para>
/// 
/// <h4>BlockCount</h4>
/// Anzahl der Textblöcke, die die Lesson bilden.
/// Jeder Block ist ein nicht-leerer String, bereits getrimmt.
/// 
/// <h4>CharacterCount</h4>
/// Anzahl aller Zeichen im <c>TargetText</c>.
/// Das entspricht:
/// <code>
/// Summe( Länge aller Blöcke ) + (BlockCount - 1)
/// </code>
/// da zwischen jedem Block genau ein Leerzeichen eingefügt wird.
/// 
/// <h4>IsEmpty</h4>
/// True, wenn <see cref="CharacterCount"/> == 0.
/// Dies tritt nur ein, wenn die Lesson keinerlei gültige Blöcke enthält.
/// 
/// <h3>Warum ein Value Object?</h3>
/// <para>
/// <see cref="LessonMetrics"/> ist ein <c>readonly record struct</c>.
/// Dadurch besitzt es:
/// </para>
/// <list type="bullet">
///   <item><description>Value Equality (zwei identische Metriken gelten als gleich)</description></item>
///   <item><description>keine Mutationen (Immutable)</description></item>
///   <item><description>Performante Speicherung ohne Heap-Allocation</description></item>
///   <item><description>klar abgegrenzte Verantwortung: reine Datenrepräsentation</description></item>
/// </list>
/// 
/// <h3>Zukunftserweiterungen</h3>
/// <para>
/// Die Klasse ist bewusst minimal gehalten, lässt sich aber später
/// problemlos erweitern, z. B. um:
/// </para>
/// <list type="bullet">
///   <item><description>Wortanzahl</description></item>
///   <item><description>Anzahl einzigartiger Zeichen</description></item>
///   <item><description>Häufigkeit von Zeichen (Frequenztabelle)</description></item>
///   <item><description>Fingerverteilung (links/rechts, Home-Row-Nutzung)</description></item>
///   <item><description>Komplexitätsindikatoren (Bigramme, Trigramme)</description></item>
///   <item><description>Sprachstatistische Kennzahlen</description></item>
/// </list>
/// 
/// <h3>Fazit</h3>
/// <para>
/// <see cref="LessonMetrics"/> bildet das quantitative Fundament einer
/// Lesson. Es abstrahiert vom konkreten Textinhalt und bietet klare,
/// unveränderliche Kennzahlen, die sowohl von der Engine als auch von
/// Analyse- oder Lernlogik-Modulen genutzt werden können.
/// </para>
/// </summary>
public readonly record struct LessonMetrics(
    int BlockCount,
    int CharacterCount,
    bool IsEmpty
)
{
    /// <summary>
    /// Berechnet alle Metriken einer Lesson direkt aus der vorhandenen
    /// Blockstruktur. Erwartet wird eine normalisierte Blockliste, wie
    /// sie von <see cref="ILessonFactory"/> erzeugt und von
    /// <see cref="Lesson"/> verwendet wird.
    /// </summary>
    public static LessonMetrics FromBlocks(IReadOnlyList<string> blocks)
    {
        if (blocks is null)
            throw new ArgumentNullException(nameof(blocks));

        var blockCount = blocks.Count;

        // TargetText = string.Join(" ", blocks)
        // Länge = Summe(Blöcke) + (BlockCount - 1) Spaces
        var charCount = blockCount switch
        {
            0 => 0,
            1 => blocks[0].Length,
            _ => blocks.Sum(b => b.Length) + (blockCount - 1)
        };

        return new LessonMetrics(
            BlockCount: blockCount,
            CharacterCount: charCount,
            IsEmpty: charCount == 0
        );
    }
}
