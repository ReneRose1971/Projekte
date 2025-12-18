# Scriptum.Wpf.Keyboard

Visuelle deutsche QWERTZ-Tastatur für Scriptum - reine WPF UI-Komponente ohne Fachlogik.

## Übersicht

Dieses Projekt enthält eine WPF UserControl-Implementierung einer visuellen deutschen QWERTZ-Tastatur. Die Tastatur ist vollständig entkoppelt von TypeTutor und kennt nur visuelle Labels - keine KeyCodes, Engine-Logik oder Persistenz.

## Architektur

### Abhängigkeiten

- ? WPF (PresentationFramework, WindowsBase)
- ? KEINE TypeTutor-Referenzen
- ? KEINE Scriptum.Engine / Progress / Persistence
- ? KEINE DI-Logik oder ServiceModules

### Komponenten

- **VisualKeyboard** (UserControl): Die Haupt-UI-Komponente
- **VisualKeyboardViewModel**: ViewModel mit label-basierter API
- **KeyViewModel**: Repräsentiert eine einzelne Taste
- **Converters**: BoolToOpacityConverter, StringNullOrEmptyToVisibilityConverter
- **Themes/KeyboardTheme.xaml**: Visuelle Styles für die Tasten

## API

### VisualKeyboardViewModel

```csharp
public sealed class VisualKeyboardViewModel
{
    // Layout-Informationen
    public int RowCount { get; }        // 6 Reihen
    public int ColumnCount { get; }     // 26 Spalten
    
    // Modifier-Zustände (für visuelle Feedback)
    public bool IsShiftActive { get; set; }
    public bool IsAltGrActive { get; set; }
    
    // Alle Tasten der Tastatur
    public ObservableCollection<KeyViewModel> AllKeys { get; }
    
    // Haupt-API: Label-basiertes Highlighting
    public void SetPressed(string label, bool isPressed);
    public void ClearPressed();
}
```

### Label-Format

Die Tastatur verwendet folgende Label-Konventionen:

**Buchstaben**: "A", "B", "C", ..., "Z"
**Ziffern**: "0", "1", ..., "9"
**Umlaute**: "Ä", "Ö", "Ü", "ß"
**Sonderzeichen**: ",", ".", "-", "+", "#"
**Sondertasten**:
- "Space", "Enter", "Backspace", "Tab", "Esc"
- "Shift", "Ctrl", "Alt", "AltGr", "Win", "Menu"
- "?", "?", "?", "?"
- "Ins", "Del", "Home", "End", "PgUp", "PgDn"
- "PrtSc", "ScrLk", "Pause"
- "F1", "F2", ..., "F12"
- "?" (Caps Lock)
- "< > |" (Kleiner-Größer-Taste)

## Verwendung in Scriptum.Wpf

### XAML

```xaml
<Window xmlns:keyboard="clr-namespace:Scriptum.Wpf.Keyboard.Controls;assembly=Scriptum.Wpf.Keyboard">
    <Grid>
        <keyboard:VisualKeyboard x:Name="KeyboardControl" />
    </Grid>
</Window>
```

### Code-Behind

```csharp
using Scriptum.Wpf.Keyboard.ViewModels;

public partial class MainWindow : Window
{
    private readonly VisualKeyboardViewModel _keyboardViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        _keyboardViewModel = new VisualKeyboardViewModel();
        KeyboardControl.DataContext = _keyboardViewModel;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // WPF Key -> Label konvertieren
        var label = MapKeyToLabel(e.Key);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, true);
            
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = true;
        }
    }

    private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        var label = MapKeyToLabel(e.Key);
        if (!string.IsNullOrEmpty(label))
        {
            _keyboardViewModel.SetPressed(label, false);
            
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _keyboardViewModel.IsShiftActive = false;
        }
    }
}
```

## Integration mit Scriptum.Engine

Wenn die Tastatur später mit der Training-Engine integriert werden soll:

```csharp
// In einem Adapter oder Koordinator
private void OnKeyPressed(KeyChord chord)
{
    // KeyChord -> Label konvertieren
    var label = ConvertChordToLabel(chord);
    
    // An Keyboard weiterleiten
    _keyboardViewModel.SetPressed(label, true);
}
```

## Design-Prinzipien

1. **Reine UI-Komponente**: Keine Fachlogik, nur visuelle Darstellung
2. **Label-basiert**: Keyboard kennt nur Beschriftungen, keine Semantik
3. **Modifier-agnostisch**: Shift/AltGr werden als normale Tasten behandelt
4. **Event-neutral**: Keyboard reagiert nicht auf Events, wird aktiv gesteuert
5. **Keine KeyCode-Abhängigkeiten**: Vollständig entkoppelt von TypeTutor

## Layout

Deutsche QWERTZ-Tastatur mit:
- 6 Zeilen (inkl. F-Tasten)
- 26 Spalten
- Abgesetzter Navigationsblock
- Numerischer Block (NumPad)
- Grid-basiertes Layout mit Star-Sizing

## Migration von TTVisualKeyboard

Dieses Projekt ersetzt `TTVisualKeyboard` vollständig. Die wesentlichen Änderungen:

- ? Entfernt: TypeTutor.Logic-Referenz
- ? Entfernt: KeyCode-basiertes Mapping
- ? Entfernt: SetPressed(KeyCode, bool) Methode
- ? Hinzugefügt: SetPressed(string label, bool) Methode
- ? Vereinfacht: ClearPressed() ohne Parameter
- ? Namespace: TTVisualKeyboard.* ? Scriptum.Wpf.Keyboard.*

## Zukunft

TTVisualKeyboard kann nach erfolgreicher Integration vollständig aus der Solution entfernt werden.
