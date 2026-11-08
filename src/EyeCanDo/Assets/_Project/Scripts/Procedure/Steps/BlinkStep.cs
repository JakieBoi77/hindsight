using System.Collections;
using System.Collections.Generic;
using Hindsight.EyeCanDo.UI;
using UnityEngine;

namespace Hindsight.EyeCanDo.Procedure.Steps
{
    /// <summary>Player taps the blink button a few times; each blink clears the blurry vision a little.</summary>
    [CreateAssetMenu(menuName = "EyeCanDo/Steps/Blink", fileName = "BlinkStep")]
    public sealed class BlinkStep : ProcedureStep
    {
        [SerializeField] string shotId = "FirstPerson";
        [SerializeField] StoryLine prompt = new();
        [SerializeField] StoryLine doneLine = new();
        [SerializeField] int blinksNeeded = 3;
        [SerializeField] float startingBlur = 0.8f;
        [Tooltip("Blur left after the last blink: it stays a little blurry for a while.")]
        [SerializeField] float endingBlur = 0.25f;

        public override IEnumerable<StoryLine> Lines => NonEmpty(prompt, doneLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            yield return ctx.Shots.CutAndWait(shotId);
            ctx.Buttons.ShowTap(Label(0));
            yield return SayIfAny(ctx, prompt, waitForNext: false);

            var counted = 0;
            while (counted < blinksNeeded)
            {
                if (ctx.Buttons.Taps > counted)
                {
                    counted++;
                    ctx.Buttons.SetTapLabel(Label(counted));
                    Sfx.Play(SfxId.Blink);
                    yield return ctx.Effects.Blink();
                    ctx.Effects.SetBlur(Mathf.Lerp(startingBlur, endingBlur, (float)counted / blinksNeeded));
                }
                yield return null;
            }

            ctx.Buttons.HideAll();
            yield return SayIfAny(ctx, doneLine);
            ctx.Captions.Hide();
        }

        string Label(int count) => $"{count}/{blinksNeeded}";

#if UNITY_EDITOR
        public BlinkStep EditorInit(StoryLine text, StoryLine done, int blinks)
        {
            prompt = text;
            doneLine = done;
            blinksNeeded = blinks;
            return this;
        }
#endif
    }
}
