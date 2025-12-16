using System;
using System.Threading;
using System.Runtime.ExceptionServices;

namespace DataToolKit.Abstractions.DataStores
{
    /// <summary>
    /// Erweiterungen für <see cref="SynchronizationContext"/>, um synchrones Marshaling
    /// (mit Ergebnis) bequem und testbar auszulagern.
    /// </summary>
    public static class SyncContextExtensions
    {
        /// <summary>
        /// Führt <paramref name="action"/> auf dem angegebenen <paramref name="ctx"/> aus,
        /// sofern nötig mittels <see cref="SynchronizationContext.Send"/>.
        /// Ist <paramref name="ctx"/> <c>null</c> oder entspricht dem aktuellen Context,
        /// wird direkt ausgeführt.
        /// </summary>
        public static void Invoke(this SynchronizationContext? ctx, Action action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            // Kein Context -> direkt ausführen
            if (ctx is null)
            {
                action();
                return;
            }

            // Bereits "auf" diesem Context?
            if (SynchronizationContext.Current == ctx)
            {
                action();
                return;
            }

            ctx.Send(_ =>
            {
                try { action(); }
                catch (Exception ex) { ExceptionDispatchInfo.Capture(ex).Throw(); }
            }, null);
        }

        /// <summary>
        /// Führt <paramref name="func"/> auf dem angegebenen <paramref name="ctx"/> aus
        /// und gibt das Ergebnis zurück. Siehe <see cref="Invoke(SynchronizationContext?, Action)"/>.
        /// </summary>
        public static TResult Invoke<TResult>(this SynchronizationContext? ctx, Func<TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));

            if (ctx is null) return func();

            if (SynchronizationContext.Current == ctx) return func();

            TResult? result = default;

            ctx.Send(_ =>
            {
                try { result = func(); }
                catch (Exception ex) { ExceptionDispatchInfo.Capture(ex).Throw(); }
            }, null);

            return result!;
        }
    }
}
