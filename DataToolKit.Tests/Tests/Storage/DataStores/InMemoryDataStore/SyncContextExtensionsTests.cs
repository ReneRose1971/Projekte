using DataToolKit.Abstractions; // SyncContextExtensions
using DataToolKit.Abstractions.DataStores;
using System;
using System.Threading;
using TestHelpers;              // RecordingSynchronizationContext, SynchronizationContextScope
using Xunit;

namespace DataToolKit.Tests.Storage.DataStores
{
    public class SyncContextExtensionsTests
    {
        // --------------------------------------------------------------------
        // Invoke(Action)
        // --------------------------------------------------------------------

        [Fact]
        public void Invoke_Action_NoContext_RunsInline()
        {
            using (SynchronizationContextScope.None())
            {
                int value = 0;
                SynchronizationContext? ctx = null;

                ctx.Invoke(() => value = 42);

                Assert.Equal(42, value);
            }
        }

        [Fact]
        public void Invoke_Action_CurrentEqualsContext_RunsInline_NoSend()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.Use(ctx))
            {
                int value = 0;

                ctx.Invoke(() => value = 99);

                Assert.Equal(99, value);
                Assert.True(ctx.SendThreadIds.IsEmpty); // kein Send, da Current == ctx
            }
        }

        [Fact]
        public void Invoke_Action_CurrentDiffersFromContext_UsesSend()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.None()) // Current != ctx
            {
                int value = 0;

                ctx.Invoke(() => value = 5);

                Assert.Equal(5, value);
                Assert.False(ctx.SendThreadIds.IsEmpty); // Send wurde ausgeführt
            }
        }

        [Fact]
        public void Invoke_Action_PropagatesExceptions()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.None())
            {
                Assert.Throws<InvalidOperationException>(() =>
                    ctx.Invoke(() => throw new InvalidOperationException("Test")));
            }
        }

        // --------------------------------------------------------------------
        // Invoke<TResult>
        // --------------------------------------------------------------------

        [Fact]
        public void Invoke_Func_NoContext_RunsInline_ReturnsValue()
        {
            using (SynchronizationContextScope.None())
            {
                SynchronizationContext? ctx = null;

                int result = ctx.Invoke(() => 123);

                Assert.Equal(123, result);
            }
        }

        [Fact]
        public void Invoke_Func_CurrentEqualsContext_RunsInline_NoSend()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.Use(ctx))
            {
                var result = ctx.Invoke(() => 777);

                Assert.Equal(777, result);
                Assert.True(ctx.SendThreadIds.IsEmpty);
            }
        }

        [Fact]
        public void Invoke_Func_CurrentDiffersFromContext_UsesSend()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.None())
            {
                var result = ctx.Invoke(() => 888);

                Assert.Equal(888, result);
                Assert.False(ctx.SendThreadIds.IsEmpty);
            }
        }

        [Fact]
        public void Invoke_Func_PropagatesExceptions()
        {
            var ctx = new RecordingSynchronizationContext();

            using (SynchronizationContextScope.None())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    ctx.Invoke<int>(() => throw new ArgumentNullException("x")));
            }
        }
    }
}
