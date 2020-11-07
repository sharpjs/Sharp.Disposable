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
    ///   A generic, mutable box that can hold a disposable object by nullable
    ///   reference, with optional ownership.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of object held in the box.
    /// </typeparam>
    public class DisposableBox<T> : Disposable
        where T : class, IDisposable
    {
        private T?   _obj;
        private bool _owned;

        /// <summary>
        ///   Initializes a new instance of <see cref="DisposableBox{T}"/>
        ///   holding a <c>null</c> reference.
        /// </summary>
        public DisposableBox() { }

        /// <summary>
        ///   Initializes a new instance of <see cref="DisposableBox{T}"/>
        ///   holding the specified reference with the specified ownership.
        /// </summary>
        /// <param name="obj">
        ///   The object or <c>null</c> reference to be held in the box.
        /// </param>
        /// <param name="owned">
        ///   <c>true</c> if the box will own <paramref name="obj"/> and will
        ///     be responsible for its disposal;
        ///   <c>false</c> if some other object owns <paramref name="obj"/>
        ///     and is responsible for its disposal.
        ///   If <paramref name="obj"/> is <c>null</c>, this parameter is
        ///     ignored; the box never owns a <c>null</c> reference.
        /// </param>
        public DisposableBox(T? obj, bool owned = true)
        {
            Set(obj, owned);
        }

        /// <summary>
        ///   Gets the object or <c>null</c> reference held in the box.
        /// </summary>
        public T? Object
        {
            get { RequireNotDisposed(); return _obj; }
        }

        /// <summary>
        ///   Gets whether the object held in the box is owned by the box.  If
        ///   <c>true</c>, then <see cref="Object"/> is non-<c>null</c> and
        ///   will be disposed when the box itself is disposed.  Otherwise,
        ///   this property is <c>false</c>.
        /// </summary>
        public bool IsOwned
        {
            get { RequireNotDisposed(); return _owned; }
        }

        /// <summary>
        ///   Sets the object or <c>null</c> reference held in the box and the
        ///   object's ownership.  If the box currenty owns a different object,
        ///   that object will be disposed.
        /// </summary>
        /// <param name="obj">
        ///   The object or <c>null</c> reference to be held in the box.
        /// </param>
        /// <param name="owned">
        ///   <c>true</c> if the box will own <paramref name="obj"/> and will
        ///     be responsible for its disposal;
        ///   <c>false</c> if some other object owns <paramref name="obj"/>
        ///     and is responsible for its disposal.
        ///   If <paramref name="obj"/> is <c>null</c>, this parameter is
        ///     ignored; the box never owns a <c>null</c> reference.
        /// </param>
        /// <returns>
        ///   The <paramref name="obj"/> argument.
        /// </returns>
        public T? Set(T? obj, bool owned = true)
        {
            RequireNotDisposed();

            var oldObj   = _obj;
            var oldOwned = _owned;

            _obj   = obj;
            _owned = owned && obj != null;

            if (oldOwned && oldObj != obj)
                // SAFETY: oldOwned implies oldObj != null
                oldObj!.Dispose();

            return obj;
        }

        /// <summary>
        ///   Sets the reference held in the box to <c>null</c>.  If the box
        ///   currently owns an object, the object will be disposed.  When this
        ///   method returns, 
        ///     <see cref="Object"/>  is <c>null</c> and
        ///     <see cref="IsOwned"/> is <c>false</c>.
        /// </summary>
        public void Clear()
        {
            Set(null, owned: false);
        }

        /// <summary>
        ///   Sets the reference held in the box to <c>null</c> and returns the
        ///   object or <c>null</c> reference currently held in the box.  If
        ///   the box currently owns an object, the caller assumes ownership of
        ///   the object and becomes responsible for the object's disposal.
        ///   When this method returns, 
        ///     <see cref="Object"/>  is <c>null</c> and
        ///     <see cref="IsOwned"/> is <c>false</c>.
        /// </summary>
        public T? Take()
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
                // SAFETY: _owned implies _obj != null
                _obj!.Dispose();

            // Clear
            _obj   = null;
            _owned = false;
            return true;
        }
    }
}
