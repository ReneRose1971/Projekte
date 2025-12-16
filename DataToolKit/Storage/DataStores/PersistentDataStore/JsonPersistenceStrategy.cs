using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Persistierungs-Strategie für JSON-Repositories.
    /// Schreibt bei jeder Änderung die gesamte Collection atomar ins Repository.
    /// </summary>
    /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit jedem POCO).</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Funktionsweise:</b> JSON-Repositories arbeiten atomar - bei jedem Write wird die
    /// gesamte Datei neu geschrieben. Dies ist für kleine Datenmengen (&lt; 100 Einträge) optimal,
    /// da der Overhead minimal ist und atomare Konsistenz garantiert wird.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Alle Operationen (Add/Remove/PropertyChanged) führen zu einem
    /// vollständigen <c>Write()</c>. Bei &lt; 100 Einträgen ist dies unkritisch.
    /// </para>
    /// <para>
    /// <b>POCO-Unterstützung:</b> Diese Strategie funktioniert mit jedem POCO ohne IEntity-Requirement.
    /// Perfekt für Settings, Konfigurationen und einfache Datenklassen.
    /// </para>
    /// </remarks>
    internal sealed class JsonPersistenceStrategy<T> : IPersistenceStrategy<T> 
        where T : class
    {
        private readonly IRepositoryBase<T> _repository;
        private readonly Func<IReadOnlyList<T>> _itemsAccessor;

        /// <summary>
        /// Erstellt eine JSON-Strategie.
        /// </summary>
        /// <param name="repository">Das JSON-Repository (IRepositoryBase).</param>
        /// <param name="itemsAccessor">Funktion zum Zugriff auf die aktuelle Collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="repository"/> oder <paramref name="itemsAccessor"/> null ist.
        /// </exception>
        public JsonPersistenceStrategy(
            IRepositoryBase<T> repository,
            Func<IReadOnlyList<T>> itemsAccessor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _itemsAccessor = itemsAccessor ?? throw new ArgumentNullException(nameof(itemsAccessor));
        }

        /// <summary>
        /// Bei Add: Gesamte Collection schreiben.
        /// </summary>
        public void OnAdded(T entity) => WriteAll();

        /// <summary>
        /// Bei Remove: Gesamte Collection schreiben.
        /// </summary>
        public void OnRemoved(T entity) => WriteAll();

        /// <summary>
        /// Bei PropertyChanged: Gesamte Collection schreiben.
        /// </summary>
        public void OnEntityChanged(T entity) => WriteAll();

        /// <summary>
        /// Bei Clear: Repository leeren (ruft Clear() auf dem Repository auf).
        /// </summary>
        public void OnCleared() => _repository.Clear();

        /// <summary>
        /// Schreibt die aktuelle Collection vollständig ins JSON-Repository.
        /// </summary>
        private void WriteAll()
        {
            _repository.Write(_itemsAccessor());
        }

        /// <summary>
        /// Gibt Ressourcen frei (keine Ressourcen in dieser Strategie).
        /// </summary>
        public void Dispose()
        {
            // Keine Ressourcen zu bereinigen
        }
    }
}
