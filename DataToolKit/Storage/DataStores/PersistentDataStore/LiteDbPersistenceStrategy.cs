using System;
using System.Collections.Generic;
using DataToolKit.Abstractions.DataStores;
using DataToolKit.Abstractions.Repositories;

namespace DataToolKit.Storage.DataStores
{
    /// <summary>
    /// Persistierungs-Strategie für LiteDB-Repositories.
    /// Nutzt granulare Operationen (Update, Delete) wo möglich.
    /// </summary>
    /// <typeparam name="T">Entitätstyp (muss IEntity implementieren für Id-Property).</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Funktionsweise:</b> LiteDB unterstützt feingranulare Operationen über <see cref="IRepository{T}"/>.
    /// Diese Strategie nutzt <c>Update()</c> und <c>Delete()</c> für einzelne Entitäten, was performanter
    /// ist als ein vollständiges <c>Write()</c> der Collection.
    /// </para>
    /// <para>
    /// <b>IEntity-Requirement:</b> Diese Strategie benötigt die <c>Id</c>-Property aus <see cref="IEntity"/>,
    /// um zu prüfen, ob eine Entität bereits persistiert ist (<c>Id &gt; 0</c>). Typischerweise wird
    /// <see cref="EntityBase"/> als Basisklasse verwendet.
    /// </para>
    /// <para>
    /// <b>Add-Operationen:</b> Da <see cref="IRepository{T}"/> keine <c>Insert()</c>-Methode hat,
    /// wird bei Add die gesamte Collection geschrieben. LiteDB erkennt automatisch neue Entitäten
    /// (mit <c>Id = 0</c>) und weist ihnen IDs zu (Delta-Detection).
    /// </para>
    /// </remarks>
    internal sealed class LiteDbPersistenceStrategy<T> : IPersistenceStrategy<T> 
        where T : class, IEntity
    {
        private readonly IRepository<T> _repository;
        private readonly Func<IReadOnlyList<T>> _itemsAccessor;

        /// <summary>
        /// Erstellt eine LiteDB-Strategie.
        /// </summary>
        /// <param name="repository">Das LiteDB-Repository (IRepository mit Update/Delete).</param>
        /// <param name="itemsAccessor">Funktion zum Zugriff auf die aktuelle Collection.</param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="repository"/> oder <paramref name="itemsAccessor"/> null ist.
        /// </exception>
        public LiteDbPersistenceStrategy(
            IRepository<T> repository,
            Func<IReadOnlyList<T>> itemsAccessor)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _itemsAccessor = itemsAccessor ?? throw new ArgumentNullException(nameof(itemsAccessor));
        }

        /// <summary>
        /// Bei Add: Gesamte Collection schreiben.
        /// </summary>
        /// <remarks>
        /// LiteDB hat keine explizite Insert-Methode in <see cref="IRepository{T}"/>.
        /// Daher wird <c>Write()</c> verwendet, welches Delta-Detection macht:
        /// Neue Entitäten (Id = 0) werden automatisch erkannt und eingefügt.
        /// </remarks>
        public void OnAdded(T entity)
        {
            _repository.Write(_itemsAccessor());
        }

        /// <summary>
        /// Bei Remove: Granulares Delete nutzen.
        /// </summary>
        /// <remarks>
        /// Nutzt <see cref="IRepository{T}.Delete"/> für performante Einzellöschung.
        /// Nur Entitäten mit <c>Id &gt; 0</c> werden gelöscht (bereits persistierte Entitäten).
        /// </remarks>
        public void OnRemoved(T entity)
        {
            if (entity.Id > 0)
            {
                _repository.Delete(entity);
            }
        }

        /// <summary>
        /// Bei PropertyChanged: Granulares Update nutzen.
        /// </summary>
        /// <remarks>
        /// Nutzt <see cref="IRepository{T}.Update"/> für performante Einzelaktualisierung.
        /// Nur Entitäten mit <c>Id &gt; 0</c> werden aktualisiert (bereits persistierte Entitäten).
        /// </remarks>
        public void OnEntityChanged(T entity)
        {
            if (entity.Id > 0)
            {
                _repository.Update(entity);
            }
        }

        /// <summary>
        /// Bei Clear: Repository komplett leeren.
        /// </summary>
        public void OnCleared()
        {
            _repository.Clear();
        }

        /// <summary>
        /// Gibt Ressourcen frei (keine Ressourcen in dieser Strategie).
        /// </summary>
        public void Dispose()
        {
            // Keine Ressourcen zu bereinigen
        }
    }
}
