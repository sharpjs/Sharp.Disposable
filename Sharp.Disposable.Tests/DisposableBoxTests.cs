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
using FluentAssertions;
using NUnit.Framework;

namespace Sharp.Disposable.Tests
{
    [TestFixture]
    public class DisposableBoxTests
    {
        [Test]
        public void Construct_Default()
        {
            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse();
            }
        }

        [Test]
        public void Construct_Null()
        {
            using (var box = new DisposableBox<TestDisposable>(null))
            {
                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Construct_Owned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeTrue();
            }

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Construct_NotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeFalse();
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_NullToNull()
        {
            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Set(null);

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse();
            }
        }

        [Test]
        public void Set_NullToOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Set(obj);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeTrue();
            }

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Set_NullToNotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Set(obj, owned: false);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeFalse();
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_OwnedToNull()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Set(null);
                obj.IsDisposed.Should().BeTrue();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Set_OwnedToSameOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Set(obj);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeTrue();
            }

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Set_OwnedToDifferentOwned()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(objA))
            {
                box.Set(objB);
                objA.IsDisposed.Should().BeTrue();
                objB.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(objB);
                box.IsOwned.Should().BeTrue();
            }

            objB.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Set_OwnedToSameNotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Set(obj, owned: false);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeFalse();
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_OwnedToDifferentNotOwned()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(objA))
            {
                box.Set(objB, owned: false);
                objA.IsDisposed.Should().BeTrue();
                objB.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(objB);
                box.IsOwned.Should().BeFalse();
            }

            objB.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_NotOwnedToNull()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Set(null);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Set_NotOwnedToSameOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Set(obj);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeTrue();
            }

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Set_NotOwnedToDifferentOwned()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(objA, owned: false))
            {
                box.Set(objB);
                objA.IsDisposed.Should().BeFalse();
                objB.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(objB);
                box.IsOwned.Should().BeTrue();
            }

            objA.IsDisposed.Should().BeFalse();
            objB.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Set_NotOwnedToSameNotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Set(obj, owned: false);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(obj);
                box.IsOwned.Should().BeFalse();
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_NotOwnedToDifferentNotOwned()
        {
            var objA = new TestDisposable();
            var objB = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(objA, owned: false))
            {
                box.Set(objB, owned: false);
                objA.IsDisposed.Should().BeFalse();
                objB.IsDisposed.Should().BeFalse();

                box.Object .Should().BeSameAs(objB);
                box.IsOwned.Should().BeFalse();
            }

            objA.IsDisposed.Should().BeFalse();
            objB.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Set_Disposed()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>();
            box.Dispose();

            box.Invoking(b => b.Set(obj, owned: false))
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void Clear_Null()
        {
            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Clear();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Clear_Owned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Clear();
                obj.IsDisposed.Should().BeTrue();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Clear_NotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Clear();
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Clear_Disposed()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>();
            box.Dispose();

            box.Invoking(b => b.Clear())
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void Take_Null()
        {
            using (var box = new DisposableBox<TestDisposable>())
            {
                box.Take().Should().BeNull();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }
        }

        [Test]
        public void Take_Owned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj))
            {
                box.Take().Should().BeSameAs(obj);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Take_NotOwned()
        {
            var obj = new TestDisposable();

            using (var box = new DisposableBox<TestDisposable>(obj, owned: false))
            {
                box.Take().Should().BeSameAs(obj);
                obj.IsDisposed.Should().BeFalse();

                box.Object .Should().BeNull();
                box.IsOwned.Should().BeFalse(); // null can't be owned
            }

            obj.IsDisposed.Should().BeFalse();
        }

        [Test]
        public void Take_Disposed()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>();
            box.Dispose();

            box.Invoking(b => b.Take())
                .Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void Dispose_Null()
        {
            var box = new DisposableBox<TestDisposable>(null);

            box.Dispose();

            box.Invoking(b => { var _ = b.Object;  }).Should().Throw<ObjectDisposedException>();
            box.Invoking(b => { var _ = b.IsOwned; }).Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void Dispose_Owned()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>(obj);

            box.Dispose();

            box.Invoking(b => { var _ = b.Object;  }).Should().Throw<ObjectDisposedException>();
            box.Invoking(b => { var _ = b.IsOwned; }).Should().Throw<ObjectDisposedException>();

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Dispose_Owned_Twice()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>(obj);

            box.Dispose();
            box.Dispose();

            box.Invoking(b => { var _ = b.Object;  }).Should().Throw<ObjectDisposedException>();
            box.Invoking(b => { var _ = b.IsOwned; }).Should().Throw<ObjectDisposedException>();

            obj.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void Dispose_NotOwned()
        {
            var obj = new TestDisposable();
            var box = new DisposableBox<TestDisposable>(obj, owned: false);

            box.Dispose();

            box.Invoking(b => { var _ = b.Object;  }).Should().Throw<ObjectDisposedException>();
            box.Invoking(b => { var _ = b.IsOwned; }).Should().Throw<ObjectDisposedException>();

            obj.IsDisposed.Should().BeFalse();
        }
    }
}
