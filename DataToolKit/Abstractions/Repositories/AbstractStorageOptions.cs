using DataToolKit.Abstractions.Repositories;
using System;
using System.IO;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Abstrakte Basisklasse für eine einheitliche, dateibasierte Speicherumgebung.
    /// Implementiert <see cref="IStorageOptions{T}"/> für typsichere Registrierung im DI-Container.
    /// 
    /// Festlegungen:
    /// - Standard: Alle Daten liegen unterhalb des Benutzerordners "Eigene Dokumente" (MyDocuments).
    /// - Optional: Für Tests kann ein eigener Root-Ordner angegeben werden.
    /// - Jede Anwendung nutzt einen eigenen Unterordner (<see cref="AppSubFolder"/>).
    /// - Optional kann ein weiterer, fachlicher Unterordner definiert werden (<see cref="SubFolder"/>).
    /// - Der eigentliche Dateiname wird als Basisname (ohne Erweiterung) geführt; die Erweiterung
    ///   legt die abgeleitete Klasse fest (z. B. ".json" oder ".db").
    /// 
    /// Ziel:
    /// - vorhersehbare, robuste Pfadstruktur
    /// - einfache Verwendung in Konsumentencode (z. B. direkt <see cref="FullPath"/> verwenden)
    /// - klare Trennung von Pfadregeln und konkreter Speichertechnologie
    /// - testbar durch optionalen Root-Ordner
    /// </summary>
    public abstract class AbstractStorageOptions<T> : IStorageOptions<T>
    {
        /// <inheritdoc />
        public string AppSubFolder { get; }

        /// <inheritdoc />
        public string? SubFolder { get; }

        /// <inheritdoc />
        public string FileNameBase { get; }

        /// <summary>
        /// Von der abgeleiteten Klasse festgelegte Dateiendung (inklusive Punkt).
        /// Beispiele: ".json", ".db".
        /// </summary>
        protected abstract string FileExtension { get; }

        /// <inheritdoc />
        public string RootFolder { get; }

        /// <inheritdoc />
        public string EffectiveRoot =>
            StoragePathHelpers.CombineRootWithOptionalSub(
                Path.Combine(RootFolder, AppSubFolder),
                SubFolder
            );

        /// <inheritdoc />
        public string FullPath =>
            StoragePathHelpers.BuildFullPath(EffectiveRoot, null, FileNameBase, FileExtension);

        /// <summary>
        /// Erstellt neue Speicher-Optionen mit konfigurierbarem Root, normiertem Dateinamen
        /// und unmittelbar angelegtem Zielverzeichnis.
        /// </summary>
        /// <param name="appSubFolder">Pflichtfeld: Anwendungs-Unterordner unterhalb von rootFolder.</param>
        /// <param name="fileNameBase">Pflichtfeld: Basisname der Datei (ohne Erweiterung).</param>
        /// <param name="subFolder">Optional: weiterer Unterordner innerhalb von <paramref name="appSubFolder"/>.</param>
        /// <param name="rootFolder">
        /// Optional: Root-Verzeichnis. 
        /// Wenn null, wird "Eigene Dokumente" (MyDocuments) verwendet.
        /// Für Tests kann hier z.B. Path.GetTempPath() oder eine Sandbox übergeben werden.
        /// </param>
        /// <exception cref="ArgumentException">Bei leerem <paramref name="appSubFolder"/> oder <paramref name="fileNameBase"/>.</exception>
        protected AbstractStorageOptions(
            string appSubFolder, 
            string fileNameBase, 
            string? subFolder = null,
            string? rootFolder = null)
        {
            if (string.IsNullOrWhiteSpace(appSubFolder))
                throw new ArgumentException("AppSubFolder darf nicht leer sein.", nameof(appSubFolder));

            AppSubFolder = appSubFolder.Trim();
            SubFolder = string.IsNullOrWhiteSpace(subFolder) ? null : subFolder.Trim();
            FileNameBase = StoragePathHelpers.NormalizeFileNameBaseOrThrow(fileNameBase);
            
            // Root: Entweder explizit angegeben oder MyDocuments
            RootFolder = string.IsNullOrWhiteSpace(rootFolder) 
                ? StoragePathHelpers.GetMyDocuments() 
                : rootFolder.Trim();

            // Einfaches Handling: Zielverzeichnis direkt anlegen, damit FullPath sofort nutzbar ist.
            StoragePathHelpers.EnsureDirectoryFor(Path.Combine(RootFolder, AppSubFolder), SubFolder);
        }

        /// <summary>
        /// Liefert eine kompakte, hilfreiche Diagnoseinformation für Logging/Debugging.
        /// </summary>
        public override string ToString()
            => $"{GetType().Name}: Path='{FullPath}'";
    }
}
