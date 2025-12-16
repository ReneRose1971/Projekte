using System;
using System.Threading;

namespace TestHelpers
{
    /// <summary>
    /// Erlaubt es, temporär einen SynchronizationContext zu setzen oder zu entfernen.
    /// Nutzt das Scope/Disposable-Pattern für saubere Wiederherstellung.
    /// </summary>
    public sealed class SynchronizationContextScope : IDisposable
    {
        private readonly SynchronizationContext? _previous;

        private SynchronizationContextScope(SynchronizationContext? newContext)
        {
            _previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(newContext);
        }

        /// <summary>
        /// Entfernt temporär den aktuellen SynchronizationContext.
        /// </summary>
        public static SynchronizationContextScope None()
            => new SynchronizationContextScope(null);

        /// <summary>
        /// Setzt temporär den angegebenen SynchronizationContext.
        /// </summary>
        public static SynchronizationContextScope Use(SynchronizationContext context)
            => new SynchronizationContextScope(context);

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(_previous);
        }
    }
}
