using System;

namespace TypeTutor.Logic.Core;

/// <summary>
/// Repräsentiert ein einzelnes, vollständig beschriebenes Tastaturereignis.
/// 
/// Ein <see cref="KeyStroke"/> bildet die zentrale, unveränderliche Dateneinheit
/// für alle Eingaben im Tipptrainer. Jede Instanz beschreibt genau eine
/// physische Tastenaktion sowie das daraus resultierende Zeichen (falls
/// vorhanden). Dadurch entsteht eine robuste Grundlage für Logging,
/// Analyse, Fehlerrückmeldungen und zur Verarbeitung in der Tipp-Engine.
///
/// Die Struktur ist als <c>readonly struct</c> ausgelegt, um folgende
/// Eigenschaften sicherzustellen:
/// 
/// • **Unveränderlichkeit**  
///   Alle Werte werden ausschließlich im Konstruktor gesetzt und können
///   anschließend nicht mehr modifiziert werden. Dies schützt vor
///   unbeabsichtigten Änderungen, erleichtert Debugging und verhindert
///   Nebeneffekte.
///
/// • **Wertsemantik (Value-Type)**  
///   Ein Tastendruck ist ein atomarer Wert (ähnlich wie ein Punkt in
///   einem Vektorraum oder ein Zeitstempel). Durch die Wahl eines
///   Strukturs (anstatt einer Klasse) bleibt die Semantik klar: Zwei
///   identische Tastendrücke sind gleich, Kopien sind voneinander
///   unabhängig.
///
/// • **Trennung von physischem Key und produziertem Zeichen**  
///   Viele Tasten erzeugen kein Zeichen (z. B. Shift, Control, Dead Keys,
///   Pfeiltasten). Deshalb kann <see cref="Char"/> null sein.
///   Die Entscheidungslogik, ob ein Zeichen existiert, liegt nicht in
///   <see cref="KeyStroke"/>, sondern in der Mappingschicht.
/// 
/// • **Modifier-Unterstützung**  
///   Mithilfe von <see cref="ModifierKeys"/> wird erfasst, welche
///   Modifikatortasten (Shift, Control, Alt) beim Drücken aktiv waren.
///   Dadurch lassen sich kombinierte Eingaben (z. B. AltGr oder Shift)
///   präzise abbilden.
/// 
/// • **Zeitstempel in UTC**  
///   Der Zeitpunkt des Tastendrucks wird stets in UTC gespeichert,
///   unabhängig davon, ob ein lokaler, ein UTC- oder ein
///   nicht spezifizierter Zeitstempel übergeben wurde. Dies garantiert
///   konsistente und kulturunabhängige Auswertung, besonders wichtig
///   für spätere Analyse- oder Loggingfunktionen.
///
/// <para>
/// Zusammengefasst bietet <see cref="KeyStroke"/> eine kompakte,
/// unveränderliche und präzise Darstellung eines Tastaturereignisses.
/// Es dient als Basisinput für die Tipp-Engine, für Metriken,
/// Fortschrittsmessung und Debugging — vollständig UI-unabhängig.
/// </para>
/// </summary>

public readonly struct KeyStroke
{
    /// <summary>
    /// Die physische Taste (layout-unabhängig), z. B. KeyCode.A, KeyCode.D7, KeyCode.Oem102.
    /// </summary>
    public KeyCode Key { get; }

    /// <summary>
    /// Das tatsächlich erzeugte Zeichen aus dieser Eingabe (falls vorhanden).
    /// Null, wenn es kein Zeichen gibt, z. B. für Shift, Pfeiltasten, Modifier, Escape usw.
    /// </summary>
    public char? Char { get; }

    /// <summary>
    /// Die gedrückten Modifier (Shift, Control, Alt). AltGr = Control | Alt.
    /// </summary>
    public ModifierKeys Modifiers { get; }

    /// <summary>
    /// Zeitstempel des Eingabeereignisses in UTC.
    /// </summary>
    public DateTime TimestampUtc { get; }

    /// <summary>
    /// Erstellt ein neues KeyStroke-Objekt.
    /// Key: physische Taste
    /// Char: Zeichen (oder null)
    /// mods: ModifierKeys
    /// timestampUtc: Optionale Zeitangabe (Standard: jetzt).
    /// </summary>
    public KeyStroke(KeyCode key, char? ch, ModifierKeys mods, DateTime? timestampUtc = null)
    {
        Key = key;
        Char = ch;
        Modifiers = mods;
        TimestampUtc = (timestampUtc ?? DateTime.UtcNow).ToUniversalTime();
    }

    /// <summary>
    /// Erzeugt eine lesbare Textdarstellung dieses <see cref="KeyStroke"/>-Objekts.
    /// 
    /// Die Darstellung ist für Debugging, Logging und Testausgaben optimiert und
    /// enthält alle relevanten Informationen des Tastaturereignisses:
    /// 
    /// • Zeitstempel (immer in UTC und festem ISO-Format)
    /// • Physische Taste (<see cref="KeyCode"/>)
    /// • Gedrückte Modifikatoren (<see cref="ModifierKeys"/>)
    /// • Das zugehörige Zeichen (falls vorhanden) oder ein Platzhalter, wenn kein
    ///   druckbares Zeichen erzeugt wurde.
    /// 
    /// Die Ausgabe ist kulturunabhängig, stabil formatiert und vermeidet implizite
    /// oder mehrdeutige Darstellungen. Dadurch eignet sie sich besonders für die
    /// Analyse von Tastatureingaben, Fehlerdiagnosen und automatisierte Tests.
    /// </summary>
    /// <returns>
    /// Eine formatierte Zeichenkette in der Form:
    /// <code>
    /// [2025-01-01T04:05:06Z] Key=A Mods=Shift Char='A'
    /// </code>
    /// oder, wenn kein druckbares Zeichen erzeugt wurde:
    /// <code>
    /// [2025-01-01T04:05:06Z] Key=Enter Mods=None Char=&lt;none&gt;
    /// </code>
    /// </returns>
    public override string ToString()
    {
        string charPart = Char.HasValue
            ? $"'{Char}'"
            : "<none>";

        return $"[{TimestampUtc:yyyy-MM-ddTHH:mm:ssZ}] Key={Key} Mods={Modifiers} Char={charPart}";
    }


}
