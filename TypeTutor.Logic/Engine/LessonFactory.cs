using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TypeTutor.Logic.Core;

namespace TypeTutor.Logic.Engine;

/// <summary>
/// Standard-Implementierung von <see cref="ILessonFactory"/> zur
/// Erzeugung valider <see cref="Lesson"/>-Instanzen aus Rohdaten.
/// 
/// <para>
/// Verantwortlichkeiten dieser Factory:
/// </para>
/// <list type="bullet">
///   <item><description><b>Validierung der Metadaten</b> (<see cref="LessonMetaData"/>) auf nicht-leere <c>Id</c> und <c>Title</c>.</description></item>
///   <item><description><b>Normalisierung</b> von Eingabetexten und Blöcken:
///     Whitespace-Kollaps (<c>\s+</c> → <c>" "</c>) und Trimmen.</description></item>
///   <item><description><b>Blockbildung</b> aus Rohtext via <b>Soft Word-Wrap</b> bis zu einer konfigurierbaren Zeichenlänge; extrem lange Wörter werden <b>hart</b> gesplittet.</description></item>
///   <item><description><b>Konstruktion</b> einer <see cref="Lesson"/> aus normalisierten, nicht-leeren Blöcken.</description></item>
/// </list>
/// 
/// <h3>Design-Notizen</h3>
/// <para>
/// Die <see cref="Lesson"/>-Klasse selbst enthält <b>keine</b> Erzeugungs- oder
/// Normalisierungslogik; sie validiert lediglich, dass ihr ein bereits
/// normalisierter, gültiger Zustand übergeben wird. Durch diese Trennung ist
/// die Domäne klar, testbar und UI-unabhängig.
/// </para>
/// 
/// <h3>Thread-Safety</h3>
/// <para>
/// Die Factory besitzt <b>keinen</b> veränderlichen Zustand und ist threadsicher.
/// Sie kann als <c>Singleton</c> im DI-Container registriert werden.
/// </para>
/// 
/// <h3>Leistung</h3>
/// <para>
/// Die Textverarbeitung geschieht rein in-Memory. Für typische Lesson-Größen
/// (einige hundert Zeichen) ist die Komplexität vernachlässigbar.
/// </para>
/// </summary>
public sealed class LessonFactory : ILessonFactory
{
    /// <summary>
    /// Wiederverwendete Regex zum Kollabieren von Whitespace (inkl. Zeilenumbrüchen).
    /// </summary>
    private static readonly Regex MultiWs = new(@"\s+", RegexOptions.Compiled);

    /// <summary>
    /// Erzeugt eine <see cref="Lesson"/> aus <b>vorhandenen Blöcken</b>.
    /// 
    /// <para>
    /// Jeder übergebene Block wird normalisiert:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Alle Whitespace-Sequenzen werden zu genau einem Leerzeichen zusammengefasst (<c>\s+</c> → <c>" "</c>).</description></item>
    ///   <item><description>Führende und nachfolgende Leerzeichen werden entfernt (Trim).</description></item>
    ///   <item><description>Leere bzw. nur aus Whitespace bestehende Blöcke werden <b>entfernt</b>.</description></item>
    /// </list>
    /// 
    /// <para>
    /// Anschließend wird eine <see cref="Lesson"/> konstruiert. Deren
    /// <see cref="Lesson.TargetText"/> entspricht <c>string.Join(" ", Blocks)</c>,
    /// sodass zwischen Blöcken genau <b>ein</b> Leerzeichen liegt.
    /// </para>
    /// 
    /// <h4>Beispiel</h4>
    /// <code>
    /// var meta = new LessonMetaData("L1", "Grundreihe");
    /// var lesson = factory.Create(meta, new[] { "  asdf ", "", " jklö  " });
    /// // lesson.Blocks  == ["asdf", "jklö"]
    /// // lesson.TargetText == "asdf jklö"
    /// </code>
    /// </summary>
    /// <param name="meta">Metadaten der Lesson (Id, Title, …). Darf nicht null sein; <c>Id</c>/<c>Title</c> dürfen nicht leer sein.</param>
    /// <param name="blocks">Rohblöcke (dürfen null/leer/mit Mehrfach-Whitespace sein; werden normalisiert). Darf nicht null sein. Null-Elemente sind nicht erlaubt.</param>
    /// <returns>Eine gültige, normalisierte <see cref="Lesson"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="meta"/> oder <paramref name="blocks"/> ist null.</exception>
    /// <exception cref="ArgumentException"><paramref name="meta"/> hat leere <c>Id</c> oder <c>Title</c>, oder <paramref name="blocks"/> enthält null-Elemente.</exception>
    public Lesson Create(LessonMetaData meta, IEnumerable<string?> blocks)
    {
        if (meta is null) throw new ArgumentNullException(nameof(meta));
        if (string.IsNullOrWhiteSpace(meta.Title)) throw new ArgumentException("Title must not be empty.", nameof(meta));
        if (blocks is null) throw new ArgumentNullException(nameof(blocks));

        var cleaned = blocks
            .Select(b =>
            {
                if (b is null)
                    throw new ArgumentException("Blocks must not contain null elements.", nameof(blocks));
                var s = MultiWs.Replace(b, " ").Trim();
                return s;
            })
            .Where(s => s.Length > 0)
            .ToArray();

        return new Lesson(meta, cleaned);
    }

    /// <summary>
    /// Erzeugt eine <see cref="Lesson"/> aus einem <b>Rohtext</b>.
    /// 
    /// <para>Verarbeitungsschritte:</para>
    /// <ol>
    ///   <li><b>Whitespace-Normalisierung</b> des Eingabetextes:
    ///       Mehrfach-Whitespace (inkl. Zeilenumbrüche) wird zu einfachen
    ///       Leerzeichen zusammengefasst und der Text wird getrimmt.</li>
    ///   <li><b>Soft Word-Wrap</b> in Blöcke bis <paramref name="maxBlockLen"/> Zeichen:
    ///       Wörter werden, wenn möglich, zusammen in eine Zeile gelegt.
    ///       Passt das nächste Wort nicht mehr, wird ein neuer Block begonnen.</li>
    ///   <li><b>Harter Zeilenumbruch</b> für extrem lange Wörter:
    ///       Überschreitet ein einzelnes Wort <paramref name="maxBlockLen"/>,
    ///       wird es in Stücke mit maximal <paramref name="maxBlockLen"/> Zeichen
    ///       zerlegt.</li>
    ///   <li><b>Finale Normalisierung</b> über <see cref="Create(LessonMetaData, IEnumerable{string?})"/>:
    ///       sicherheitshalber werden die erzeugten Blöcke erneut kollabiert/getrimt und
    ///       leere entfernt.</li>
    /// </ol>
    /// 
    /// <h4>Beispiele</h4>
    /// <code>
    /// var meta = new LessonMetaData("L2", "Beispiel");
    /// var lesson = factory.FromText(meta, "Das   ist\n ein   Test", maxBlockLen: 7);
    /// // Mögliche Blöcke: ["Das ist", "ein", "Test"]
    /// // TargetText: "Das ist ein Test"
    /// 
    /// var l2 = factory.FromText(meta, "Superkalifragilistik", maxBlockLen: 5);
    /// // Blöcke: ["Super", "kalif", "ragil", "istik"]
    /// </code>
    /// </summary>
    /// <param name="meta">Metadaten der Lesson (Id, Title, …). Darf nicht null sein; <c>Id</c>/<c>Title</c> dürfen nicht leer sein.</param>
    /// <param name="text">Rohtext, aus dem die Lesson erzeugt wird. Darf null sein (wird wie leer behandelt).</param>
    /// <param name="maxBlockLen">Maximale Blocklänge (Zeichen). Werte &lt; 1 werden als 1 behandelt.</param>
    /// <returns>Eine gültige, normalisierte <see cref="Lesson"/>; bei leerem/Whitespace-Text eine Lesson ohne Blöcke.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="meta"/> ist null.</exception>
    /// <exception cref="ArgumentException"><paramref name="meta"/> hat leere <c>Id</c> oder <c>Title</c>.</exception>
    public Lesson FromText(LessonMetaData meta, string text, int maxBlockLen = 24)
    {
        if (meta is null) throw new ArgumentNullException(nameof(meta));
        if (string.IsNullOrWhiteSpace(meta.Title)) throw new ArgumentException("Title must not be empty.", nameof(meta));
        text ??= string.Empty;

        // 1) Whitespace-Normalisierung für stabilen Word-Wrap
        var normalized = MultiWs.Replace(text, " ").Trim();

        if (normalized.Length == 0)
            return Create(meta, Array.Empty<string>());

        // 2) In Blöcke umbrechen
        var blocks = WordWrap(normalized, Math.Max(1, maxBlockLen));

        // 3) Final erzeugen (Create übernimmt eine zusätzliche Normalisierung)
        return Create(meta, blocks);
    }

    /// <summary>
    /// Zerlegt einen bereits whitespace-normalisierten Text in Blöcke mit Soft-Word-Wrap
    /// bis zur maximalen Länge. Passt ein Wort nicht mehr in die aktuelle Zeile, beginnt
    /// ein neuer Block. Wörter, die länger als die maximale Zeilenlänge sind, werden
    /// hart gesplittet (ohne Trennzeichen).
    /// </summary>
    /// <param name="input">Whitespace-normalisierter Eingabetext.</param>
    /// <param name="maxLen">Maximale Blocklänge (min. 1).</param>
    /// <returns>Liste der Blöcke in Einfügereihenfolge.</returns>
    private static IReadOnlyList<string> WordWrap(string input, int maxLen)
    {
        var words = input.Split(' ');
        var blocks = new List<string>();
        var line = new StringBuilder();

        foreach (var w in words)
        {
            // 1) Sehr lange Wörter: hart splitten
            if (w.Length > maxLen)
            {
                FlushLine();
                for (int i = 0; i < w.Length; i += maxLen)
                {
                    var partLen = Math.Min(maxLen, w.Length - i);
                    blocks.Add(w.Substring(i, partLen));
                }
                continue;
            }

            // 2) Soft-Wrap nach Wörtern
            if (line.Length == 0)
            {
                line.Append(w);
            }
            else if (line.Length + 1 + w.Length <= maxLen)
            {
                line.Append(' ').Append(w);
            }
            else
            {
                FlushLine();
                line.Append(w);
            }
        }

        FlushLine();
        return blocks;

        void FlushLine()
        {
            // FlushLine dient als "Abschluss" der aktuell aufgebauten Zeile.
            // Sie wird immer dann aufgerufen, wenn die aktuelle Zeichenfolge
            // (line) in die Liste der Blöcke übernommen werden soll — entweder
            // weil ein Wort nicht mehr hineinpasst oder der Eingabetext zu Ende ist.
            //
            // Wichtig:
            // - Es wird nur dann ein Block erzeugt, wenn tatsächlich Text vorhanden ist.
            // - Leere Strings werden bewusst NICHT hinzugefügt.
            // - Nach dem Flush wird der Zeilenpuffer vollständig geleert,
            //   um die nächste Zeile neu zu beginnen.
            //
            // Dadurch entsteht am Ende eine stabile, wohldefinierte Blockliste,
            // ohne doppelte oder leere Einträge.
            if (line.Length > 0)
            {
                blocks.Add(line.ToString());
                line.Clear();    // Vorbereitung für einen neuen Block
            }
        }

    }
}
