using System.Collections.Generic;
using EyeCanDo.Core.Games;
using EyeCanDo.Core.Interaction;
using NUnit.Framework;

namespace EyeCanDo.Tests.EditMode
{
    public sealed class DropCatchRoundTests
    {
        private static DropCatchRound Round(int level, List<bool> released = null)
        {
            var round = new DropCatchRound(DropCatchSettings.ForLevel(level), 7);
            if (released != null)
            {
                round.DropReleased += (bottle, golden) => released.Add(golden);
            }

            return round;
        }

        private static void Run(DropCatchRound round, float seconds)
        {
            for (var t = 0f; t < seconds; t += 0.05f)
            {
                round.Tick(0.05f);
            }
        }

        [Test]
        public void Drops_KeepComing_UntilTimeIsUp()
        {
            var released = new List<bool>();
            var round = Round(1, released);
            Run(round, 10f);
            Assert.Greater(released.Count, 3);
            var before = released.Count;
            Run(round, 60f);
            Assert.IsTrue(round.TimeUp);
            var after = released.Count;
            Run(round, 5f);
            Assert.AreEqual(after, released.Count, "No drops after time is up.");
            Assert.Greater(after, before);
        }

        [Test]
        public void HigherLevels_DropFaster()
        {
            var easy = new List<bool>();
            var hard = new List<bool>();
            Run(Round(1, easy), 30f);
            Run(Round(3, hard), 30f);
            Assert.Greater(hard.Count, easy.Count);
            Assert.Less(DropCatchSettings.ForLevel(3).FallSeconds, DropCatchSettings.ForLevel(1).FallSeconds);
        }

        [Test]
        public void Catching_Scores_AndGoldenDropsAreWorthMore()
        {
            var round = Round(1);
            Assert.AreEqual(DropCatchRound.PointsPerDrop, round.Catch(false));
            Assert.AreEqual(DropCatchRound.PointsPerGolden, round.Catch(true));
            Assert.AreEqual(DropCatchRound.PointsPerDrop + DropCatchRound.PointsPerGolden, round.Score);
            Assert.AreEqual(2, round.Combo);
        }

        [Test]
        public void Missing_OnlyResetsTheCombo()
        {
            var round = Round(1);
            round.Catch(false);
            round.Catch(false);
            round.Miss();
            Assert.AreEqual(2, round.Score, "Nothing is taken away for a miss.");
            Assert.AreEqual(0, round.Combo);
            Assert.AreEqual(2, round.BestCombo);
        }

        [Test]
        public void FullEyes_TwoBlinkTaps_EarnABonus()
        {
            var round = Round(1);
            Assert.AreEqual(0, round.Blink(), "Blinking before the eyes are full does nothing.");
            for (var i = 0; i < round.Settings.CatchesUntilBlink; i++)
            {
                round.Catch(false);
            }

            Assert.IsTrue(round.EyesFull);
            Assert.AreEqual(0, round.Blink());
            Assert.AreEqual(DropCatchRound.BlinkBonus, round.Blink());
            Assert.IsFalse(round.EyesFull);
            Assert.AreEqual(round.Settings.CatchesUntilBlink + DropCatchRound.BlinkBonus, round.Score);
        }

        [Test]
        public void ThreeStars_NeedEveryDropCaught()
        {
            var round = Round(1);
            Run(round, 60f);
            for (var i = 0; i < round.Released - 1; i++)
            {
                round.Catch(false);
            }

            round.Miss();
            Assert.AreEqual(2, round.Stars, "One miss means no third star.");
        }

        [Test]
        public void Stars_FinishingAlwaysEarnsOne()
        {
            var round = Round(1);
            Run(round, 60f);
            Assert.AreEqual(1, round.Stars, "Catching nothing still earns a star for finishing.");
            for (var i = 0; i < round.Released; i++)
            {
                round.Catch(false);
            }

            Assert.AreEqual(3, round.Stars);
        }
    }

    public sealed class BlurrySpyRoundTests
    {
        [Test]
        public void Targets_AreDistinctItems()
        {
            var round = new BlurrySpyRound(BlurrySpySettings.ForLevel(3), 3);
            Assert.AreEqual(BlurrySpySettings.ForLevel(3).Targets, round.Targets.Count);
            CollectionAssert.AllItemsAreUnique(round.Targets);
            foreach (var target in round.Targets)
            {
                Assert.That(target, Is.InRange(0, round.Settings.Items - 1));
            }
        }

        [Test]
        public void WrongTaps_CostNothing_AndFindingClearsTheBlur()
        {
            var round = new BlurrySpyRound(BlurrySpySettings.ForLevel(1), 1);
            var startBlur = round.Blur;
            var wrong = (round.CurrentTarget + 1) % round.Settings.Items;
            Assert.AreEqual(SpyTapResult.NotThisOne, round.Tap(wrong));
            Assert.AreEqual(0, round.Found);
            Assert.AreEqual(SpyTapResult.Found, round.Tap(round.CurrentTarget));
            Assert.Less(round.Blur, startBlur);
            while (!round.Done)
            {
                round.Tap(round.CurrentTarget);
            }

            Assert.AreEqual(0f, round.Blur, 0.001f, "The blur is gone at the end: it goes away.");
            Assert.AreEqual(SpyTapResult.Ignored, round.Tap(0));
        }

        [Test]
        public void ThreeStars_NeedAPerfectRound()
        {
            var round = new BlurrySpyRound(BlurrySpySettings.ForLevel(1), 4);
            round.Tap((round.CurrentTarget + 1) % round.Settings.Items);
            while (!round.Done)
            {
                round.Tick(1f);
                round.Tap(round.CurrentTarget);
            }

            Assert.AreEqual(2, round.Stars, "One wrong tap means no third star, however fast.");
        }

        [Test]
        public void Stars_DependOnSpeed_ButNeverDropBelowOne()
        {
            var fast = new BlurrySpyRound(BlurrySpySettings.ForLevel(1), 2);
            var slow = new BlurrySpyRound(BlurrySpySettings.ForLevel(1), 2);
            while (!fast.Done)
            {
                fast.Tick(2f);
                fast.Tap(fast.CurrentTarget);
            }

            while (!slow.Done)
            {
                slow.Tick(60f);
                slow.Tap(slow.CurrentTarget);
            }

            Assert.AreEqual(3, fast.Stars);
            Assert.AreEqual(1, slow.Stars);
            Assert.Greater(fast.Score, slow.Score);
        }
    }

    public sealed class StillHoldTests
    {
        [Test]
        public void LettingGo_Pauses_AndKeepsTheTimeHeld()
        {
            var hold = new StillHold(2f);
            hold.Press();
            hold.Tick(1.2f);
            hold.Release();
            Assert.AreEqual(1, hold.Pauses);
            Assert.AreEqual(1.2f, hold.Held, 0.001f, "Progress is kept, not reset.");
            hold.Tick(5f);
            Assert.IsFalse(hold.Done, "Nothing happens while not holding.");
            hold.Press();
            Assert.IsTrue(hold.Tick(0.9f));
            Assert.IsTrue(hold.Done);
        }

        [Test]
        public void QuickTaps_AreNotPauses()
        {
            var hold = new StillHold(2f);
            hold.Press();
            hold.Tick(0.1f);
            hold.Release();
            Assert.AreEqual(0, hold.Pauses);
        }
    }
}
