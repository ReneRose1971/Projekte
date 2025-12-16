using System.Collections.ObjectModel;
using System.Linq;
using TypeTutor.Logic.Core;

namespace TypeTutor.WPF
{
    public sealed class KeyboardViewModel
    {
        public ObservableCollection<KeyboardKeyViewModel> Keys { get; } = new();
        public int SuggestedColumns { get; } = 14;

        public KeyboardViewModel()
        {
            // --- Zeile 1 (Ziffernreihe + Sondertasten) -------------------------
            Keys.Add(new KeyboardKeyViewModel("Esc", KeyCode.Escape, isWide: true));
            Keys.Add(new KeyboardKeyViewModel("1", KeyCode.D1, toolTip: "!"));
            Keys.Add(new KeyboardKeyViewModel("2", KeyCode.D2, toolTip: "\"") );
            Keys.Add(new KeyboardKeyViewModel("3", KeyCode.D3, toolTip: "§"));
            Keys.Add(new KeyboardKeyViewModel("4", KeyCode.D4, toolTip: "$"));
            Keys.Add(new KeyboardKeyViewModel("5", KeyCode.D5, toolTip: "%"));
            Keys.Add(new KeyboardKeyViewModel("6", KeyCode.D6, toolTip: "&"));
            Keys.Add(new KeyboardKeyViewModel("7", KeyCode.D7, toolTip: "/ {"));
            Keys.Add(new KeyboardKeyViewModel("8", KeyCode.D8, toolTip: "( ["));
            Keys.Add(new KeyboardKeyViewModel("9", KeyCode.D9, toolTip: ") ]"));
            Keys.Add(new KeyboardKeyViewModel("0", KeyCode.D0, toolTip: "= }"));
            Keys.Add(new KeyboardKeyViewModel("ß", KeyCode.Oem5, toolTip: "? \\") );
            Keys.Add(new KeyboardKeyViewModel("?", KeyCode.Backspace, isWide: true));

            // --- Zeile 2 -------------------------------------------------------
            Keys.Add(new KeyboardKeyViewModel("Tab", KeyCode.Tab, isWide: true));
            Keys.Add(new KeyboardKeyViewModel("Q", KeyCode.Q, toolTip: "@"));
            Keys.Add(new KeyboardKeyViewModel("W", KeyCode.W));
            Keys.Add(new KeyboardKeyViewModel("E", KeyCode.E, toolTip: "€"));
            Keys.Add(new KeyboardKeyViewModel("R", KeyCode.R));
            Keys.Add(new KeyboardKeyViewModel("T", KeyCode.T));
            Keys.Add(new KeyboardKeyViewModel("Z", KeyCode.Z));
            Keys.Add(new KeyboardKeyViewModel("U", KeyCode.U));
            Keys.Add(new KeyboardKeyViewModel("I", KeyCode.I));
            Keys.Add(new KeyboardKeyViewModel("O", KeyCode.O));
            Keys.Add(new KeyboardKeyViewModel("P", KeyCode.P));
            Keys.Add(new KeyboardKeyViewModel("Ü", KeyCode.Oem1));
            Keys.Add(new KeyboardKeyViewModel("Enter", KeyCode.Enter, isWide: true));

            // --- Zeile 3 -------------------------------------------------------
            Keys.Add(new KeyboardKeyViewModel("Caps", KeyCode.None, isWide: true)); // placeholder
            Keys.Add(new KeyboardKeyViewModel("A", KeyCode.A));
            Keys.Add(new KeyboardKeyViewModel("S", KeyCode.S));
            Keys.Add(new KeyboardKeyViewModel("D", KeyCode.D));
            Keys.Add(new KeyboardKeyViewModel("F", KeyCode.F));
            Keys.Add(new KeyboardKeyViewModel("G", KeyCode.G));
            Keys.Add(new KeyboardKeyViewModel("H", KeyCode.H));
            Keys.Add(new KeyboardKeyViewModel("J", KeyCode.J));
            Keys.Add(new KeyboardKeyViewModel("K", KeyCode.K));
            Keys.Add(new KeyboardKeyViewModel("L", KeyCode.L));
            Keys.Add(new KeyboardKeyViewModel("Ö", KeyCode.Oem3));
            Keys.Add(new KeyboardKeyViewModel("Ä", KeyCode.Oem7));
            Keys.Add(new KeyboardKeyViewModel("+ *", KeyCode.OemPlus));

            // --- Zeile 4 -------------------------------------------------------
            Keys.Add(new KeyboardKeyViewModel("< >", KeyCode.Oem102));
            Keys.Add(new KeyboardKeyViewModel("Y", KeyCode.Y));
            Keys.Add(new KeyboardKeyViewModel("X", KeyCode.X));
            Keys.Add(new KeyboardKeyViewModel("C", KeyCode.C));
            Keys.Add(new KeyboardKeyViewModel("V", KeyCode.V));
            Keys.Add(new KeyboardKeyViewModel("B", KeyCode.B));
            Keys.Add(new KeyboardKeyViewModel("N", KeyCode.N));
            Keys.Add(new KeyboardKeyViewModel("M", KeyCode.M));
            Keys.Add(new KeyboardKeyViewModel(", ;", KeyCode.OemComma));
            Keys.Add(new KeyboardKeyViewModel(". :", KeyCode.OemPeriod));
            Keys.Add(new KeyboardKeyViewModel("- _", KeyCode.OemMinus));
            Keys.Add(new KeyboardKeyViewModel("Alt", KeyCode.None)); // placeholder
            Keys.Add(new KeyboardKeyViewModel("Space", KeyCode.Space, isWide: true));
            Keys.Add(new KeyboardKeyViewModel("AltGr", KeyCode.None)); // placeholder
        }

        public void SetPressed(System.Collections.Generic.ISet<KeyCode> pressed)
        {
            pressed ??= new System.Collections.Generic.HashSet<KeyCode>();
            foreach (var key in Keys)
                key.IsPressed = pressed.Contains(key.Code);
        }

        public void SetPressed(KeyCode code, bool isPressed)
        {
            var vm = Keys.FirstOrDefault(k => k.Code == code);
            if (vm != null) vm.IsPressed = isPressed;
        }
    }
}
