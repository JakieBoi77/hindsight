using System.Linq;
using EyeCanDo.Content;
using EyeCanDo.Core.Content;
using EyeCanDo.Core.Interaction;
using EyeCanDo.EditorTools;
using NUnit.Framework;
using UnityEditor;

namespace EyeCanDo.Tests.EditMode
{
    public sealed class InteractionRulesTests
    {
        [Test]
        public void InputGate_LetsOneInputThroughPerScreen()
        {
            var gate = new InputGate();
            Assert.IsTrue(gate.TryPass());
            Assert.IsFalse(gate.TryPass());
            Assert.IsFalse(gate.TryPass());
            gate.Open();
            Assert.IsTrue(gate.TryPass());
        }

        [Test]
        public void SkipHold_NeedsAFullSecond_AndRestartsOnRelease()
        {
            var skip = new SkipHold();
            skip.Press();
            Assert.IsFalse(skip.Tick(0.6f));
            skip.Release();
            skip.Press();
            Assert.IsFalse(skip.Tick(0.6f));
            Assert.IsTrue(skip.Tick(0.5f));
            Assert.IsFalse(skip.Tick(5f), "Triggers only once per hold.");
        }

        [Test]
        public void SkipHold_DoesNothingWhenDisabled()
        {
            var skip = new SkipHold { Enabled = false };
            skip.Press();
            Assert.IsFalse(skip.Tick(5f));
        }

        [Test]
        public void IdleHint_ComesQuicklyFirst_ThenLessOften()
        {
            var hint = new IdleHint();
            Assert.IsFalse(hint.Tick(IdleHint.FirstHintSeconds - 0.1f));
            Assert.IsTrue(hint.Tick(0.2f));
            Assert.IsFalse(hint.Tick(IdleHint.FirstHintSeconds + 0.1f));
            Assert.IsTrue(hint.Tick(IdleHint.RepeatHintSeconds));
            hint.Restart();
            Assert.IsTrue(hint.Tick(IdleHint.FirstHintSeconds));
        }
    }

    public sealed class ContentRulesTests
    {
        [Test]
        public void LongSentences_AndThreatWords_AreReported()
        {
            Assert.IsNotEmpty(ContentRules.CheckCaption("t", "This sentence has far too many words for a young child to follow."));
            Assert.IsNotEmpty(ContentRules.CheckCaption("t", "It won't hurt at all."));
            Assert.IsNotEmpty(ContentRules.CheckCaption("t", "No needle here."));
            Assert.IsEmpty(ContentRules.CheckCaption("t", "The drop feels cold. Then it goes away."));
            Assert.IsEmpty(ContentRules.CheckCaption("t", "You earned a badge!"), "'bad' inside another word is fine.");
        }

        [Test]
        public void Story_MustDescribeSensationsFirst_AndNotLetThemBeSkipped()
        {
            var bad = new[]
            {
                new StoryStepInfo(StoryStepKind.DropIn, true, new[] { "Your turn!" }),
                new StoryStepInfo(StoryStepKind.Sensation, true, new[] { "Cold." }),
                new StoryStepInfo(StoryStepKind.Close, true, new[] { "Bye!" }),
            };
            var problems = ContentRules.CheckStory(bad).ToList();
            Assert.IsTrue(problems.Any(p => p.Contains("before the Sensation")));
            Assert.IsTrue(problems.Any(p => p.Contains("must not be skippable")));
        }

        [Test]
        public void DefaultContent_PassesEveryRule()
        {
            var problems = ContentValidator.Problems(DefaultContent.CreateStory(), DefaultContent.CreateCaptionBank());
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void ProjectContentAssets_PassEveryRule()
        {
            var story = AssetDatabase.LoadAssetAtPath<StoryScript>(PocBuilder.StoryPath);
            var captions = AssetDatabase.LoadAssetAtPath<CaptionBank>(PocBuilder.CaptionsPath);
            Assert.IsNotNull(story, "Run EyeCanDo → Build All to create the content assets.");
            var problems = ContentValidator.Problems(story, captions);
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void MinimumCaptionTime_GrowsWithLength()
        {
            Assert.Greater(ContentRules.MinimumSeconds("one two three four five"), ContentRules.MinimumSeconds("one"));
        }
    }
}
