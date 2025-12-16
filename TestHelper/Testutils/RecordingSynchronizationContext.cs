using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TestHelpers
{
    /// <summary>
    /// SynchronizationContext für Tests:
    /// - Protokolliert Send/Post-Aufrufe und Thread-IDs.
    /// - Bietet ein bequemes <see cref="Use"/>-Pattern, um ihn temporär als Current zu setzen.
    /// </summary>
    public sealed class RecordingSynchronizationContext : SynchronizationContext
    {
        public ConcurrentQueue<int> SendThreadIds { get; } = new();
        public int PostCount { get; private set; }

        public override void Send(SendOrPostCallback d, object? state)
        {
            SendThreadIds.Enqueue(Environment.CurrentManagedThreadId);
            d(state);
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            PostCount++;
            base.Post(d, state);
        }

        /// <summary>Setzt diesen Context als Current und gibt ein Disposable zurück, das beim Dispose den alten Context wiederherstellt.</summary>
        public IDisposable Use()
        {
            var prev = Current;
            SetSynchronizationContext(this);
            return new RevertDisposable(prev);
        }

        /// <summary>Setzt Zähler/Queues zurück.</summary>
        public void Reset()
        {
            while (SendThreadIds.TryDequeue(out _)) { }
            PostCount = 0;
        }

        private sealed class RevertDisposable : IDisposable
        {
            private readonly SynchronizationContext? _prev;
            private bool _disposed;

            public RevertDisposable(SynchronizationContext? prev) => _prev = prev;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                SetSynchronizationContext(_prev);
            }
        }
    }
}
