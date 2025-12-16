using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;

namespace DataToolKit.Abstractions.DataStores
{
    /// <summary>
    /// Provider zur Verwaltung von DataStore-Instanzen mit Singleton-Pattern.
    /// Verwaltet einen Cache von DataStore-Instanzen pro Typ.
    /// </summary>
    public interface IDataStoreProvider
    {
        /// <summary>
        /// Gibt einen bereits registrierten DataStore zurück, unabhängig davon ob InMemory oder Persistent.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp des DataStores.</typeparam>
        /// <returns>Der bereits existierende DataStore.</returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn kein DataStore für den Typ <typeparamref name="T"/> registriert wurde.
        /// </exception>
        /// <remarks>
        /// Diese Methode erstellt <b>keinen</b> neuen DataStore, sondern gibt nur einen bereits
        /// existierenden zurück. Nützlich für Komponenten, die einen DataStore verwenden möchten,
        /// ohne zu wissen ob er als InMemory oder Persistent erstellt wurde.
        /// </remarks>
        IDataStore<T> GetDataStore<T>() where T : class;

        /// <summary>
        /// Gibt einen bereits registrierten DataStore asynchron zurück, unabhängig davon ob InMemory oder Persistent.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp des DataStores.</typeparam>
        /// <returns>Der bereits existierende DataStore.</returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn kein DataStore für den Typ <typeparamref name="T"/> registriert wurde.
        /// </exception>
        Task<IDataStore<T>> GetDataStoreAsync<T>() where T : class;

        /// <summary>
        /// Gibt einen InMemoryDataStore zurück (Singleton oder neue Instanz).
        /// </summary>
        /// <typeparam name="T">Entitätstyp.</typeparam>
        /// <param name="isSingleton">
        /// Wenn <c>true</c>, wird eine Singleton-Instanz zurückgegeben (eine pro Typ).
        /// Wenn <c>false</c>, wird eine neue Instanz erstellt.
        /// </param>
        /// <param name="comparer">Optionaler EqualityComparer.</param>
        /// <returns>InMemoryDataStore-Instanz.</returns>
        InMemoryDataStore<T> GetInMemory<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class;

        /// <summary>
        /// Gibt einen InMemoryDataStore asynchron zurück (thread-safe).
        /// </summary>
        /// <typeparam name="T">Entitätstyp.</typeparam>
        /// <param name="isSingleton">Singleton oder neue Instanz.</param>
        /// <param name="comparer">Optionaler EqualityComparer.</param>
        /// <returns>InMemoryDataStore-Instanz.</returns>
        Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class;

        /// <summary>
        /// Gibt einen PersistentDataStore zurück (Singleton oder neue Instanz).
        /// </summary>
        /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit POCOs und EntityBase).</typeparam>
        /// <param name="repositoryFactory">Factory zum Auflösen des Repositories.</param>
        /// <param name="isSingleton">Singleton oder neue Instanz.</param>
        /// <param name="trackPropertyChanges">PropertyChanged-Tracking aktivieren.</param>
        /// <param name="autoLoad">
        /// Wenn <c>true</c>, werden Daten automatisch aus dem Repository geladen.
        /// </param>
        /// <returns>PersistentDataStore-Instanz.</returns>
        /// <remarks>
        /// <para>
        /// <b>Repository-Auswahl:</b> Automatisch basierend auf Typ:
        /// </para>
        /// <list type="bullet">
        /// <item><see cref="EntityBase"/>-Typen ? LiteDB-Repository (granulare Operationen)</item>
        /// <item>POCOs (nur class) ? JSON-Repository (atomares WriteAll)</item>
        /// </list>
        /// </remarks>
        PersistentDataStore<T> GetPersistent<T>(
            IRepositoryFactory repositoryFactory,
            bool isSingleton = true,
            bool trackPropertyChanges = true,
            bool autoLoad = true)
            where T : class;

        /// <summary>
        /// Gibt einen PersistentDataStore asynchron zurück (mit optionalem AutoLoad).
        /// </summary>
        /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit POCOs und EntityBase).</typeparam>
        /// <param name="repositoryFactory">Factory zum Auflösen des Repositories.</param>
        /// <param name="isSingleton">Singleton oder neue Instanz.</param>
        /// <param name="trackPropertyChanges">PropertyChanged-Tracking aktivieren.</param>
        /// <param name="autoLoad">
        /// Wenn <c>true</c>, werden Daten automatisch asynchron geladen.
        /// </param>
        /// <returns>PersistentDataStore-Instanz.</returns>
        /// <remarks>
        /// Asynchrone Variante von <see cref="GetPersistent{T}"/>. Siehe dort für Details zur Repository-Auswahl.
        /// </remarks>
        Task<PersistentDataStore<T>> GetPersistentAsync<T>(
            IRepositoryFactory repositoryFactory,
            bool isSingleton = true,
            bool trackPropertyChanges = true,
            bool autoLoad = true)
            where T : class;

        /// <summary>
        /// Entfernt eine Singleton-Instanz aus dem Cache.
        /// Ruft Dispose() auf, falls die Instanz IDisposable implementiert.
        /// </summary>
        /// <typeparam name="T">Entitätstyp.</typeparam>
        /// <returns><c>true</c>, wenn die Instanz entfernt wurde; andernfalls <c>false</c>.</returns>
        bool RemoveSingleton<T>() where T : class;

        /// <summary>
        /// Entfernt alle Singleton-Instanzen aus dem Cache.
        /// Ruft Dispose() auf allen IDisposable-Instanzen auf.
        /// </summary>
        void ClearAll();
    }
}
