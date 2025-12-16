using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TypeTutor.Logic.Core;
using System.Linq;
using System;

namespace TTVisualKeyboard.ViewModels
{
    /// <summary>
    /// Erzeugt ein DE-QWERTZ Layout mit abgesetztem Navigationsblock und NumPad.
    /// Ein zentrales Grid: Columns = 26, Rows = 6 (inkl. F-Reihe).
    /// Hauptfeld: C0..C15 | Lücke C16 | Navigation C17..C19 | Lücke C20 | NumPad C21..C25
    /// </summary>
    public sealed class VisualKeyboardViewModel : INotifyPropertyChanged
    {
        public int RowCount { get; private set; } = 6;
        public int ColumnCount { get; private set; } = 26;

        private bool _isShiftActive;
        public bool IsShiftActive
        {
            get => _isShiftActive;
            set { if (_isShiftActive != value) { _isShiftActive = value; OnPropertyChanged(); } }
        }

        private bool _isAltGrActive;
        public bool IsAltGrActive
        {
            get => _isAltGrActive;
            set { if (_isAltGrActive != value) { _isAltGrActive = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<KeyViewModel> AllKeys { get; } = new();

        public VisualKeyboardViewModel() => LoadGermanQwertzLayout();

        private void Add(int row, int col, string label, int colSpan = 1, int rowSpan = 1,
                         string? shift = null, string? altgr = null)
        {
            AllKeys.Add(new KeyViewModel
            {
                Row = row,
                Column = col,
                RowSpan = rowSpan,
                ColSpan = colSpan,
                LabelPrimary = label,
                LabelShift = shift,
                LabelAltGr = altgr
            });
        }

        // Map a domain KeyCode to the label used in the keyboard layout and set pressed state
        public void SetPressed(KeyCode code, bool isPressed)
        {
            string? label = MapKeyCodeToLabel(code);
            if (label == null)
                return;

            var matches = FindByLabel(label);

            // Fallback: if no direct label match, also try single-character match (case-insensitive)
            if (!matches.Any() && label.Length == 1)
            {
                var ch = label[0];
                matches = AllKeys.Where(k => (k.LabelPrimary?.Length == 1 && CharEqualsInvariant(k.LabelPrimary![0], ch))
                                            || (k.LabelShift?.Length == 1 && CharEqualsInvariant(k.LabelShift![0], ch))
                                            || (k.LabelAltGr?.Length == 1 && CharEqualsInvariant(k.LabelAltGr![0], ch)));
            }

            foreach (var k in matches)
                k.IsPressed = isPressed;
        }

        // New: highlight by character/label (used when TextInput provides the char and KeyDown mapping missed it)
        public void SetPressedByLabel(string label, bool isPressed)
        {
            if (string.IsNullOrEmpty(label)) return;
            var matches = FindByLabel(label);

            // fallback single-char match
            if (!matches.Any() && label.Length == 1)
            {
                var ch = label[0];
                matches = AllKeys.Where(k => (k.LabelPrimary?.Length == 1 && CharEqualsInvariant(k.LabelPrimary![0], ch))
                                            || (k.LabelShift?.Length == 1 && CharEqualsInvariant(k.LabelShift![0], ch))
                                            || (k.LabelAltGr?.Length == 1 && CharEqualsInvariant(k.LabelAltGr![0], ch)));
            }

            foreach (var k in matches)
                k.IsPressed = isPressed;
        }

        private IEnumerable<KeyViewModel> FindByLabel(string label)
            => AllKeys.Where(k => StringEqualsInvariant(k.LabelPrimary, label)
                                || StringEqualsInvariant(k.LabelShift, label)
                                || StringEqualsInvariant(k.LabelAltGr, label));

        private static bool StringEqualsInvariant(string? a, string? b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static bool CharEqualsInvariant(char a, char b)
            => char.ToUpperInvariant(a) == char.ToUpperInvariant(b);

        private static string? MapKeyCodeToLabel(KeyCode code)
        {
            // Letters
            if (code >= KeyCode.A && code <= KeyCode.Z)
                return ((char)('A' + ((int)code - (int)KeyCode.A))).ToString();

            // Digits D0..D9 map to "0".."9"
            if (code >= KeyCode.D0 && code <= KeyCode.D9)
                return ((char)('0' + ((int)code - (int)KeyCode.D0))).ToString();

            return code switch
            {
                KeyCode.Space => "Space",
                KeyCode.Enter => "Enter",
                KeyCode.Backspace => "Backspace",
                KeyCode.Tab => "Tab",
                KeyCode.Escape => "Esc",
                KeyCode.OemComma => ",",
                KeyCode.OemPeriod => ".",
                KeyCode.OemMinus => "-",
                KeyCode.OemPlus => "+",
                KeyCode.Oem102 => "< > |",
                // Correct OEM labels for DE layout
                KeyCode.Oem1 => "Ü",    // Oem1 -> Ü/ü on DE keyboards
                KeyCode.Oem3 => "Ö",    // Oem3 -> Ö/ö
                KeyCode.Oem5 => "ß",    // Oem5 -> ß/?
                KeyCode.Oem7 => "Ä",    // Oem7 -> Ä/ä
                KeyCode.Oem2 => "#",    // map possible '#' key
                KeyCode.NumPadDecimal => ",",
                KeyCode.Left => "←",
                KeyCode.Right => "→",
                KeyCode.Up => "↑",
                KeyCode.Down => "↓",
                KeyCode.NumPad0 => "0",
                KeyCode.NumPad1 => "1",
                KeyCode.NumPad2 => "2",
                KeyCode.NumPad3 => "3",
                KeyCode.NumPad4 => "4",
                KeyCode.NumPad5 => "5",
                KeyCode.NumPad6 => "6",
                KeyCode.NumPad7 => "7",
                KeyCode.NumPad8 => "8",
                KeyCode.NumPad9 => "9",
                KeyCode.Add => "+",
                KeyCode.Subtract => "-",
                KeyCode.Multiply => "*",
                KeyCode.Divide => "/",
                _ => null
            };
        }

        private void LoadGermanQwertzLayout()
        {
            // --- R0: F-Reihe ---
            int c = 0;
            Add(0, c, "Esc", 2); c += 2;
            foreach (var f in new[] { "F1", "F2", "F3", "F4" }) Add(0, c++, f);
            c++; // kleine Lücke
            foreach (var f in new[] { "F5", "F6", "F7", "F8" }) Add(0, c++, f);
            c++;
            foreach (var f in new[] { "F9", "F10", "F11", "F12" }) Add(0, c++, f);

            // Navigations-Top
            Add(0, 17, "PrtSc");
            Add(0, 18, "ScrLk");
            Add(0, 19, "Pause");

            // NumPad Top
            Add(0, 21, "/"); Add(0, 22, "*"); Add(0, 23, "-");
            // NumPad '+' eine Zeile nach oben verschieben (beginnt jetzt in R0 und erstreckt sich über R0..R1)
            Add(0, 24, "+", 1, 2);

            // --- R1: Ziffernreihe ---
            c = 0;
            foreach (var k in new[] { "^", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "ß", "´" }) Add(1, c++, k);
            Add(1, c, "Backspace", 2); // ≈1.75u, hier 2 Columns

            // Nav 1
            Add(1, 17, "Ins"); Add(1, 18, "Home"); Add(1, 19, "PgUp");

            // NumPad 7 8 9 + (2r)
            Add(1, 21, "7"); Add(1, 22, "8"); Add(1, 23, "9");
            // Hinweis: NumPad '+' wurde nach oben verschoben (siehe oben)

            // --- R2: Q-Reihe ---
            c = 0;
            Add(2, c, "Tab", 2); c += 2;
            foreach (var k in "QWERTZUIOPÜ") Add(2, c++, k.ToString());
            Add(2, c, "+"); // Taste rechts neben Ü

            // Nav 2
            Add(2, 17, "Del"); Add(2, 18, "End"); Add(2, 19, "PgDn");

            // NumPad 4 5 6
            Add(2, 21, "4"); Add(2, 22, "5"); Add(2, 23, "6");

            // --- R3: A-Reihe ---
            c = 0;
            Add(3, c, "⇪", 2); c += 2;
            foreach (var k in new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ö", "Ä", "#" }) Add(3, c++, k);
            // Haupt-Enter eine Zeile weiter oben platzieren (Start in R2, über R2..R3)
            Add(2, c, "Enter", 2, 2); // 2 Spalten breit, 2 Zeilen hoch

            // NumPad 1 2 3  + Enter (2r)
            Add(3, 21, "1"); Add(3, 22, "2"); Add(3, 23, "3");
            // NumPad-Enter eine Zeile nach oben verschieben (beginnt jetzt in R2 und erstreckt sich über R2..R3)
            Add(2, 24, "Enter", 1, 2);

            // --- R4: Y-Reihe ---
            c = 0;
            Add(4, c, "Shift", 2); c += 2;
            Add(4, c++, "< > |");
            foreach (var k in new[] { "Y", "X", "C", "V", "B", "N", "M", ",", ".", "-" }) Add(4, c++, k);
            Add(4, c, "Shift", 3);

            // Nav: Pfeil oben
            Add(4, 18, "↑");

            // --- R5: unterste Reihe ---
            c = 0;
            Add(5, c++, "Ctrl");
            Add(5, c++, "Win");
            Add(5, c++, "Alt");
            Add(5, c, "Space", 6); c += 6;
            Add(5, c++, "AltGr");
            Add(5, c++, "Menu");
            Add(5, c, "Ctrl", 2);

            // Nav: Pfeile unten
            Add(5, 17, "←"); Add(5, 18, "↓"); Add(5, 19, "→");

            // NumPad: 0 und Dezimalzeichen eine Zeile nach oben verschieben (von R5 nach R4)
            Add(4, 21, "0", 2); Add(4, 23, ".");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
