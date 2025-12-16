using System;
using System.Threading;
using DataToolKit.Storage.DataStores;
using TestHelpers;
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class InMemoryDataStore_SynchronizationContextTests
    {
        [Fact]
        public void Ctor_UsesProvidedSynchronizationContext_CurrentEqualsContext_NoSend()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.Use(ctx))
            {
                var store = new InMemoryDataStore<string>(context: ctx);

                var added = store.Add("A");
                Assert.True(added);

                // Weil Current == ctx: kein Send
                Assert.True(ctx.SendThreadIds.IsEmpty);
            }
        }

        [Fact]
        public void CrossThread_Add_MarshalsToCtorContext()
        {
            var ctx = new RecordingSynchronizationContext();
            var store = new InMemoryDataStore<string>(context: ctx);

            Exception? ex = null;
            var t = new Thread(() =>
            {
                // kein Current-Kontext im Worker-Thread setzen → erzwingt Marshaling
                using (SynchronizationContextScope.None())
                {
                    try { store.Add("A"); }
                    catch (Exception e) { ex = e; }
                }
            });

            t.Start();
            t.Join();

            Assert.Null(ex);
            Assert.False(ctx.SendThreadIds.IsEmpty); // es gab einen Send
            Assert.Equal(1, store.Count);
        }

        [Fact]
        public void NullSynchronizationContext_NoMarshaling()
        {
            // explizit kein Context: Operationen laufen inline
            using (SynchronizationContextScope.None())
            {
                var store = new InMemoryDataStore<string>(context: null);

                var added = store.Add("A");
                Assert.True(added);
                Assert.Equal(1, store.Count);
            }
        }
    }
}
