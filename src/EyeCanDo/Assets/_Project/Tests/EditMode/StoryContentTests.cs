using System.Collections.Generic;
using System.Linq;
using Hindsight.EyeCanDo.Procedure;
using Hindsight.EyeCanDo.Procedure.Steps;
using NUnit.Framework;
using UnityEditor;

namespace Hindsight.EyeCanDo.Tests
{
    /// <summary>
    /// Automated checks from PoCResearch.md ("How we will show it works", layer 1): every line is
    /// narrated, captions stay short, the level follows the agreed storyboard, nothing reads as
    /// failing, and Sam (the peer model) goes first and speaks. Runs against the generated Eye Drops procedure.
    /// </summary>
    public sealed class StoryContentTests
    {
        const string ProcedurePath = "Assets/_Project/Data/EyeDrops/EyeDropsProcedure.asset";
        const int MaxTotalCaptionWords = 120;
        const int Panels = 14; // the storyboard agreed after the research plan's 12

        /// <summary>
        /// Flip to true once the recordings exist: missing narration audio then fails instead of warning.
        /// </summary>
        const bool RequireAudio = true;

        static readonly string[] FailWords = { "again", "oops", "miss", "missed", "wrong", "fail", "failed", "bad" };

        /// <summary>Lines allowed to use a word from <see cref="FailWords"/>: "not so bad" reassures, it doesn't judge.</summary>
        static readonly HashSet<string> AllowedFailWordLines = new() { "p05_cold", "p08_yay" };

        ProcedureDefinition procedure;
        List<StoryLine> lines;

        [OneTimeSetUp]
        public void Load()
        {
            procedure = AssetDatabase.LoadAssetAtPath<ProcedureDefinition>(ProcedurePath);
            Assume.That(procedure, Is.Not.Null, "Run POC → Rebuild All first: " + ProcedurePath);
            lines = procedure.Steps.Where(s => s != null).SelectMany(s => s.Lines).ToList();
        }

        [Test]
        public void EveryLineHasAnIdAndNarration()
        {
            Assert.IsNotEmpty(lines);
            foreach (var line in lines)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(line.id), $"a line has no id: \"{line.caption}\"");
                Assert.IsFalse(string.IsNullOrWhiteSpace(line.narration), $"{line.id} has no narration text");
            }
        }

        [Test]
        public void LineIdsAreUnique()
        {
            var duplicates = lines.GroupBy(l => l.id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            CollectionAssert.IsEmpty(duplicates, "each id is also a narration file name");
        }

        [Test]
        public void CaptionsAreEightWordsOrFewer()
        {
            foreach (var line in lines)
                Assert.LessOrEqual(StoryLine.WordCount(line.caption), StoryLine.MaxCaptionWords, $"{line.id}: \"{line.caption}\"");
        }

        [Test]
        public void AllCaptionsTogetherStayUnder120Words()
        {
            var total = lines.Sum(l => StoryLine.WordCount(l.caption));
            TestContext.WriteLine($"Caption words: {total}");
            Assert.Less(total, MaxTotalCaptionWords);
        }

        [Test]
        public void LevelFollowsTheStoryboardEndingOnTheReward()
        {
            Assert.AreEqual(Panels, procedure.Steps.Count);
            Assert.IsInstanceOf<RewardStep>(procedure.Steps.Last());
        }

        [Test]
        public void NothingReadsAsFailing()
        {
            foreach (var line in lines.Where(l => !AllowedFailWordLines.Contains(l.id)))
            {
                var words = (line.narration + " " + line.caption).ToLowerInvariant()
                    .Split(' ', '.', ',', '!', '?', '\'').Where(w => w.Length > 0);
                foreach (var w in words)
                    CollectionAssert.DoesNotContain(FailWords, w, $"{line.id}: \"{line.narration}\"");
            }
        }

        [Test]
        public void PeerModelComesBeforeThePlayersTurn()
        {
            var samPanel = procedure.Steps.ToList().FindIndex(s => s.Lines.Any(l => l.id == "p05_cold"));
            var holdPanel = procedure.Steps.ToList().FindIndex(s => s is HoldStillStep);
            Assert.GreaterOrEqual(samPanel, 0, "Sam gets drops on screen");
            Assert.Less(samPanel, holdPanel, "Sam goes first");
        }

        [Test]
        public void TheChartLightsUpInOrderWhileItIsExplained()
        {
            var chart = procedure.Steps.OfType<ChartStep>().Single();
            var line = chart.Lines.Single();
            var times = ChartStep.CueTimes(line, new[] { "special", "big", "see" });
            Assert.AreEqual(3, times.Count);
            Assert.Greater(times[0], 0f);
            Assert.Greater(times[1], times[0]);
            Assert.Greater(times[2], times[1]);
            Assert.Less(times[2], line.clip != null ? line.clip.length : line.EstimatedSeconds);
        }

        [Test]
        public void SamSpeaksForHimself()
        {
            Assert.GreaterOrEqual(lines.Count(l => l.speaker == Speaker.Sam), 2, "the peer model has his own voice lines");
        }

        [Test]
        public void EveryLineHasNarrationAudio()
        {
            var missing = lines.Where(l => l.clip == null).Select(l => l.id).ToList();
            if (missing.Count == 0) return;
            var message = $"{missing.Count} of {lines.Count} lines have no recording yet " +
                          $"(add Audio/Narration/<id>.wav, then POC → Rebuild All): {string.Join(", ", missing)}";
#pragma warning disable CS0162 // RequireAudio is a switch flipped by hand
            if (RequireAudio) Assert.Fail(message);
#pragma warning restore CS0162
            // Unity's NUnit has no Assert.Warn: report the gap in the log and the test output, and pass.
            UnityEngine.Debug.LogWarning("StoryContentTests: " + message);
            TestContext.WriteLine(message);
        }
    }
}
