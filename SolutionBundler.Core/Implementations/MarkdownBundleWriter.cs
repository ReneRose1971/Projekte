using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Implementations.BundleWriting;
using SolutionBundler.Core.Models;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Schreibt ein Markdown-Bundle aus den übergebenen Projekt-Daten und Dateien.
/// Der Dateiname wird aus settings.OutputFileName verwendet.
/// </summary>
public sealed class MarkdownBundleWriter : IBundleWriter
{
    private readonly FileContentReader _contentReader;

    /// <summary>
    /// Erstellt einen neuen Writer mit einem Secret-Masker zum Entfernen sensibler Informationen.
    /// </summary>
    /// <param name="masker">Implementierung, die Geheimnisse in Dateiinhalten maskiert.</param>
    public MarkdownBundleWriter(ISecretMasker masker)
    {
        _contentReader = new FileContentReader(masker);
    }

    /// <summary>
    /// Schreibt das Bundle als Markdown-Datei.
    /// </summary>
    /// <param name="rootPath">Root-Pfad des Projekts.</param>
    /// <param name="files">Sammlung von Dateien, die ins Bundle aufgenommen werden sollen.</param>
    /// <param name="settings">Einstellungen für Scan und Ausgabe (enthält OutputFileName und MaskSecrets).</param>
    /// <returns>Vollständiger Pfad zur erzeugten Markdown-Datei.</returns>
    public string Write(string rootPath, IEnumerable<FileEntry> files, ScanSettings settings)
    {
        var fileList = files.ToList();
        var projectName = Path.GetFileName(rootPath);

        var markdownContent = MarkdownGenerator.Generate(
            projectName,
            fileList,
            _contentReader,
            settings.MaskSecrets);

        var outputPath = OutputPathResolver.ResolveOutputPath(settings, projectName);

        File.WriteAllText(outputPath, markdownContent, Encoding.UTF8);

        return outputPath;
    }
}