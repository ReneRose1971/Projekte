using Xunit;
using DataToolKit.Abstractions.Repositories;
using DataToolKit.Storage.DataStores;
using DataToolKit.Tests.Testing;
using DataToolKit.Tests.Common;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class PersistentDataStoreTests
    {
        [Fact]
        public void Add_persistiert_sofort_LiteDB()
        {
            // TestEntity : EntityBase → LiteDB-Repository mit granularen Operationen
            var repo = new FakeRepository<TestEntity>();
            var store = new PersistentDataStore<TestEntity>(repo, trackPropertyChanges: false);
            
            var e = new TestEntity { Name = "N" };
            store.Add(e);
            
            // LiteDB Strategy ruft Write() auf (da kein Insert im Interface)
            Assert.Equal(1, repo.WriteCount);
        }

        [Fact]
        public void Remove_persistiert_sofort_LiteDB()
        {
            var repo = new FakeRepository<TestEntity>();
            var store = new PersistentDataStore<TestEntity>(repo, trackPropertyChanges: false);
            
            var e = new TestEntity { Id = 10, Name = "X" };
            store.Add(e);
            store.Remove(e);
            
            // LiteDB Strategy ruft Delete() auf
            Assert.Equal(1, repo.DeleteCount);
        }

        [Fact]
        public void Clear_ruft_Repo_Clear()
        {
            var repo = new FakeRepository<TestEntity>();
            var store = new PersistentDataStore<TestEntity>(repo, trackPropertyChanges: false);
            
            store.Clear();
            
            Assert.Equal(1, repo.ClearCount);
        }

        [Fact]
        public void Load_füllt_Items()
        {
            var repo = new FakeRepository<TestEntity>();
            repo.SetData(new[] { new TestEntity { Id = 1, Name = "A" }, new TestEntity { Id = 2, Name = "B" } });
            
            var store = new PersistentDataStore<TestEntity>(repo, trackPropertyChanges: false);
            store.Load();  // Muss manuell aufgerufen werden!
            
            Assert.Equal(2, store.Count);
        }

        [Fact]
        public void PropertyChanged_persistiert_sofort_LiteDB()
        {
            var repo = new FakeRepository<TestEntity>();
            var store = new PersistentDataStore<TestEntity>(repo, trackPropertyChanges: true);
            
            var e = new TestEntity { Id = 1, Name = "A" };
            store.Add(e);
            
            var updateCountBefore = repo.UpdateCount;
            e.Name = "B";  // PropertyChanged sollte Update() auslösen
            
            // LiteDB Strategy ruft Update() auf
            Assert.True(repo.UpdateCount > updateCountBefore);
        }
    }
}