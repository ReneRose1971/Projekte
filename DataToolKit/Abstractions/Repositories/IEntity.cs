using System.ComponentModel;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Schnittstelle für persistierbare Entitäten mit einer integer Identität.
    /// Implementierungen stellen mindestens die Eigenschaft <see cref="Id"/> bereit.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Eindeutige Identität der Entität (Integer).
        /// Die ID wird von LiteDB automatisch beim ersten Einfügen in eine <c>ILiteCollection</c> gesetzt.
        /// Eine neu erzeugte Entität muss vor dem Einfügen den Wert <c>0</c> haben.
        /// 
        /// Hinweis: Änderungen dieser <c>Id</c>-Eigenschaft sollten **keine** <c>PropertyChanged</c>-Benachrichtigung
        /// auslösen. Implementierungen (z. B. in `EntityBase`) sollten das Setzen von <c>Id</c> still durchführen
        /// oder spezielle Logik verwenden, damit Konsumenten nicht auf Id-Änderungen reagieren müssen.
        /// </summary>
        int Id { get; set; }
    }
}
