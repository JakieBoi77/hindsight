using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Player taps "Blink!" several times; each blink clears the blurry vision a bit.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Blink", fileName = "BlinkStep")]
    public sealed class BlinkStep : ProcedureStep
    {
        [SerializeField] string shotId = "FirstPerson";
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField, TextArea(2, 3)] string prompt = "Now blink, blink, blink! Blinking spreads the drops all around your eye.";
        [SerializeField, TextArea(2, 3)] string doneLine = "Perfect! See how everything gets clearer?";
        [SerializeField] int blinksNeeded = 6;
        [SerializeField] float startingBlur = 0.8f;

        public override IEnumerator Run(IProcedureContext ctx)
        {
            yield return ctx.Shots.CutAndWait(shotId);
            ctx.Buttons.ShowTap(Label(0));
            yield return ctx.Dialogue.Say(speaker, prompt, waitForNext: false);

            var counted = 0;
            while (counted < blinksNeeded)
            {
                if (ctx.Buttons.Taps > counted)
                {
                    counted++;
                    ctx.Buttons.SetTapLabel(Label(counted));
                    yield return ctx.Effects.Blink();
                    ctx.Effects.SetBlur(startingBlur * (1f - (float)counted / blinksNeeded));
                }
                yield return null;
            }

            ctx.Buttons.HideAll();
            yield return ctx.Dialogue.Say(speaker, doneLine);
            ctx.Dialogue.Hide();
        }

        string Label(int count) => $"Blink!\n{count}/{blinksNeeded}";
    }
}
