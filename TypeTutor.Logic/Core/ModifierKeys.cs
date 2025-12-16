// Pseudocode / Plan (ausführlich) - in Kommentaren, Schritt-für-Schritt:
// 1. Definiere ein Flag-Enum namens `ModifierKeys` im Namespace `TypeTutor.Logic.Core`.
// 2. Markiere das Enum mit dem Attribut [Flags], damit bitweise Kombinationen möglich werden.
// 3. Lege die einzelnen Modifier mit eindeutigen Bitwerten fest:
//    - `None` = 0 (kein Modifier aktiv).
//    - `Shift` = 1 << 0 (Bit 0).
//    - `Control` = 1 << 1 (Bit 1).
//    - `Alt` = 1 << 2 (Bit 2).
// 4. Ergänze ausführliche XML- und Inline-Kommentare (Deutsch), die erklären:
//    - Warum [Flags] verwendet wird.
//    - Wie die Bits zu interpretieren sind.
//    - Beispiele zur Kombination (z.B. AltGr = Control | Alt).
//    - Hinweise zur Verwendung in Vergleichen (z.B. `(flags & ModifierKeys.Control) != 0`).
// 5. Optional: Dokumentationshinweis zu Plattformkonventionen (z. B. AltGr auf manchen Layouts).
// 6. Den Quellcode so schreiben, dass er mit C# 12 / .NET 8 kompatibel und gut lesbar ist.
// 7. Keine zusätzlichen Abhängigkeiten oder Hilfsklassen erforderlich — das Enum reicht für die Tests.
//
// Umsetzung:
// - Erzeuge die Datei `ModifierKeys.cs` mit dem Enum und umfangreichen Kommentaren/Beispielen.
// - Stelle sicher, dass das Enum die in den Tests verwendeten Werte (`None`, `Shift`, `Control`, `Alt`) enthält.
// - Füge Hinweise zur bitweisen Prüfung und zum Zusammenfügen hinzu.

using System;

namespace TypeTutor.Logic.Core
{
    /// <summary>
    /// Kennzeichnet Tastatur-Modifier als kombinierbare Flags.
    /// </summary>
    /// <remarks>
    /// Dieses Enum ist mit dem Attribut <see cref="FlagsAttribute"/> versehen, damit
    /// mehrere Modifier durch bitweises ODER kombiniert werden können, z. B.:
    /// <code>
    /// var altGr = ModifierKeys.Control | ModifierKeys.Alt;
    /// </code>
    ///
    /// Hinweise zur Verwendung:
    /// - Um zu prüfen, ob ein bestimmter Modifier gesetzt ist, verwenden Sie:
    ///   (modifiers & ModifierKeys.Control) != 0
    /// - Verwenden Sie <see cref="ModifierKeys.None"/> für den Zustand "keine Modifier".
    ///
    /// Implementation-Entscheidung:
    /// - Jeder Modifier belegt ein eigenes Bit (1, 2, 4, ...). Diese Bitvergabe
    ///   erlaubt einfache Kombinationen und Vergleiche ohne zusätzlichen Overhead.
    /// </remarks>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// Keine Modifier-Taste ist gedrückt.
        /// Wert 0 bedeutet: keine Bits gesetzt.
        /// </summary>
        None = 0,

        /// <summary>
        /// Die Shift-Taste (Großschreibung/Alternative Zeichen).
        /// Belegt Bit 0 (Wert 1).
        /// </summary>
        Shift = 1 << 0, // 0b0001

        /// <summary>
        /// Die Control-Strg-Taste.
        /// Belegt Bit 1 (Wert 2).
        /// </summary>
        Control = 1 << 1, // 0b0010

        /// <summary>
        /// Die Alt-Taste (alternative Eingabeebene).
        /// Belegt Bit 2 (Wert 4).
        /// </summary>
        Alt = 1 << 2 // 0b0100

        // Hinweis:
        // - Ein explizites AltGr-Feld wird hier nicht als separates Enum-Feld
        //   definiert, weil AltGr typischerweise als Kombination von Control | Alt
        //   repräsentiert wird. Tests und Verwendung können die Kombination direkt bilden:
        //   var altGr = ModifierKeys.Control | ModifierKeys.Alt;
    }
}