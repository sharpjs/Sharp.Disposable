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

using FluentAssertions.Extensions;

namespace Sharp.Disposable;

[TestFixture]
public class FinalizerTests
{
    [Test]
    public void RunUntil_NullCondition()
    {
        this.Invoking(_ => Finalizer.RunUntil(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RunUntil_NullCondition_AnyInterval()
    {
        this.Invoking(_ => Finalizer.RunUntil(null!, 1.Seconds()))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void RunUntil_SomeCondition_NegativeInterval()
    {
        this.Invoking(_ => Finalizer.RunUntil(() => true, (-1).Seconds()))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [Timeout(1 * 1000 /*ms*/)]
    public void RunUntil_Immediate()
    {
        Finalizer.RunUntil(() => true);
    }

    [Test]
    [Timeout(5 * 1000 /*ms*/)]
    public void RunUntil_Delayed()
    {
        var done = false;
        var task = Task.Delay(50.Milliseconds()).ContinueWith(_ => done = true);

        Finalizer.RunUntil(() => done);
    }
}
