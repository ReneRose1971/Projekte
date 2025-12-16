using System;
using System.ComponentModel;
using DataToolKit.Abstractions.Repositories;
using LiteDB;
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für <see cref="EntityBase"/>.
    /// Ziel: Jede öffentlich sichtbare Semantik präzise prüfen (ein Test je Aspekt).
    /// </summary>
    public sealed class EntityBaseTests
    {
        // Zwei einfache abgeleitete Typen, um Typhierarchien/Polymorphie zu prüfen.
        private sealed class DummyEntityA : EntityBase { }
        private sealed class DummyEntityB : EntityBase { }

        /// <summary>
        /// Gleiche Referenz -> Equals muss true liefern.
        /// </summary>
        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            var a = new DummyEntityA { Id = 5 };
            Assert.True(a.Equals(a));
        }

        /// <summary>
        /// Unterschiedliche Instanzen, gleiche Id -> Equals muss true liefern (Id-basierte Gleichheit).
        /// </summary>
        [Fact]
        public void Equals_SameIdDifferentInstances_ReturnsTrue()
        {
            var a = new DummyEntityA { Id = 42 };
            var b = new DummyEntityA { Id = 42 };

            Assert.True(a.Equals(b));
            Assert.True(b.Equals(a)); // Symmetrie
        }

        /// <summary>
        /// Unterschiedliche Id -> Equals muss false liefern.
        /// </summary>
        [Fact]
        public void Equals_DifferentId_ReturnsFalse()
        {
            var a = new DummyEntityA { Id = 1 };
            var b = new DummyEntityA { Id = 2 };

            Assert.False(a.Equals(b));
        }

        /// <summary>
        /// Unterschiedliche abgeleitete Typen mit gleicher Id -> Equals muss true liefern
        /// (Vergleich ist (EntityBase)-weit, nicht typgenau).
        /// </summary>
        [Fact]
        public void Equals_DifferentDerivedTypesWithSameId_ReturnsTrue()
        {
            var a = new DummyEntityA { Id = 7 };
            var b = new DummyEntityB { Id = 7 };

            Assert.True(a.Equals(b));
            Assert.True(b.Equals(a));
        }

        /// <summary>
        /// Vergleich mit null -> false.
        /// </summary>
        [Fact]
        public void Equals_Null_ReturnsFalse()
        {
            var a = new DummyEntityA { Id = 3 };
            Assert.False(a.Equals(null));
        }

        /// <summary>
        /// Vergleich mit Objekt anderen Typs -> false.
        /// </summary>
        [Fact]
        public void Equals_OtherType_ReturnsFalse()
        {
            var a = new DummyEntityA { Id = 3 };
            Assert.False(a.Equals("not an entity"));
        }

        /// <summary>
        /// Transitivität: Wenn a==b und b==c (gleiche Id), dann a==c.
        /// </summary>
        [Fact]
        public void Equals_Transitivity_HoldsForSameId()
        {
            var a = new DummyEntityA { Id = 9 };
            var b = new DummyEntityA { Id = 9 };
            var c = new DummyEntityA { Id = 9 };

            Assert.True(a.Equals(b));
            Assert.True(b.Equals(c));
            Assert.True(a.Equals(c));
        }

        /// <summary>
        /// HashCode-Gleichheit bei gleicher Id.
        /// </summary>
        [Fact]
        public void GetHashCode_SameId_SameHash()
        {
            var a = new DummyEntityA { Id = 100 };
            var b = new DummyEntityA { Id = 100 };

            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        /// <summary>
        /// HashCode-Unterschied bei unterschiedlichen Ids (keine harte Garantie, hier aber erwartbar).
        /// </summary>
        [Fact]
        public void GetHashCode_DifferentId_UsuallyDifferent()
        {
            var a = new DummyEntityA { Id = 101 };
            var b = new DummyEntityA { Id = 102 };

            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
        }

        /// <summary>
        /// ToString enthält Typnamen und Id exakt im Format: "Typname{Id=<n>}".
        /// </summary>
        [Fact]
        public void ToString_ContainsTypeNameAndId()
        {
            var a = new DummyEntityA { Id = 42 };
            Assert.Equal("DummyEntityA{Id=42}", a.ToString());

            var b = new DummyEntityB { Id = 5 };
            Assert.Equal("DummyEntityB{Id=5}", b.ToString());
        }

        /// <summary>
        /// Default-Id (0) führt nach aktueller Implementierung dazu, dass zwei frische Entitäten als gleich gelten.
        /// (Das ist eine wichtige, bewusste Semantik-Entscheidung.)
        /// </summary>
        [Fact]
        public void Equals_DefaultIdZero_TwoFreshInstancesAreEqual()
        {
            var a = new DummyEntityA(); // Id = 0
            var b = new DummyEntityA(); // Id = 0

            Assert.True(a.Equals(b));
            Assert.Equal(a.GetHashCode(), b.GetHashCode()); // beide Hash = 0
        }

        /// <summary>
        /// Setzen der Id löst KEIN PropertyChanged aus (wegen [DoNotNotify] auf Id in EntityBase).
        /// </summary>
        [Fact]
        public void PropertyChanged_Id_SetDoesNotRaiseEvent()
        {
            var a = new DummyEntityA();
            int events = 0;

            // Event abonnieren (EntityBase implementiert INotifyPropertyChanged via Fody).
            ((INotifyPropertyChanged)a).PropertyChanged += (_, __) => events++;

            a.Id = 1;
            a.Id = 2;

            Assert.Equal(0, events);
        }

        /// <summary>
        /// Die Id-Eigenschaft ist mit [BsonId] markiert (LiteDB-Schlüssel).
        /// </summary>
        [Fact]
        public void Attributes_Id_HasBsonId()
        {
            var prop = typeof(EntityBase).GetProperty(nameof(EntityBase.Id));
            Assert.NotNull(prop);

            var attr = Attribute.GetCustomAttribute(prop!, typeof(BsonIdAttribute));
            Assert.NotNull(attr);
        }

        /// <summary>
        /// EntityBase implementiert das IEntity-Interface (vertragliche Zusicherung der Id-Eigenschaft).
        /// </summary>
        [Fact]
        public void Interface_ImplementsIEntity()
        {
            Assert.True(typeof(IEntity).IsAssignableFrom(typeof(EntityBase)));
            Assert.True(typeof(IEntity).IsAssignableFrom(typeof(DummyEntityA)));
        }
    }
}
