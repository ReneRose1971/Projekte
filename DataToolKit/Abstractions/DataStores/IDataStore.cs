using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DataToolKit.Abstractions.DataStores
{
    /// <summary>
    /// Minimaler, UI-frameworkneutraler In-Memory-Datenspeicher für Elemente vom Typ <typeparamref name="T"/>.
    /// Stellt eine schreibgeschützte Sicht auf die Elemente bereit und bietet grundlegende Änderungsoperationen.
    /// Persistenz ist ausdrücklich nicht Bestandteil dieser Schnittstelle.
    /// </summary>
    /// <typeparam name="T">Der Typ der zu verwaltenden Elemente.</typeparam>
    public interface IDataStore<T>
    {
        /// <summary>
        /// Schreibgeschützte Sicht auf alle aktuell verwalteten Elemente.
        /// </summary>
        ReadOnlyObservableCollection<T> Items { get; }

        /// <summary>
        /// Anzahl der aktuell gespeicherten Elemente.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Fügt ein Element hinzu, sofern es noch nicht vorhanden ist.
        /// </summary>
        /// <param name="item">Das hinzuzufügende Element.</param>
        /// <returns><c>true</c>, wenn das Element hinzugefügt wurde; andernfalls <c>false</c>.</returns>
        bool Add(T item);

        /// <summary>
        /// Fügt mehrere Elemente hinzu; bereits vorhandene werden übersprungen.
        /// </summary>
        /// <param name="items">Die hinzuzufügenden Elemente.</param>
        /// <returns>Anzahl der tatsächlich hinzugefügten Elemente.</returns>
        int AddRange(IEnumerable<T> items);

        /// <summary>
        /// Entfernt ein Element, sofern vorhanden.
        /// </summary>
        /// <param name="item">Das zu entfernende Element.</param>
        /// <returns><c>true</c>, wenn ein Element entfernt wurde; andernfalls <c>false</c>.</returns>
        bool Remove(T item);

        /// <summary>
        /// Entfernt mehrere Elemente, sofern vorhanden.
        /// </summary>
        /// <param name="items">Die zu entfernenden Elemente.</param>
        /// <returns>Anzahl der entfernten Elemente.</returns>
        int RemoveRange(IEnumerable<T> items);

        /// <summary>
        /// Entfernt alle Elemente, die die angegebene Bedingung erfüllen.
        /// </summary>
        /// <param name="predicate">Bedingung zur Auswahl der zu entfernenden Elemente.</param>
        /// <returns>Anzahl der entfernten Elemente.</returns>
        int RemoveWhere(Predicate<T> predicate);

        /// <summary>
        /// Entfernt alle Elemente.
        /// </summary>
        void Clear();
    }
}
