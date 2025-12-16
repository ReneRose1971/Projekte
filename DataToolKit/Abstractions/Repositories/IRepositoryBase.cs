using System;
using System.Collections.Generic;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Minimaler Basiskontrakt für Repositories mit atomaren Volloperationen:
    /// Laden, Schreiben und Leeren der gesamten Collection.
    /// </summary>
    /// <typeparam name="T">Der Entitätstyp, den dieses Repository verwaltet.</typeparam>
    /// <remarks>
    /// <para>
    /// <b>Atomarität:</b> Alle Operationen (<see cref="Load"/>, <see cref="Write"/>, <see cref="Clear"/>)
    /// sind atomar, d.h. sie werden vollständig oder gar nicht ausgeführt.
    /// </para>
    /// <para>
    /// <b>Thread-Safety:</b> Implementierungen sollten Thread-Safe sein. Konkurrierender Zugriff
    /// auf <see cref="Load"/> ist typischerweise sicher, aber gleichzeitige <see cref="Write"/>
    /// oder <see cref="Clear"/> Operationen sollten durch Lock-Mechanismen geschützt werden.
    /// </para>
    /// <para>
    /// <b>IDisposable:</b> Repository-Implementierungen können Ressourcen wie Datenbankverbindungen
    /// oder Dateizugriffe halten. Rufen Sie <see cref="IDisposable.Dispose"/> auf, wenn das Repository
    /// nicht mehr benötigt wird, oder nutzen Sie <c>using</c>-Blöcke.
    /// </para>
    /// </remarks>
    public interface IRepositoryBase<T> : IDisposable
    {
        /// <summary>
        /// Lädt die vollständige Collection aus dem Speicher atomar.
        /// </summary>
        /// <returns>
        /// Eine schreibgeschützte Liste aller Entitäten. Bei leerem Repository wird eine leere Liste zurückgegeben.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Wenn der Speicherpfad ungültig ist oder das Repository nicht initialisiert wurde.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// Wenn ein Dateisystem-Fehler beim Lesen auftritt (z.B. bei JSON-Repositories).
        /// </exception>
        /// <remarks>
        /// Die zurückgegebene Liste ist read-only (<see cref="IReadOnlyList{T}"/>). Änderungen müssen
        /// über <see cref="Write"/> persistiert werden.
        /// </remarks>
        IReadOnlyList<T> Load();

        /// <summary>
        /// Schreibt die vollständige Collection atomar in den Speicher.
        /// Ersetzt alle bestehenden Daten vollständig.
        /// </summary>
        /// <param name="items">Die zu persistierende Collection von Entitäten.</param>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="items"/> <c>null</c> ist.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Wenn die Collection <c>null</c>-Elemente enthält.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Wenn der Speicherpfad ungültig ist oder die Transaktion fehlschlägt (bei LiteDB).
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// Wenn ein Dateisystem-Fehler beim Schreiben auftritt.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Wichtig:</b> Diese Operation ersetzt <b>alle</b> bestehenden Daten. Nicht in der
        /// übergebenen Collection enthaltene Entitäten werden gelöscht.
        /// </para>
        /// <para>
        /// <b>Atomarität:</b> Implementierungen garantieren atomares Schreiben (z.B. via
        /// temporäre Dateien bei JSON oder Transaktionen bei LiteDB).
        /// </para>
        /// </remarks>
        void Write(IEnumerable<T> items);

        /// <summary>
        /// Leert das Repository vollständig und atomar (löscht alle Entitäten).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Wenn das Repository nicht initialisiert ist oder die Operation fehlschlägt.
        /// </exception>
        /// <remarks>
        /// Diese Operation ist äquivalent zu <c>Write(Array.Empty&lt;T&gt;())</c>.
        /// </remarks>
        void Clear();
    }
}
