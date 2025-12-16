using System.Collections.Generic;
using TypeTutor.Logic.Core;

namespace TypeTutor.Logic.Engine;

/// <summary>
/// Deutsches QWERTZ-Layout (V1) für Zeichen-erzeugende Tasten.
///
/// Unterstützt:
/// --------------------------------------
/// ✔ Buchstaben A..Z (ohne Shift → klein, mit Shift → groß)
/// ✔ Ziffern D0..D9 (Shift → ! " § $ % & / ( ) =)
/// ✔ Space
/// ✔ Häufige OEM/Satzzeichen: , . - +  (mit Shift)
/// ✔ ß-Taste (Oem5): ß / ? / AltGr="\"
/// ✔ Oem102 (< > |)
/// ✔ Umlaute (Ü/Ö/Ä) über Oem1/Oem3/Oem7
/// ✔ AltGr-Kombinationen (@ € { [ ] } \ |)
///
/// Nicht druckbare Tasten (Enter, Backspace, Tab, Escape) → null.
///
/// Hinweis:
/// OEM-Tasten (Oem1..Oem7) sind hardwareabhängig. Für deutsche Tastaturen
/// (DIN 2137, Apple/Hama/Logitech/Cherry) gilt jedoch stabil:
///   Oem1 = Ü/ü
///   Oem3 = Ö/ö
///   Oem7 = Ä/ä
///   Oem5 = ß/?/AltGr-\
/// </summary>
public sealed class GermanKeyToCharMapper : IKeyToCharMapper
{
    private static bool HasShift(ModifierKeys mods) => (mods & ModifierKeys.Shift) != 0;
    private static bool HasCtrl(ModifierKeys mods) => (mods & ModifierKeys.Control) != 0;
    private static bool HasAlt(ModifierKeys mods) => (mods & ModifierKeys.Alt) != 0;

    /// <summary>Unter Windows entspricht AltGr normalerweise Control|Alt.</summary>
    private static bool IsAltGr(ModifierKeys mods) => HasCtrl(mods) && HasAlt(mods);

    // A..Z → klein/groß
    private static readonly Dictionary<KeyCode, (char normal, char shifted)> Letters = new()
    {
        { KeyCode.A, ('a','A') }, { KeyCode.B, ('b','B') }, { KeyCode.C, ('c','C') }, { KeyCode.D, ('d','D') },
        { KeyCode.E, ('e','E') }, { KeyCode.F, ('f','F') }, { KeyCode.G, ('g','G') }, { KeyCode.H, ('h','H') },
        { KeyCode.I, ('i','I') }, { KeyCode.J, ('j','J') }, { KeyCode.K, ('k','K') }, { KeyCode.L, ('l','L') },
        { KeyCode.M, ('m','M') }, { KeyCode.N, ('n','N') }, { KeyCode.O, ('o','O') }, { KeyCode.P, ('p','P') },
        { KeyCode.Q, ('q','Q') }, { KeyCode.R, ('r','R') }, { KeyCode.S, ('s','S') }, { KeyCode.T, ('t','T') },
        { KeyCode.U, ('u','U') }, { KeyCode.V, ('v','V') }, { KeyCode.W, ('w','W') }, { KeyCode.X, ('x','X') },
        { KeyCode.Y, ('y','Y') }, { KeyCode.Z, ('z','Z') },
    };

    // Ziffernreihe (DE)
    private static readonly Dictionary<KeyCode, (char normal, char shifted)> Digits = new()
    {
        { KeyCode.D1, ('1','!') }, { KeyCode.D2, ('2','"') }, { KeyCode.D3, ('3','§') }, { KeyCode.D4, ('4','$') },
        { KeyCode.D5, ('5','%') }, { KeyCode.D6, ('6','&') }, { KeyCode.D7, ('7','/') }, { KeyCode.D8, ('8','(') },
        { KeyCode.D9, ('9',')') }, { KeyCode.D0, ('0','=') },
    };

    // Häufige Satzzeichen
    private static readonly Dictionary<KeyCode, (char normal, char shifted)> Oem = new()
    {
        { KeyCode.OemComma,  (',',';') },
        { KeyCode.OemPeriod, ('.',':') },
        { KeyCode.OemMinus,  ('-','_') },
        { KeyCode.OemPlus,   ('+','*') },

        // ß-Taste (auf DE immer Scancode OEM_4)
        { KeyCode.Oem5,      ('ß','?') },

        // < / > / AltGr-|   – Oem102 links neben Y
        { KeyCode.Oem102,    ('<','>') },
    };

    // ---------------------------------------------
    // 🔹 NEU: Umlaut-Zuordnung (DE QWERTZ)
    // ---------------------------------------------
    private static readonly Dictionary<KeyCode, (char normal, char shifted)> Umlauts = new()
    {
        { KeyCode.Oem1, ('ü','Ü') },   // rechts oben in Reihe 1
        { KeyCode.Oem3, ('ö','Ö') },   // rechts in Reihe 2
        { KeyCode.Oem7, ('ä','Ä') },   // rechts in Reihe 3
    };

    // AltGr-Kombinationen (DE)
    private static readonly Dictionary<KeyCode, char> AltGr = new()
    {
        { KeyCode.Q,      '@' },
        { KeyCode.E,      '€' },
        { KeyCode.D7,     '{' },
        { KeyCode.D8,     '[' },
        { KeyCode.D9,     ']' },
        { KeyCode.D0,     '}' },

        { KeyCode.Oem5,   '\\' },  // AltGr+ß -> \
        { KeyCode.Oem102, '|'  },  // AltGr+< -> |
    };

    public char? Map(KeyStroke stroke)
    {
        var key = stroke.Key;
        var mods = stroke.Modifiers;

        // Nicht druckbare Tasten
        if (key is KeyCode.Enter or KeyCode.Backspace or KeyCode.Tab or KeyCode.Escape)
            return null;

        if (key == KeyCode.Space)
            return ' ';

        // AltGr hat Vorrang
        if (IsAltGr(mods) && AltGr.TryGetValue(key, out var altgrChar))
            return altgrChar;

        // Buchstaben
        if (Letters.TryGetValue(key, out var letter))
            return HasShift(mods) ? letter.shifted : letter.normal;

        // Ziffernreihe
        if (Digits.TryGetValue(key, out var digit))
            return HasShift(mods) ? digit.shifted : digit.normal;

        // ↳ NEU: Umlaute
        if (Umlauts.TryGetValue(key, out var uml))
            return HasShift(mods) ? uml.shifted : uml.normal;

        // OEM (Satzzeichen etc.)
        if (Oem.TryGetValue(key, out var oem))
            return HasShift(mods) ? oem.shifted : oem.normal;

        // Unbekannte OEM-Taste oder unmappbarer Key
        return null;
    }
}
