namespace DataToolKit.Abstractions.DataStores;

using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using System.Collections.Generic;

/// <summary>
/// Factory zur Erzeugung von DataStore-Instanzen.
/// Reine Fabrik-Verantwortung - keine Logik für AutoLoad oder Singleton-Management.
/// </summary>
public interface IDataStoreFactory
{
    /// <summary>
    /// Erstellt einen InMemoryDataStore für den Typ <typeparamref name="T"/>.
    /// </summary>
    /// <param name="comparer">Optionaler EqualityComparer für Duplikats-Erkennung.</param>
    /// <returns>Neue InMemoryDataStore-Instanz.</returns>
    InMemoryDataStore<T> CreateInMemoryStore<T>(
        IEqualityComparer<T>? comparer = null)
        where T : class;

    /// <summary>
    /// Erstellt einen PersistentDataStore für den Typ <typeparamref name="T"/>.
    /// </summary>
    /// <param name="repository">Repository für Persistierung.</param>
    /// <param name="trackPropertyChanges">PropertyChanged-Tracking aktivieren.</param>
    /// <returns>Neue PersistentDataStore-Instanz (ohne geladene Daten).</returns>
    /// <remarks>
    /// <para>
    /// <b>Repository-Typen:</b>
    /// </para>
    /// <list type="bullet">
    /// <item>JSON-Repository (IRepositoryBase) ? Funktioniert mit jedem POCO</item>
    /// <item>LiteDB-Repository (IRepository) ? Benötigt IEntity (EntityBase)</item>
    /// </list>
    /// </remarks>
    PersistentDataStore<T> CreatePersistentStore<T>(
        IRepositoryBase<T> repository,
        bool trackPropertyChanges = true)
        where T : class;
}
