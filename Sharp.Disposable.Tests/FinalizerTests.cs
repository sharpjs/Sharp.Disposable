using System;
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
    }
}
