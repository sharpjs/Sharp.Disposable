/*
    Copyright 2020 Jeffrey Sharp

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

using System;
using System.Collections.Concurrent;

namespace Sharp.Disposable
{
    // For guidance, see:
    // https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern

    /// <summary>
    ///   A pool that can own multiple disposable objects, disposing them when
    ///   the pool itself is disposed.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Note that a <see cref="DisposablePool"/> references its 'owned'
    ///     objects by weak references.  Because of this, pool ownership of an
    ///     object does not prevent that object from being reclaimed (and thus
    ///     disposed) by the garbage collector.  To prevent garbage collection,
    ///     there must be some other reference to that object.
    ///   </para>
    ///   <para>
    ///     All methods of this class are thread-safe.
    ///   </para>
    /// </remarks>
    public class DisposablePool : Disposable
    {
        private readonly ConcurrentBag<WeakReference<IDisposable>>
            _disposables = new ConcurrentBag<WeakReference<IDisposable>>();

        /// <summary>
        ///   Registers the specified object to be disposed when the pool
        ///   itself is disposed.
        /// </summary>
        /// <typeparam name="T">
        ///   The type of <paramref name="obj"/>.
        /// </typeparam>
        /// <param name="obj">
        ///   The object to be disposed.
        /// </param>
        /// <returns>
        ///   <paramref name="obj"/>, unchanged.
        /// </returns>
        public T AddDisposable<T>(T obj)
            where T : IDisposable
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            RequireNotDisposed();
            _disposables.Add(new WeakReference<IDisposable>(obj));
            return obj;
        }

        /// <inheritdoc/>
        protected override bool Dispose(bool managed)
        {
            // Check if already disposed
            if (!base.Dispose(managed))
                return false;

            // Check if performing an unmanaged disposal
            if (!managed)
                return true;

            // Dispose contained objects
            while (_disposables.TryTake(out var disposable))
            {
                // Check if object was garbage-collected already
                if (!disposable.TryGetTarget(out var obj))
                    continue;

                // Make best effort to dispose, but ignore errors arising from it
                try { obj.Dispose(); } catch { }
            }

            return true;
        }
    }
}
