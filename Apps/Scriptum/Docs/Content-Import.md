# Content Import Feature

## Übersicht

Das Content-Import-Feature ermöglicht das Importieren von Modulen, Lektionen und Anleitungen aus JSON-Dateien in die Scriptum-Anwendung.

## Architektur

### Schichten

1. **Scriptum.Application** (UI-frei)
   - `IContentImportService`: Service-Interface
   - `ContentImportService`: Import-Logik-Implementierung
   - `ContentImportRequest`: Request-DTO
   - `ContentImportResult`: Result-DTO
   - Import-DTOs: `ModuleImportDto`, `LessonImportDto`, `GuideImportDto`

2. **Scriptum.Wpf** (UI-Schicht)
   - `ContentImportViewModel`: ViewModel mit UI-Interaktion
   - `ContentImportView`: XAML-View für den Import
   - Integration in Navigation und Shell

### Datenpersistenz

Der Import nutzt die bestehenden JSON-Repositories:
- Module: `%APPDATA%\Scriptum\Content\modules.json`
- Lektionen: `%APPDATA%\Scriptum\Content\lessons.json`
- Anleitungen: `%APPDATA%\Scriptum\Content\lesson-guides.json`

## Verwendung

### Über die UI

1. Starten Sie Scriptum.Wpf
2. Navigieren Sie zu "Content importieren" im Sekundärmenü
3. Wählen Sie die drei JSON-Dateien aus:
   - Module
   - Lektionen
   - Anleitungen
4. Optional: Aktivieren Sie "Vorhandenen Content überschreiben"
5. Klicken Sie auf "Import starten"
6. Nach erfolgreichem Import: Anwendung neu starten

### Import-Dateiformat

#### modules.json

```json
[
  {
    "ModuleId": "M01",
    "Titel": "Grundreihe",
    "Beschreibung": "Die Grundreihe des deutschen QWERTZ-Layouts"
  }
]
```

- `ModuleId`: Eindeutige ID (z.B. "M01", "M02")
- Die Zahl in der ModuleId wird als `Order` extrahiert

#### lessons.json

```json
[
  {
    "LessonId": "L01_01",
    "ModuleId": "M01",
    "Titel": "Grundstellung ASDF JKL;",
    "Beschreibung": "Erste Übung mit der Grundreihe",
    "Schwierigkeit": 1,
    "Tags": ["grundreihe", "anfänger"],
    "Uebungstext": "asdf jklö asdf jklö\nff jj dd kk ss ll aa öö"
  }
]
```

- `LessonId`: Optional. Wird automatisch generiert, wenn nicht angegeben
- `ModuleId`: Muss zu einem vorhandenen Modul passen
- `Uebungstext`: Zeilenumbrüche werden auf "\n" normalisiert

#### guides.json

```json
[
  {
    "LessonId": "L01_01",
    "GuideTextMarkdown": "# Grundstellung\n\nAnleitung im Markdown-Format"
  }
]
```

Alternativ mit Titel-Mapping:

```json
[
  {
    "LessonTitel": "Grundstellung ASDF JKL;",
    "GuideTextMarkdown": "# Grundstellung\n\nAnleitung"
  }
]
```

- `LessonId` oder `LessonTitel`: Zuordnung zur Lektion
- `GuideTextMarkdown`: Anleitung im Markdown-Format

## Validierung und Fehlerbehandlung

### Validierungen

- **Fehlende Dateien**: Fehlermeldung mit Dateipfad
- **Ungültiges JSON**: JsonException mit Details
- **Referenzielle Integrität**: 
  - Lektionen müssen auf existierende Module verweisen
  - Guides müssen auf existierende Lektionen verweisen
- **Doppelte IDs**: Werden als Warnung ausgegeben

### Warnungen

Warnungen führen nicht zum Abbruch, werden aber in der UI angezeigt:
- Fehlende LessonId (wird automatisch generiert)
- Guide ohne Zuordnung
- Referenzen auf nicht existierende Entitäten

### Überschreib-Modus

- **`OverwriteExisting = false`**: Import wird abgebrochen, wenn bereits Content vorhanden ist
- **`OverwriteExisting = true`**: Vorhandener Content wird gelöscht und durch Import ersetzt

## Beispieldateien

Beispieldateien finden Sie unter:
`Apps/Scriptum/Production/Scriptum.Wpf/SampleImport/`

- `modules.json`: 2 Module
- `lessons.json`: 3 Lektionen
- `guides.json`: 3 Anleitungen

## Technische Details

### Abhängigkeiten

Der `ContentImportService` nutzt:
- `IDataStoreProvider`: Zugriff auf Content-DataStores
- `IRepositoryFactory`: Auflösung der JSON-Repositories

### ID-Generierung

Wenn keine `LessonId` angegeben ist, wird sie automatisch generiert:
- Format: `{ModuleId}_{sanitized_titel}`
- Beispiel: `M01_grundstellung_asdf_jkl`
- Bei Duplikaten wird eine fortlaufende Nummer angehängt

### Zeilenumbruch-Normalisierung

Der Übungstext wird normalisiert:
- `\r\n` ? `\n`
- `\r` ? `\n`

Dies stellt sicher, dass die Engine korrekt funktioniert.

## Troubleshooting

### Import schlägt fehl: "Content bereits vorhanden"

**Lösung**: Aktivieren Sie "Vorhandenen Content überschreiben" oder löschen Sie den vorhandenen Content manuell.

### Warnung: "Guide konnte nicht zugeordnet werden"

**Ursache**: Der `LessonTitel` in guides.json stimmt nicht exakt mit dem Titel in lessons.json überein.

**Lösung**: Verwenden Sie `LessonId` statt `LessonTitel` oder korrigieren Sie die Schreibweise.

### Nach Import keine Änderungen sichtbar

**Ursache**: Die DataStores werden nur beim Start geladen.

**Lösung**: Starten Sie die Anwendung neu.

## Erweiterungsmöglichkeiten

- Export-Feature (Content in JSON-Dateien exportieren)
- Bulk-Import von mehreren Sets
- Validierung gegen ein JSON-Schema
- Preview vor dem Import
- Undo/Rollback nach Import
