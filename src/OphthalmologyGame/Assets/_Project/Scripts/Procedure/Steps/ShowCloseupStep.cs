using System.Collections;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Fades to the giant model eye and shows the pupil getting bigger.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Show Closeup", fileName = "ShowCloseupStep")]
    public sealed class ShowCloseupStep : ProcedureStep
    {
        [SerializeField] string closeupShotId = "EyeCloseup";
        [SerializeField] string returnShotId = "DoctorExplain";
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField, TextArea(2, 4)] string beforeLine = "";
        [SerializeField, TextArea(2, 4)] string afterLine = "";
        [SerializeField] float dilateSeconds = 2.5f;

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.EyeCloseup.ResetPupil();
            yield return ctx.Effects.FadeOut();
            ctx.Shots.Cut(closeupShotId, instant: true);
            yield return ctx.Effects.FadeIn();

            yield return ctx.Dialogue.Say(speaker, beforeLine);
            var dilation = ctx.CoroutineHost.StartCoroutine(ctx.EyeCloseup.Dilate(dilateSeconds));
            yield return ctx.Dialogue.Say(speaker, afterLine);
            yield return dilation;
            ctx.Dialogue.Hide();

            yield return ctx.Effects.FadeOut();
            ctx.Shots.Cut(returnShotId, instant: true);
            yield return ctx.Effects.FadeIn();
        }

#if UNITY_EDITOR
        public ShowCloseupStep EditorInit(string before, string after, string returnShot)
        {
            beforeLine = before;
            afterLine = after;
            returnShotId = returnShot;
            return this;
        }
#endif
    }
}
