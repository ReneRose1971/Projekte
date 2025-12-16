using System;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

namespace DataToolKit.Tests.Common
{
    /// <summary>
    /// Fake-Implementierung von IRepositoryFactory für Tests.
    /// Ermöglicht die Konfiguration von Repository-Rückgabewerten.
    /// Unterstützt automatische Erkennung: EntityBase ? LiteDB, sonst ? JSON.
    /// </summary>
    public class FakeRepositoryFactory : IRepositoryFactory
    {
        private readonly Func<Type, object>? _jsonRepositoryProvider;
        private readonly Func<Type, object>? _liteDbRepositoryProvider;

        public FakeRepositoryFactory()
        {
        }

        public FakeRepositoryFactory(
            Func<Type, object>? jsonRepositoryProvider = null,
            Func<Type, object>? liteDbRepositoryProvider = null)
        {
            _jsonRepositoryProvider = jsonRepositoryProvider;
            _liteDbRepositoryProvider = liteDbRepositoryProvider;
        }

        public IRepositoryBase<T> GetJsonRepository<T>()
        {
            if (_jsonRepositoryProvider != null)
            {
                return (IRepositoryBase<T>)_jsonRepositoryProvider(typeof(T));
            }

            return (IRepositoryBase<T>)Activator.CreateInstance(typeof(FakeRepositoryBase<>).MakeGenericType(typeof(T)))!;
        }

        public IRepository<T> GetLiteDbRepository<T>() where T : class
        {
            if (_liteDbRepositoryProvider != null)
            {
                return (IRepository<T>)_liteDbRepositoryProvider(typeof(T));
            }

            return (IRepository<T>)Activator.CreateInstance(typeof(FakeRepository<>).MakeGenericType(typeof(T)))!;
        }

        /// <summary>
        /// Erstellt eine FakeRepositoryFactory mit vorkonfigurierten Daten.
        /// Automatische Erkennung: EntityBase ? LiteDB, sonst ? JSON.
        /// </summary>
        public static FakeRepositoryFactory WithData<T>(T[] data) where T : class, IEntity
        {
            // EntityBase ? LiteDB-Repository
            if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
            {
                var liteDbRepo = new FakeRepository<T>();
                liteDbRepo.SetData(data);

                return new FakeRepositoryFactory(
                    jsonRepositoryProvider: null,
                    liteDbRepositoryProvider: type => liteDbRepo);
            }

            // Nur IEntity ? JSON-Repository
            var jsonRepo = new FakeRepositoryBase<T>();
            jsonRepo.SetData(data);

            return new FakeRepositoryFactory(
                jsonRepositoryProvider: type => jsonRepo,
                liteDbRepositoryProvider: null);
        }

        /// <summary>
        /// Erstellt eine FakeRepositoryFactory mit einem spezifischen Repository.
        /// </summary>
        public static FakeRepositoryFactory WithRepository<T>(IRepositoryBase<T> repository) where T : class, IEntity
        {
            // Wenn IRepository, dann als LiteDB registrieren
            if (repository is IRepository<T> liteDbRepo)
            {
                return new FakeRepositoryFactory(
                    jsonRepositoryProvider: null,
                    liteDbRepositoryProvider: type => type == typeof(T) ? liteDbRepo : Activator.CreateInstance(typeof(FakeRepository<>).MakeGenericType(type))!);
            }

            // Sonst als JSON
            return new FakeRepositoryFactory(
                jsonRepositoryProvider: type => type == typeof(T) ? repository : Activator.CreateInstance(typeof(FakeRepositoryBase<>).MakeGenericType(type))!,
                liteDbRepositoryProvider: null);
        }
    }
}
