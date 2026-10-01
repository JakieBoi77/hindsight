using Hindsight.Core.Animation;
using NUnit.Framework;

namespace Hindsight.Tests.EditMode
{
    public sealed class EasingTests
    {
        [Test]
        public void AllEases_StartAtZeroAndEndAtOne([Values] EaseType ease)
        {
            Assert.That(Easing.Evaluate(ease, 0f), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(Easing.Evaluate(ease, 1f), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void InputOutsideRange_IsClamped()
        {
            Assert.That(Easing.Evaluate(EaseType.Linear, 2f), Is.EqualTo(1f));
            Assert.That(Easing.Evaluate(EaseType.Linear, -1f), Is.EqualTo(0f));
        }
    }
}
