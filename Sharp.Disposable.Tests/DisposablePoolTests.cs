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
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Sharp.Disposable.Tests
{
    [TestFixture]
    public class DisposablePoolTests
    {
        [Test]
        public void AddDisposable_Null()
        {
            var pool = new DisposablePool();

            pool.Invoking(p => p.AddDisposable(default(IDisposable)!))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddDisposable_Disposed()
        {
            var objA = new TestDisposable();
            var pool = new DisposablePool();

            pool.Dispose();

            pool.Invoking(p => p.AddDisposable(objA))
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void AddDisposable_ThenDispose()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();
            var pool = new DisposablePool();

            pool.AddDisposable(objA).Should().BeSameAs(objA);
            pool.AddDisposable(objB).Should().BeSameAs(objB);
            pool.Dispose();

            objA.IsDisposed.Should().BeTrue();
            objB.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void AddDisposable_Collect_ThenDispose()
        {
            var pool = new DisposablePool();

            (var refA, var refB) = AddGarbageToPool(pool);

            Finalizer.RunUntil(() => !refA.IsAlive && !refB.IsAlive);

            pool.Dispose();
        }

        private (WeakReference, WeakReference) AddGarbageToPool(DisposablePool pool)
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();

            pool.AddDisposable(objA);
            pool.AddDisposable(objB);

            return (new WeakReference(objA), new WeakReference(objB));
        }

        [Test]
        public void AddDisposable_ThenFinalize()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();
            var pool = new DisposablePool();

            pool.AddDisposable(objA).Should().BeSameAs(objA);
            pool.AddDisposable(objB).Should().BeSameAs(objB);
            pool.DisposeUnmanaged(); // simulate finalizer

            // Does not dispose managed resources during finalization
            objA.IsDisposed.Should().BeFalse();
            objB.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Dispose_Exception()
        {
            var objA = new Mock<IDisposable>(MockBehavior.Strict);
            objA.Setup(o => o.Dispose()).Throws<InvalidOperationException>();

            var pool = new DisposablePool();
            pool.AddDisposable(objA.Object);

            pool.Dispose();

            objA.Verify();
        }
    }
}
