using System;
using System.Collections.Generic;
using System.Linq;
using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions;
using DataToolKit.Abstractions.Repositories;

namespace TestHelper.DataToolKit.Fakes.Repositories;

/// <summary>
/// In-Memory Fake für LiteDbRepository mit automatischer ID-Vergabe.
/// Simuliert granulare Operationen (Update/Delete) ohne echte Datenbank.
/// </summary>
/// <typeparam name="T">Entitätstyp (muss EntityBase sein).</typeparam>
public class FakeLiteDbRepository<T> : IRepository<T>
    where T : EntityBase
{
    private readonly Dictionary<int, T> _items = new();
    private int _nextId = 1;
    private readonly IEqualityComparer<T> _comparer;
    private readonly List<RepositoryOperation> _history = new();

    /// <summary>
    /// Simuliert Write-Fehler, wenn true.
    /// </summary>
    public bool ThrowOnWrite { get; set; }

    /// <summary>
    /// Simuliert Update-Fehler, wenn true.
    /// </summary>
    public bool ThrowOnUpdate { get; set; }

    /// <summary>
    /// Simuliert Delete-Fehler, wenn true.
    /// </summary>
    public bool ThrowOnDelete { get; set; }

    /// <summary>
    /// Historie aller Repository-Operationen für Assertions.
    /// </summary>
    public IReadOnlyList<RepositoryOperation> History => _history.AsReadOnly();

    /// <summary>
    /// Aktuelle maximale ID (höchste ID im Repository).
    /// </summary>
    public int CurrentMaxId => _items.Count > 0 ? _items.Keys.Max() : (_nextId - 1);

    /// <summary>
    /// Anzahl der Write-Aufrufe.
    /// </summary>
    public int WriteCallCount => _history.Count(x => x.Action == "Write");

    /// <summary>
    /// Anzahl der Load-Aufrufe.
    /// </summary>
    public int LoadCallCount => _history.Count(x => x.Action == "Load");

    /// <summary>
    /// Erstellt ein FakeLiteDbRepository mit Default-Comparer.
    /// Dieser parameterlose Konstruktor wird für Activator.CreateInstance benötigt.
    /// </summary>
    public FakeLiteDbRepository() : this(null)
    {
    }

    /// <summary>
    /// Erstellt ein FakeLiteDbRepository mit optionalem EqualityComparer.
    /// </summary>
    public FakeLiteDbRepository(IEqualityComparer<T>? comparer)
    {
        _comparer = comparer ?? new FallbackEqualsComparer<T>();
    }

    /// <summary>
    /// Lädt alle Elemente aus dem In-Memory-Store.
    /// </summary>
    public IReadOnlyList<T> Load()
    {
        _history.Add(new RepositoryOperation("Load", DateTime.UtcNow, _items.Count));
        return _items.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Schreibt Elemente mit Delta-Synchronisierung (Insert/Update/Delete).
    /// </summary>
    public void Write(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (ThrowOnWrite)
            throw new InvalidOperationException("Simulated write failure");

        var incoming = items.ToList();
        var existing = _items.Values.ToList();

        var diff = RepositoryDiffBuilder.BuildForEntityBase(
            existing,
            incoming,
            _comparer,
            missingAsInsert: true
        );

        try
        {
            foreach (var update in diff.ToUpdate)
            {
                if (_items.ContainsKey(update.Id))
                    _items[update.Id] = update;
            }

            foreach (var deleteId in diff.ToDeleteIds)
            {
                _items.Remove(deleteId);
            }

            foreach (var insert in diff.ToInsert)
            {
                if (insert.Id == 0)
                    insert.Id = _nextId++;
                _items[insert.Id] = insert;
            }

            _history.Add(new RepositoryOperation("Write", DateTime.UtcNow, incoming.Count));
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Aktualisiert eine einzelne Entität.
    /// </summary>
    public int Update(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (item.Id <= 0)
            throw new ArgumentException("Invalid Id (>0 expected)", nameof(item));

        if (ThrowOnUpdate)
            throw new InvalidOperationException("Simulated update failure");

        if (!_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Entity with Id {item.Id} not found");

        _items[item.Id] = item;
        _history.Add(new RepositoryOperation("Update", DateTime.UtcNow, 1));
        return 1;
    }

    /// <summary>
    /// Löscht eine einzelne Entität.
    /// </summary>
    public int Delete(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (item.Id <= 0)
            return 0;

        if (ThrowOnDelete)
            throw new InvalidOperationException("Simulated delete failure");

        if (!_items.Remove(item.Id))
            throw new InvalidOperationException($"Entity with Id {item.Id} not found");

        _history.Add(new RepositoryOperation("Delete", DateTime.UtcNow, 1));
        return 1;
    }

    /// <summary>
    /// Leert den In-Memory-Store.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _history.Add(new RepositoryOperation("Clear", DateTime.UtcNow, 0));
    }

    /// <summary>
    /// Setzt das Repository in den Ausgangszustand zurück.
    /// </summary>
    public void Reset()
    {
        _items.Clear();
        _history.Clear();
        _nextId = 1;
        ThrowOnWrite = false;
        ThrowOnUpdate = false;
        ThrowOnDelete = false;
    }

    /// <summary>
    /// Füllt das Repository mit Test-Daten (mit Auto-ID für Id == 0).
    /// Explizite IDs können den Auto-ID-Counter überspringen.
    /// </summary>
    public void SeedData(params T[] items)
    {
        foreach (var item in items)
        {
            if (item.Id == 0)
            {
                item.Id = _nextId++;
            }
            else
            {
                if (item.Id >= _nextId)
                    _nextId = item.Id + 1;
            }
            _items[item.Id] = item;
        }
    }

    /// <summary>
    /// Gibt eine Entität anhand ihrer ID zurück (null, wenn nicht gefunden).
    /// </summary>
    public T? GetById(int id) => _items.TryGetValue(id, out var item) ? item : null;

    public void Dispose()
    {
    }
}
