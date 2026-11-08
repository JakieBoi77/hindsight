using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure
{
    /// <summary>
    /// One storyboard panel of a clinic visit: one framed shot, one narrated beat, at most one action.
    /// Steps are ScriptableObject assets so a new level is mostly new data, not new code.
    /// </summary>
    public abstract class ProcedureStep : ScriptableObject
    {
        [Tooltip("Editor-only label for the panel, e.g. \"Meet Sam\". Not shown to the child.")]
        [SerializeField] string title = "";

        public string Title => string.IsNullOrEmpty(title) ? name : title;

        /// <summary>Runs the step to completion. The runner waits for the coroutine to finish.</summary>
        public abstract IEnumerator Run(IProcedureContext context);

        /// <summary>Every narrated line this step can play (used by the content tests and the narration script).</summary>
        public abstract IEnumerable<StoryLine> Lines { get; }

        /// <summary>Plays a line if it has any content. Shared by steps with optional lines.</summary>
        protected static IEnumerator SayIfAny(IProcedureContext ctx, StoryLine line, bool waitForNext = true)
        {
            if (line == null || line.IsEmpty) yield break;
            yield return ctx.Captions.Say(line, waitForNext);
        }

        protected static IEnumerable<StoryLine> NonEmpty(params StoryLine[] lines)
        {
            foreach (var line in lines)
                if (line != null && !line.IsEmpty) yield return line;
        }

#if UNITY_EDITOR
        public void EditorSetTitle(string value) => title = value;
#endif
    }
}
