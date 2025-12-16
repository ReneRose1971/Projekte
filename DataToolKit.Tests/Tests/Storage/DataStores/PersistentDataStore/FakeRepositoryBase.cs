using System;
using System.Collections.Generic;
using System.Linq;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Tests.Common
{
    /// <summary>
    /// Fake-Basisrepository für Snapshot-Schreiben (JSON-Pfad).
    /// Entspricht exakt den Signaturen von IRepositoryBase<T>.
    /// </summary>
    public class FakeRepositoryBase<T> : IRepositoryBase<T>, IDisposable where T : class, IEntity
    {
        public int WriteCount { get; set; }
        public int ClearCount { get; private set; }
        public List<T> LastWritten { get; } = new List<T>();
        public bool Disposed { get; private set; }

        /// <summary>
        /// Liefert den zuletzt geschriebenen Snapshot zurück.
        /// </summary>
        public IReadOnlyList<T> Load() => LastWritten.AsReadOnly();

        /// <summary>
        /// Simuliert das Schreiben aller Items ("Snapshot").
        /// </summary>
        public void Write(IEnumerable<T> items)
        {
            WriteCount++;
            LastWritten.Clear();
            if (items != null)
                LastWritten.AddRange(items);
        }

        /// <summary>
        /// Löscht alle Daten.
        /// </summary>
        public void Clear()
        {
            ClearCount++;
            LastWritten.Clear();
        }

        /// <summary>
        /// Setzt die Daten für Load() (Test-Helper).
        /// </summary>
        public void SetData(IEnumerable<T> items)
        {
            LastWritten.Clear();
            if (items != null)
                LastWritten.AddRange(items);
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
