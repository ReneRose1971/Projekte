namespace SolutionBundler.Core.Abstractions;

/// <summary>
/// Maskiert sensible Informationen in Dateiinhalten vor der Ausgabe.
/// </summary>
public interface ISecretMasker
{
    /// <summary>
    /// Verarbeitet den Dateiinhalt und maskiert erkannte Geheimnisse (z. B. API Keys).
    /// </summary>
    /// <param name="relativePath">Relative Pfadangabe der Datei innerhalb der Solution.</param>
    /// <param name="content">Der Textinhalt der Datei.</param>
    /// <returns>Der bearbeitete Inhalt mit maskierten Geheimnissen.</returns>
    string Process(string relativePath, string content);
}