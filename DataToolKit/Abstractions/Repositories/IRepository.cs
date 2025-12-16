using System;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Erweiterter Repository-Kontrakt für elementweise Änderungen an einzelnen Entitäten.
    /// Erweitert <see cref="IRepositoryBase{T}"/> um <see cref="Update"/> und <see cref="Delete"/>.
    /// </summary>
    /// <typeparam name="T">Der Entitätstyp, den dieses Repository verwaltet.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Wichtig:</b> Dieses Interface wird nur von <c>LiteDbRepository&lt;T&gt;</c> implementiert,
    /// nicht von <c>JsonRepository&lt;T&gt;</c>. JSON-Repositories unterstützen nur Volloperationen
    /// über <see cref="IRepositoryBase{T}.Write"/>.
    /// </para>
    /// <para>
    /// <b>Delta-Verhalten:</b> Im Gegensatz zu <see cref="IRepositoryBase{T}.Write"/>, das die
    /// gesamte Collection ersetzt, ändern <see cref="Update"/> und <see cref="Delete"/> nur
    /// die jeweils angegebene Entität.
    /// </para>
    /// </remarks>
    public interface IRepository<T> : IRepositoryBase<T>
    {
        /// <summary>
        /// Aktualisiert eine einzelne vorhandene Entität im Repository.
        /// </summary>
        /// <param name="item">
        /// Die zu aktualisierende Entität. Die Identifikation erfolgt typischerweise über die
        /// <c>Id</c>-Eigenschaft (bei <see cref="IEntity"/>-Implementierungen).
        /// </param>
        /// <returns>
        /// Die Anzahl der aktualisierten Datensätze (typischerweise <c>1</c> bei Erfolg, <c>0</c> wenn nicht gefunden).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="item"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn die Entität eine ungültige <c>Id</c> hat (z.B. <c>Id &lt;= 0</c>).
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn die Entität nicht im Repository gefunden wurde (bei strikten Implementierungen).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Implementierungs-Hinweis:</b> LiteDB nutzt die <c>Id</c>-Eigenschaft zur Identifikation
        /// und führt ein transaktionales Update durch.
        /// </para>
        /// </remarks>
        int Update(T item);

        /// <summary>
        /// Löscht eine einzelne Entität aus dem Repository.
        /// </summary>
        /// <param name="item">
        /// Die zu löschende Entität. Die Identifikation erfolgt typischerweise über die
        /// <c>Id</c>-Eigenschaft (bei <see cref="IEntity"/>-Implementierungen).
        /// </param>
        /// <returns>
        /// Die Anzahl der gelöschten Datensätze (typischerweise <c>1</c> bei Erfolg, <c>0</c> wenn nicht gefunden).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="item"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn die Entität nicht im Repository gefunden wurde (bei strikten Implementierungen).
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Implementierungs-Hinweis:</b> LiteDB nutzt die <c>Id</c>-Eigenschaft zur Identifikation.
        /// Wenn <c>Id &lt;= 0</c>, gibt die Methode <c>0</c> zurück (keine Exception).
        /// </para>
        /// </remarks>
        int Delete(T item);
    }
}
