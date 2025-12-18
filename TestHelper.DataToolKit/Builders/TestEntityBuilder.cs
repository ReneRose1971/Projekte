using System;
using System.Collections.Generic;
using System.Linq;
using DataToolKit.Abstractions.Repositories;

namespace TestHelper.DataToolKit.Builders;

/// <summary>
/// Fluent Builder für Test-Entities mit sinnvollen Defaults.
/// Vereinfacht die Erstellung von Test-Daten mit einer lesbaren API.
/// </summary>
/// <typeparam name="T">Entitätstyp (muss EntityBase sein).</typeparam>
public class TestEntityBuilder<T> where T : EntityBase, new()
{
    private readonly List<Action<T>> _configurations = new();

    /// <summary>
    /// Setzt die ID der Entität.
    /// </summary>
    public TestEntityBuilder<T> WithId(int id)
    {
        _configurations.Add(e => e.Id = id);
        return this;
    }

    /// <summary>
    /// Wendet eine benutzerdefinierte Konfiguration auf die Entität an.
    /// </summary>
    public TestEntityBuilder<T> With(Action<T> configure)
    {
        _configurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Erstellt die konfigurierte Entität.
    /// </summary>
    public T Build()
    {
        var entity = new T();
        foreach (var config in _configurations)
            config(entity);
        return entity;
    }

    /// <summary>
    /// Erstellt mehrere Kopien der konfigurierten Entität.
    /// </summary>
    /// <param name="count">Anzahl der zu erstellenden Entities.</param>
    public List<T> BuildMany(int count)
    {
        var entities = new List<T>();
        for (int i = 0; i < count; i++)
        {
            entities.Add(Build());
        }
        return entities;
    }

    /// <summary>
    /// Erstellt mehrere Entities mit Index-basierter Konfiguration.
    /// </summary>
    /// <param name="count">Anzahl der zu erstellenden Entities.</param>
    /// <param name="configureWithIndex">Konfiguration mit Index-Parameter.</param>
    public List<T> BuildMany(int count, Action<T, int> configureWithIndex)
    {
        var entities = new List<T>();
        for (int i = 0; i < count; i++)
        {
            var entity = Build();
            configureWithIndex(entity, i);
            entities.Add(entity);
        }
        return entities;
    }
}
