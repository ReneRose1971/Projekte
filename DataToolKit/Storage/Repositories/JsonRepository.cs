using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// JSON-basiertes Repository mit atomarer Persistierung.
    /// Jede Änderung an der Collection wird vollständig und atomar in eine JSON-Datei geschrieben.
    /// Storage-Optionen werden über <see cref="IStorageOptions{T}"/> aus DI injiziert.
    /// </summary>
    /// <typeparam name="T">Der Entitätstyp, den dieses Repository verwaltet.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Atomarer Write-Mechanismus:</b>
    /// </para>
    /// <list type="number">
    /// <item>Schreibe Daten in temporäre Datei (<c>.tmp</c>)</item>
    /// <item>Flush alle Puffer zur Festplatte</item>
    /// <item>Ersetze Zieldatei atomar via <see cref="File.Replace"/> (mit Backup als <c>.bak</c>)</item>
    /// </list>
    /// <para>
    /// <b>Thread-Safety:</b> Alle Operationen sind durch einen internen Lock geschützt.
    /// Konkurrierender Zugriff ist sicher.
    /// </para>
    /// <para>
    /// <b>Backup:</b> Bei jedem Schreibvorgang wird die vorherige Datei als <c>.bak</c> gesichert.
    /// </para>
    /// </remarks>
    public sealed class JsonRepository<T> : AbstractRepositoryBase<T>
    {
        private readonly object _gate = new();
        private readonly JsonSerializerOptions _json;

        /// <summary>
        /// Erstellt ein JSON-Repository mit injizierten Storage-Optionen.
        /// </summary>
        /// <param name="options">
        /// Die für <typeparamref name="T"/> registrierten Storage-Optionen (aus DI).
        /// Definiert den Speicherpfad (<see cref="IStorageOptions{T}.FullPath"/>).
        /// </param>
        /// <param name="jsonOptions">
        /// Optionale JSON-Serialisierungs-Optionen. Standard: <c>WriteIndented = true</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="options"/> <c>null</c> ist.
        /// </exception>
        public JsonRepository(IStorageOptions<T> options, JsonSerializerOptions? jsonOptions = null)
            : base(options)
        {
            _json = jsonOptions ?? new JsonSerializerOptions { WriteIndented = true };
        }

        /// <summary>
        /// Lädt die vollständige Collection aus der JSON-Datei.
        /// </summary>
        /// <returns>
        /// Eine schreibgeschützte Liste aller Entitäten. Wenn die Datei nicht existiert, wird eine leere Liste zurückgegeben.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn der Dateipfad ungültig ist (<see cref="AbstractRepositoryBase{T}.FilePath"/> ist <c>null</c>).
        /// </exception>
        /// <exception cref="JsonException">
        /// Wenn die JSON-Datei beschädigt oder ungültig ist.
        /// </exception>
        public override IReadOnlyList<T> Load()
        {
            lock (_gate)
            {
                var path = FilePath ?? throw new InvalidOperationException("Ungültiger Dateipfad.");
                if (!File.Exists(path)) return Array.Empty<T>();

                using var s = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var list = JsonSerializer.Deserialize<List<T>>(s, _json);
                return (list ?? new List<T>()).AsReadOnly();
            }
        }

        /// <summary>
        /// Schreibt die vollständige Collection atomar in die JSON-Datei.
        /// Ersetzt alle bestehenden Daten und erstellt ein Backup der vorherigen Datei.
        /// </summary>
        /// <param name="items">Die zu persistierende Collection von Entitäten.</param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="items"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn die Collection <c>null</c>-Elemente enthält.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn der Dateipfad ungültig ist oder das Zielverzeichnis nicht ermittelt werden kann.
        /// </exception>
        /// <exception cref="IOException">
        /// Wenn ein Dateisystem-Fehler beim Schreiben auftritt.
        /// </exception>
        public override void Write(IEnumerable<T> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var list = items as IList<T> ?? items.ToList();
            if (list.Any(e => e is null))
                throw new ArgumentException("Die Auflistung enthält null-Elemente.", nameof(items));

            lock (_gate)
            {
                var path = FilePath ?? throw new InvalidOperationException("Ungültiger Dateipfad.");
                var dir = Path.GetDirectoryName(path);
                if (string.IsNullOrWhiteSpace(dir))
                    throw new InvalidOperationException($"Ungültiger Pfad: '{path}'");

                Directory.CreateDirectory(dir!);

                var tmp = path + ".tmp";
                var bak = path + ".bak";
                var buffer = list;

                using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(fs, buffer, _json);
                    fs.Flush(true);
                }

                if (File.Exists(path))
                    File.Replace(tmp, path, bak, ignoreMetadataErrors: true);
                else
                    File.Move(tmp, path);
            }
        }

        /// <summary>
        /// Leert das Repository vollständig (schreibt eine leere Liste).
        /// </summary>
        public override void Clear()
        {
            // Leeren == atomarer Write leere Liste
            Write(Array.Empty<T>());
        }
    }
}
