namespace DataToolKit.Abstractions.Repositories;

/// <summary>
/// Generisches Interface für typsichere Storage-Optionen.
/// Jeder Datentyp T hat genau eine IStorageOptions&lt;T&gt;-Registrierung als Singleton im DI-Container.
/// </summary>
/// <typeparam name="T">Der Entitätstyp, für den diese Optionen gelten.</typeparam>
public interface IStorageOptions<T>
{
    /// <summary>
    /// Name des Anwendungs-Unterordners unterhalb von "Eigene Dokumente".
    /// Beispiel: "TypeTutor", "DataToolKit".
    /// </summary>
    string AppSubFolder { get; }

    /// <summary>
    /// Optionaler, zusätzlicher Unterordner innerhalb von <see cref="AppSubFolder"/>.
    /// Dient der weiteren Strukturierung (z. B. "Backup", "Module", "Logs").
    /// </summary>
    string? SubFolder { get; }

    /// <summary>
    /// Basisname der Zieldatei ohne Erweiterung.
    /// </summary>
    string FileNameBase { get; }

    /// <summary>
    /// Absoluter Pfad zum Benutzerordner "Eigene Dokumente".
    /// </summary>
    string RootFolder { get; }

    /// <summary>
    /// Vollständiger Pfad des Zielverzeichnisses:
    /// MyDocuments\<see cref="AppSubFolder"/>\<see cref="SubFolder"/> (Sub optional).
    /// </summary>
    string EffectiveRoot { get; }

    /// <summary>
    /// Vollständiger Dateipfad inklusive Dateiname und abgeleiteter Erweiterung.
    /// Beispiel: C:\Users\Name\Documents\MyApp\data.json
    /// </summary>
    string FullPath { get; }
}
