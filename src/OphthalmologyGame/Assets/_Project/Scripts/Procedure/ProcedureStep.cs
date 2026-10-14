using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure
{
    /// <summary>
    /// One beat of a clinic procedure (a line of dialogue, a quiz, a mini-game...).
    /// Steps are ScriptableObject assets so a new level is mostly new data, not new code.
    /// </summary>
    public abstract class ProcedureStep : ScriptableObject
    {
        [Tooltip("Short label shown in the progress bar, e.g. \"Say hello\".")]
        [SerializeField] string title = "";

        public string Title => string.IsNullOrEmpty(title) ? name : title;

        /// <summary>Runs the step to completion. The runner waits for the coroutine to finish.</summary>
        public abstract IEnumerator Run(IProcedureContext context);

#if UNITY_EDITOR
        public void EditorSetTitle(string value) => title = value;
#endif
    }
}
