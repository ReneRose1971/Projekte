using Xunit;
using DataToolKit.Storage.DataStores;
using DataToolKit.Storage.Repositories;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestHelper.TestUtils;

namespace DataToolKit.Tests.Storage.DataStores
{
    /// <summary>
    /// Tests für PersistentDataStore mit POCOs (keine EntityBase) und JSON-Repository.
    /// Stellt sicher, dass PersistentDataStore mit einfachen POCOs funktioniert.
    /// </summary>
    public class PersistentDataStore_POCO_Tests : IDisposable
    {
        private readonly TestDirectorySandbox _sandbox;

        public PersistentDataStore_POCO_Tests()
        {
            _sandbox = new TestDirectorySandbox();
        }

        public void Dispose()
        {
            _sandbox.Dispose();
        }

        [Fact]
        public void POCO_Without_IEntity_Should_Work_With_JsonRepository()
        {
            // Arrange: POCO ohne IEntity/EntityBase
            var options = new JsonStorageOptions<SimplePOCO>(
                appSubFolder: "TestApp",
                fileNameBase: "pocos",
                subFolder: "Data",
                rootFolder: _sandbox.Root);

            var jsonRepo = new JsonRepository<SimplePOCO>(options);
            var store = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);

            // Act: Add
            var poco = new SimplePOCO { Name = "Alice", Value = 42 };
            var added = store.Add(poco);

            // Assert
            Assert.True(added);
            Assert.Equal(1, store.Count);
            Assert.Contains(poco, store.Items);
        }

        [Fact]
        public void POCO_Add_Should_Persist_To_JSON()
        {
            // Arrange
            var options = new JsonStorageOptions<SimplePOCO>(
                appSubFolder: "TestApp",
                fileNameBase: "pocos",
                subFolder: "Data",
                rootFolder: _sandbox.Root);

            var jsonRepo = new JsonRepository<SimplePOCO>(options);
            var store = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);

            // Act: Add und persistieren
            store.Add(new SimplePOCO { Name = "Alice", Value = 42 });
            store.Add(new SimplePOCO { Name = "Bob", Value = 99 });

            // Assert: Neuer Store laden und prüfen
            var store2 = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);
            store2.Load();

            Assert.Equal(2, store2.Count);
            Assert.Contains(store2.Items, p => p.Name == "Alice" && p.Value == 42);
            Assert.Contains(store2.Items, p => p.Name == "Bob" && p.Value == 99);
        }

        [Fact]
        public void POCO_Remove_Should_Persist_To_JSON()
        {
            // Arrange
            var options = new JsonStorageOptions<SimplePOCO>(
                appSubFolder: "TestApp",
                fileNameBase: "pocos",
                subFolder: "Data",
                rootFolder: _sandbox.Root);

            var jsonRepo = new JsonRepository<SimplePOCO>(options);
            var store = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);

            var alice = new SimplePOCO { Name = "Alice", Value = 42 };
            var bob = new SimplePOCO { Name = "Bob", Value = 99 };

            store.Add(alice);
            store.Add(bob);

            // Act: Remove
            var removed = store.Remove(alice);

            // Assert
            Assert.True(removed);
            Assert.Equal(1, store.Count);
            Assert.DoesNotContain(alice, store.Items);
            Assert.Contains(bob, store.Items);

            // Neuer Store: Änderung wurde persistiert
            var store2 = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);
            store2.Load();

            Assert.Equal(1, store2.Count);
            Assert.DoesNotContain(store2.Items, p => p.Name == "Alice");
            Assert.Contains(store2.Items, p => p.Name == "Bob");
        }

        [Fact]
        public void POCO_Clear_Should_Persist_To_JSON()
        {
            // Arrange
            var options = new JsonStorageOptions<SimplePOCO>(
                appSubFolder: "TestApp",
                fileNameBase: "pocos",
                subFolder: "Data",
                rootFolder: _sandbox.Root);

            var jsonRepo = new JsonRepository<SimplePOCO>(options);
            var store = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);

            store.Add(new SimplePOCO { Name = "Alice", Value = 42 });
            store.Add(new SimplePOCO { Name = "Bob", Value = 99 });

            // Act: Clear
            store.Clear();

            // Assert
            Assert.Equal(0, store.Count);

            // Neuer Store: Änderung wurde persistiert
            var store2 = new PersistentDataStore<SimplePOCO>(jsonRepo, trackPropertyChanges: false);
            store2.Load();

            Assert.Equal(0, store2.Count);
        }

        [Fact]
        public void POCO_With_PropertyChanged_Should_Persist_On_Change()
        {
            // Arrange: POCO mit INotifyPropertyChanged
            var options = new JsonStorageOptions<NotifyingPOCO>(
                appSubFolder: "TestApp",
                fileNameBase: "notifying",
                subFolder: "Data",
                rootFolder: _sandbox.Root);

            var jsonRepo = new JsonRepository<NotifyingPOCO>(options);
            var store = new PersistentDataStore<NotifyingPOCO>(jsonRepo, trackPropertyChanges: true);

            var poco = new NotifyingPOCO { Name = "Alice", Value = 42 };
            store.Add(poco);

            // Act: Property ändern
            poco.Name = "Alice Updated";
            poco.Value = 100;

            // Assert: Neuer Store laden - Änderung wurde persistiert
            var store2 = new PersistentDataStore<NotifyingPOCO>(jsonRepo, trackPropertyChanges: false);
            store2.Load();

            Assert.Equal(1, store2.Count);
            var loaded = store2.Items[0];
            Assert.Equal("Alice Updated", loaded.Name);
            Assert.Equal(100, loaded.Value);
        }

        // ===== Test-POCOs =====

        /// <summary>
        /// Einfaches POCO ohne IEntity, ohne INotifyPropertyChanged.
        /// </summary>
        public class SimplePOCO
        {
            public string Name { get; set; } = "";
            public int Value { get; set; }
        }

        /// <summary>
        /// POCO mit INotifyPropertyChanged für PropertyChanged-Tracking.
        /// </summary>
        public class NotifyingPOCO : INotifyPropertyChanged
        {
            private string _name = "";
            private int _value;

            public string Name
            {
                get => _name;
                set
                {
                    if (_name != value)
                    {
                        _name = value;
                        OnPropertyChanged();
                    }
                }
            }

            public int Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnPropertyChanged();
                    }
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
