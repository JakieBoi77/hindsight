using System.Collections;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// The eye drop itself. The player holds a button to "keep their eye open" while the drop falls.
    /// Letting go = blinking: the drop misses and the doctor kindly asks to try again.
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Hold Still (Eye Drop)", fileName = "HoldStillStep")]
    public sealed class HoldStillStep : ProcedureStep
    {
        [SerializeField] string shotId = "FirstPersonTiltBack";
        [SerializeField] string doctorAnchorId = "DoctorDropPoint";
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField] string holdLabel = "Hold to keep\nyour eye open!";
        [SerializeField, TextArea(2, 3)] string startPrompt = "Press and HOLD the big button. That keeps your eye open!";
        [SerializeField, TextArea(2, 3)] string dropComing = "Here comes the drop... keep holding...";
        [SerializeField, TextArea(2, 3)] string missedLine = "Oops, you blinked! The drop missed. That's okay - let's try one more time.";
        [SerializeField, TextArea(2, 3)] string successLine = "Got it! Brrr, a little cold, right? You held super still!";
        [SerializeField] float dropSeconds = 1.6f;
        [SerializeField] float blurAfterDrop = 0.8f;

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            ctx.EyeDrop.ShowHolding();
            yield return ctx.Shots.CutAndWait(shotId);

            var landed = false;
            while (!landed)
            {
                ctx.Buttons.ShowHold(holdLabel);
                yield return ctx.Dialogue.Say(speaker, startPrompt, waitForNext: false);
                while (!ctx.Buttons.Hold.IsHeld) yield return null;

                ctx.Doctor.Play(DoctorPose.Interact);
                ctx.CoroutineHost.StartCoroutine(ctx.Dialogue.Say(speaker, dropComing, waitForNext: false));
                yield return ctx.EyeDrop.Fall(dropSeconds, () => ctx.Buttons.Hold.IsHeld, ok => landed = ok, ctx.Buttons.SetHoldProgress);
                ctx.Buttons.SetHoldProgress(0f);

                if (!landed)
                {
                    ctx.Buttons.HideAll();
                    yield return ctx.Effects.Blink();
                    yield return ctx.Dialogue.Say(speaker, missedLine);
                }
            }

            ctx.Buttons.HideAll();
            ctx.Effects.SetBlur(blurAfterDrop);
            ctx.Doctor.Play(DoctorPose.Cheer);
            yield return ctx.Effects.ColdSplash();
            yield return ctx.Dialogue.Say(speaker, successLine);
            ctx.Dialogue.Hide();
            ctx.EyeDrop.ShowAtRest();
        }
    }
}
