using System;
using Hindsight.Core.Interactions;
using NUnit.Framework;

namespace Hindsight.Tests.EditMode
{
    public sealed class HoldStillChallengeTests
    {
        [Test]
        public void HoldingForRequiredTime_Succeeds()
        {
            var challenge = new HoldStillChallenge(3f);
            challenge.BeginHold();

            Assert.That(challenge.Tick(1.5f), Is.EqualTo(HoldStillState.Holding));
            Assert.That(challenge.Progress, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(challenge.Tick(1.5f), Is.EqualTo(HoldStillState.Succeeded));
            Assert.That(challenge.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void ReleasingEarly_IsAnInterruptionAndResetsProgress()
        {
            var challenge = new HoldStillChallenge(3f);
            challenge.BeginHold();
            challenge.Tick(2f);

            challenge.ReleaseHold();

            Assert.That(challenge.State, Is.EqualTo(HoldStillState.Interrupted));
            Assert.That(challenge.InterruptionCount, Is.EqualTo(1));
            Assert.That(challenge.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void AfterInterruption_ChildCanTryAgainAndSucceed()
        {
            var challenge = new HoldStillChallenge(1f);
            challenge.BeginHold();
            challenge.ReleaseHold();

            challenge.BeginHold();
            var state = challenge.Tick(1f);

            Assert.That(state, Is.EqualTo(HoldStillState.Succeeded));
            Assert.That(challenge.InterruptionCount, Is.EqualTo(1));
        }

        [Test]
        public void TickWithoutHolding_DoesNothing()
        {
            var challenge = new HoldStillChallenge(1f);

            Assert.That(challenge.Tick(5f), Is.EqualTo(HoldStillState.Idle));
            Assert.That(challenge.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void ReleasingAfterSuccess_DoesNotCountAsInterruption()
        {
            var challenge = new HoldStillChallenge(1f);
            challenge.BeginHold();
            challenge.Tick(1f);

            challenge.ReleaseHold();

            Assert.That(challenge.State, Is.EqualTo(HoldStillState.Succeeded));
            Assert.That(challenge.InterruptionCount, Is.EqualTo(0));
        }

        [Test]
        public void NegativeDelta_IsIgnored()
        {
            var challenge = new HoldStillChallenge(1f);
            challenge.BeginHold();
            challenge.Tick(-10f);

            Assert.That(challenge.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void NonPositiveDuration_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HoldStillChallenge(0f));
        }
    }
}
