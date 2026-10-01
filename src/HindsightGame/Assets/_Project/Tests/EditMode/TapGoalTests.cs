using System;
using Hindsight.Core.Interactions;
using NUnit.Framework;

namespace Hindsight.Tests.EditMode
{
    public sealed class TapGoalTests
    {
        [Test]
        public void ReachingGoal_ReturnsTrueOnlyOnTheFinalTap()
        {
            var goal = new TapGoal(3);

            Assert.That(goal.RegisterTap(), Is.False);
            Assert.That(goal.RegisterTap(), Is.False);
            Assert.That(goal.RegisterTap(), Is.True);
            Assert.That(goal.IsReached, Is.True);
        }

        [Test]
        public void ExtraTaps_AreIgnored()
        {
            var goal = new TapGoal(1);
            goal.RegisterTap();

            Assert.That(goal.RegisterTap(), Is.False);
            Assert.That(goal.TapCount, Is.EqualTo(1));
            Assert.That(goal.Progress, Is.EqualTo(1f));
        }
    }
}
