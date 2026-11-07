using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// Dr. Iris and Sam walk over to the wall chart while the camera pans to it. Then she explains it,
    /// and each picture on the chart lights up as she says its cue word.
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Chart", fileName = "ChartStep")]
    public sealed class ChartStep : ProcedureStep
    {
        [SerializeField] string shotId = "ChartTalk";
        [SerializeField] string doctorAnchorId = "DoctorChartPoint";
        [SerializeField] string samAnchorId = "SamChartPoint";
        [SerializeField] StoryLine line = new();
        [Tooltip("One word of the narration per chart panel: the panel lights as that word is said (in order).")]
        [SerializeField] string[] cueWords = Array.Empty<string>();

        public override IEnumerable<StoryLine> Lines => NonEmpty(line);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            var chart = ctx.Chart;
            if (chart != null) chart.ResetChart();

            // Both walk over while the camera pans; the line starts once everyone is there.
            var walking = 2;
            IEnumerator Walk(IEnumerator move)
            {
                yield return move;
                walking--;
            }
            ctx.CoroutineHost.StartCoroutine(Walk(ctx.Doctor.MoveTo(ctx.GetAnchor(doctorAnchorId))));
            ctx.CoroutineHost.StartCoroutine(Walk(ctx.Sam.WalkTo(ctx.GetAnchor(samAnchorId))));
            yield return ctx.Shots.CutAndWait(shotId);
            while (walking > 0) yield return null;

            if (chart != null) ctx.CoroutineHost.StartCoroutine(LightOnCues(chart));
            yield return SayIfAny(ctx, line);
            if (chart != null)
                for (var i = 0; i < chart.PanelCount; i++) chart.Light(i);
            ctx.Captions.Hide();
        }

        /// <summary>Lights each panel when its cue word comes up, timed from the word's place in the line.</summary>
        IEnumerator LightOnCues(SceneObjects.TeachingChart chart)
        {
            var times = CueTimes(line, cueWords);
            var start = Time.time;
            for (var i = 0; i < times.Count; i++)
            {
                while (Time.time - start < times[i]) yield return null;
                chart.Light(i);
            }
        }

        /// <summary>Seconds into the line at which each cue word starts (assumes a steady pace).</summary>
        public static List<float> CueTimes(StoryLine storyLine, string[] cues)
        {
            var words = storyLine.narration.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var seconds = storyLine.clip != null ? storyLine.clip.length : storyLine.EstimatedSeconds;
            var times = new List<float>();
            var from = 0;
            foreach (var cue in cues)
            {
                var at = from;
                while (at < words.Length && !Clean(words[at]).Equals(cue, StringComparison.OrdinalIgnoreCase)) at++;
                if (at >= words.Length) at = Mathf.Min(from, words.Length - 1);
                times.Add(seconds * at / Mathf.Max(1, words.Length));
                from = at + 1;
            }
            return times;
        }

        static string Clean(string word) => word.Trim('.', ',', '!', '?', '\'', '"');

#if UNITY_EDITOR
        public ChartStep EditorInit(StoryLine text, params string[] cues)
        {
            line = text;
            cueWords = cues;
            return this;
        }
#endif
    }
}
