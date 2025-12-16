using SolutionBundler.Core.Models;
using System.Collections.Generic;

namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Liest und ergänzt Metadaten zu Projekten (z. B. welche Dateien welche BuildAction haben).
/// </summary>
public interface IProjectMetadataReader
{
    /// <summary>
    /// Ermittelt Build-Actions für die Einträge und schreibt die Informationen in die übergebene Liste.
    /// </summary>
    /// <param name="entries">Liste der zu ergänzenden Dateieinträge.</param>
    /// <param name="rootPath">Wurzelpfad der Solution, zur Berechnung relativer Pfade.</param>
    void EnrichBuildActions(IList<FileEntry> entries, string rootPath);
}