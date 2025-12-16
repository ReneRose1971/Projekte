using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Models;

/// <summary>
/// Repräsentiert eine einzelne Datei innerhalb der gescannten Solution mit Metadaten.
/// </summary>
public sealed class FileEntry
{
    /// <summary>
    /// Relativer Pfad zur Datei innerhalb der Solution (Slash-separiert).
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Absoluter Pfad zur Datei im Dateisystem.
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// Dateigröße in Bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// SHA1-Prüfsumme des Dateiinhalts (hex-kodiert).
    /// </summary>
    public string Sha1 { get; set; } = "";

    /// <summary>
    /// Sprach-/Formatkennzeichnung für Code-Fences (z. B. "csharp", "json").
    /// </summary>
    public string Language { get; set; } = "";

    /// <summary>
    /// Abgebildete BuildAction (Compile, Content, Resource etc.).
    /// </summary>
    public BuildAction Action { get; set; } = BuildAction.Unknown;
}