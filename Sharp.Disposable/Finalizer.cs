/*
    Copyright 2022 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/


#if NETFRAMEWORK
using System.Threading;
#elif NETSTANDARD
using System.Threading.Tasks;
#endif

namespace Sharp.Disposable;

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
#if NETFRAMEWORK
            Thread.Sleep(interval);
#elif NETSTANDARD
            Task.Delay(interval).Wait();
#endif
            Run();
        }
    }
}
