using System;
using System.Collections.Generic;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;
using TestHelper.DataToolKit.Fakes.Repositories;

namespace TestHelper.DataToolKit.Builders;

/// <summary>
/// Builder für komplexe Repository-Szenarien mit vorgefertigten Daten.
/// Ermöglicht die Erstellung von Test-Repositories mit vordefinierten Datensets.
/// </summary>
/// <typeparam name="T">Entitätstyp (muss EntityBase sein).</typeparam>
public class RepositoryScenarioBuilder<T> where T : EntityBase, new()
{
    private readonly FakeRepositoryFactory _factory;
    private readonly List<T> _entities = new();

    /// <summary>
    /// Erstellt einen neuen RepositoryScenarioBuilder.
    /// </summary>
    public RepositoryScenarioBuilder(FakeRepositoryFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Fügt eine einzelne Entität zum Szenario hinzu.
    /// </summary>
    public RepositoryScenarioBuilder<T> WithEntity(T entity)
    {
        _entities.Add(entity);
        return this;
    }

    /// <summary>
    /// Fügt mehrere Entitäten zum Szenario hinzu.
    /// </summary>
    public RepositoryScenarioBuilder<T> WithEntities(params T[] entities)
    {
        _entities.AddRange(entities);
        return this;
    }

    /// <summary>
    /// Fügt mehrere Entitäten mit einer Factory-Funktion hinzu.
    /// </summary>
    /// <param name="count">Anzahl der zu erstellenden Entities.</param>
    /// <param name="factory">Factory-Funktion (Index ? Entity).</param>
    public RepositoryScenarioBuilder<T> WithRandomEntities(int count, Func<int, T> factory)
    {
        for (int i = 0; i < count; i++)
            _entities.Add(factory(i));
        return this;
    }

    /// <summary>
    /// Erstellt ein LiteDB-Repository mit den konfigurierten Entities.
    /// </summary>
    public IRepository<T> BuildLiteDb()
    {
        if (typeof(T).GetInterface(nameof(IEntity)) == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} must implement IEntity for LiteDB repositories");
            
        var repo = _factory.GetFakeLiteDbRepository<T>();
        repo.SeedData(_entities.ToArray());
        return repo;
    }

    /// <summary>
    /// Erstellt ein JSON-Repository mit den konfigurierten Entities.
    /// </summary>
    public IRepositoryBase<T> BuildJson()
    {
        var repo = _factory.GetFakeJsonRepository<T>();
        repo.SeedData(_entities.ToArray());
        return repo;
    }

    /// <summary>
    /// Gibt das typisierte Fake-LiteDB-Repository für Assertions zurück.
    /// </summary>
    public FakeLiteDbRepository<T> BuildFakeLiteDb()
    {
        if (typeof(T).GetInterface(nameof(IEntity)) == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} must implement IEntity for LiteDB repositories");
            
        var repo = _factory.GetFakeLiteDbRepository<T>();
        repo.SeedData(_entities.ToArray());
        return repo;
    }

    /// <summary>
    /// Gibt das typisierte Fake-JSON-Repository für Assertions zurück.
    /// </summary>
    public FakeJsonRepository<T> BuildFakeJson()
    {
        var repo = _factory.GetFakeJsonRepository<T>();
        repo.SeedData(_entities.ToArray());
        return repo;
    }
}
