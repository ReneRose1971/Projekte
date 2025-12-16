using SolutionBundler.Core.Abstractions;
using System.IO;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Einfache Implementierung zur Klassifikation von Dateien anhand ihrer Extension.
/// Liefert Code-Fence-Sprachkennungen für Markdown-Ausgabe.
/// </summary>
public sealed class SimpleContentClassifier : IContentClassifier
{
    /// <summary>
    /// Liefert eine Zeichenfolge, die die Sprache/Format für Markdown-Codefences beschreibt.
    /// </summary>
    /// <param name="filePath">Pfad zur Datei.</param>
    /// <returns>Sprachkennzeichnung wie "csharp", "json" oder leere Zeichenfolge.</returns>
    public string Classify(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "csharp",
            ".xaml" => "xaml",
            ".csproj" or ".props" or ".targets" or ".resx" or ".config" => "xml",
            ".json" => "json",
            ".sln" => "",
            _ => ""
        };
    }
}