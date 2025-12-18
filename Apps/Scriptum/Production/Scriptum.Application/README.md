# Scriptum.Application

## Überblick

`Scriptum.Application` ist die Anwendungsschicht des Scriptum Discipulus-Tipptrainers. Sie koordiniert die Zusammenarbeit zwischen Content, Engine, Progress und Persistence.

## Architektur

### Zentrale Komponente: TrainingSessionCoordinator

Der `TrainingSessionCoordinator` implementiert das `ITrainingSessionCoordinator`-Interface und stellt die zentrale Koordinationslogik bereit:

```
???????????????????????????????????????????
?   TrainingSessionCoordinator           ?
?                                         ?
?  ???????????????????????????????????  ?
?  ? Content-Layer                   ?  ?
?  ? • LessonData                    ?  ?
?  ? • ModuleData                    ?  ?
?  ???????????????????????????????????  ?
?                                         ?
?  ???????????????????????????????????  ?
?  ? Engine-Layer                    ?  ?
?  ? • ITrainingEngine               ?  ?
?  ? • IInputInterpreter             ?  ?
?  ???????????????????????????????????  ?
?                                         ?
?  ???????????????????????????????????  ?
?  ? Progress-Layer                  ?  ?
?  ? • TrainingSession               ?  ?
?  ? • StoredInput / Evaluation      ?  ?
?  ???????????????????????????????????  ?
?                                         ?
?  ???????????????????????????????????  ?
?  ? Persistence (DataToolKit)       ?  ?
?  ? • PersistentDataStore           ?  ?
?  ? • IRepositoryFactory            ?  ?
?  ???????????????????????????????????  ?
???????????????????????????????????????????
```

### Ablauf einer Trainingssitzung

1. **StartSession(moduleId, lessonId)**
   - Lädt Content-Daten (Lektion und Modul)
   - Erstellt TargetSequence aus Übungstext
   - Initialisiert TrainingState über Engine
   - Erstellt neue TrainingSession

2. **ProcessInput(chord)**
   - Interpretiert Tastenkombination via IInputInterpreter
   - Erstellt StoredInput-Eintrag
   - Verarbeitet Eingabe via Engine
   - Speichert StoredEvaluation (falls relevant)
   - Bei Abschluss: IsCompleted=true, EndedAt setzen

3. **Persistierung**
   - Erfolgt automatisch via PersistentDataStore
   - PropertyChanged-Tracking für Entity-Updates
   - Vollständiger Verlauf wird gespeichert

## Dependency Injection

Das `ScriptumApplicationServiceModule` registriert alle Services:

```csharp
services.AddSingleton<ITrainingSessionCoordinator, TrainingSessionCoordinator>();
services.AddSingleton<IInputInterpreter, DeQwertzInputInterpreter>();
services.AddSingleton<IClock, SystemClock>();
services.AddSingleton<ITrainingEngine, TrainingEngine>();
```

### Abhängigkeiten

Das Modul benötigt folgende vorherige Registrierungen:
- **DataToolKit** (DataStoreProvider, Repositories)
- **Scriptum.Engine** (TrainingEngine, InputInterpreter, Clock)
- **Scriptum.Content** (Content-DataStores)
- **Scriptum.Progress** (Progress-DataStores)

## Designprinzipien

### UI-Freiheit
- Keine WPF-Abhängigkeiten
- Arbeitet mit fachlichen Typen (KeyChord, EvaluationEvent)
- Testbar ohne UI-Kontext

### Deterministisch
- IClock-Abstraktion für testbare Zeitsteuerung
- IInputInterpreter für austauschbare Tastatur-Mappings
- Keine direkten DateTime.Now-Aufrufe

### Vollständige Historie
- Alle Eingaben werden als StoredInput gespeichert
- Alle Bewertungen werden als StoredEvaluation gespeichert
- Ermöglicht spätere Auswertungen und Statistiken

### DataToolKit-Integration
- Nutzt PersistentDataStore für automatische Persistierung
- Keine manuellen Repository-Aufrufe
- PropertyChanged-Tracking für Entity-Updates (TrainingSession)

## Tests

Alle Tests befinden sich in `Scriptum.Application.Tests`:

- **TrainingSessionCoordinatorStartSessionTests**: Tests für Session-Start
- **TrainingSessionCoordinatorProcessInputTests**: Tests für Eingabeverarbeitung
- **TrainingSessionCoordinatorCompletionTests**: Tests für Session-Abschluss

### Test-Hilfsmittel

- **FakeClock**: Deterministische Zeitsteuerung für Tests
- **FakeDataStoreProvider**: In-Memory-Persistierung für Tests
- **FakeRepositoryFactory**: Fake-Repositories ohne I/O

## Verwendung

```csharp
// Setup DI
var services = new ServiceCollection();
services.AddModulesFromAssemblies(
    typeof(DataToolKitServiceModule).Assembly,
    typeof(ScriptumEngineServiceModule).Assembly,
    typeof(ScriptumApplicationServiceModule).Assembly);

var provider = services.BuildServiceProvider();
var coordinator = provider.GetRequiredService<ITrainingSessionCoordinator>();

// Training starten
coordinator.StartSession("Modul1", "Lektion1");

// Eingaben verarbeiten
var evaluation = coordinator.ProcessInput(new KeyChord(KeyId.A, ModifierSet.None));
if (evaluation != null)
{
    Console.WriteLine($"Bewertung: {evaluation.Outcome}");
}

// Status prüfen
if (coordinator.IsSessionRunning)
{
    Console.WriteLine($"Position: {coordinator.CurrentState.CurrentTargetIndex}");
}
```

## Referenzen

- [Scriptum Domänenmodell](../../Modelle/Scriptum_Domaenenmodell_Core_Engine.md)
- [DataToolKit Repository-Pattern](../../../../DataToolKit/README.md)
- [Common.Bootstrap Modulares DI](../../../../Common.BootStrap/README.md)
