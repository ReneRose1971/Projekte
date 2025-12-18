using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using TestHelper.DataToolKit.Fakes.Repositories;

namespace TestHelper.DataToolKit.Fakes.Providers;

/// <summary>
/// Fake DataStoreProvider für Test-Szenarien ohne echte I/O.
/// Simuliert das Verhalten von DataStoreProvider mit In-Memory-Repositories.
/// </summary>
public class FakeDataStoreProvider : IDataStoreProvider
{
    private readonly Dictionary<(Type type, string storeType), object> _singletons = new();
    private readonly FakeRepositoryFactory _repositoryFactory;

    /// <summary>
    /// Erstellt einen FakeDataStoreProvider mit optionaler Factory.
    /// </summary>
    public FakeDataStoreProvider(FakeRepositoryFactory? repositoryFactory = null)
    {
        _repositoryFactory = repositoryFactory ?? new FakeRepositoryFactory();
    }

    /// <summary>
    /// Zugriff auf die Repository-Factory für Test-Assertions.
    /// </summary>
    public FakeRepositoryFactory RepositoryFactory => _repositoryFactory;

    /// <summary>
    /// Gibt einen bereits registrierten DataStore zurück.
    /// </summary>
    public IDataStore<T> GetDataStore<T>() where T : class
    {
        if (_singletons.TryGetValue((typeof(T), "Persistent"), out var persistentStore))
            return (IDataStore<T>)persistentStore;
        
        if (_singletons.TryGetValue((typeof(T), "InMemory"), out var inMemoryStore))
            return (IDataStore<T>)inMemoryStore;

        throw new InvalidOperationException(
            $"No DataStore for type {typeof(T).Name} registered. " +
            $"Use GetInMemory<T>() or GetPersistent<T>() first.");
    }

    /// <summary>
    /// Gibt einen bereits registrierten DataStore asynchron zurück.
    /// </summary>
    public Task<IDataStore<T>> GetDataStoreAsync<T>() where T : class
        => Task.FromResult(GetDataStore<T>());

    /// <summary>
    /// Gibt einen InMemoryDataStore zurück (Singleton oder neue Instanz).
    /// </summary>
    public InMemoryDataStore<T> GetInMemory<T>(
        bool isSingleton = true,
        IEqualityComparer<T>? comparer = null) where T : class
    {
        var key = (typeof(T), "InMemory");
        
        if (isSingleton && _singletons.TryGetValue(key, out var existing))
            return (InMemoryDataStore<T>)existing;

        var store = new InMemoryDataStore<T>(comparer);

        if (isSingleton)
            _singletons[key] = store;

        return store;
    }

    /// <summary>
    /// Gibt einen InMemoryDataStore asynchron zurück.
    /// </summary>
    public Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(
        bool isSingleton = true,
        IEqualityComparer<T>? comparer = null) where T : class
        => Task.FromResult(GetInMemory<T>(isSingleton, comparer));

    /// <summary>
    /// Gibt einen PersistentDataStore zurück (mit Fake-Repository).
    /// </summary>
    public PersistentDataStore<T> GetPersistent<T>(
        IRepositoryFactory repositoryFactory,
        bool isSingleton = true,
        bool trackPropertyChanges = true,
        bool autoLoad = true) where T : class
    {
        var key = (typeof(T), "Persistent");
        
        if (isSingleton && _singletons.TryGetValue(key, out var existing))
            return (PersistentDataStore<T>)existing;

        IRepositoryBase<T> repository;
        if (typeof(IEntity).IsAssignableFrom(typeof(T)))
        {
            var method = repositoryFactory.GetType().GetMethod("GetLiteDbRepository");
            var genericMethod = method!.MakeGenericMethod(typeof(T));
            repository = (IRepositoryBase<T>)genericMethod.Invoke(repositoryFactory, null)!;
        }
        else
        {
            repository = repositoryFactory.GetJsonRepository<T>();
        }

        var store = new PersistentDataStore<T>(repository, trackPropertyChanges);

        if (autoLoad)
            store.Load();

        if (isSingleton)
            _singletons[key] = store;

        return store;
    }

    /// <summary>
    /// Gibt einen PersistentDataStore asynchron zurück.
    /// </summary>
    public async Task<PersistentDataStore<T>> GetPersistentAsync<T>(
        IRepositoryFactory repositoryFactory,
        bool isSingleton = true,
        bool trackPropertyChanges = true,
        bool autoLoad = true) where T : class
    {
        var store = GetPersistent<T>(
            repositoryFactory,
            isSingleton,
            trackPropertyChanges,
            autoLoad: false);

        if (autoLoad)
            await Task.Run(() => store.Load());

        return store;
    }

    /// <summary>
    /// Entfernt eine Singleton-Instanz aus dem Cache.
    /// </summary>
    public bool RemoveSingleton<T>() where T : class
    {
        bool removed = false;
        
        if (_singletons.Remove((typeof(T), "InMemory"), out var inMemoryStore))
        {
            (inMemoryStore as IDisposable)?.Dispose();
            removed = true;
        }
        
        if (_singletons.Remove((typeof(T), "Persistent"), out var persistentStore))
        {
            (persistentStore as IDisposable)?.Dispose();
            removed = true;
        }
        
        return removed;
    }

    /// <summary>
    /// Entfernt alle Singleton-Instanzen und setzt die Factory zurück.
    /// </summary>
    public void ClearAll()
    {
        foreach (var store in _singletons.Values.OfType<IDisposable>())
            store.Dispose();

        _singletons.Clear();
        _repositoryFactory.ClearAll();
    }
}
