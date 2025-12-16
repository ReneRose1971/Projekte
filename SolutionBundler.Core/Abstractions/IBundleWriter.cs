using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Schreibt ein Bundle aus den gesammelten Informationen in eine Datei.
/// </summary>
public interface IBundleWriter
{
    /// <summary>
    /// Schreibt das Bundle für das angegebene Projekt.
    /// </summary>
    /// <param name="rootPath">Root-Pfad des Projekts.</param>
    /// <param name="files">Sammlung von Dateieinträgen, die in das Bundle aufgenommen werden sollen.</param>
    /// <param name="settings">Scan- und Ausgabe-Einstellungen (enthält OutputFileName).</param>
    /// <returns>Pfad zur erzeugten Bundle-Datei.</returns>
    string Write(string rootPath, IEnumerable<FileEntry> files, ScanSettings settings);
}