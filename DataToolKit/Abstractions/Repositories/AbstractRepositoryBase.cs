using System;
using System.Collections.Generic;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Abstrakte Basisklasse für Repositories.
    /// Bezieht die Storage-Optionen über <see cref="IStorageOptions{T}"/> aus dem DI-Container.
    /// </summary>
    public abstract class AbstractRepositoryBase<T> : IRepositoryBase<T>
    {
        private bool _disposed;

        /// <summary>Storage-Optionen für den Typ T.</summary>
        protected IStorageOptions<T> Options { get; }

        /// <summary>Bequemer Zugriff auf den vollständigen Dateipfad.</summary>
        protected string FilePath => Options.FullPath;

        /// <summary>
        /// Erstellt die Basisklasse und injiziert die typspezifischen Storage-Optionen.
        /// </summary>
        /// <param name="options">Die für T registrierten Storage-Optionen (aus DI).</param>
        protected AbstractRepositoryBase(IStorageOptions<T> options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        // ——— IRepositoryBase ———
        public abstract IReadOnlyList<T> Load();
        public abstract void Write(System.Collections.Generic.IEnumerable<T> items);
        public abstract void Clear();

        // ——— IDisposable ———
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
