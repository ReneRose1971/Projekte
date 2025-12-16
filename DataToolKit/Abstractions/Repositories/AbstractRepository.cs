using System;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Abstrakte Zwischenstufe für Repositories mit Einzeloperationen (Update/Delete).
    /// Erbt von <see cref="AbstractRepositoryBase{T}"/> und implementiert <see cref="IRepository{T}"/>.
    /// Enthält nur die abstrakten Sync-Stubs.
    /// </summary>
    public abstract class AbstractRepository<T> : AbstractRepositoryBase<T>, IRepository<T>
    {
        /// <summary>
        /// Konstruktor, der die <see cref="IStorageOptions{T}"/> an die Basisklasse durchreicht.
        /// </summary>
        protected AbstractRepository(IStorageOptions<T> options)
            : base(options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
        }

        /// <summary>Aktualisiert eine bestehende Entität in der Datenquelle.</summary>
        public abstract int Update(T item);

        /// <summary>Entfernt eine bestehende Entität aus der Datenquelle.</summary>
        public abstract int Delete(T item);
    }
}
