using System;
using System.IO;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Hilfsklasse zur konsistenten Erzeugung und Normalisierung von Datei- und Verzeichnispfaden.
    /// Zweck:
    /// - zentrale, wiederverwendbare Pfadlogik ohne Geschäftslogik
    /// - klar testbare, reine Funktionen (bis auf das gezielte Anlegen von Verzeichnissen)
    /// 
    /// Hinweise:
    /// - Diese Klasse ist bewusst im gleichen Namespace wie die Optionsklassen gehalten,
    ///   um weitere Verschachtelungen zu vermeiden.
    /// </summary>
    internal static class StoragePathHelpers
    {
        /// <summary>
        /// Normalisiert den Basis-Dateinamen:
        /// - wirft bei null/leer/Whitespace
        /// - trimmt führende/trailing Leerzeichen
        /// - entfernt eine versehentlich mitgegebene Dateierweiterung (z. B. ".json")
        /// 
        /// Ergebnis ist immer ein nackter Dateiname ohne Erweiterung.
        /// </summary>
        /// <param name="fileNameBase">Vom Aufrufer gewünschter Basisname ohne Erweiterung.</param>
        /// <returns>Bereinigter Basisname (ohne Erweiterung).</returns>
        /// <exception cref="ArgumentException">Wenn der Name leer oder nur Whitespace ist.</exception>
        public static string NormalizeFileNameBaseOrThrow(string fileNameBase)
        {
            if (string.IsNullOrWhiteSpace(fileNameBase))
                throw new ArgumentException("Der Dateibasisname darf nicht leer sein.", nameof(fileNameBase));

            var trimmed = fileNameBase.Trim();

            // Entfernt eine ungewollte Erweiterung, um eine konsistente interne Darstellung zu garantieren.
            var ext = Path.GetExtension(trimmed);
            if (!string.IsNullOrEmpty(ext))
                trimmed = Path.GetFileNameWithoutExtension(trimmed);

            return trimmed;
        }

        /// <summary>
        /// Kombiniert einen Root-Pfad mit einem optionalen Unterordner.
        /// Ist der Unterordner leer oder null, wird nur der Root-Pfad verwendet.
        /// </summary>
        public static string CombineRootWithOptionalSub(string rootFolder, string? subFolder)
        {
            return string.IsNullOrWhiteSpace(subFolder)
                ? rootFolder
                : Path.Combine(rootFolder, subFolder.Trim());
        }

        /// <summary>
        /// Baut einen vollständigen Dateipfad aus Root, optionalem Unterordner, Basisname und Dateiendung.
        /// </summary>
        public static string BuildFullPath(string rootFolder, string? subFolder, string fileNameBase, string fileExtension)
        {
            var effectiveRoot = CombineRootWithOptionalSub(rootFolder, subFolder);
            return Path.Combine(effectiveRoot, fileNameBase + fileExtension);
        }

        /// <summary>
        /// Stellt sicher, dass das effektive Verzeichnis existiert.
        /// Ist es bereits vorhanden, geschieht nichts; andernfalls wird es angelegt.
        /// </summary>
        public static void EnsureDirectoryFor(string rootFolder, string? subFolder)
        {
            var effectiveRoot = CombineRootWithOptionalSub(rootFolder, subFolder);
            Directory.CreateDirectory(effectiveRoot);
        }

        /// <summary>
        /// Liefert den absoluten Pfad zum Benutzerordner "Eigene Dokumente" (MyDocuments).
        /// Dieser Ordner ist die feste Wurzel für alle dateibasierten Speicherpfade in dieser Bibliothek.
        /// </summary>
        public static string GetMyDocuments()
            => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
