using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// The eye drop itself. The player holds the button to keep their eye open while the drop falls.
    /// Letting go only pauses the drop in mid-air, and Dr. Iris repeats her reminder ("Make sure to keep
    /// your head back and eyes open."); holding again carries on.
    /// The drop never misses, so there is no fail state.
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Hold Still (Eye Drop)", fileName = "HoldStillStep")]
    public sealed class HoldStillStep : ProcedureStep
    {
        [SerializeField] string shotId = "FirstPersonTiltBack";
        [SerializeField] string doctorAnchorId = "DoctorDropPoint";
        [SerializeField] StoryLine comingLine = new();
        [SerializeField] StoryLine retryLine = new();
        [SerializeField] StoryLine landLine = new();
        [SerializeField] float dropSeconds = 1.8f;
        [SerializeField] float blurAfterDrop = 0.8f;

        public override IEnumerable<StoryLine> Lines => NonEmpty(comingLine, retryLine, landLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            ctx.EyeDrop.ShowHolding();
            yield return ctx.Shots.CutAndWait(shotId);
            ctx.Buttons.ShowHold("");
            while (!ctx.Buttons.Hold.IsHeld) yield return null;

            ctx.Doctor.Play(DoctorPose.Interact);
            ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, comingLine, waitForNext: false));
            Sfx.Play(SfxId.DropFall);
            ctx.EyeDrop.BeginDrop();
            var paused = false;
            for (var k = 0f; k < 1f;)
            {
                if (ctx.Buttons.Hold.IsHeld)
                {
                    if (paused)
                    {
                        paused = false;
                        ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, comingLine, waitForNext: false));
                    }
                    k = Mathf.Min(1f, k + Time.deltaTime / dropSeconds);
                }
                else if (!paused)
                {
                    // Let go: the drop waits in the air while Dr. Iris repeats the reminder.
                    paused = true;
                    ctx.CoroutineHost.StartCoroutine(ctx.Effects.Blink());
                    ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, retryLine, waitForNext: false));
                }
                ctx.EyeDrop.SetDrop(k);
                ctx.Buttons.SetHoldProgress(k);
                yield return null;
            }
            ctx.EyeDrop.EndDrop();

            ctx.Buttons.HideAll();
            ctx.Effects.SetBlur(blurAfterDrop);
            ctx.Doctor.Play(DoctorPose.Cheer);
            Sfx.Play(SfxId.Shiver);
            yield return ctx.Effects.ColdSplash();
            yield return SayIfAny(ctx, landLine);
            ctx.Captions.Hide();
            ctx.EyeDrop.ShowAtRest();
        }

#if UNITY_EDITOR
        public HoldStillStep EditorInit(StoryLine coming, StoryLine retry, StoryLine land)
        {
            comingLine = coming;
            retryLine = retry;
            landLine = land;
            return this;
        }
#endif
    }
}
