using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// The doctor looks into the eye with a light: it moves over a few spots and flashes. The child
    /// just watches (or, with <see cref="tapToFollow"/>, taps the light at each spot).
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Exam", fileName = "ExamStep")]
    public sealed class ExamStep : ProcedureStep
    {
        [SerializeField] string shotId = "ExamCloseFP";
        [SerializeField] string doctorAnchorId = "DoctorExamPoint";
        [SerializeField] StoryLine introLine = new();
        [SerializeField] StoryLine doneLine = new();
        [SerializeField] bool tapToFollow;
        [Tooltip("How long the light rests on each spot when the child only watches.")]
        [SerializeField] float lookSeconds = 0.9f;

        public override IEnumerable<StoryLine> Lines => NonEmpty(introLine, doneLine);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Effects.SetBlur(0f);
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            yield return ctx.Shots.CutAndWait(shotId);

            var scope = ctx.Ophthalmoscope;
            var target = tapToFollow ? scope.TapTarget : null;
            scope.Show(true);
            ctx.Doctor.SetProp(DoctorProp.Light);
            ctx.Doctor.Play(DoctorPose.Interact);
            ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, introLine, waitForNext: false));

            var tapped = false;
            void OnTap() => tapped = true;
            if (target != null) target.Tapped += OnTap;
            for (var i = 0; i < scope.PointCount; i++)
            {
                yield return scope.MoveToPoint(i);
                if (target == null)
                {
                    for (var t = 0f; t < lookSeconds; t += Time.deltaTime) yield return null;
                    continue;
                }
                tapped = false;
                target.SetAccepting(true);
                while (!tapped) yield return null;
                target.SetAccepting(false);
                Sfx.Play(SfxId.Pop);
            }
            if (target != null) target.Tapped -= OnTap;

            yield return scope.Flash(1.2f);
            while (ctx.Captions.Narrator.IsSpeaking) yield return null;
            scope.Show(false);
            ctx.Doctor.SetProp(DoctorProp.None);
            ctx.Doctor.Play(DoctorPose.Cheer);
            yield return SayIfAny(ctx, doneLine);
            ctx.Captions.Hide();
        }

#if UNITY_EDITOR
        public ExamStep EditorInit(StoryLine intro, StoryLine done)
        {
            introLine = intro;
            doneLine = done;
            return this;
        }
#endif
    }
}
