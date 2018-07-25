using System;

#if NETFX
using System.Threading;
#elif NETSTANDARD
using System.Threading.Tasks;
#endif

namespace Sharp.Disposable
{
    /// <summary>
    ///   Methods to force object finalization.
    /// </summary>
    public static class Finalizer
    {
        private const int
            DefaultIntervalMs = 50;

        /// <summary>
        ///   Performs an immediate, blocking garbage collection of all
        ///   generations, then waits for the finalizer thread to invoke any
        ///   pending finalizers.
        /// </summary>
        public static void Run()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        ///   Performs blocking garbage collections of all generations at a
        ///   short preset interval, until the specified condition delegate
        ///   returns <c>true</c>.
        /// </summary>
        /// <param name="condition">
        ///   A delegate that returns <c>true</c> when this method should return.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="condition"/> is <c>null</c>.
        /// </exception>
        public static void RunUntil(Func<bool> condition)
        {
            RunUntil(condition, TimeSpan.FromMilliseconds(DefaultIntervalMs));
        }

        /// <summary>
        ///   Performs blocking garbage collections of all generations at the
        ///   given interval, until the specified condition delegate returns
        ///   <c>true</c>.
        /// </summary>
        /// <param name="condition">
        ///   A delegate that returns <c>true</c> when this method should return.
        /// </param>
        /// <param name="interval">
        ///   The interval at which to perform garbage collections.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="condition"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="interval"/> is negative.
        /// </exception>
        public static void RunUntil(Func<bool> condition, TimeSpan interval)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (interval < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(condition));

            Run();

            while (!condition())
            {
#if NETFX
                Thread.Sleep(interval);
#elif NETSTANDARD
                Task.Delay(interval).Wait();
#endif
                Run();
            }
        }
    }
}
