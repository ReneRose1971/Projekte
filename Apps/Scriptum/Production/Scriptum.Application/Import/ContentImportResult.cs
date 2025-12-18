namespace Scriptum.Application.Import;

/// <summary>
/// Beschreibt das Ergebnis eines Content-Imports.
/// </summary>
public sealed class ContentImportResult
{
    /// <summary>
    /// Gibt an, ob der Import erfolgreich war.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Anzahl der importierten Module.
    /// </summary>
    public int ModulesImported { get; init; }

    /// <summary>
    /// Anzahl der importierten Lektionen.
    /// </summary>
    public int LessonsImported { get; init; }

    /// <summary>
    /// Anzahl der importierten Anleitungen.
    /// </summary>
    public int GuidesImported { get; init; }

    /// <summary>
    /// Liste von Warnungen, die während des Imports aufgetreten sind.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Pfad zum Ausgabeordner, in dem die importierten Daten gespeichert wurden.
    /// </summary>
    public string OutputFolderPath { get; init; } = string.Empty;

    /// <summary>
    /// Fehlermeldung, wenn der Import fehlgeschlagen ist.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
