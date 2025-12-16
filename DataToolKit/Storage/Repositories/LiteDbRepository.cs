using System;
using System.Collections.Generic;
using System.Linq;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using LiteDB;

namespace DataToolKit.Storage.Repositories
{
    /// <summary>
    /// Repository-Implementierung für LiteDB (v5) mit Delta-Synchronisierung.
    /// Unterstützt Volloperationen (<see cref="Load"/>, <see cref="Write"/>, <see cref="Clear"/>)
    /// und Einzeloperationen (<see cref="Update"/>, <see cref="Delete"/>).
    /// Storage-Optionen werden über <see cref="IStorageOptions{T}"/> aus DI injiziert.
    /// </summary>
    /// <typeparam name="T">
    /// Der Entitätstyp, der von <see cref="EntityBase"/> erben muss (für automatische ID-Verwaltung).
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// <b>Delta-Write:</b> Die <see cref="Write"/>-Methode verwendet <see cref="RepositoryDiffBuilder"/>
    /// zur Erkennung von Änderungen. Nur neue, geänderte oder zu löschende Entitäten werden verarbeitet.
    /// </para>
    /// <para>
    /// <b>Transaktionen:</b> Alle Schreiboperationen (Insert/Update/Delete) werden innerhalb einer
    /// LiteDB-Transaktion ausgeführt. Bei Fehlern erfolgt automatisch ein Rollback.
    /// </para>
    /// <para>
    /// <b>Collection-Name:</b> Die LiteDB-Collection wird automatisch nach dem Typ benannt:
    /// <c>typeof(T).Name</c>. Beispiel: <c>Customer</c> → Collection "Customer".
    /// </para>
    /// <para>
    /// <b>Automatische ID-Vergabe:</b> Neue Entitäten (mit <c>Id = 0</c>) erhalten automatisch
    /// eine fortlaufende ID von LiteDB beim ersten Insert.
    /// </para>
    /// <para>
    /// <b>EqualityComparer:</b> Der injizierte <see cref="IEqualityComparer{T}"/> wird für die
    /// Delta-Erkennung verwendet (Vergleich von bestehenden und neuen Entitäten).
    /// </para>
    /// </remarks>
    public sealed class LiteDbRepository<T> : AbstractRepository<T> where T : EntityBase
    {
        private readonly LiteDatabase _db;
        private ILiteCollection<T> _collection;
        private readonly IEqualityComparer<T> _comparer;

        /// <summary>
        /// Erstellt ein LiteDB-Repository mit injizierten Storage-Optionen und EqualityComparer.
        /// </summary>
        /// <param name="options">
        /// Die für <typeparamref name="T"/> registrierten Storage-Optionen (aus DI).
        /// Definiert den Datenbankpfad (<see cref="IStorageOptions{T}.FullPath"/>).
        /// </param>
        /// <param name="comparer">
        /// Comparer für Delta-Erkennung (aus DI). Typischerweise <c>FallbackEqualsComparer&lt;T&gt;</c>
        /// aus Common.Bootstrap.Defaults oder eine typspezifische Implementierung.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="options"/> oder <paramref name="comparer"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn <see cref="IStorageOptions{T}.FullPath"/> leer oder ungültig ist.
        /// </exception>
        public LiteDbRepository(IStorageOptions<T> options, IEqualityComparer<T> comparer)
            : base(options)
        {
            if (comparer is null) throw new ArgumentNullException(nameof(comparer));

            if (string.IsNullOrWhiteSpace(Options.FullPath))
                throw new ArgumentException("FullPath darf nicht leer sein.", nameof(options));

            _db = new LiteDatabase(Options.FullPath);

            // Collection-Name wird aus T bestimmt
            var collectionName = typeof(T).Name;

            _collection = _db.GetCollection<T>(collectionName);
            _comparer = comparer;
        }

        /// <summary>
        /// Lädt alle Elemente aus der LiteDB-Collection.
        /// </summary>
        /// <returns>
        /// Eine schreibgeschützte Liste aller Entitäten in der Collection.
        /// </returns>
        public override IReadOnlyList<T> Load()
        {
            var all = _collection.FindAll().ToList();
            return all.AsReadOnly();
        }

        /// <summary>
        /// Führt Delta-Synchronisierung durch: Vergleicht bestehende mit übergebenen Entitäten
        /// und führt nur notwendige Insert/Update/Delete-Operationen aus (innerhalb einer Transaktion).
        /// </summary>
        /// <param name="items">Die neue Ziel-Collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="items"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn die Collection <c>null</c>-Elemente enthält.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn die Transaktion nicht gestartet werden kann oder ein Fehler während
        /// Insert/Update/Delete auftritt (Rollback erfolgt automatisch).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Delta-Logik:</b>
        /// </para>
        /// <list type="bullet">
        /// <item><b>Update:</b> Entitäten, die in beiden Collections existieren, aber unterschiedlich sind (laut Comparer)</item>
        /// <item><b>Delete:</b> Entitäten, die nur in der Datenbank existieren</item>
        /// <item><b>Insert:</b> Entitäten, die nur in der neuen Collection existieren</item>
        /// </list>
        /// </remarks>
        public override void Write(IEnumerable<T> items)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));

            var incoming = items.ToList();

            if (incoming.Any(i => i is null))
                throw new ArgumentException("Die Sammlung enthält null-Elemente.", nameof(items));

            var existing = _collection.FindAll().ToList();

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, _comparer, missingAsInsert: true);

            if (!_db.BeginTrans())
                throw new InvalidOperationException("Transaktion konnte nicht gestartet werden.");

            try
            {
                // UPDATE
                if (diff.ToUpdate.Count > 0)
                    _collection.Update(diff.ToUpdate);

                // DELETE
                if (diff.ToDeleteIds.Count > 0)
                    _collection.DeleteMany(x => diff.ToDeleteIds.Contains(x.Id));

                // INSERT
                foreach (var n in diff.ToInsert)
                    _collection.Insert(n);

                _db.Commit();
            }
            catch
            {
                _db.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Leert die Collection vollständig (Drop & Recreate).
        /// </summary>
        public override void Clear()
        {
            var name = typeof(T).Name;
            _db.DropCollection(name);
            _collection = _db.GetCollection<T>(name);
        }

        /// <summary>
        /// Aktualisiert eine vorhandene Entität in der Datenbank.
        /// </summary>
        /// <param name="item">Die zu aktualisierende Entität (Identifikation via <c>Id</c>).</param>
        /// <returns>Immer <c>1</c> bei Erfolg.</returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="item"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn <c>item.Id &lt;= 0</c> (ungültige ID).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn die Entität nicht in der Datenbank gefunden wurde.
        /// </exception>
        public override int Update(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (item.Id <= 0) throw new ArgumentException("Ungültige Id (>0 erwartet).", nameof(item));

            var ok = _collection.Update(item);
            if (!ok)
                throw new InvalidOperationException($"Entität mit Id {item.Id} wurde nicht gefunden.");

            return 1;
        }

        /// <summary>
        /// Löscht eine vorhandene Entität aus der Datenbank.
        /// </summary>
        /// <param name="item">Die zu löschende Entität (Identifikation via <c>Id</c>).</param>
        /// <returns><c>1</c> bei Erfolg, <c>0</c> wenn <c>item.Id &lt;= 0</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="item"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn die Entität nicht in der Datenbank gefunden wurde (bei <c>Id &gt; 0</c>).
        /// </exception>
        public override int Delete(T item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            if (item.Id <= 0) return 0;

            var ok = _collection.Delete(item.Id);
            if (!ok)
                throw new InvalidOperationException($"Entität mit Id {item.Id} wurde nicht gefunden.");

            return 1;
        }

        /// <summary>
        /// Schließt die LiteDB-Datenbankverbindung und gibt Ressourcen frei.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c>, wenn von <see cref="IDisposable.Dispose"/> aufgerufen; <c>false</c> bei Finalisierung.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _db?.Dispose();
        }
    }
}
