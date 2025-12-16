using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// Konkrete Speicheroptionen für LiteDB-Datenbanken.
    /// Festlegungen:
    /// - Dateiendung: ".db"
    /// - Root: Standard "Eigene Dokumente" oder konfigurierbar für Tests
    /// - Kein Shared-Modus: Connection-Strings enthalten ausschließlich "Filename=<Pfad>"
    /// Wird als Singleton IStorageOptions&lt;T&gt; im DI-Container registriert.
    /// </summary>
    public sealed class LiteDbStorageOptions<T> : AbstractStorageOptions<T>
    {
        /// <summary>
        /// Dateiendung für LiteDB-Dateien.
        /// </summary>
        protected override string FileExtension => ".db";

        /// <summary>
        /// Erstellt LiteDB-Storage-Optionen für den Typ <typeparamref name="T"/>.
        /// </summary>
        /// <param name="appSubFolder">Pflichtfeld: Anwendungs-Unterordner unterhalb von rootFolder.</param>
        /// <param name="fileNameBase">Pflichtfeld: Basisname der Datei (ohne Erweiterung).</param>
        /// <param name="subFolder">Optional: weiterer Unterordner innerhalb von <paramref name="appSubFolder"/>.</param>
        /// <param name="rootFolder">
        /// Optional: Root-Verzeichnis. 
        /// Wenn null, wird "Eigene Dokumente" (MyDocuments) verwendet.
        /// Für Tests kann hier z.B. eine Sandbox übergeben werden.
        /// </param>
        public LiteDbStorageOptions(
            string appSubFolder, 
            string fileNameBase, 
            string? subFolder = null,
            string? rootFolder = null)
            : base(appSubFolder, fileNameBase, subFolder, rootFolder)
        {
        }

        /// <summary>
        /// Liefert einen schlanken Connection-String ohne "shared"-Zusatz.
        /// Beispiel:
        ///   Filename=C:\Users\Name\Documents\MyApp\data.db
        /// </summary>
        public string GetConnectionString()
            => $"Filename={FullPath}";
    }
}
