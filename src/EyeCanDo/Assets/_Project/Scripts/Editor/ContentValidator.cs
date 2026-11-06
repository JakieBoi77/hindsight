using System.Collections.Generic;
using System.Linq;
using EyeCanDo.Content;
using EyeCanDo.Core.Content;
using UnityEditor;
using UnityEngine;

namespace EyeCanDo.EditorTools
{
    /// <summary>
    /// Checks the story and captions against the content rules (sentence length, banned words,
    /// sensation before the drop, sensation not skippable), and reports captions still waiting
    /// for recorded narration.
    /// </summary>
    public static class ContentValidator
    {
        [MenuItem("EyeCanDo/Validate Content", priority = 10)]
        public static void ValidateFromMenu()
        {
            var story = AssetDatabase.LoadAssetAtPath<StoryScript>(PocBuilder.StoryPath);
            var captions = AssetDatabase.LoadAssetAtPath<CaptionBank>(PocBuilder.CaptionsPath);
            var problems = Problems(story, captions);
            foreach (var problem in problems)
            {
                Debug.LogError("[EyeCanDo] Content: " + problem);
            }

            var missing = CaptionsWithoutNarration(story, captions).ToList();
            Debug.Log($"[EyeCanDo] Narration to record: {missing.Count} captions.\n" + string.Join("\n", missing));
            if (problems.Count == 0)
            {
                Debug.Log("[EyeCanDo] Content passed every rule.");
            }
        }

        public static List<string> Problems(StoryScript story, CaptionBank captions)
        {
            var problems = new List<string>();
            if (story == null)
            {
                problems.Add("StoryScript asset is missing.");
            }
            else
            {
                problems.AddRange(ContentRules.CheckStory(story.ToInfo()));
            }

            if (captions == null)
            {
                problems.Add("CaptionBank asset is missing.");
            }
            else
            {
                foreach (var pair in captions.All())
                {
                    problems.AddRange(ContentRules.CheckCaption($"Caption '{pair.Key}'", pair.Value?.text));
                }
            }

            return problems;
        }

        public static IEnumerable<string> CaptionsWithoutNarration(StoryScript story, CaptionBank captions)
        {
            if (story != null)
            {
                foreach (var step in story.steps)
                {
                    foreach (var line in step.lines.Where(l => l != null && l.narration == null))
                    {
                        yield return $"{step.kind}: {line.text}";
                    }
                }
            }

            if (captions != null)
            {
                foreach (var pair in captions.All().Where(p => p.Value != null && p.Value.narration == null))
                {
                    yield return $"{pair.Key}: {pair.Value.text}";
                }
            }
        }
    }
}
