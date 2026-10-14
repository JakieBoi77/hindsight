using System.Collections;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>The doctor looks into the eye with a light. The player "follows the light" by tapping it.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Exam", fileName = "ExamStep")]
    public sealed class ExamStep : ProcedureStep
    {
        [SerializeField] string shotId = "ExamCloseFP";
        [SerializeField] string doctorAnchorId = "DoctorExamPoint";
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField, TextArea(2, 3)] string introLine = "Now I'll use my special light to look inside your eye. It's bright, but it doesn't hurt one bit.";
        [SerializeField, TextArea(2, 3)] string followPrompt = "Follow the light with your eyes - tap it every time it moves!";
        [SerializeField, TextArea(2, 3)] string lookingLine = "Hold still... I'm looking all the way inside...";
        [SerializeField, TextArea(2, 3)] string doneLine = "Wow! Your eyes look super healthy. Great job!";

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            yield return ctx.Shots.CutAndWait(shotId);
            yield return ctx.Dialogue.Say(speaker, introLine);

            var scope = ctx.Ophthalmoscope;
            var target = scope.TapTarget;
            scope.Show(true);
            ctx.Doctor.SetProp(DoctorProp.Light);
            ctx.Doctor.Play(DoctorPose.Interact);
            yield return ctx.Dialogue.Say(speaker, followPrompt, waitForNext: false);

            var tapped = false;
            void OnTap() => tapped = true;
            if (target != null) target.Tapped += OnTap;
            for (var i = 0; i < scope.PointCount; i++)
            {
                yield return scope.MoveToPoint(i);
                if (target == null) continue;
                tapped = false;
                target.SetAccepting(true);
                while (!tapped) yield return null;
                target.SetAccepting(false);
            }
            if (target != null) target.Tapped -= OnTap;

            ctx.CoroutineHost.StartCoroutine(ctx.Dialogue.Say(speaker, lookingLine, waitForNext: false));
            yield return scope.Flash(1.5f);
            scope.Show(false);
            ctx.Doctor.SetProp(DoctorProp.None);
            ctx.Doctor.Play(DoctorPose.Cheer);
            yield return ctx.Dialogue.Say(speaker, doneLine);
            ctx.Dialogue.Hide();
        }
    }
}
