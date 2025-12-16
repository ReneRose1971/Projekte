using System;
using System.Collections.Generic;
using System.Linq;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

namespace DataToolKit.Tests.Fakes.Repositories
{
    /// <summary>
    /// Fake Factory für Repositories mit zentralem Reset und Inspection.
    /// Verwaltet Fake-Repositories typsicher und ermöglicht einfache Test-Assertions.
    /// </summary>
    public class FakeRepositoryFactory : IRepositoryFactory
    {
        private readonly Dictionary<Type, object> _jsonRepositories = new();
        private readonly Dictionary<Type, object> _liteDbRepositories = new();

        /// <summary>
        /// Gibt das Fake JSON-Repository für den Typ T zurück (Singleton pro Typ).
        /// </summary>
        public IRepositoryBase<T> GetJsonRepository<T>()
        {
            if (!_jsonRepositories.TryGetValue(typeof(T), out var repo))
            {
                var repoType = typeof(FakeJsonRepository<>).MakeGenericType(typeof(T));
                repo = Activator.CreateInstance(repoType)!;
                _jsonRepositories[typeof(T)] = repo;
            }
            return (IRepositoryBase<T>)repo;
        }

        /// <summary>
        /// Gibt das Fake LiteDB-Repository für den Typ T zurück (Singleton pro Typ).
        /// </summary>
        public IRepository<T> GetLiteDbRepository<T>() where T : class
        {
            if (!_liteDbRepositories.TryGetValue(typeof(T), out var repo))
            {
                var repoType = typeof(FakeLiteDbRepository<>).MakeGenericType(typeof(T));
                repo = Activator.CreateInstance(repoType)!;
                _liteDbRepositories[typeof(T)] = repo;
            }
            return (IRepository<T>)repo;
        }

        /// <summary>
        /// Gibt das typisierte Fake JSON-Repository für Test-Assertions zurück.
        /// </summary>
        public FakeJsonRepository<T> GetFakeJsonRepository<T>() where T : class
            => (FakeJsonRepository<T>)GetJsonRepository<T>();

        /// <summary>
        /// Gibt das typisierte Fake LiteDB-Repository für Test-Assertions zurück.
        /// </summary>
        public FakeLiteDbRepository<T> GetFakeLiteDbRepository<T>() where T : EntityBase
            => (FakeLiteDbRepository<T>)GetLiteDbRepository<T>();

        /// <summary>
        /// Setzt alle Repositories in den Ausgangszustand zurück (leert Daten und History).
        /// </summary>
        public void ResetAll()
        {
            foreach (var repo in _jsonRepositories.Values)
            {
                var resetMethod = repo.GetType().GetMethod("Reset");
                resetMethod?.Invoke(repo, null);
            }

            foreach (var repo in _liteDbRepositories.Values)
            {
                var resetMethod = repo.GetType().GetMethod("Reset");
                resetMethod?.Invoke(repo, null);
            }
        }

        /// <summary>
        /// Entfernt alle Repositories aus dem Cache.
        /// </summary>
        public void ClearAll()
        {
            _jsonRepositories.Clear();
            _liteDbRepositories.Clear();
        }
    }
}
