/*
    Copyright (C) 2018 Jeffrey Sharp

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
using NUnit.Framework;

namespace Sharp.Disposable.Tests
{
    [TestFixture]
    public class DisposableTests
    {
        private class TestDisposable : Disposable
        {
            public bool? DisposedManaged { get; set; }

            protected override bool Dispose(bool managed)
            {
                DisposedManaged = managed;
                if (!managed) FinalizerRan = true;
                return base.Dispose(managed);
            }

            public new void RequireNotDisposed()
                => base.RequireNotDisposed();
        }

        private static bool FinalizerRan;

        [Test]
        public void Finalizer()
        {
            new WeakReference<TestDisposable>(new TestDisposable());
            GC.Collect();
            GC.WaitForPendingFinalizers();
            FinalizerRan.Should().BeTrue();
        }

        [Test]
        public void Dispose()
        {
            var obj = new TestDisposable();
            obj.DisposedManaged.Should().NotHaveValue();

            obj.Dispose();

            obj.DisposedManaged.Should().BeTrue();
        }

        [Test]
        public void IsDisposed_False()
        {
            var obj = new TestDisposable();

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void IsDisposed_True()
        {
            var obj = new TestDisposable();
            obj.Dispose();

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void RequireNotDisposed_Ok()
        {
            var obj = new TestDisposable();

            obj.RequireNotDisposed();
        }

        [Test]
        public void RequireNotDisposed_Throw()
        {
            var obj = new TestDisposable();
            obj.Dispose();

            obj.Invoking(o => o.RequireNotDisposed())
                .Should().Throw<ObjectDisposedException>();
        }
    }
}
