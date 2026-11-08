using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Fades to the giant model eye. The child taps the pupil and watches it grow.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Show Closeup", fileName = "ShowCloseupStep")]
    public sealed class ShowCloseupStep : ProcedureStep
    {
        [SerializeField] string closeupShotId = "EyeCloseup";
        [SerializeField] string pupilTargetId = "Pupil";
        [SerializeField] StoryLine line = new();
        [SerializeField] StoryLine afterLine = new();
        [Tooltip("Instant beat played while the screen is black, before returning.")]
        [SerializeField] StoryBeat returnSetupBeat = StoryBeat.None;
        [SerializeField] string returnShotId = "DoctorGreeting";
        [SerializeField] float dilateSeconds = 2f;

        public override IEnumerable<StoryLine> Lines => NonEmpty(line, afterLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.EyeCloseup.ResetPupil();
            yield return ctx.Effects.FadeOut();
            ctx.Shots.Cut(closeupShotId, instant: true);
            yield return ctx.Effects.FadeIn();

            // The pupil can be tapped while the line is still being said.
            ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, line, waitForNext: false));
            var pupil = ctx.GetInteractable(pupilTargetId);
            if (pupil != null)
            {
                var tapped = false;
                void OnTap() => tapped = true;
                pupil.Tapped += OnTap;
                pupil.SetAccepting(true);
                while (!tapped) yield return null;
                pupil.Tapped -= OnTap;
                pupil.SetAccepting(false);
            }

            UI.Sfx.Play(UI.SfxId.Sparkle);
            var dilation = ctx.CoroutineHost.StartCoroutine(ctx.EyeCloseup.Dilate(dilateSeconds));
            yield return SayIfAny(ctx, afterLine);
            yield return dilation;
            ctx.Captions.Hide();

            yield return ctx.Effects.FadeOut();
            if (returnSetupBeat != StoryBeat.None) yield return ctx.PlayBeat(returnSetupBeat);
            ctx.Shots.Cut(returnShotId, instant: true);
            yield return ctx.Effects.FadeIn();
        }

#if UNITY_EDITOR
        public ShowCloseupStep EditorInit(StoryLine before, StoryLine after, string returnShot, StoryBeat returnSetup)
        {
            line = before;
            afterLine = after;
            returnShotId = returnShot;
            returnSetupBeat = returnSetup;
            return this;
        }
#endif
    }
}
