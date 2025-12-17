using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Orchestriert den Ablauf eines Scans und das Erstellen des Bundles.
/// </summary>
public interface IBundleOrchestrator
{
    /// <summary>
    /// Führt einen Scan des gegebenen Root-Verzeichnisses aus und erstellt das Bundle.
    /// </summary>
    /// <param name="rootPath">Absoluter Pfad zum Root-Verzeichnis der Solution.</param>
    /// <param name="settings">Einstellungen, die den Scan und die Ausgabe steuern.</param>
    /// <returns>Vollständiger Pfad zur erzeugten Ausgabedatei.</returns>
    string Run(string rootPath, ScanSettings settings);

    /// <summary>
    /// Führt einen Scan des gegebenen Root-Verzeichnisses aus und erstellt das Bundle.
    /// </summary>
    /// <param name="rootPath">Absoluter Pfad zum Root-Verzeichnis der Solution.</param>
    /// <param name="settings">Einstellungen, die den Scan und die Ausgabe steuern.</param>
    /// <param name="group">Optionaler Gruppenname aus ProjectInfo.Group für Unterordner-Organisation.</param>
    /// <returns>Vollständiger Pfad zur erzeugten Ausgabedatei.</returns>
    string Run(string rootPath, ScanSettings settings, string? group);
}