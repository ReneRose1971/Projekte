using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Standard-Factory zur Erzeugung von DataStore-Instanzen.
    /// Erzeugt nur Instanzen - keine zusätzliche Logik.
    /// </summary>
    public sealed class DataStoreFactory : IDataStoreFactory
    {
        /// <summary>
        /// Erstellt einen InMemoryDataStore.
        /// </summary>
        public InMemoryDataStore<T> CreateInMemoryStore<T>(
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            return new InMemoryDataStore<T>(comparer);
        }

        /// <summary>
        /// Erstellt einen PersistentDataStore (ohne geladene Daten).
        /// </summary>
        /// <remarks>
        /// Funktioniert mit POCOs (JSON-Repository) und EntityBase (LiteDB-Repository).
        /// </remarks>
        public PersistentDataStore<T> CreatePersistentStore<T>(
            IRepositoryBase<T> repository,
            bool trackPropertyChanges = true)
            where T : class
        {
            return new PersistentDataStore<T>(repository, trackPropertyChanges);
        }
    }
}
