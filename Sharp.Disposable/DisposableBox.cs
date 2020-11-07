/*
    Copyright (C) 2020 Jeffrey Sharp

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

namespace Sharp.Disposable
{
    /// <summary>
    ///   A generic, mutable box that can hold a disposable object by
    ///   reference, with optional ownership.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of object held in the box.
    /// </typeparam>
    public class DisposableBox<T> : Disposable
        where T : class, IDisposable
    {
        private T    _obj;
        private bool _owned;

        /// <summary>
        ///   Initializes a new instance of <see cref="DisposableBox{T}"/>
        ///   holding a <c>null</c> reference.
        /// </summary>
        public DisposableBox() { }

        /// <summary>
        ///   Initializes a new instance of <see cref="DisposableBox{T}"/>
        ///   holding the specified object with the specified ownership.
        /// </summary>
        /// <param name="obj">
        ///   The object to be held in the box.
        /// </param>
        /// <param name="owned">
        ///   <c>true</c> if the box will own <paramref name="obj"/> and will
        ///     be responsible for its disposal;
        ///   <c>false</c> if some other object owns <paramref name="obj"/>
        ///     and is responsible for its disposal.
        /// </param>
        public DisposableBox(T obj, bool owned = true)
        {
            Set(obj, owned);
        }

        /// <summary>
        ///   Gets the object held in the box.
        /// </summary>
        public T Object
        {
            get { RequireNotDisposed(); return _obj; }
        }

        /// <summary>
        ///   Gets whether the object held in the box is owned by the box.  If
        ///   <c>true</c>, the object is owned by the box and will be disposed
        ///   when the box itself is disposed.
        /// </summary>
        public bool IsOwned
        {
            get { RequireNotDisposed(); return _owned; }
        }

        /// <summary>
        ///   Sets the object held in the box and the object's ownership.  If
        ///   the box currenty owns a different object, that object will be
        ///   disposed.
        /// </summary>
        /// <param name="obj">
        ///   The object to be held in the box.
        /// </param>
        /// <param name="owned">
        ///   <c>true</c> if the box will own <paramref name="obj"/> and will
        ///     be responsible for its disposal;
        ///   <c>false</c> if some other object owns <paramref name="obj"/>
        ///     and is responsible for its disposal.
        /// </param>
        /// <returns>
        ///   The <paramref name="obj"/> argument.
        /// </returns>
        public T Set(T obj, bool owned = true)
        {
            RequireNotDisposed();

            var oldObj   = _obj;
            var oldOwned = _owned;

            _obj   = obj;
            _owned = owned && obj != null;

            if (oldOwned && oldObj != obj)
                oldObj.Dispose();

            return obj;
        }

        /// <summary>
        ///   Removes the held object, if any, from the box.  If the object is
        ///   owned by the box, the object will be disposed.
        /// </summary>
        public void Clear()
        {
            Set(null, owned: false);
        }

        /// <summary>
        ///   Removes the held object, if any, from the box.  If the object is
        ///   owned by the box, the caller assumes ownership of the object and
        ///   becomes responsible for the object's disposal.
        /// </summary>
        public T Take()
        {
            RequireNotDisposed();

            var obj = _obj;
            _obj    = null;
            _owned  = false;
            return obj;
        }

        /// <inheritdoc/>
        protected override bool Dispose(bool managed = true)
        {
            // Check if already disposed
            if (!base.Dispose(managed))
                return false;

            // Dispose held object if necessary
            if (managed && _owned)
                _obj.Dispose();

            // Clear
            _obj   = null;
            _owned = false;
            return true;
        }
    }
}
