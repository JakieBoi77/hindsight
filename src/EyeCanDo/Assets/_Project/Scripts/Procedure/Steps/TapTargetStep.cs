using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.EyeCanDo.Procedure.Steps
{
    /// <summary>Asks the player to tap a 3D object (Sam, the exam chair...), plays a beat, then optionally moves to another shot.</summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Steps/Tap Target", fileName = "TapTargetStep")]
    public sealed class TapTargetStep : ProcedureStep
    {
        [SerializeField] string shotId = "";
        [SerializeField] StoryBeat setupBeat = StoryBeat.None;
        [SerializeField] string interactableId = "";
        [SerializeField] StoryLine line = new();
        [Tooltip("Beat played when the object is tapped (e.g. Sam waves).")]
        [SerializeField] StoryBeat tapBeat = StoryBeat.None;
        [Tooltip("Optional line said when the object is tapped (e.g. Sam: \"Hi! I'm Sam!\").")]
        [SerializeField] StoryLine tapLine = new();
        [Tooltip("Instant beat before cutting to the after shot (e.g. hide the player's kid).")]
        [SerializeField] StoryBeat afterSetupBeat = StoryBeat.None;
        [Tooltip("Shot to blend to after the tap (e.g. first person in the chair). Empty = stay.")]
        [SerializeField] string afterShotId = "";

        public override IEnumerable<StoryLine> Lines => NonEmpty(line, tapLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            if (setupBeat != StoryBeat.None) yield return ctx.PlayBeat(setupBeat);
            yield return ctx.Shots.CutAndWait(shotId);
            var target = ctx.GetInteractable(interactableId);
            if (target == null) yield break;

            var tapped = false;
            void OnTap() => tapped = true;
            target.Tapped += OnTap;
            target.SetAccepting(true);

            // The object answers a tap at once, even mid-sentence; the panel still lets the line finish.
            ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, line, waitForNext: false));
            while (!tapped) yield return null;

            target.Tapped -= OnTap;
            target.SetAccepting(false);
            if (!tapLine.IsEmpty)
            {
                // The beat answers the tap at once; the spoken reaction waits for the current line to end.
                if (tapBeat != StoryBeat.None) ctx.CoroutineHost.StartCoroutine(ctx.PlayBeat(tapBeat));
                while (ctx.Captions.Narrator.IsSpeaking) yield return null;
                yield return ctx.Captions.Say(tapLine, waitForNext: false);
            }
            else if (tapBeat != StoryBeat.None)
            {
                yield return ctx.PlayBeat(tapBeat);
            }
            while (ctx.Captions.Narrator.IsSpeaking) yield return null;
            ctx.Captions.Hide();
            if (afterSetupBeat != StoryBeat.None) yield return ctx.PlayBeat(afterSetupBeat);
            yield return ctx.Shots.CutAndWait(afterShotId);
        }

#if UNITY_EDITOR
        public TapTargetStep EditorInit(string shot, string target, StoryLine text, string afterShot = "",
            StoryBeat onTap = StoryBeat.None, StoryBeat setup = StoryBeat.None, StoryBeat afterSetup = StoryBeat.None, StoryLine reaction = null)
        {
            tapLine = reaction ?? new StoryLine();
            shotId = shot;
            interactableId = target;
            line = text;
            afterShotId = afterShot;
            tapBeat = onTap;
            setupBeat = setup;
            afterSetupBeat = afterSetup;
            return this;
        }
#endif
    }
}
