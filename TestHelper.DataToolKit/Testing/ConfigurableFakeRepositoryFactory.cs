using System;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.Repositories;

namespace TestHelper.DataToolKit.Testing;

/// <summary>
/// Fake-Implementierung von IRepositoryFactory für Tests.
/// Ermöglicht die Konfiguration von Repository-Rückgabewerten.
/// Unterstützt automatische Erkennung: EntityBase ? LiteDB, sonst ? JSON.
/// </summary>
public class ConfigurableFakeRepositoryFactory : IRepositoryFactory
{
    private readonly Func<Type, object>? _jsonRepositoryProvider;
    private readonly Func<Type, object>? _liteDbRepositoryProvider;

    public ConfigurableFakeRepositoryFactory()
    {
    }

    public ConfigurableFakeRepositoryFactory(
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

        return (IRepositoryBase<T>)Activator.CreateInstance(typeof(Fakes.Repositories.FakeRepositoryBase<>).MakeGenericType(typeof(T)))!;
    }

    public IRepository<T> GetLiteDbRepository<T>() where T : class
    {
        if (_liteDbRepositoryProvider != null)
        {
            return (IRepository<T>)_liteDbRepositoryProvider(typeof(T));
        }

        return (IRepository<T>)Activator.CreateInstance(typeof(Fakes.Repositories.FakeRepository<>).MakeGenericType(typeof(T)))!;
    }

    /// <summary>
    /// Erstellt eine ConfigurableFakeRepositoryFactory mit vorkonfigurierten Daten.
    /// Automatische Erkennung: EntityBase ? LiteDB, sonst ? JSON.
    /// </summary>
    public static ConfigurableFakeRepositoryFactory WithData<T>(T[] data) where T : class, IEntity
    {
        if (typeof(EntityBase).IsAssignableFrom(typeof(T)))
        {
            var liteDbRepo = new Fakes.Repositories.FakeRepository<T>();
            liteDbRepo.SetData(data);

            return new ConfigurableFakeRepositoryFactory(
                jsonRepositoryProvider: null,
                liteDbRepositoryProvider: type => liteDbRepo);
        }

        var jsonRepo = new Fakes.Repositories.FakeRepositoryBase<T>();
        jsonRepo.SetData(data);

        return new ConfigurableFakeRepositoryFactory(
            jsonRepositoryProvider: type => jsonRepo,
            liteDbRepositoryProvider: null);
    }

    /// <summary>
    /// Erstellt eine ConfigurableFakeRepositoryFactory mit einem spezifischen Repository.
    /// </summary>
    public static ConfigurableFakeRepositoryFactory WithRepository<T>(IRepositoryBase<T> repository) where T : class, IEntity
    {
        if (repository is IRepository<T> liteDbRepo)
        {
            return new ConfigurableFakeRepositoryFactory(
                jsonRepositoryProvider: null,
                liteDbRepositoryProvider: type => type == typeof(T) ? liteDbRepo : Activator.CreateInstance(typeof(Fakes.Repositories.FakeRepository<>).MakeGenericType(type))!);
        }

        return new ConfigurableFakeRepositoryFactory(
            jsonRepositoryProvider: type => type == typeof(T) ? repository : Activator.CreateInstance(typeof(Fakes.Repositories.FakeRepositoryBase<>).MakeGenericType(type))!,
            liteDbRepositoryProvider: null);
    }
}
