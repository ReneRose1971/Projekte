# TrainingViewModel Refactoring - Schritt 3: Commands

## ? Abgeschlossen

### Änderungen

#### Neue Dateien

1. **`RelayCommand.cs`** (Scriptum.Wpf)
   - Einfache ICommand-Implementierung
   - Unterstützt CanExecute-Logik
   - Integration mit CommandManager.RequerySuggested

#### Geänderte Dateien

1. **`TrainingViewModel.cs`**
   - **Hinzugefügt**: `NavigateBackCommand` als ICommand
   - **Hinzugefügt**: `ToggleGuideCommand` als ICommand
   - **Deprecated**: `NavigateBack()` Methode (Rückwärtskompatibilität)
   - **Deprecated**: `ToggleGuide()` Methode (Rückwärtskompatibilität)
   - **Neu**: `ExecuteNavigateBack()` (private)
   - **Neu**: `ExecuteToggleGuide()` (private)

2. **`TrainingViewModelTests.cs`**
   - **Hinzugefügt**: 7 neue Command-Tests
   - Tests für CanExecute, Execute, und Behavior

### Vorteile

? **MVVM-Konformität**: Commands statt Methoden-Aufrufe  
? **Bessere Testbarkeit**: Commands sind einfacher zu testen  
? **Deklaratives Binding**: XAML kann direkt an Commands binden  
? **CanExecute-Support**: UI kann automatisch disabled werden  
? **Rückwärtskompatibilität**: Alte Methoden bleiben (deprecated)

### Code-Vergleich

**Vorher (Methoden)**:
```csharp
public void NavigateBack()
{
    _navigationService.NavigateToLessonDetails(ModuleId, LessonId);
}

public void ToggleGuide()
{
    IsGuideVisible = !IsGuideVisible;
}
```

**View Code-Behind**:
```csharp
private void BackButton_Click(object sender, RoutedEventArgs e)
{
    ViewModel?.NavigateBack();
}
```

**Nachher (Commands)**:
```csharp
public ICommand NavigateBackCommand { get; }
public ICommand ToggleGuideCommand { get; }

// Constructor
NavigateBackCommand = new RelayCommand(_ => ExecuteNavigateBack());
ToggleGuideCommand = new RelayCommand(_ => ExecuteToggleGuide());

private void ExecuteNavigateBack()
{
    _navigationService.NavigateToLessonDetails(ModuleId, LessonId);
}

private void ExecuteToggleGuide()
{
    IsGuideVisible = !IsGuideVisible;
}
```

**XAML (zukünftige Verwendung)**:
```xaml
<Button Content="Zurück" Command="{Binding NavigateBackCommand}" />
<Button Content="Hilfe" Command="{Binding ToggleGuideCommand}" />
```

### Migration Path

Die View verwendet aktuell noch die deprecated Methoden:
```csharp
// TrainingView.xaml.cs
private void BackButton_Click(object sender, RoutedEventArgs e)
{
    ViewModel?.NavigateBack(); // ruft ExecuteNavigateBack() auf
}
```

**Nächster Schritt**: XAML aktualisieren, um Commands direkt zu verwenden, dann Code-Behind entfernen.

### Test-Ergebnisse

```
? DeQwertzKeyCodeMapperTests:           36/36 Tests bestanden
? TrainingKeyboardInputHandlerTests:     8/20 Tests bestanden (12 übersprungen)
? TrainingViewModelTests:               34/38 Tests bestanden (4 übersprungen)
    - Neue Command-Tests:                 7/7 Tests bestanden
? Gesamt:                               78/94 Tests bestanden (16 übersprungen)
```

### Neue Tests

1. **NavigateBackCommand_ShouldNotBeNull**: Verifiziert Command-Initialisierung
2. **NavigateBackCommand_CanExecute_ShouldReturnTrue**: Immer ausführbar
3. **NavigateBackCommand_Execute_ShouldCallNavigationService**: Funktionalität
4. **ToggleGuideCommand_ShouldNotBeNull**: Verifiziert Command-Initialisierung
5. **ToggleGuideCommand_CanExecute_ShouldReturnTrue**: Immer ausführbar
6. **ToggleGuideCommand_Execute_WhenFalse_ShouldSetToTrue**: Toggle-Logik
7. **ToggleGuideCommand_Execute_WhenTrue_ShouldSetToFalse**: Toggle-Logik

### Nächste Schritte

#### Schritt 4: RefreshUI eliminieren
- Verwende Fody PropertyChanged-Weaving effektiver
- Entferne manuelle PropertyChanged-Aufrufe
- [DoNotNotify] für berechnete Properties

#### Optional: XAML Command-Binding
- Aktualisiere TrainingView.xaml
- Entferne Click-Handler aus Code-Behind
- Nutze Command-Binding direkt

---

**Status**: ? Schritt 3 abgeschlossen  
**Zeilen hinzugefügt**: +20 Zeilen (Commands + Tests)  
**Neue Tests**: +7 Tests (gesamt: +52 Tests)  
**Build**: ? Erfolgreich  
**Rückwärtskompatibilität**: ? Deprecated Methoden bleiben funktional
