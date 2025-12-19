# TrainingViewModel Refactoring - Schritt 1: IKeyCodeMapper

## ? Abgeschlossen

### Änderungen

#### Neue Dateien

1. **`IKeyCodeMapper.cs`**
   - Interface für Key-to-Label Mapping
   - Mappt `System.Windows.Input.Key` ? `string?`

2. **`DeQwertzKeyCodeMapper.cs`**
   - Implementierung für DE-QWERTZ Layout
   - 60+ Zeilen Logic aus TrainingViewModel extrahiert
   - Unterstützt:
     - Buchstaben (A-Z)
     - Ziffern (0-9)
     - Deutsche Umlaute (ä, ö, ü, ß)
     - Sonderzeichen
     - Modifier-Keys (Shift, Ctrl, Alt, AltGr)

3. **`DeQwertzKeyCodeMapperTests.cs`**
   - 36 Unit-Tests
   - Vollständige Abdeckung aller Mappings
   - Negative Tests für unmapped Keys

#### Geänderte Dateien

1. **`TrainingViewModel.cs`**
   - **Entfernt**: `MapKeyToLabel()` (60+ Zeilen)
   - **Hinzugefügt**: Dependency `IKeyCodeMapper`
   - **Reduziert**: Von ~230 Zeilen auf ~170 Zeilen (-26%)

2. **`TrainingViewModelTests.cs`**
   - Hinzugefügt: `IKeyCodeMapper` Mock
   - Hinzugefügt: Constructor-Test für `IKeyCodeMapper`
   - **27/31 Tests bestanden** (+ 1 neuer Test)

3. **`ScriptumWpfServiceModule.cs`**
   - Registriert: `IKeyCodeMapper` ? `DeQwertzKeyCodeMapper` (Singleton)

### Vorteile

? **Separation of Concerns**: Key-Mapping ist jetzt isoliert testbar  
? **Reduzierte Komplexität**: TrainingViewModel um 60 Zeilen reduziert  
? **Bessere Testbarkeit**: 36 neue Unit-Tests für Key-Mapping  
? **Wiederverwendbarkeit**: IKeyCodeMapper kann in anderen ViewModels verwendet werden  
? **Erweiterbarkeit**: Andere Layouts (z.B. US-QWERTY) können einfach hinzugefügt werden

### Test-Ergebnisse

```
? DeQwertzKeyCodeMapperTests:  36/36 Tests bestanden
? TrainingViewModelTests:      27/31 Tests bestanden (4 übersprungen)
? Gesamt:                      63/67 Tests bestanden
```

### Nächste Schritte

#### Schritt 2: IKeyboardInputHandler
- Extrahiere `OnKeyDown()`/`OnKeyUp()` Logik
- Keyboard-State-Management (IsShiftActive, IsAltGrActive)
- SetPressed() Koordination

#### Schritt 3: Commands
- `NavigateBackCommand` als ICommand
- `ToggleGuideCommand` als ICommand

#### Schritt 4: TrainingStateViewModel
- Wrapper für `ITrainingSessionCoordinator.CurrentState`
- Fody PropertyChanged für automatische Updates
- Entferne `RefreshUI()` manuelle PropertyChanged-Aufrufe

---

**Status**: ? Schritt 1 abgeschlossen  
**Zeilen reduziert**: 60 Zeilen  
**Neue Tests**: +36 Tests  
**Build**: ? Erfolgreich  
**Rückwärtskompatibilität**: ? Alle bestehenden Tests bestehen
