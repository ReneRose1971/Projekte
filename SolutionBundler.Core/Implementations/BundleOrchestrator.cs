using SolutionBundler.Core.Abstractions;
using SolutionBundler.Core.Models;
using System.IO;
using System.Linq;

namespace SolutionBundler.Core.Implementations;

/// <summary>
/// Koordiniert die Durchführung des Scans und das Erstellen des Bundles.
/// </summary>
public sealed class BundleOrchestrator : IBundleOrchestrator
{
    private readonly IFileScanner _scanner;
    private readonly IProjectMetadataReader _metadata;
    private readonly IContentClassifier _classifier;
    private readonly IHashCalculator _hasher;
    private readonly IBundleWriter _writer;

    /// <summary>
    /// Erstellt eine neue Instanz des Orchestrators mit den benötigten Diensten.
    /// </summary>
    public BundleOrchestrator(
        IFileScanner scanner,
        IProjectMetadataReader metadata,
        IContentClassifier classifier,
        IHashCalculator hasher,
        IBundleWriter writer)
    {
        _scanner = scanner;
        _metadata = metadata;
        _classifier = classifier;
        _hasher = hasher;
        _writer = writer;
    }

    /// <summary>
    /// Führt Scan, Hash-Berechnung, Klassifizierung und Bundle-Erstellung aus.
    /// </summary>
    /// <param name="rootPath">Wurzelverzeichnis des Projekts.</param>
    /// <param name="settings">Scan- und Ausgabeeinstellungen (enthält OutputFileName).</param>
    /// <returns>Pfad zur erzeugten Ausgabedatei.</returns>
    public string Run(string rootPath, ScanSettings settings)
    {
        var files = _scanner.Scan(rootPath, settings).ToList();

        // Hash + Language
        foreach (var f in files)
        {
            try
            {
                var bytes = File.ReadAllBytes(f.FullPath);
                f.Sha1 = _hasher.Sha1(bytes);
            }
            catch
            {
                f.Sha1 = "";
            }
            f.Language = _classifier.Classify(f.FullPath);
        }

        _metadata.EnrichBuildActions(files, rootPath);

        // Schreibe Bundle direkt mit rootPath - OutputFileName kommt aus settings
        return _writer.Write(rootPath, files, settings);
    }
}