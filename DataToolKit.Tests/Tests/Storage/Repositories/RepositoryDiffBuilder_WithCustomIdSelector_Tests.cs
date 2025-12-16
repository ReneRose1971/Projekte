using Common.Bootstrap.Defaults;
using DataToolKit.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataToolKit.Tests.Abstractions.Repositories
{
    /// <summary>
    /// Tests für RepositoryDiffBuilder.Build mit benutzerdefiniertem Id-Selektor.
    /// Verwendet eine Pseudo-Entity ohne EntityBase, um Id-Selektoren zu überprüfen.
    /// </summary>
    public sealed class RepositoryDiffBuilder_WithCustomIdSelector_Tests
    {
        private static P P_(int key, string name, int value) => new P { Key = key, Name = name, Value = value };
        private static IReadOnlyList<P> L(params P[] ps) => ps.ToList().AsReadOnly();

        [Fact]
        public void Build_Should_Use_Custom_Id_Selector()
        {
            var existing = L(P_(1, "a", 1), P_(2, "b", 2));
            var incoming = L(P_(1, "a*", 1), P_(3, "c", 3), P_(0, "new", 9)); // Key==0 simuliert „neu"

            var diff = RepositoryDiffBuilder.Build(
                existing,
                incoming,
                getId: e => e.Key,
                comparer: new FallbackEqualsComparer<P>(), // nutzt Equals
                missingAsInsert: true
            );

            Assert.Single(diff.ToUpdate);             // 1 geändert
            Assert.Single(diff.ToDeleteIds);          // 2 fehlt in incoming
            Assert.Equal(2, diff.ToInsert.Count);     // 3 (missing) + 0 (neu)
        }

        [Fact]
        public void Build_Should_Respect_Custom_Comparer_Semantics()
        {
            var existing = L(P_(1, "a", 1));
            var incoming = L(P_(1, "a", 99)); // Id gleich, aber Value unterschiedlich

            // Comparer ignoriert Value, vergleicht nur Name
            var nameOnlyComparer = new NameOnlyComparer();

            var diff = RepositoryDiffBuilder.Build(
                existing, incoming,
                getId: e => e.Key,
                comparer: nameOnlyComparer,
                missingAsInsert: true);

            // Da Name gleich ist, gilt als unverändert
            Assert.Empty(diff.ToUpdate);
            Assert.Empty(diff.ToDeleteIds);
            Assert.Empty(diff.ToInsert);
        }

        [Fact]
        public void Build_Should_Throw_On_Null_Args()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() =>
                RepositoryDiffBuilder.Build<P>(null!, Enumerable.Empty<P>(), e => e.Key, new FallbackEqualsComparer<P>()));
            Assert.Equal("existing", ex1.ParamName);

            var ex2 = Assert.Throws<ArgumentNullException>(() =>
                RepositoryDiffBuilder.Build<P>(Enumerable.Empty<P>(), null!, e => e.Key, new FallbackEqualsComparer<P>()));
            Assert.Equal("incoming", ex2.ParamName);

            var ex3 = Assert.Throws<ArgumentNullException>(() =>
                RepositoryDiffBuilder.Build<P>(Enumerable.Empty<P>(), Enumerable.Empty<P>(), null!, new FallbackEqualsComparer<P>()));
            Assert.Equal("getId", ex3.ParamName);
        }

        [Fact]
        public void Build_Should_Throw_On_Duplicate_Ids()
        {
            var existing = L(P_(1, "a", 1));
            var incoming = L(P_(1, "a", 1), P_(1, "a", 2)); // doppelter Key

            Assert.Throws<ArgumentException>(() =>
                RepositoryDiffBuilder.Build(existing, incoming, e => e.Key, new FallbackEqualsComparer<P>()));
        }

        // --- Hilfstypen für Custom-Selector-Tests ---
        private sealed class P : IEquatable<P>
        {
            public int Key { get; set; }
            public string? Name { get; set; }
            public int Value { get; set; }

            public bool Equals(P? other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Key == other.Key && string.Equals(Name, other.Name, StringComparison.Ordinal) && Value == other.Value;
            }

            public override bool Equals(object? obj) => obj is P p && Equals(p);
            public override int GetHashCode() => HashCode.Combine(Key, Name, Value);
        }

        private sealed class NameOnlyComparer : IEqualityComparer<P>
        {
            public bool Equals(P? x, P? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return string.Equals(x.Name, y.Name, StringComparison.Ordinal);
            }

            public int GetHashCode(P obj) => obj?.Name?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }
    }
}
