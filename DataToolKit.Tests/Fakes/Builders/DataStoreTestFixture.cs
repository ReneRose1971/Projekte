using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Tests.Fakes.Providers;
using DataToolKit.Tests.Fakes.Repositories;

namespace DataToolKit.Tests.Fakes.Builders
{
    /// <summary>
    /// xUnit-Fixture für DataStore-Tests mit automatischem Setup und Cleanup.
    /// Vereinfacht Test-Setup durch vorkonfigurierte Fake-Provider und DataStores.
    /// </summary>
    /// <typeparam name="T">Entitätstyp.</typeparam>
    public class DataStoreTestFixture<T> : IDisposable where T : class
    {
        /// <summary>
        /// Fake DataStoreProvider für den Test.
        /// </summary>
        public FakeDataStoreProvider Provider { get; }

        /// <summary>
        /// Fake RepositoryFactory für den Test.
        /// </summary>
        public FakeRepositoryFactory RepositoryFactory { get; }

        /// <summary>
        /// Der für den Test konfigurierte DataStore.
        /// </summary>
        public IDataStore<T> DataStore { get; }

        /// <summary>
        /// Erstellt eine neue Fixture mit optionalem Persistent-DataStore.
        /// </summary>
        /// <param name="usePersistent">Wenn true, wird ein PersistentDataStore erstellt.</param>
        /// <param name="autoLoad">Wenn true, werden Daten automatisch geladen (nur bei Persistent).</param>
        /// <param name="comparer">Optionaler EqualityComparer (nur bei InMemory).</param>
        public DataStoreTestFixture(
            bool usePersistent = false,
            bool autoLoad = true,
            IEqualityComparer<T>? comparer = null)
        {
            RepositoryFactory = new FakeRepositoryFactory();
            Provider = new FakeDataStoreProvider(RepositoryFactory);

            DataStore = usePersistent
                ? Provider.GetPersistent<T>(RepositoryFactory, autoLoad: autoLoad)
                : Provider.GetInMemory<T>(comparer: comparer);
        }

        /// <summary>
        /// Füllt den DataStore mit Test-Daten.
        /// </summary>
        public void SeedData(params T[] items)
        {
            foreach (var item in items)
                DataStore.Add(item);
        }

        /// <summary>
        /// Setzt den DataStore und alle Repositories in den Ausgangszustand zurück.
        /// </summary>
        public void Reset()
        {
            DataStore.Clear();
            RepositoryFactory.ResetAll();
        }

        /// <summary>
        /// Gibt alle Ressourcen frei.
        /// </summary>
        public void Dispose()
        {
            Provider.ClearAll();
        }
    }
}
