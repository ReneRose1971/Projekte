using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// Konkrete Speicheroptionen für JSON-Dateien.
    /// Legt ausschließlich die Dateiendung ".json" fest;
    /// sämtliche Pfadregeln stammen aus <see cref="AbstractStorageOptions{T}"/>.
    /// Wird als Singleton IStorageOptions&lt;T&gt; im DI-Container registriert.
    /// </summary>
    public sealed class JsonStorageOptions<T> : AbstractStorageOptions<T>
    {
        /// <summary>
        /// Dateiendung für JSON-Dateien.
        /// </summary>
        protected override string FileExtension => ".json";

        /// <summary>
        /// Erstellt JSON-Storage-Optionen für den Typ <typeparamref name="T"/>.
        /// </summary>
        /// <param name="appSubFolder">Pflichtfeld: Anwendungs-Unterordner unterhalb von rootFolder.</param>
        /// <param name="fileNameBase">Pflichtfeld: Basisname der Datei (ohne Erweiterung).</param>
        /// <param name="subFolder">Optional: weiterer Unterordner innerhalb von <paramref name="appSubFolder"/>.</param>
        /// <param name="rootFolder">
        /// Optional: Root-Verzeichnis. 
        /// Wenn null, wird "Eigene Dokumente" (MyDocuments) verwendet.
        /// Für Tests kann hier z.B. eine Sandbox übergeben werden.
        /// </param>
        public JsonStorageOptions(
            string appSubFolder, 
            string fileNameBase, 
            string? subFolder = null,
            string? rootFolder = null)
            : base(appSubFolder, fileNameBase, subFolder, rootFolder)
        {
        }
    }
}
