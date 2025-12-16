using System;
using System.ComponentModel;
using LiteDB;
using PropertyChanged;

namespace DataToolKit.Abstractions.Repositories
{
    /// <summary>
    /// Abstrakte Basisklasse f?r Entit?ten, die <see cref="IEntity"/> implementieren.
    /// Fody.PropertyChanged (OptIn) wird verwendet; die Klasse ist daher mit
    /// <see cref="AddINotifyPropertyChangedInterfaceAttribute"/> ausgezeichnet und erh?lt
    /// automatisch die Implementierung von <see cref="System.ComponentModel.INotifyPropertyChanged"/>.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public abstract class EntityBase : IEntity, INotifyPropertyChanged
    {
        private int _id;

        /// <summary>
        /// Eindeutige Identit?t der Entit?t (Integer).
        /// Die ID wird von LiteDB automatisch beim ersten Einf?gen in eine <c>ILiteCollection</c> gesetzt.
        /// Eine neu erzeugte Entit?t muss vor dem Einf?gen den Wert <c>0</c> haben.
        /// 
        /// Hinweis: ?nderungen dieser <c>Id</c>-Eigenschaft sollten **keine** <c>PropertyChanged</c>-Benachrichtigung
        /// ausl?sen. Die Fody-Annotation [DoNotNotify] verhindert das Generieren von Benachrichtigungen f?r dieses Feld.
        /// </summary>
        [BsonId]
        [DoNotNotify]
        public int Id
        {
            get => _id;
            set => _id = value;
        }

        // Provide the event to satisfy the compiler. Fody will weave invocations where appropriate.
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Liefert eine string-Repr?sentation der Entit?t (Typname + Id).
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}{{Id={Id}}}";
        }

        /// <summary>
        /// Vergleicht zwei Entit?ten anhand ihrer Id.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not EntityBase other) return false;
            // gleiche Instanz oder gleiche Id
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        /// <summary>
        /// HashCode basierend auf Id.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
