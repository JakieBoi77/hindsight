using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EyeCanDo.Core.Content
{
    public enum StoryStepKind
    {
        /// <summary>Dr. Iris says hello.</summary>
        Hello,

        /// <summary>Mia comes in; she gets drops first while the child watches.</summary>
        MeetMia,

        /// <summary>Tap the bottle: the pupil grows, so the doctor can see inside.</summary>
        WhatDropsDo,

        /// <summary>Picture question: what do the drops do?</summary>
        QuizDrops,

        /// <summary>How the drops feel, honestly. Cannot be skipped (FR_DescribeSensations).</summary>
        Sensation,

        /// <summary>Mia gets her drops; the child watches and listens.</summary>
        WatchMia,

        /// <summary>Picture question: what is your job when the drops go in?</summary>
        QuizJob,

        /// <summary>Your turn: tap the chair to hop up.</summary>
        HopUp,

        /// <summary>Drag the head back.</summary>
        TipHead,

        /// <summary>Look up at the ceiling star and hold still.</summary>
        LookUp,

        /// <summary>The drop goes in: cold, then it goes away.</summary>
        DropIn,

        /// <summary>Tap the eye to blink.</summary>
        Blink,

        /// <summary>Drag the tissue to the tear.</summary>
        Wipe,

        /// <summary>Clinics differ, you can speak up, and the sticker.</summary>
        Close,
    }

    /// <summary>The engine-free view of one story step that the content rules check.</summary>
    public sealed class StoryStepInfo
    {
        public StoryStepInfo(StoryStepKind kind, bool skippable, IReadOnlyList<string> captions)
        {
            Kind = kind;
            Skippable = skippable;
            Captions = captions ?? Array.Empty<string>();
        }

        public StoryStepKind Kind { get; }

        public bool Skippable { get; }

        public IReadOnlyList<string> Captions { get; }
    }

    /// <summary>
    /// Checks child-facing text and story order against the requirements, so content edits
    /// cannot quietly break them. Used by the editor validator and an EditMode test.
    /// </summary>
    public static class ContentRules
    {
        /// <summary>SENTENCE_MAX_WORDS in the SRS (NFR_Language).</summary>
        public const int MaxWordsPerSentence = 10;

        /// <summary>Threat words and false reassurance (NFR_WordChoice, FR_DescribeSensations).</summary>
        public static readonly IReadOnlyList<string> BannedPhrases = new[]
        {
            "hurt", "hurts", "pain", "shot", "needle", "cut", "scary",
            "won't feel", "will not feel", "doesn't hurt", "does not hurt", "won't hurt",
            "nothing to worry", "be good", "bad",
        };

        /// <summary>Steps that show a drop going into an eye, which the child may feel.</summary>
        public static readonly IReadOnlyList<StoryStepKind> StepsShowingDrop = new[]
        {
            StoryStepKind.WatchMia,
            StoryStepKind.DropIn,
        };

        public static IEnumerable<string> CheckCaption(string context, string caption)
        {
            if (string.IsNullOrWhiteSpace(caption))
            {
                yield return $"{context}: caption is empty.";
                yield break;
            }

            foreach (var sentence in SplitSentences(caption))
            {
                var words = CountWords(sentence);
                if (words > MaxWordsPerSentence)
                {
                    yield return $"{context}: \"{sentence}\" has {words} words (max {MaxWordsPerSentence}).";
                }
            }

            var lower = caption.ToLowerInvariant();
            foreach (var phrase in BannedPhrases)
            {
                if (Regex.IsMatch(lower, $@"(?<![a-z']){Regex.Escape(phrase)}(?![a-z'])"))
                {
                    yield return $"{context}: \"{caption}\" uses the word \"{phrase}\".";
                }
            }
        }

        public static IEnumerable<string> CheckStory(IReadOnlyList<StoryStepInfo> steps)
        {
            if (steps == null || steps.Count == 0)
            {
                yield return "Story has no steps.";
                yield break;
            }

            var sensationIndex = IndexOf(steps, StoryStepKind.Sensation);
            if (sensationIndex < 0)
            {
                yield return "Story has no Sensation step (FR_DescribeSensations).";
            }

            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                if (step.Kind == StoryStepKind.Sensation && step.Skippable)
                {
                    yield return $"Step {i + 1} (Sensation) must not be skippable.";
                }

                if (StepsShowingDrop.Contains(step.Kind) && (sensationIndex < 0 || sensationIndex > i))
                {
                    yield return $"Step {i + 1} ({step.Kind}) shows a drop before the Sensation step.";
                }

                if (step.Captions.Count == 0)
                {
                    yield return $"Step {i + 1} ({step.Kind}) has no captions.";
                }

                for (var c = 0; c < step.Captions.Count; c++)
                {
                    foreach (var problem in CheckCaption($"Step {i + 1} ({step.Kind}) caption {c + 1}", step.Captions[c]))
                    {
                        yield return problem;
                    }
                }
            }

            if (IndexOf(steps, StoryStepKind.Close) != steps.Count - 1)
            {
                yield return "The last step must be Close (FR_ClinicsDiffer, FR_SpeakUp).";
            }
        }

        /// <summary>
        /// How long a caption stays before the child can continue, so a repeated tap cannot skip
        /// it before a grown-up could read it aloud.
        /// </summary>
        public static float MinimumSeconds(string caption)
        {
            return 1.2f + 0.35f * CountWords(caption);
        }

        public static int CountWords(string text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? 0
                : text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static IEnumerable<string> SplitSentences(string text)
        {
            return Regex.Split(text, @"(?<=[.!?])\s+").Where(s => !string.IsNullOrWhiteSpace(s));
        }

        private static int IndexOf(IReadOnlyList<StoryStepInfo> steps, StoryStepKind kind)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i].Kind == kind)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
