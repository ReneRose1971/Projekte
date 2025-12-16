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
    /// Verwaltet Singleton-Instanzen in einem Dictionary pro Typ UND Store-Art.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Thread-Safety:</b> Alle Operationen sind durch <see cref="SemaphoreSlim"/> geschützt.
    /// Konkurrierender Zugriff auf Singleton-Instanzen ist sicher.
    /// </para>
    /// <para>
    /// <b>AutoLoad:</b> PersistentDataStores können beim Abrufen automatisch geladen werden.
    /// Dies geschieht im Provider, nicht in der Factory (Separation of Concerns).
    /// </para>
    /// <para>
    /// <b>Repository-Auswahl:</b> Automatische Erkennung basierend auf Typ:
    /// - <see cref="EntityBase"/>-Typen ? LiteDB-Repository (granulare Operationen)
    /// - Nur <see cref="IEntity"/>-Typen ? JSON-Repository (atomares WriteAll)
    /// </para>
    /// <para>
    /// <b>Singleton-Keys:</b> Verwendet separate Keys für InMemory vs. Persistent Stores,
    /// um Kollisionen zu vermeiden.
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
        /// Erzeugt einen eindeutigen Cache-Key für den Store-Typ.
        /// </summary>
        private static string GetInMemoryKey<T>() => $"InMemory_{typeof(T).FullName}";
        
        /// <summary>
        /// Erzeugt einen eindeutigen Cache-Key für den Store-Typ.
        /// </summary>
        private static string GetPersistentKey<T>() => $"Persistent_{typeof(T).FullName}";

        /// <summary>
        /// Gibt einen bereits registrierten DataStore zurück, unabhängig davon ob InMemory oder Persistent.
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
        /// ohne zu wissen ob er als InMemory oder Persistent erstellt wurde.
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
        /// <para>
        /// <b>Fehlerbehandlung:</b> Wenn kein DataStore gefunden wird, enthält die Exception eine
        /// hilfreiche Fehlermeldung mit Anweisungen, wie der Store vorher erstellt werden muss.
        /// </para>
        /// </remarks>
        public IDataStore<T> GetDataStore<T>() where T : class
        {
            _lock.Wait();
            try
            {
                var inMemoryKey = GetInMemoryKey<T>();
                var persistentKey = GetPersistentKey<T>();

                // Prüfe zuerst Persistent, dann InMemory (Persistent ist häufiger)
                if (_singletons.TryGetValue(persistentKey, out var persistentStore))
                {
                    return (IDataStore<T>)persistentStore;
                }

                if (_singletons.TryGetValue(inMemoryKey, out var inMemoryStore))
                {
                    return (IDataStore<T>)inMemoryStore;
                }

                // Kein Store gefunden - hilfreiche Fehlermeldung
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
        /// Gibt einen bereits registrierten DataStore asynchron zurück, unabhängig davon ob InMemory oder Persistent.
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
                var inMemoryKey = GetInMemoryKey<T>();
                var persistentKey = GetPersistentKey<T>();

                // Prüfe zuerst Persistent, dann InMemory (Persistent ist häufiger)
                if (_singletons.TryGetValue(persistentKey, out var persistentStore))
                {
                    return (IDataStore<T>)persistentStore;
                }

                if (_singletons.TryGetValue(inMemoryKey, out var inMemoryStore))
                {
                    return (IDataStore<T>)inMemoryStore;
                }

                // Kein Store gefunden - hilfreiche Fehlermeldung
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
        public InMemoryDataStore<T> GetInMemory<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            if (!isSingleton)
            {
                return _factory.CreateInMemoryStore<T>(comparer);
            }

            // Thread-safe Singleton-Zugriff
            _lock.Wait();
            try
            {
                var key = GetInMemoryKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    return (InMemoryDataStore<T>)existing;
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
        public async Task<InMemoryDataStore<T>> GetInMemoryAsync<T>(
            bool isSingleton = true,
            IEqualityComparer<T>? comparer = null)
            where T : class
        {
            if (!isSingleton)
            {
                return await Task.Run(() => _factory.CreateInMemoryStore<T>(comparer));
            }

            // Thread-safe Singleton-Zugriff
            await _lock.WaitAsync();
            try
            {
                var key = GetInMemoryKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    return (InMemoryDataStore<T>)existing;
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
        /// </remarks>
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
                // Nicht-Singleton: Erstellen und optional laden
                var repository = ResolveRepository<T>(repositoryFactory);
                var store = _factory.CreatePersistentStore<T>(repository, trackPropertyChanges);
                
                if (autoLoad)
                {
                    store.Load();
                }
                
                return store;
            }

            // Thread-safe Singleton-Zugriff
            _lock.Wait();
            try
            {
                var key = GetPersistentKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    return (PersistentDataStore<T>)existing;
                }

                // Repository über Factory auflösen (automatische JSON/LiteDB-Erkennung)
                var repo = ResolveRepository<T>(repositoryFactory);
                var newStore = _factory.CreatePersistentStore<T>(repo, trackPropertyChanges);
                
                // AutoLoad im Provider (nicht in Factory!)
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
                // Nicht-Singleton: Async erstellen und laden
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

            // Thread-safe Singleton-Zugriff
            await _lock.WaitAsync();
            try
            {
                var key = GetPersistentKey<T>();
                
                if (_singletons.TryGetValue(key, out var existing))
                {
                    return (PersistentDataStore<T>)existing;
                }

                // Async Repository auflösen und Store erstellen
                var newStore = await Task.Run(() =>
                {
                    var repository = ResolveRepository<T>(repositoryFactory);
                    var store = _factory.CreatePersistentStore<T>(repository, trackPropertyChanges);
                    
                    // AutoLoad im Provider (nicht in Factory!)
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
            // EntityBase ? LiteDB (granulare Operationen mit Update/Delete)
            if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
            {
                // Runtime-Cast ist sicher, da EntityBase : IEntity implementiert
                var method = typeof(IRepositoryFactory).GetMethod(nameof(IRepositoryFactory.GetLiteDbRepository))!;
                var genericMethod = method.MakeGenericMethod(typeof(T));
                return (IRepositoryBase<T>)genericMethod.Invoke(repositoryFactory, null)!;
            }

            // POCOs ? JSON (atomares WriteAll, funktioniert mit jedem POCO)
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
                // Versuche, sowohl InMemory- als auch Persistent-Keys zu entfernen
                var inMemoryKey = GetInMemoryKey<T>();
                var persistentKey = GetPersistentKey<T>();
                
                bool removed = false;
                
                if (_singletons.TryGetValue(inMemoryKey, out var inMemoryStore))
                {
                    (inMemoryStore as IDisposable)?.Dispose();
                    removed |= _singletons.Remove(inMemoryKey);
                }
                
                if (_singletons.TryGetValue(persistentKey, out var persistentStore))
                {
                    (persistentStore as IDisposable)?.Dispose();
                    removed |= _singletons.Remove(persistentKey);
                }
                
                return removed;
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
