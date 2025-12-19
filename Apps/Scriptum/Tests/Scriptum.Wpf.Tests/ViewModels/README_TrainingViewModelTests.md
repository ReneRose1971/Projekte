# TrainingViewModel Characterization Tests

## Zweck

Diese Tests sichern das **bestehende Verhalten** des `TrainingViewModel` ab, bevor ein Refactoring durchgeführt wird.

## Test-Abdeckung

### ? Vollständig getestet (26 Tests)

#### Constructor Tests (6 Tests)
- Validierung aller Constructor-Parameter
- ArgumentNullException für fehlende Dependencies

#### Initialize Tests (3 Tests)
- Setzt ModuleId und LessonId
- Ruft Coordinator.StartSession auf

#### UI-State Tests (12 Tests)
- `DisplayTarget`: Zieltext-Anzeige
- `DisplayInput`: Eingabe-Historie
- `CurrentIndex`: Aktuelle Position
- `IsCompleted`: Abschluss-Status
- `ErrorCount`: Fehleranzahl
- `StatusText`: Status-Meldung

#### Navigation Tests (2 Tests)
- `NavigateBack`: Navigation zu LessonDetails
- `ToggleGuide`: Guide-Sichtbarkeit

#### GuideText Tests (2 Tests)
- Anzeige von Hilfe-Texten
- Fallback-Meldungen

### ?? Übersprungen (4 Tests)

#### OnKeyDown Tests (4 Tests)
**Grund**: Benötigen WPF STA-Thread für `KeyEventArgs`-Erstellung

Diese Tests werden **nach dem Refactoring** neu geschrieben, wenn:
- `IKeyboardInputHandler` extrahiert ist
- Input-Handling testbar ist ohne WPF-Dependencies

## Test-Strategie

### Was getestet wird
- ? Constructor-Validierung
- ? Property-Initialisierung
- ? Navigation-Aufrufe
- ? Daten-Projection (DisplayTarget, DisplayInput)
- ? Status-Berechnung (IsCompleted, ErrorCount)

### Was NICHT getestet wird
- ? `MapKeyToLabel()` (wird in separaten KeyCodeMapper extrahiert)
- ? `OnKeyDown()`/`OnKeyUp()` (wird in KeyboardInputHandler extrahiert)
- ? `RefreshUI()` (wird durch Fody PropertyChanged ersetzt)

## Refactoring-Plan

Nach Extraktion der Verantwortlichkeiten werden folgende neue Test-Klassen erstellt:

1. **KeyboardInputHandlerTests**
   - OnKeyDown/OnKeyUp Logik
   - Keyboard-State-Management

2. **KeyCodeMapperTests**
   - MapKeyToLabel() für alle Keys
   - DE-QWERTZ spezifisches Mapping

3. **TrainingViewModelTests (neu)**
   - Reduziert auf Komposition & Koordination
   - Keine Low-Level Input-Logik

## Verwendung

```bash
# Alle Tests ausführen
dotnet test Scriptum.Wpf.Tests.csproj --filter "FullyQualifiedName~TrainingViewModelTests"

# Nur erfolgreiche Tests
dotnet test Scriptum.Wpf.Tests.csproj --filter "FullyQualifiedName~TrainingViewModelTests" --filter "FullyQualifiedName!~OnKeyDown"
```

## Erkenntnisse

### Bestätigtes Verhalten
1. ViewModel ist **stark gekoppelt** an Coordinator und DataStore
2. Property-Updates erfolgen **manuell** via `RefreshUI()`
3. Keyboard-Mapping ist **hart codiert** in ViewModel
4. Navigation erfolgt **direkt** ohne Command-Pattern

### Refactoring-Potenzial
1. ?? 60+ Zeilen `MapKeyToLabel()` ? Extrahieren
2. ?? Manuelle PropertyChanged ? Fody nutzen
3. ?? UI-Event-Handling ? IKeyboardInputHandler
4. ?? Direkte Navigation-Calls ? Commands

## Status

- ? **26/30 Tests erfolgreich**
- ?? **4 Tests übersprungen** (WPF STA-Thread erforderlich)
- ?? **Bereit für Refactoring**

---

**Erstellt**: 2024
**Zweck**: Anti-Regression Tests vor TrainingViewModel Refactoring
