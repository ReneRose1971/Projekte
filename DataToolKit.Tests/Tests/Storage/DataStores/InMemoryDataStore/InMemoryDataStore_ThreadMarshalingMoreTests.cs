using System.Threading;
using DataToolKit.Storage.DataStores;
using TestHelpers;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_ThreadMarshalingMoreTests
    {
        [Fact]
        public void AddRange_Marshals_WhenCurrentDiffers()
        {
            var ctx = new RecordingSynchronizationContext();
            var store = new InMemoryDataStore<string>(context: ctx);

            var t = new Thread(() =>
            {
                using (SynchronizationContextScope.None()) // Current != ctx
                {
                    store.AddRange(new[] { "A", "B" });
                }
            });

            t.Start(); t.Join();

            Assert.Equal(2, store.Count);
            Assert.False(ctx.SendThreadIds.IsEmpty);
        }

        [Fact]
        public void RemoveRange_Marshals_WhenCurrentDiffers()
        {
            var ctx = new RecordingSynchronizationContext();
            var store = new InMemoryDataStore<string>(context: ctx);
            store.AddRange(new[] { "A", "B", "C" });

            var t = new Thread(() =>
            {
                using (SynchronizationContextScope.None())
                {
                    store.RemoveRange(new[] { "A", "X", "C" });
                }
            });

            t.Start(); t.Join();

            Assert.Equal(1, store.Count); // B
            Assert.False(ctx.SendThreadIds.IsEmpty);
        }

        [Fact]
        public void RemoveWhere_Marshals_WhenCurrentDiffers()
        {
            var ctx = new RecordingSynchronizationContext();
            var store = new InMemoryDataStore<int>(context: ctx);
            store.AddRange(new[] { 1, 2, 3, 4, 5 });

            var t = new Thread(() =>
            {
                using (SynchronizationContextScope.None())
                {
                    store.RemoveWhere(x => x % 2 == 0);
                }
            });

            t.Start(); t.Join();

            Assert.Equal(3, store.Count); // 1,3,5
            Assert.False(ctx.SendThreadIds.IsEmpty);
        }

        [Fact]
        public void Clear_Marshals_WhenCurrentDiffers()
        {
            var ctx = new RecordingSynchronizationContext();
            var store = new InMemoryDataStore<string>(context: ctx);
            store.AddRange(new[] { "A", "B" });

            var t = new Thread(() =>
            {
                using (SynchronizationContextScope.None())
                {
                    store.Clear();
                }
            });

            t.Start(); t.Join();

            Assert.Equal(0, store.Count);
            Assert.False(ctx.SendThreadIds.IsEmpty);
        }

        [Fact]
        public void AddRange_Inline_WhenCurrentEqualsContext()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.Use(ctx)) // Current == ctx
            {
                var store = new InMemoryDataStore<string>(context: ctx);

                var added = store.AddRange(new[] { "A", "B" });
                Assert.Equal(2, added);
                Assert.True(ctx.SendThreadIds.IsEmpty); // kein Send
            }
        }

        [Fact]
        public void RemoveRange_Inline_WhenCurrentEqualsContext()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.Use(ctx))
            {
                var store = new InMemoryDataStore<string>(context: ctx);
                store.AddRange(new[] { "A", "B", "C" });

                var removed = store.RemoveRange(new[] { "A", "C" });
                Assert.Equal(2, removed);
                Assert.True(ctx.SendThreadIds.IsEmpty); // kein Send
            }
        }
    }
}
