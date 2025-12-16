using DataToolKit.Abstractions.Repositories;
using System;

namespace DataToolKit.Abstractions.DataStores
{
    /// <summary>
    /// Schnittstelle für Persistierungs-Strategien.
    /// Kapselt die Unterschiede zwischen JSON (WriteAll) und LiteDB (granular).
    /// </summary>
    /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit POCOs und EntityBase).</typeparam>
    /// <remarks>
    /// <para>
    /// <b>JSON-Strategie:</b> Alle Methoden führen zu einem vollständigen <c>Write()</c> der Collection.
    /// Dies ist optimal für kleine Datenmengen (&lt; 100 Einträge), da JSON-Repositories atomar arbeiten.
    /// Funktioniert mit jedem POCO.
    /// </para>
    /// <para>
    /// <b>LiteDB-Strategie:</b> Nutzt granulare Operationen (<c>Update</c>, <c>Delete</c>) wo möglich,
    /// um die Datenbank-Performance zu optimieren. Benötigt <see cref="IEntity"/> (wird von EntityBase implementiert).
    /// </para>
    /// </remarks>
    public interface IPersistenceStrategy<T> : IDisposable where T : class
    {
        /// <summary>
        /// Wird aufgerufen, wenn ein Element zur Collection hinzugefügt wurde.
        /// </summary>
        /// <param name="entity">Das hinzugefügte Element.</param>
        void OnAdded(T entity);

        /// <summary>
        /// Wird aufgerufen, wenn ein Element aus der Collection entfernt wurde.
        /// </summary>
        /// <param name="entity">Das entfernte Element.</param>
        void OnRemoved(T entity);

        /// <summary>
        /// Wird aufgerufen, wenn eine Property einer Entität geändert wurde (via INotifyPropertyChanged).
        /// </summary>
        /// <param name="entity">Die geänderte Entität.</param>
        void OnEntityChanged(T entity);

        /// <summary>
        /// Wird aufgerufen, wenn die Collection komplett geleert wurde.
        /// </summary>
        void OnCleared();
    }
}
