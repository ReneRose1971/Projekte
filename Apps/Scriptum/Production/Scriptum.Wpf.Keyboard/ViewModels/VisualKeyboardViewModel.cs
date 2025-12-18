using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Scriptum.Wpf.Keyboard.ViewModels;

/// <summary>
/// ViewModel für die visuelle deutsche QWERTZ-Tastatur.
/// Kennt nur Labels, keine fachliche Logik oder KeyCodes.
/// </summary>
public sealed class VisualKeyboardViewModel : INotifyPropertyChanged
{
    public int RowCount { get; private set; } = 6;
    public int ColumnCount { get; private set; } = 26;

    private bool _isShiftActive;
    public bool IsShiftActive
    {
        get => _isShiftActive;
        set 
        { 
            if (_isShiftActive != value) 
            { 
                _isShiftActive = value; 
                OnPropertyChanged(); 
            } 
        }
    }

    private bool _isAltGrActive;
    public bool IsAltGrActive
    {
        get => _isAltGrActive;
        set 
        { 
            if (_isAltGrActive != value) 
            { 
                _isAltGrActive = value; 
                OnPropertyChanged(); 
            } 
        }
    }

    public ObservableCollection<KeyViewModel> AllKeys { get; } = new();

    public VisualKeyboardViewModel() 
    {
        LoadGermanQwertzLayout();
    }

    /// <summary>
    /// Setzt den visuellen Zustand einer Taste anhand ihres Labels.
    /// </summary>
    /// <param name="label">Das Label der Taste (z.B. "A", "Ü", "Enter", "Shift")</param>
    /// <param name="isPressed">true = gedrückt (hervorgehoben), false = nicht gedrückt</param>
    public void SetPressed(string label, bool isPressed)
    {
        if (string.IsNullOrEmpty(label)) 
            return;

        var matches = FindByLabel(label);

        if (!matches.Any() && label.Length == 1)
        {
            var ch = label[0];
            matches = AllKeys.Where(k => 
                (k.LabelPrimary?.Length == 1 && CharEqualsInvariant(k.LabelPrimary[0], ch))
                || (k.LabelShift?.Length == 1 && CharEqualsInvariant(k.LabelShift[0], ch))
                || (k.LabelAltGr?.Length == 1 && CharEqualsInvariant(k.LabelAltGr[0], ch)));
        }

        foreach (var key in matches)
            key.IsPressed = isPressed;
    }

    /// <summary>
    /// Hebt alle gedrückten Tasten auf.
    /// </summary>
    public void ClearPressed()
    {
        foreach (var key in AllKeys.Where(k => k.IsPressed))
            key.IsPressed = false;
    }

    private IEnumerable<KeyViewModel> FindByLabel(string label)
        => AllKeys.Where(k => 
            StringEqualsInvariant(k.LabelPrimary, label)
            || StringEqualsInvariant(k.LabelShift, label)
            || StringEqualsInvariant(k.LabelAltGr, label));

    private static bool StringEqualsInvariant(string? a, string? b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private static bool CharEqualsInvariant(char a, char b)
        => char.ToUpperInvariant(a) == char.ToUpperInvariant(b);

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

    private void LoadGermanQwertzLayout()
    {
        // --- R0: F-Reihe ---
        int c = 0;
        Add(0, c, "Esc", 2); c += 2;
        foreach (var f in new[] { "F1", "F2", "F3", "F4" }) Add(0, c++, f);
        c++;
        foreach (var f in new[] { "F5", "F6", "F7", "F8" }) Add(0, c++, f);
        c++;
        foreach (var f in new[] { "F9", "F10", "F11", "F12" }) Add(0, c++, f);

        Add(0, 17, "PrtSc");
        Add(0, 18, "ScrLk");
        Add(0, 19, "Pause");

        Add(0, 21, "/"); Add(0, 22, "*"); Add(0, 23, "-");
        Add(0, 24, "+", 1, 2);

        // --- R1: Ziffernreihe ---
        c = 0;
        foreach (var k in new[] { "^", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "ß", "´" }) 
            Add(1, c++, k);
        Add(1, c, "Backspace", 2);

        Add(1, 17, "Ins"); Add(1, 18, "Home"); Add(1, 19, "PgUp");
        Add(1, 21, "7"); Add(1, 22, "8"); Add(1, 23, "9");

        // --- R2: Q-Reihe ---
        c = 0;
        Add(2, c, "Tab", 2); c += 2;
        foreach (var k in "QWERTZUIOPÜ") 
            Add(2, c++, k.ToString());
        Add(2, c, "+");

        Add(2, 17, "Del"); Add(2, 18, "End"); Add(2, 19, "PgDn");
        Add(2, 21, "4"); Add(2, 22, "5"); Add(2, 23, "6");

        // --- R3: A-Reihe ---
        c = 0;
        Add(3, c, "?", 2); c += 2;
        foreach (var k in new[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", "Ö", "Ä", "#" }) 
            Add(3, c++, k);
        Add(2, c, "Enter", 2, 2);

        Add(3, 21, "1"); Add(3, 22, "2"); Add(3, 23, "3");
        Add(2, 24, "Enter", 1, 2);

        // --- R4: Y-Reihe ---
        c = 0;
        Add(4, c, "Shift", 2); c += 2;
        Add(4, c++, "< > |");
        foreach (var k in new[] { "Y", "X", "C", "V", "B", "N", "M", ",", ".", "-" }) 
            Add(4, c++, k);
        Add(4, c, "Shift", 3);

        Add(4, 18, "?");

        // --- R5: unterste Reihe ---
        c = 0;
        Add(5, c++, "Ctrl");
        Add(5, c++, "Win");
        Add(5, c++, "Alt");
        Add(5, c, "Space", 6); c += 6;
        Add(5, c++, "AltGr");
        Add(5, c++, "Menu");
        Add(5, c, "Ctrl", 2);

        Add(5, 17, "?"); Add(5, 18, "?"); Add(5, 19, "?");
        Add(4, 21, "0", 2); Add(4, 23, ".");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
