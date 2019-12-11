/*
    Copyright (C) 2019 Jeffrey Sharp

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
using System.Threading;

namespace Sharp.Disposable
{
    // A complete implementation .NET IDisposable pattern
    // https://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
    //
    // Covers:
    //   - Managed resources (i.e. other IDisposables)
    //   - Unamanged resources (like a temp file)
    //   - Thread-safe prevention of multiple disposes
    //   - IsDisposed property
    //
    // For further guidance, see:
    // https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern

    /// <summary>
    ///   An object that owns managed and/or unmanaged resources needing disposal.
    ///   Provides a reusable implementation of the .NET IDisposable pattern.
    /// </summary>
    /// <remarks>
    ///   All methods of this class are thread-safe.
    /// </remarks>
    public abstract class Disposable : IDisposable
    {
        // Interlocked doesn't support bool, so we fake it with int
        private const int Yes = -1, No = 0;

        private int _isDisposed;

        /// <summary>
        ///   Disposes unmanaged resources owned by the object.
        /// </summary>
        ~Disposable()
        {
            DisposeUnmanaged();
        }

        // Allows tests to simulate finalizer call reliably
        internal void DisposeUnmanaged()
        {
            Dispose(managed: false);
        }

        /// <summary>
        ///   Disposes managed and unmanaged resources owned by the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Disposes resources owned by the object.
        /// </summary>
        /// <param name="managed">
        ///   Whether to dispose managed resources
        ///   (in addition to unmanaged resources, which are always disposed).
        /// </param>
        /// <returns>
        ///   <c>true</c> if the object transitioned from not-disposed to disposed,
        ///   <c>false</c> if the object was disposed already.
        /// </returns>
        protected virtual bool Dispose(bool managed = true)
        {
            // Subclasses override this to dispose their IDisposables.

            return Interlocked.Exchange(ref _isDisposed, Yes) == No;
        }

        /// <summary>
        ///   Gets a value indicating whether the object has been disposed.
        /// </summary>
        public bool IsDisposed => _isDisposed != No;

        /// <summary>
        ///   Throws an exception if the object has been disposed.
        /// </summary>
        protected void RequireNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
