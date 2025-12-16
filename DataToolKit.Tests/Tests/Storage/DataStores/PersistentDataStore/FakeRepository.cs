using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Tests.Common
{
    /// <summary>
    /// Fake-Entity-Repository (LiteDB-Pfad):
    /// Unterstützt Update/Delete wie im echten IRepository<T>.
    /// </summary>
    public class FakeRepository<T> : FakeRepositoryBase<T>, IRepository<T> where T : class, IEntity
    {
        public int UpdateCount { get; private set; }
        public int DeleteCount { get; private set; }

        /// <summary>
        /// Simuliert ein Elementupdate.
        /// </summary>
        public int Update(T entity)
        {
            UpdateCount++;
            return 1; // Rückgabewert int – wie im echten Contract
        }

        /// <summary>
        /// Simuliert das Löschen eines Elements.
        /// </summary>
        public int Delete(T entity)
        {
            DeleteCount++;
            return 1;
        }
    }
}
