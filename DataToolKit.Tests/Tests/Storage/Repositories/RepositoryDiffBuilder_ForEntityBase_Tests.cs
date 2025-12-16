using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für RepositoryDiffBuilder.BuildForEntityBase (Id-Selektor = e => e.Id).
    /// </summary>
    public sealed class RepositoryDiffBuilder_ForEntityBase_Tests
    {
        // --- Testdaten-Helfer ---
        private static TestEntity E(int id, string name, int value) => new TestEntity { Id = id, Name = name, Value = value };
        private static IReadOnlyList<TestEntity> L(params TestEntity[] es) => es.ToList().AsReadOnly();

        // -------- 1) Leere & triviale Fälle --------

        [Fact]
        public void BuildForEntityBase_Should_Return_Empty_Diff_When_Both_Sides_Are_Empty()
        {
            var existing = L();
            var incoming = L();

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        [Fact]
        public void BuildForEntityBase_Should_Insert_All_When_Existing_Empty_And_Incoming_All_New_Id0()
        {
            var existing = L();
            var incoming = L(E(0, "a", 1), E(0, "b", 2));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Equal(2, diff.ToInsert.Count);
        }

        [Fact]
        public void BuildForEntityBase_Should_Handle_Incoming_With_IdGreater0_When_Existing_Empty_MissingAsInsert_True()
        {
            var existing = L();
            var incoming = L(E(10, "a", 1), E(11, "b", 2));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, comparer: null, missingAsInsert: true);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Equal(2, diff.ToInsert.Count); // Missing-IDs werden als Insert behandelt
        }

        [Fact]
        public void BuildForEntityBase_Should_Ignore_Incoming_With_IdGreater0_When_Existing_Empty_MissingAsInsert_False()
        {
            var existing = L();
            var incoming = L(E(10, "a", 1), E(11, "b", 2));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, comparer: null, missingAsInsert: false);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert); // ohne Missing-Insert-Policy
        }

        [Fact]
        public void BuildForEntityBase_Should_Delete_All_When_Incoming_Empty_And_Existing_NotEmpty()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2));
            var incoming = L();

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Equal(new[] { 1, 2 }, diff.ToDeleteIds.OrderBy(x => x));
            Assert.Empty(diff.ToInsert);
        }

        // -------- 2) Unverändert vs. verändert --------

        [Fact]
        public void BuildForEntityBase_Should_Do_Nothing_When_Same_Content()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2));
            var incoming = L(E(1, "a", 1), E(2, "b", 2));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        [Fact]
        public void BuildForEntityBase_Should_Update_When_Content_Changed_For_Same_Id()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2));
            var incoming = L(E(1, "a*", 1), E(2, "b", 2));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, comparer: new FallbackEqualsComparer<TestEntity>());

            Assert.Single(diff.ToUpdate);
            Assert.Equal(1, diff.ToUpdate[0].Id);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        [Fact]
        public void BuildForEntityBase_Should_Update_Multiple_When_Many_Changed()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2), E(3, "c", 3));
            var incoming = L(E(1, "a*", 1), E(2, "b*", 22), E(3, "c", 3));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Equal(new[] { 1, 2 }, diff.ToUpdate.Select(x => x.Id).OrderBy(x => x));
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        // -------- 3) Nur Inserts / Nur Deletes --------

        [Fact]
        public void BuildForEntityBase_Should_Insert_Id0_Only()
        {
            var existing = L(E(1, "a", 1));
            var incoming = L(E(1, "a", 1), E(0, "new", 9));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Single(diff.ToInsert);
            Assert.Equal("new", ((TestEntity)diff.ToInsert[0]).Name);
        }

        [Fact]
        public void BuildForEntityBase_Should_Delete_Missing_From_Incoming()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2), E(3, "c", 3));
            var incoming = L(E(1, "a", 1)); // 2 & 3 fehlen

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            Assert.Empty(diff.ToUpdate);
            Assert.Equal(new[] { 2, 3 }, diff.ToDeleteIds.OrderBy(x => x));
            Assert.Empty(diff.ToInsert);
        }

        // -------- 4) Gemischte Deltas inkl. Missing-IDs-Policy --------

        [Fact]
        public void BuildForEntityBase_Should_Produce_Mixed_Delta_With_MissingIds_As_Insert()
        {
            var existing = L(E(1, "a", 1), E(2, "b", 2), E(3, "c", 3));
            var incoming = L(
                E(1, "a*", 1),     // changed -> Update
                E(2, "b", 2),      // same -> no-op
                E(0, "new", 9),    // new -> Insert
                E(5, "x", 50));    // Id>0, not in DB -> Insert (missingIds-Policy)

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, comparer: null, missingAsInsert: true);

            Assert.Equal(new[] { 1 }, diff.ToUpdate.Select(x => x.Id));
            Assert.Equal(new[] { 3 }, diff.ToDeleteIds);                    // 3 fehlt im incoming
            Assert.Equal(new[] { 0, 5 }, diff.ToInsert.Select(x => x.Id));  // 0(neu), 5(missing)
        }

        [Fact]
        public void BuildForEntityBase_Should_Not_Insert_MissingIds_When_Policy_False()
        {
            var existing = L(E(1, "a", 1));
            var incoming = L(E(1, "a", 1), E(9, "xx", 99)); // 9 existiert nicht in DB

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming, comparer: null, missingAsInsert: false);

            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert); // 9 wird NICHT als Insert behandelt
        }

        // -------- 5) Defensive/Null/Duplikate/Negative Ids --------

        [Fact]
        public void BuildForEntityBase_Should_Throw_On_Null_Existing()
        {
            Assert.Throws<ArgumentNullException>(() =>
                RepositoryDiffBuilder.BuildForEntityBase(existing: null!, incoming: L()));
        }

        [Fact]
        public void BuildForEntityBase_Should_Throw_On_Null_Incoming()
        {
            Assert.Throws<ArgumentNullException>(() =>
                RepositoryDiffBuilder.BuildForEntityBase(existing: L(), incoming: null!));
        }

        [Fact]
        public void BuildForEntityBase_Should_Handle_Duplicate_Incoming_Ids_By_Exception()
        {
            var existing = L(E(1, "a", 1));
            var incoming = L(E(1, "a", 1), E(1, "a*", 1)); // doppelte Id 1

            // ToDictionary(getId) wirft ArgumentException
            Assert.Throws<ArgumentException>(() =>
                RepositoryDiffBuilder.BuildForEntityBase(existing, incoming));
        }

        [Fact]
        public void BuildForEntityBase_Should_Ignore_Negative_Ids_On_Both_Sides()
        {
            var existing = L(E(-1, "neg", 1), E(1, "a", 1));
            var incoming = L(E(-5, "neg2", 2), E(1, "a", 1));

            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            // -1 & -5 werden weder upgedatet noch gelöscht/insertet
            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        // -------- 6) Determinismus / Reihenfolge-Unabhängigkeit / Unveränderlichkeit --------

        [Fact]
        public void BuildForEntityBase_Should_Be_Deterministic_And_Order_Independent()
        {
            var existingA = L(E(1, "a", 1), E(2, "b", 2), E(3, "c", 3));
            var incomingA = L(E(1, "a", 1), E(3, "c", 3), E(0, "new", 9));

            var existingB = L(E(3, "c", 3), E(1, "a", 1), E(2, "b", 2)); // permutiert
            var incomingB = L(E(0, "new", 9), E(3, "c", 3), E(1, "a", 1)); // permutiert

            var diffA = RepositoryDiffBuilder.BuildForEntityBase(existingA, incomingA);
            var diffB = RepositoryDiffBuilder.BuildForEntityBase(existingB, incomingB);

            Assert.Equal(diffA.ToUpdate.Select(x => x.Id).OrderBy(x => x), diffB.ToUpdate.Select(x => x.Id).OrderBy(x => x));
            Assert.Equal(diffA.ToDeleteIds.OrderBy(x => x), diffB.ToDeleteIds.OrderBy(x => x));
            Assert.Equal(diffA.ToInsert.Select(x => x.Id).OrderBy(x => x), diffB.ToInsert.Select(x => x.Id).OrderBy(x => x));
        }

        [Fact]
        public void BuildForEntityBase_Should_Not_Be_Affected_By_Mutating_Input_After_Build()
        {
            // Arrange
            var existing = new List<TestEntity> { E(1, "a", 1) };
            var incoming = new List<TestEntity> { E(0, "new", 9) };

            // Act: Diff erzeugen (Baseline)
            var diff = RepositoryDiffBuilder.BuildForEntityBase(existing, incoming);

            // Baseline festhalten
            var beforeUpdateIds = diff.ToUpdate.Select(x => x.Id).OrderBy(x => x).ToArray();
            var beforeDeleteIds = diff.ToDeleteIds.OrderBy(x => x).ToArray();
            var beforeInsertIds = diff.ToInsert.OfType<TestEntity>().Select(x => x.Id).OrderBy(x => x).ToArray();

            // Mutationen an den Eingaben NACH dem Build
            existing.Add(E(2, "x", 2));
            incoming.Clear();

            // Assert: diff bleibt unverändert (gegen Baseline prüfen)
            Assert.Equal(beforeUpdateIds, diff.ToUpdate.Select(x => x.Id).OrderBy(x => x).ToArray());
            Assert.Equal(beforeDeleteIds, diff.ToDeleteIds.OrderBy(x => x).ToArray());
            Assert.Equal(beforeInsertIds, diff.ToInsert.OfType<TestEntity>().Select(x => x.Id).OrderBy(x => x).ToArray());
        }


        // --- Testtyp: EntityBase + Equals (vergleicht Id UND Felder) ---
        private sealed class TestEntity : EntityBase, IEquatable<TestEntity>
        {
            public string? Name { get; set; }
            public int Value { get; set; }

            public bool Equals(TestEntity? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id && string.Equals(Name, other.Name, StringComparison.Ordinal) && Value == other.Value;
            }

            public override bool Equals(object? obj) => obj is TestEntity te && Equals(te);
            public override int GetHashCode() => HashCode.Combine(Id, Name, Value);
            public override string ToString() => $"#{Id}:{Name}:{Value}";
        }
    }
}
