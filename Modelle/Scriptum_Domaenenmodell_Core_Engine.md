# Scriptum Discipulus – Überarbeitetes Domänenmodell (Core & Engine)

## Einleitung

Dieses Dokument beschreibt das **überarbeitete und konsolidierte Domänenmodell** für Scriptum Discipulus.
Es ersetzt frühere Entwürfe und dient als **verbindliche fachliche Referenz** für:

- Architekturentscheidungen
- Implementierung in Scriptum.Core und Scriptum.Engine
- Copilot-Instruktionen
- Tests und spätere Erweiterungen

Der Fokus liegt darauf, **klar zu trennen**,  
*was trainiert wird*, *was gemessen wird* und *wie der Ablauf logisch funktioniert*.

---

## Didaktische Grundannahmen (fest)

1. Fehler **müssen korrigiert werden**, bevor fortgefahren werden darf.
2. Zeilenumbrüche sind **vollwertige Zielzeichen**.
3. Der **vollständige Eingabeverlauf** wird gespeichert.
4. Es handelt sich **nicht** um einen Texteditor, sondern um ein **positionsbasiertes Training**.

Diese Annahmen beeinflussen das Modell fundamental.

---

## Zentrale Modellkorrektur (gegenüber früheren Entwürfen)

### 1. Trennung von drei Ebenen

Das Modell unterscheidet nun strikt zwischen:

1. **Ziel (was soll gelernt werden?)**
2. **Eingabe (was hat der Nutzer getan?)**
3. **Bewertung (wie verhält sich die Eingabe zum Ziel?)**

Diese Trennung verhindert Vermischung von UI-, Editor- oder Persistenzlogik.

---

## Scriptum.Core – Fachliche Grundbausteine

Scriptum.Core enthält **keinen Ablauf**, sondern ausschließlich **Bedeutungsträger**.

### Zielmodell

#### Zielzeichen (TargetSymbol)

Ein Zielzeichen ist die **kleinste didaktische Einheit** einer Übung.

Eigenschaften:
- Position (Index)
- Erwartetes Graphem (string)

Beispiele:
- „a“
- „A“
- „ä“
- „ß“
- „\n“ (Zeilenumbruch)

Ein Zielzeichen kennt **keine Taste**, keine Umschalttaste und keine Bewertung.

---

#### Zielsequenz (TargetSequence)

Die Zielsequenz ist eine **geordnete Liste von Zielzeichen**.

Sie definiert:
- Länge der Übung
- Reihenfolge der erwarteten Eingaben
- den didaktischen Inhalt

Die Zielsequenz ist **unveränderlich** während einer Sitzung.

---

### Eingabemodell

#### Taste (KeyId)

Eine fachliche Kennung einer Taste auf einer DE‑QWERTZ‑Tastatur.

Beispiele:
- A
- Enter
- Space
- Backspace
- Digit1
- Oem102

Keine UI-Abhängigkeit, keine Scan-Codes.

---

#### Umschalttasten (ModifierSet)

Kombination aus:
- Keine
- Umschalt (Shift)
- AltGr

Ctrl wird nicht als Trainingsinhalt betrachtet.

---

#### Tastenkombination (KeyChord)

Kombination aus:
- KeyId
- ModifierSet

Dies ist die **roheste** fachliche Beschreibung einer Eingabe.

---

### Bewertungsbegriffe

#### Bewertungsergebnis (EvaluationOutcome)

- Richtig
- Falsch
- Korrigiert

Diese Begriffe sind fachlich eindeutig und UI-neutral.

---

## Scriptum.Engine – Ablauf- und Bewertungslogik

Scriptum.Engine beschreibt **wie eine Übung abläuft**, nicht was gespeichert wird.

---

### Eingabeereignis (InputEvent)

Ein InputEvent beschreibt **eine Handlung des Nutzers**.

Es enthält:
- Zeitpunkt
- Tastenkombination (KeyChord)
- Eingabeart:
  - Zeichen
  - Rücktaste
  - Ignoriert

Optional:
- erzeugtes Graphem (string)

Wichtig:
Ein InputEvent weiß **nicht**, ob es richtig oder falsch war.

---

### Bewertungsergebnis (EvaluationEvent)

Ein EvaluationEvent beschreibt die **Beziehung zwischen Eingabe und Ziel**.

Es enthält:
- Zielposition
- Erwartetes Graphem
- Tatsächlich erzeugtes Graphem (oder leer)
- Bewertungsergebnis (Outcome)

EvaluationEvents entstehen **nur**, wenn eine Eingabe fachlich relevant ist.

---

### Trainingszustand (TrainingState)

Der Trainingszustand ist ein **rein fachlicher Zustandsschnappschuss**.

Er beschreibt:
- Referenz auf die Zielsequenz
- Aktuelle Zielposition
- Startzeit
- Endzeit (falls abgeschlossen)
- Fehlerstatus:
  - IstFehlerAktiv
  - FehlerPosition
- Zähler:
  - Gesamteingaben
  - Fehler
  - Korrekturen
  - Rücktasten

Der Trainingszustand enthält **keine Historie**.

---

### Zentrale Ablaufregeln

#### Richtige Eingabe
- Zielposition wird erhöht
- Fehlerstatus bleibt inaktiv
- Bewertung: Richtig

---

#### Falsche Eingabe
- Fehlerstatus wird aktiviert
- Zielposition bleibt stehen
- Bewertung: Falsch

---

#### Rücktaste bei aktivem Fehler
- Fehlerstatus wird aufgehoben
- Bewertung: Korrigiert
- Zielposition bleibt unverändert

---

#### Rücktaste ohne Fehler
- Eingabe wird protokolliert
- Keine Zustandsänderung

---

#### Zeilenumbruch
- Wird exakt wie jedes andere Zielzeichen behandelt
- Enter ist fachlich relevant

---

### Trainingsmaschine (ITrainingEngine)

Die Trainingsmaschine:
- erzeugt einen Startzustand
- wendet InputEvents an
- liefert:
  - neuen Trainingszustand
  - optional ein EvaluationEvent

Sie ist:
- deterministisch
- UI-frei
- zustandsbasiert

---

## Speicherung (außerhalb der Engine)

Die Engine speichert **nichts**.

Gespeichert werden später:
- Zielreferenz
- Start- und Endzeit
- vollständige Liste der InputEvents
- vollständige Liste der EvaluationEvents

Dadurch sind spätere Auswertungen beliebig erweiterbar.

---

## Bewusste Abgrenzungen

- Kein Texteditor-Verhalten
- Kein Cursor-Springen
- Keine Layout-Umschaltung
- Keine UI-Logik
- Keine Persistenzlogik

---

## Fazit

Dieses Modell ist:
- didaktisch eindeutig
- technisch testbar
- architektonisch stabil
- erweiterbar ohne Bruch

Es bildet die verbindliche Grundlage für Scriptum.Core und Scriptum.Engine.
