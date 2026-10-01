using System.IO;
using Hindsight.App;
using Hindsight.Core.Progress;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hindsight.Tests.EditMode
{
    public sealed class UnlockRulesTests
    {
        [Test]
        public void UnbuiltLevels_AreComingSoon()
        {
            var availability = UnlockRules.Evaluate("vision_test", false, null, new PlayerProgress());

            Assert.That(availability, Is.EqualTo(LevelAvailability.ComingSoon));
            Assert.That(UnlockRules.CanPlay(availability), Is.False);
        }

        [Test]
        public void LevelWithoutPrerequisite_IsUnlocked()
        {
            Assert.That(UnlockRules.Evaluate("eye_drops", true, null, new PlayerProgress()), Is.EqualTo(LevelAvailability.Unlocked));
        }

        [Test]
        public void LevelWithUnmetPrerequisite_IsLocked()
        {
            Assert.That(UnlockRules.Evaluate("slit_lamp", true, "eye_drops", new PlayerProgress()), Is.EqualTo(LevelAvailability.Locked));
        }

        [Test]
        public void LevelWithMetPrerequisite_IsUnlocked()
        {
            var progress = new PlayerProgress(new[] { "eye_drops" }, new string[0]);

            Assert.That(UnlockRules.Evaluate("slit_lamp", true, "eye_drops", progress), Is.EqualTo(LevelAvailability.Unlocked));
        }

        [Test]
        public void CompletedLevel_IsCompletedAndPlayable()
        {
            var progress = new PlayerProgress(new[] { "eye_drops" }, new string[0]);
            var availability = UnlockRules.Evaluate("eye_drops", true, null, progress);

            Assert.That(availability, Is.EqualTo(LevelAvailability.Completed));
            Assert.That(UnlockRules.CanPlay(availability), Is.True);
        }
    }
}
