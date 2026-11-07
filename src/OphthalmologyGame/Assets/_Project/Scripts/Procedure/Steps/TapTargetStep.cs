using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Asks the player to tap a 3D object (e.g. the exam chair), then moves to another shot.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Tap Target", fileName = "TapTargetStep")]
    public sealed class TapTargetStep : ProcedureStep
    {
        [SerializeField] string shotId = "";
        [SerializeField] string interactableId = "";
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField, TextArea(2, 4)] string prompt = "";
        [Tooltip("Shot to blend to after the tap (e.g. first person in the chair).")]
        [SerializeField] string afterShotId = "";

        public override IEnumerator Run(IProcedureContext ctx)
        {
            yield return ctx.Shots.CutAndWait(shotId);
            var target = ctx.GetInteractable(interactableId);
            if (target == null) yield break;

            var tapped = false;
            void OnTap() => tapped = true;
            target.Tapped += OnTap;
            target.SetAccepting(true);

            yield return ctx.Dialogue.Say(speaker, prompt, waitForNext: false);
            while (!tapped) yield return null;

            target.Tapped -= OnTap;
            target.SetAccepting(false);
            ctx.Dialogue.Hide();
            yield return ctx.Shots.CutAndWait(afterShotId);
        }

#if UNITY_EDITOR
        public TapTargetStep EditorInit(string shot, string target, string text, string afterShot)
        {
            shotId = shot;
            interactableId = target;
            prompt = text;
            afterShotId = afterShot;
            return this;
        }
#endif
    }
}
