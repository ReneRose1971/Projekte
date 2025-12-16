using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Scannt ein Verzeichnis nach relevanten Projekt-/Dateieinträgen für das Bundle.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// Scannt das übergebene Root-Verzeichnis nach Dateien entsprechend der übergebenen Einstellungen.
    /// </summary>
    /// <param name="rootPath">Absoluter Pfad des Root-Verzeichnisses.</param>
    /// <param name="settings">Scan-Einstellungen, die Inklusions-/Exklusionsmuster enthalten.</param>
    /// <returns>Eine unveränderbare Liste von `FileEntry`-Objekten.</returns>
    IReadOnlyList<FileEntry> Scan(string rootPath, ScanSettings settings);
}