using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Tests.Fakes.Repositories
{
    /// <summary>
    /// In-Memory Fake für JsonRepository ohne Dateisystem-Zugriff.
    /// Ideal für Unit-Tests mit konfigurierbarem Verhalten.
    /// </summary>
    /// <typeparam name="T">Entitätstyp.</typeparam>
    public class FakeJsonRepository<T> : IRepositoryBase<T> where T : class
    {
        private List<T> _items = new();
        private readonly List<RepositoryOperation> _history = new();

        /// <summary>
        /// Simuliert Load-Fehler, wenn true.
        /// </summary>
        public bool ThrowOnLoad { get; set; }

        /// <summary>
        /// Simuliert Write-Fehler, wenn true.
        /// </summary>
        public bool ThrowOnWrite { get; set; }

        /// <summary>
        /// Simuliert verzögerte Operationen.
        /// </summary>
        public TimeSpan? SimulatedDelay { get; set; }

        /// <summary>
        /// Historie aller Repository-Operationen für Assertions.
        /// </summary>
        public IReadOnlyList<RepositoryOperation> History => _history.AsReadOnly();

        /// <summary>
        /// Anzahl der Load-Aufrufe.
        /// </summary>
        public int LoadCallCount => _history.Count(x => x.Action == "Load");

        /// <summary>
        /// Anzahl der Write-Aufrufe.
        /// </summary>
        public int WriteCallCount => _history.Count(x => x.Action == "Write");

        /// <summary>
        /// Lädt alle Elemente aus dem In-Memory-Store.
        /// </summary>
        public IReadOnlyList<T> Load()
        {
            if (ThrowOnLoad)
                throw new IOException("Simulated load failure");

            if (SimulatedDelay.HasValue)
                System.Threading.Thread.Sleep(SimulatedDelay.Value);

            _history.Add(new RepositoryOperation("Load", DateTime.UtcNow, _items.Count));
            return _items.AsReadOnly();
        }

        /// <summary>
        /// Schreibt alle Elemente in den In-Memory-Store (ersetzt bestehende Daten).
        /// </summary>
        public void Write(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (ThrowOnWrite)
                throw new IOException("Simulated write failure");

            var list = items.ToList();
            if (list.Any(x => x == null))
                throw new ArgumentException("Collection contains null elements", nameof(items));

            if (SimulatedDelay.HasValue)
                System.Threading.Thread.Sleep(SimulatedDelay.Value);

            _items = list;
            _history.Add(new RepositoryOperation("Write", DateTime.UtcNow, _items.Count));
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
            ThrowOnLoad = false;
            ThrowOnWrite = false;
            SimulatedDelay = null;
        }

        /// <summary>
        /// Füllt das Repository mit Test-Daten (ohne History-Eintrag).
        /// </summary>
        public void SeedData(params T[] items)
        {
            _items = items.ToList();
        }

        public void Dispose()
        {
            // Keine Ressourcen freizugeben
        }
    }

    /// <summary>
    /// Repräsentiert eine Repository-Operation für Test-Assertions.
    /// </summary>
    public record RepositoryOperation(string Action, DateTime Timestamp, int ItemCount);
}
