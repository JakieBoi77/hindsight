using System;
using System.Collections.Generic;
using System.Linq;
using EyeCanDo.Core.Content;
using UnityEngine;

namespace EyeCanDo.Content
{
    [Serializable]
    public sealed class StoryStep
    {
        public StoryStepKind kind;

        [Tooltip("Sensation steps must never be skippable (FR_DescribeSensations).")]
        public bool skippable = true;

        public List<CaptionLine> lines = new List<CaptionLine>();

        [Tooltip("Quiz steps: said after a picture that is not the answer. Gentle, never \"wrong\".")]
        public CaptionLine hint;

        public IEnumerable<string> AllText()
        {
            foreach (var line in lines)
            {
                yield return line?.text;
            }

            if (hint != null && !string.IsNullOrEmpty(hint.text))
            {
                yield return hint.text;
            }
        }
    }

    /// <summary>
    /// The Special Drops story, in order: meet Dr. Iris, learn what the drops do and how they feel,
    /// watch Mia get her drops, then do it yourself in the chair. Each line can carry a beat that
    /// plays on stage. Edit the text here; the order and skippable flags are checked by the content
    /// validator. All text is a DRAFT pending clinician and child-life review.
    /// </summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Story Script", fileName = "StoryScript")]
    public sealed class StoryScript : ScriptableObject
    {
        public List<StoryStep> steps = new List<StoryStep>();

        public IReadOnlyList<StoryStepInfo> ToInfo()
        {
            return steps
                .Select(s => new StoryStepInfo(s.kind, s.skippable, s.AllText().ToList()))
                .ToList();
        }
    }
}
