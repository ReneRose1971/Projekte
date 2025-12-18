namespace Scriptum.Application.Import;

/// <summary>
/// Service für den Import von Content-Daten aus externen Dateien.
/// </summary>
public interface IContentImportService
{
    /// <summary>
    /// Importiert Content-Daten aus den angegebenen Dateien.
    /// </summary>
    /// <param name="request">Die Import-Anfrage mit den Dateipfaden und Optionen.</param>
    /// <param name="cancellationToken">Token zur Abbruchsteuerung.</param>
    /// <returns>Das Ergebnis des Imports.</returns>
    Task<ContentImportResult> ImportAsync(
        ContentImportRequest request,
        CancellationToken cancellationToken = default);
}
