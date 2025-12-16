using System;
using System.Collections.Generic;
using System.Text;
using TypeTutor.Logic.Core;

namespace TypeTutor.Logic.Engine
{
    /// <summary>
    /// Puffer für Tastendrücke (KeyStrokes) und den daraus resultierenden Text.
    /// - Speichert alle Strokes chronologisch.
    /// - Baut einen laufenden Texteingabe-String auf.
    /// - UI-/Layout-unabhängig: Zeichen kommen entweder direkt aus dem UI (stroke.Char)
    ///   oder werden per <see cref="IKeyToCharMapper"/> aus dem KeyCode+Modifiers ermittelt.
    /// 
    /// Design-Hinweis:
    ///  - Korrekturen (Backspace etc.) verarbeitet NICHT der Buffer, sondern die TypingEngine.
    ///  - Der Buffer ist bewusst „append-only“ für Text; er protokolliert aber jeden Stroke.
    /// </summary>
    public sealed class InputBuffer
    {
        private readonly IKeyToCharMapper _mapper;
        private readonly List<KeyStroke> _strokes = new();
        private readonly StringBuilder _text = new();

        /// <summary>
        /// Erzeugt einen neuen Eingabepuffer.
        /// </summary>
        /// <param name="mapper">Mapper von physischer Taste → Zeichen (Fallback, wenn kein Char geliefert wird).</param>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="mapper"/> null ist.</exception>
        public InputBuffer(IKeyToCharMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Alle bislang erfassten Tastendrücke in zeitlicher Reihenfolge.</summary>
        public IReadOnlyList<KeyStroke> Strokes => _strokes;

        /// <summary>Der aktuell aufgebaute Eingabetext.</summary>
        public string CurrentInput => _text.ToString();

        /// <summary>
        /// Fügt einen Tastendruck hinzu und aktualisiert, falls ein Zeichen entsteht, den Eingabetext.
        /// Reihenfolge:
        ///  1) Char-First: Falls das UI bereits ein konkretes Zeichen liefert (z. B. 'ä' aus WPF/TextInput),
        ///     wird dieses direkt übernommen (layout-/hardware-robust).
        ///  2) Fallback: Mapping aus KeyCode+Modifiers (A..Z, Ziffern, Umlaute via Oem-Keys, AltGr-Kombinationen).
        /// Nicht-druckbare Tasten (Enter/Backspace/Tab/Escape …) verändern den Text nicht,
        /// werden aber als Stroke protokolliert.
        /// </summary>
        public void Add(KeyStroke stroke)
        {
            // Hinweis: KeyStroke ist eine struct → niemals null.
            _strokes.Add(stroke);

            // (1) Char-First: echtes UI-Zeichen hat Vorrang (Umlaute/Dead-Keys sofort korrekt)
            if (stroke.Char.HasValue)
            {
                _text.Append(stroke.Char.Value);
                return;
            }

            // (2) Fallback: Mapping über das aktive Tastaturlayout
            char? ch = _mapper.Map(stroke);
            if (ch.HasValue)
                _text.Append(ch.Value);
        }

        /// <summary>
        /// Leert Puffer und Text vollständig.
        /// </summary>
        public void Clear()
        {
            _strokes.Clear();
            _text.Clear();
        }
    }
}
