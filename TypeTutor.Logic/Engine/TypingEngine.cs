using System;
using System.Collections.Generic;
using TypeTutor.Logic.Core;

namespace TypeTutor.Logic.Engine
{
    /// <summary>
    /// TypingEngine – Kern der Tippverarbeitung (V1 + CaseHandling).
    ///
    /// Neu:
    /// - Konfigurierbares Groß-/Kleinschreibungs-Verhalten via <see cref="CaseSensitivity"/>.
    ///   Standard ist Strict; optional IgnoreCase.
    /// </summary>
    public sealed class TypingEngine : ITypingEngine
    {
        private readonly InputBuffer _buffer;
        private readonly CaseSensitivity _mode;

        private string _target = "";
        private readonly List<int> _errors = new();

        private int _prefixLength = 0;
        private int _nextIndex = 0;
        private char? _expectedNext = null;
        private char? _lastInput = null;

        private bool _completedEventRaised = false;

        /// <summary>
        /// Erstellt eine neue Engine.
        /// </summary>
        /// <param name="mapper">Mapper für den <see cref="InputBuffer"/> (Char-First Fallback).</param>
        /// <param name="mode">Case-Verhalten (Strict = exakt, IgnoreCase = tolerant).</param>
        public TypingEngine(IKeyToCharMapper mapper, CaseSensitivity mode = CaseSensitivity.Strict)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            _buffer = new InputBuffer(mapper);
            _mode = mode;

            Reset("");
        }

        /// <summary>Aktueller State-Snapshot.</summary>
        public TypingEngineState State { get; private set; } = TypingEngineState.Start("");
        /// <summary>
        /// Setzt die Engine auf einen neuen Zieltext zurück und erzeugt den Startzustand.
        /// </summary>
        public void Reset(string targetText)
        {
            _target = targetText ?? "";
            _buffer.Clear();
            _errors.Clear();

            _prefixLength = 0;
            _nextIndex = 0;
            _lastInput = null;
            _expectedNext = _target.Length > 0 ? _target[0] : null;

            _completedEventRaised = false;

            UpdateState();
        }

        /// <summary>Verarbeitet einen Tastendruck und aktualisiert die Bewertung.</summary>
        public void Process(KeyStroke stroke)
        {
            // Wenn bereits abgeschlossen, ignorieren wir weitere Eingaben
            if (State.IsComplete)
                return;

            _buffer.Add(stroke);

            // Char-First: das tatsächliche Zeichen (falls vorhanden)
            char? typed = stroke.Char;
            _lastInput = typed;

            // Kein druckbares Zeichen → nur State neu bilden
            if (typed == null)
            {
                UpdateState();
                return;
            }

            // Ziel leer oder bereits fertig → nur State neu bilden
            if (_target.Length == 0 || _nextIndex >= _target.Length)
            {
                UpdateState();
                return;
            }

            // Vergleich mit Case-Einstellung
            if (CharsEqual(typed.Value, _expectedNext!.Value))
            {
                _prefixLength++;
            }
            else
            {
                _errors.Add(_nextIndex);
            }

            _nextIndex++;
            _expectedNext = (_nextIndex < _target.Length) ? _target[_nextIndex] : (char?)null;

            UpdateState();

            // Wenn wir gerade komplett geworden sind, feuern wir einmalig das Ereignis
            if (State.IsComplete && !_completedEventRaised)
            {
                _completedEventRaised = true;
                bool success = (_errors.Count == 0);
                LessonCompleted?.Invoke(success);
            }
        }

        /// <summary>Vergleicht zwei Zeichen gemäß Case-Einstellung.</summary>
        private bool CharsEqual(char typed, char expected)
        {
            if (_mode == CaseSensitivity.Strict)
                return typed == expected;

            // IgnoreCase – kulturinvariant
            return char.ToUpperInvariant(typed) == char.ToUpperInvariant(expected);
        }

        /// <summary>Erzeugt den unveränderlichen State-Snapshot.</summary>
        private void UpdateState()
        {
            bool complete = (_nextIndex >= _target.Length);

            State = new TypingEngineState(
                targetText: _target,
                inputText: _buffer.CurrentInput,
                correctPrefixLength: _prefixLength,
                errorCount: _errors.Count,
                nextIndex: _nextIndex,
                isComplete: complete,
                errorPositions: _errors,
                expectedNextChar: _expectedNext,
                lastInputChar: _lastInput
            );
        }

        /// <summary>
        /// Wird ausgelöst, wenn eine Lesson einmalig als abgeschlossen gilt.
        /// Der boolesche Parameter signalisiert, ob die Lektion erfolgreich (true) oder mit Fehlern (false) abgeschlossen wurde.
        /// </summary>
        public event Action<bool>? LessonCompleted;
    }
}
