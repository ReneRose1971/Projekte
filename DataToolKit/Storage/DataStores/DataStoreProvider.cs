using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Thread-sicherer Provider zur Verwaltung von DataStore-Instanzen.
    /// Verwaltet Singleton-Instanzen in einem Dictionary pro Typ T.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Thread-Safety:</b> Alle Operationen sind durch <see cref="SemaphoreSlim"/> geschützt.
    /// Konkurrierender Zugriff auf Singleton-Instanzen ist sicher.
    /// </para>
    /// <para>
    /// <b>Singleton-Garantie:</b> Pro Typ T existiert anwendungsweit maximal eine DataStore-Instanz.
    /// Egal ob InMemory oder Persistent - der erste Aufruf legt die Art fest.
    /// </para>
    /// <para>
    /// <b>AutoLoad:</b> PersistentDataStores können beim Abrufen automatisch geladen werden.
    /// Dies geschieht im Provider, nicht in der Factory (Separation of Concerns).
    /// </para>
    /// <para>
    /// <b>Repository-Auswahl:</b> Automatische Erkennung basierend auf Typ:
    /// - <see cref="EntityBase"/>-Typen ? LiteDB-Repository (granulare Operationen)
    /// - POCOs ? JSON-Repository (atomares WriteAll)
    /// </para>
    /// <para>
    /// <b>Dispose:</b> Beim Entfernen von Singletons wird automatisch <c>Dispose()</c> aufgerufen,
    /// falls die Instanz <see cref="IDisposable"/> implementiert.
    /// </para>
    /// </remarks>
    public sealed class DataStoreProvider : IDataStoreProvider, IDisposable
    {
        private readonly IDataStoreFactory _factory;
        private readonly Dictionary<string, object> _singletons = new();
        private readonly SemaphoreSlim _lock = new(1, 1);
        private bool _disposed;

        /// <summary>
        /// Erstellt einen DataStoreProvider mit der angegebenen Factory.
        /// </summary>
        /// <param name="factory">Factory zur Erzeugung von DataStore-Instanzen.</param>
        /// <exception cref="ArgumentNullException">Wenn <paramref name="factory"/> null ist.</exception>
        public DataStoreProvider(IDataStoreFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Erzeugt einen eindeutigen Cache-Key für den Typ T.
        /// </summary>
        private static string GetKey<T>() => typeof(T).FullName ?? typeof(T).Name;

        /// <summary>
        /// Gibt einen bereits registrierten DataStore zurück.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp des DataStores.</typeparam>
        /// <returns>Der bereits existierende DataStore.</returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn kein DataStore für den Typ <typeparamref name="T"/> registriert wurde.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Diese Methode erstellt <b>keinen</b> neuen DataStore, sondern gibt nur einen bereits
        /// existierenden zurück. Sie ist nützlich für Komponenten, die einen DataStore verwenden möchten,
        /// ohne zu wissen wie er erstellt wurde.
        /// </para>
        /// <para>
        /// <b>Verwendung:</b>
        /// </para>
        /// <code>
        /// // Irgendwo in der Anwendung (z.B. Startup):
        /// var customerStore = provider.GetPersistent&lt;Customer&gt;(factory, autoLoad: true);
        /// 
        /// // Später in einer Komponente:
        /// var store = provider.GetDataStore&lt;Customer&gt;();  // Gibt den gleichen Store zurück
        /// </code>
        /// </remarks>
        public IDataStore<T> GetDataStore<T>() where T : class
        {
            _lock.Wait();
            try
            {
                var key = GetKey<T>();

                if (_singletons.TryGetValue(key, out var store))
                {
                    return (IDataStore<T>)store;
                }

                var typeName = typeof(T).Name;
                var typeFullName = typeof(T).FullName;
                
                throw new InvalidOperationException(
                    $"Kein DataStore für Typ '{typeName}' ({typeFullName}) wurde registriert.\n\n" +
                    $"Mögliche Lösungen:\n" +
                    $"1. Für Persistent-DataStore (mit Datenbank):\n" +
                    $"   provider.GetPersistent<{typeName}>(repositoryFactory, isSingleton: true, autoLoad: true);\n\n" +
                    $"2. Für InMemory-DataStore (nur im Speicher):\n" +
                    $"   provider.GetInMemory<{typeName}>(isSingleton: true);\n\n" +
                    $"Stellen Sie sicher, dass einer dieser Aufrufe vor GetDataStore<{typeName}>() ausgeführt wird,\n" +
                    $"z.B. in der Startup-Konfiguration oder im Konstruktor einer übergeordneten Komponente.");
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt einen bereits registrierten DataStore asynchron zurück.
        /// </summary>
        /// <typeparam name="T">Der Entitätstyp des DataStores.</typeparam>
        /// <returns>Der bereits existierende DataStore.</returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn kein DataStore für den Typ <typeparamref name="T"/> registriert wurde.
        /// </exception>
        /// <remarks>
        /// Asynchrone Variante von <see cref="GetDataStore{T}"/>. Siehe dort für Details.
        /// </remarks>
        public async Task<IDataStore<T>> GetDataStoreAsync<T>() where T : class
        {
            await _lock.WaitAsync();
            try
            {
                var key = GetKey<T>();

                if (_singletons.TryGetValue(key, out var store))
                {
                    return (IDataStore<T>)store;
                }

                var typeName = typeof(T).Name;
                var typeFullName = typeof(T).FullName;
                
                throw new InvalidOperationException(
                    $"Kein DataStore für Typ '{typeName}' ({typeFullName}) wurde registriert.\n\n" +
                    $"Mögliche Lösungen:\n" +
                    $"1. Für Persistent-DataStore (mit Datenbank):\n" +
                    $"   await provider.GetPersistentAsync<{typeName}>(repositoryFactory, isSingleton: true, autoLoad: true);\n\n" +
                    $"2. Für InMemory-DataStore (nur im Speicher):\n" +
                    $"   await provider.GetInMemoryAsync<{typeName}>(isSingleton: true);\n\n" +
                    $"Stellen Sie sicher, dass einer dieser Aufrufe vor GetDataStoreAsync<{typeName}>() ausgeführt wird,\n" +
                    $"z.B. in der Startup-Konfiguration oder im Konstruktor einer übergeordneten Komponente.");
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt einen InMemoryDataStore zurück (thread-safe).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Singleton-Verhalten:</b> Wenn <paramref name="isSingleton"/> = <c>true</c> und bereits
        /// ein Store für <typeparamref name="T"/> existiert, wird dieser zurückgegeben wenn er
        /// vom Typ <see cref="InMemoryDataStore{T}"/> ist (oder davon erbt, wie <see cref="PersistentDataStore{T}"/>).
        /// </para>
        /// <para>
        /// <b>Wichtig:</b> Wenn bereits ein <see cref="PersistentDataStore{T}"/> existiert, wird dieser
        /// zurückgegeben (da PersistentDataStore von InMemoryDataStore erbt). Verwenden Sie
        /// <see cref="GetDataStore{T}"/> wenn Sie typen-agnostisch arbeiten möchten.
        /// </para>
        /// </remarks>
        public InMemoryDataStore<T> GetInMemory<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            if (!isSingleton)
            {
                return _factory.CreateInMemoryStore<T>(comparer);
            }

            _lock.Wait();
            try
            {
                var key = GetKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    // PersistentDataStore erbt von InMemoryDataStore, also ist der Cast gültig
                    if (existing is InMemoryDataStore<T> inMemoryStore)
                    {
                        return inMemoryStore;
                    }
                    
                    // Sollte nicht passieren, aber zur Sicherheit
                    throw new InvalidOperationException(
                        $"Ein DataStore für Typ '{typeof(T).Name}' existiert bereits, aber als '{existing.GetType().Name}'.\n" +
                        $"Pro Typ kann nur eine Singleton-Instanz existieren.\n" +
                        $"Verwenden Sie GetDataStore<{typeof(T).Name}>() um den existierenden Store zu erhalten.");
                }

                var store = _factory.CreateInMemoryStore<T>(comparer);
                _singletons[key] = store;
                return store;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt einen InMemoryDataStore asynchron zurück (thread-safe).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Wichtig:</b> Wenn bereits ein <see cref="PersistentDataStore{T}"/> existiert, wird dieser
        /// zurückgegeben (da PersistentDataStore von InMemoryDataStore erbt). Verwenden Sie
        /// <see cref="GetDataStoreAsync{T}"/> wenn Sie typen-agnostisch arbeiten möchten.
        /// </para>
        /// </remarks>
        public async Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            if (!isSingleton)
            {
                return await Task.Run(() => _factory.CreateInMemoryStore<T>(comparer));
            }

            await _lock.WaitAsync();
            try
            {
                var key = GetKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    if (existing is InMemoryDataStore<T> inMemoryStore)
                    {
                        return inMemoryStore;
                    }
                    
                    throw new InvalidOperationException(
                        $"Ein DataStore für Typ '{typeof(T).Name}' existiert bereits, aber als '{existing.GetType().Name}'.\n" +
                        $"Pro Typ kann nur eine Singleton-Instanz existieren.\n" +
                        $"Verwenden Sie GetDataStoreAsync<{typeof(T).Name}>() um den existierenden Store zu erhalten.");
                }

                var store = await Task.Run(() => _factory.CreateInMemoryStore<T>(comparer));
                _singletons[key] = store;
                return store;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt einen PersistentDataStore zurück (thread-safe, mit optionalem AutoLoad).
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Repository-Auswahl:</b> Automatisch basierend auf Typ:
        /// </para>
        /// <list type="bullet">
        /// <item><see cref="EntityBase"/>-Typen ? LiteDB-Repository (granulare Operationen mit Update/Delete)</item>
        /// <item>POCOs (nur class) ? JSON-Repository (atomares WriteAll)</item>
        /// </list>
        /// <para>
        /// <b>AutoLoad:</b> Wenn <c>true</c>, werden Daten sofort aus dem Repository geladen.
        /// Bei <c>false</c> muss <c>store.Load()</c> manuell aufgerufen werden.
        /// </para>
        /// <para>
        /// <b>Singleton-Verhalten:</b> Wenn <paramref name="isSingleton"/> = <c>true</c> und bereits
        /// ein Store für <typeparamref name="T"/> existiert, wird eine Exception geworfen falls
        /// der existierende Store nicht vom Typ <see cref="PersistentDataStore{T}"/> ist.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Wenn bereits ein InMemoryDataStore für <typeparamref name="T"/> registriert ist.
        /// </exception>
        public PersistentDataStore<T> GetPersistent<T>(
            IRepositoryFactory repositoryFactory,
            bool isSingleton = true,
            bool trackPropertyChanges = true,
            bool autoLoad = true)
            where T : class
        {
            if (repositoryFactory is null)
                throw new ArgumentNullException(nameof(repositoryFactory));

            if (!isSingleton)
            {
                var repository = ResolveRepository<T>(repositoryFactory);
                var store = _factory.CreatePersistentStore<T>(repository, trackPropertyChanges);
                
                if (autoLoad)
                {
                    store.Load();
                }
                
                return store;
            }

            _lock.Wait();
            try
            {
                var key = GetKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    if (existing is PersistentDataStore<T> persistentStore)
                    {
                        return persistentStore;
                    }
                    
                    throw new InvalidOperationException(
                        $"Ein DataStore für Typ '{typeof(T).Name}' existiert bereits, aber als '{existing.GetType().Name}'.\n" +
                        $"Pro Typ kann nur eine Singleton-Instanz existieren (entweder InMemory ODER Persistent).\n" +
                        $"Verwenden Sie GetDataStore<{typeof(T).Name}>() um den existierenden Store zu erhalten.");
                }

                var repo = ResolveRepository<T>(repositoryFactory);
                var newStore = _factory.CreatePersistentStore<T>(repo, trackPropertyChanges);
                
                if (autoLoad)
                {
                    newStore.Load();
                }
                
                _singletons[key] = newStore;
                return newStore;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt einen PersistentDataStore asynchron zurück (thread-safe, mit optionalem AutoLoad).
        /// </summary>
        /// <remarks>
        /// Asynchrone Variante von <see cref="GetPersistent{T}"/>. Siehe dort für Details zur Repository-Auswahl.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Wenn bereits ein InMemoryDataStore für <typeparamref name="T"/> registriert ist.
        /// </exception>
        public async Task<PersistentDataStore<T>> GetPersistentAsync<T>(
            IRepositoryFactory repositoryFactory,
            bool isSingleton = true,
            bool trackPropertyChanges = true,
            bool autoLoad = true)
            where T : class
        {
            if (repositoryFactory is null)
                throw new ArgumentNullException(nameof(repositoryFactory));

            if (!isSingleton)
            {
                return await Task.Run(() =>
                {
                    var repository = ResolveRepository<T>(repositoryFactory);
                    var store = _factory.CreatePersistentStore<T>(repository, trackPropertyChanges);
                    
                    if (autoLoad)
                    {
                        store.Load();
                    }
                    
                    return store;
                });
            }

            await _lock.WaitAsync();
            try
            {
                var key = GetKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    if (existing is PersistentDataStore<T> persistentStore)
                    {
                        return persistentStore;
                    }
                    
                    throw new InvalidOperationException(
                        $"Ein DataStore für Typ '{typeof(T).Name}' existiert bereits, aber als '{existing.GetType().Name}'.\n" +
                        $"Pro Typ kann nur eine Singleton-Instanz existieren (entweder InMemory ODER Persistent).\n" +
                        $"Verwenden Sie GetDataStoreAsync<{typeof(T).Name}>() um den existierenden Store zu erhalten.");
                }

                var newStore = await Task.Run(() =>
                {
                    var repository = ResolveRepository<T>(repositoryFactory);
                    var store = _factory.CreatePersistentStore<T>(repository, trackPropertyChanges);
                    
                    if (autoLoad)
                    {
                        store.Load();
                    }
                    
                    return store;
                });
                
                _singletons[key] = newStore;
                return newStore;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Löst das passende Repository basierend auf dem Typ auf.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Auswahl-Logik:</b>
        /// </para>
        /// <list type="number">
        /// <item><see cref="EntityBase"/>-Typen ? LiteDB-Repository (hat Update/Delete für granulare Operationen)</item>
        /// <item>Alle anderen Typen (POCOs) ? JSON-Repository (atomares WriteAll, funktioniert mit jedem POCO)</item>
        /// </list>
        /// <para>
        /// <b>Wichtig:</b> LiteDB benötigt <see cref="IEntity"/> (für Id-Property), daher nur für EntityBase.
        /// JSON-Repository funktioniert mit jedem Typ ohne Einschränkungen.
        /// </para>
        /// </remarks>
        private IRepositoryBase<T> ResolveRepository<T>(IRepositoryFactory repositoryFactory)
            where T : class
        {
            if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
            {
                var method = typeof(IRepositoryFactory).GetMethod(nameof(IRepositoryFactory.GetLiteDbRepository))!;
                var genericMethod = method.MakeGenericMethod(typeof(T));
                return (IRepositoryBase<T>)genericMethod.Invoke(repositoryFactory, null)!;
            }

            return repositoryFactory.GetJsonRepository<T>();
        }

        /// <summary>
        /// Entfernt eine Singleton-Instanz aus dem Cache (mit Dispose).
        /// </summary>
        public bool RemoveSingleton<T>() where T : class
        {
            _lock.Wait();
            try
            {
                var key = GetKey<T>();
                
                if (_singletons.TryGetValue(key, out var store))
                {
                    (store as IDisposable)?.Dispose();
                    return _singletons.Remove(key);
                }
                
                return false;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Entfernt alle Singleton-Instanzen (mit Dispose).
        /// </summary>
        public void ClearAll()
        {
            _lock.Wait();
            try
            {
                foreach (var store in _singletons.Values)
                {
                    (store as IDisposable)?.Dispose();
                }
                
                _singletons.Clear();
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gibt alle Ressourcen frei und disposed alle Singleton-Instanzen.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ClearAll();
            _lock.Dispose();
        }
    }
}
