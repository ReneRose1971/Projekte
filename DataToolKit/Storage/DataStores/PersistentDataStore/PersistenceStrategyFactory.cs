using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Factory zur automatischen Auswahl der passenden Persistierungs-Strategie
    /// basierend auf dem Repository-Typ (JSON oder LiteDB).
    /// </summary>
    public static class PersistenceStrategyFactory
    {
        /// <summary>
        /// Erstellt die passende Persistenzstrategie basierend auf Repository-Fähigkeiten.
        /// </summary>
        /// <typeparam name="T">Entitätstyp (class constraint - funktioniert mit POCOs und EntityBase).</typeparam>
        /// <param name="repository">Das Repository (IRepositoryBase oder IRepository).</param>
        /// <param name="currentItemsAccessor">Funktion zum Zugriff auf die aktuelle Collection.</param>
        /// <returns>
        /// Eine <see cref="IPersistenceStrategy{T}"/>-Implementierung:
        /// - <see cref="LiteDbPersistenceStrategy{T}"/> wenn Repository <see cref="IRepository{T}"/> implementiert (granulare Operationen, benötigt IEntity)
        /// - <see cref="JsonPersistenceStrategy{T}"/> für <see cref="IRepositoryBase{T}"/> (atomares WriteAll, funktioniert mit jedem POCO)
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="repository"/> oder <paramref name="currentItemsAccessor"/> null ist.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn Repository IRepository{T} implementiert, aber T nicht IEntity implementiert.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>LiteDB-Erkennung:</b> Wenn das Repository <see cref="IRepository{T}"/> implementiert,
        /// werden die Methoden <c>Update()</c> und <c>Delete()</c> genutzt für granulare Persistierung.
        /// Dies erfordert IEntity (für die Id-Property).
        /// </para>
        /// <para>
        /// <b>JSON-Fallback:</b> Alle anderen Repositories (nur <see cref="IRepositoryBase{T}"/>)
        /// nutzen atomares <c>Write()</c> für die gesamte Collection. Funktioniert mit jedem POCO.
        /// </para>
        /// </remarks>
        public static IPersistenceStrategy<T> Create<T>(
            IRepositoryBase<T> repository,
            Func<IReadOnlyList<T>> currentItemsAccessor)
            where T : class
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));
            
            if (currentItemsAccessor is null)
                throw new ArgumentNullException(nameof(currentItemsAccessor));

            // LiteDB: Unterstützt granulare Operationen (Update/Delete) - benötigt IEntity
            if (repository is IRepository<T> liteDbRepository)
            {
                // Runtime-Check: T muss IEntity implementieren für LiteDB
                if (typeof(T).GetInterface(nameof(IEntity)) == null)
                {
                    throw new InvalidOperationException(
                        $"LiteDB-Repository benötigt IEntity-Implementierung. " +
                        $"Typ '{typeof(T).Name}' implementiert IEntity nicht. " +
                        $"Verwenden Sie EntityBase als Basisklasse oder ein JSON-Repository.");
                }

                // Cast ist sicher, da wir oben geprüft haben
                return (IPersistenceStrategy<T>)Activator.CreateInstance(
                    typeof(LiteDbPersistenceStrategy<>).MakeGenericType(typeof(T)),
                    liteDbRepository,
                    currentItemsAccessor)!;
            }

            // JSON: Nur WriteAll-Operationen - funktioniert mit jedem POCO
            return new JsonPersistenceStrategy<T>(repository, currentItemsAccessor);
        }
    }
}
