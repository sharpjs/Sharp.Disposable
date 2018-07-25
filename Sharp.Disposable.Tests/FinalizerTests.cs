using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace Sharp.Disposable
{
    [TestFixture]
    public class FinalizerTests
    {
        [Test]
        public void RunUntil_NullCondition()
        {
            this.Invoking(_ => Finalizer.RunUntil(null))
                .Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void RunUntil_NullCondition_AnyInterval()
        {
            this.Invoking(_ => Finalizer.RunUntil(null, 1.Seconds()))
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
        [Timeout(1 * 1000 /*ms*/)]
        public void RunUntil_Delayed()
        {
            var done = false;
            var task = Task.Delay(500.Milliseconds()).ContinueWith(_ => done = true);

            Finalizer.RunUntil(() => done);
        }
    }
}
