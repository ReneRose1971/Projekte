namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Klassifiziert Dateien nach Inhaltstyp, z.B. für Syntax-Highlighting.
/// </summary>
public interface IContentClassifier
{
    /// <summary>
    /// Liefert einen Sprach- oder Format-Identifikator für die angegebene Datei.
    /// </summary>
    /// <param name="filePath">Pfad zur Datei (vollständig oder relativ).</param>
    /// <returns>String, der die Sprache/Format beschreibt (z. B. "csharp", "json").</returns>
    string Classify(string filePath);
}