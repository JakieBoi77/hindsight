using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// The doctor looks into the eye with a light. She leans in from where she stands, holds the light
    /// up to her own eye and looks at a few spots of the child's eye, and the light flashes. The child
    /// just watches (or, with <see cref="tapToFollow"/>, taps a floating light at each spot instead).
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Exam", fileName = "ExamStep")]
    public sealed class ExamStep : ProcedureStep
    {
        [SerializeField] string shotId = "ExamCloseFP";
        [SerializeField] string doctorAnchorId = "DoctorExamPoint";
        [Tooltip("Where the child's eyes are (the camera); the doctor leans in toward them.")]
        [SerializeField] string eyesAnchorId = "ChairEyes";
        [Tooltip("How close the doctor's face comes to the child's eye, in metres.")]
        [SerializeField] float standoff = 0.42f;
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
            if (target != null) scope.Show(true);
            else scope.ShowBeam(true);

            // She leans in and holds the light up to her eye, steady, looking into the child's eye.
            var eyes = ctx.GetAnchor(eyesAnchorId);
            if (eyes != null) ctx.Doctor.Examine(eyes.position, standoff);
            ctx.CoroutineHost.StartCoroutine(SayIfAny(ctx, introLine, waitForNext: false));
            for (var t = 0f; t < 1.2f; t += Time.deltaTime) yield return null;

            var tapped = false;
            void OnTap() => tapped = true;
            if (target != null) target.Tapped += OnTap;
            for (var i = 0; i < scope.PointCount; i++)
            {
                if (target == null)
                {
                    for (var t = 0f; t < lookSeconds; t += Time.deltaTime) yield return null;
                    continue;
                }
                yield return scope.MoveToPoint(i);
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
            ctx.Doctor.FinishTask();
            for (var t = 0f; t < 0.5f; t += Time.deltaTime) yield return null;
            ctx.Doctor.SetProp(DoctorProp.None);
            ctx.Doctor.Play(DoctorPose.Cheer);
            yield return SayIfAny(ctx, doneLine);
            ctx.Captions.Hide();
        }

#if UNITY_EDITOR
        public ExamStep EditorInit(StoryLine intro, StoryLine done, float faceDistance = 0.42f)
        {
            standoff = faceDistance;
            introLine = intro;
            doneLine = done;
            return this;
        }
#endif
    }
}
