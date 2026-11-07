using System.Collections;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>The doctor (optionally walks somewhere and) says a few lines while a shot is live.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Dialogue", fileName = "DialogueStep")]
    public sealed class DialogueStep : ProcedureStep
    {
        [SerializeField] string shotId = "";
        [Tooltip("Anchor the doctor walks to first. Empty = stay put.")]
        [SerializeField] string doctorAnchorId = "";
        [Tooltip("Teleport instead of walking (when the camera can't see the doctor).")]
        [SerializeField] bool teleportDoctor;
        [SerializeField] DoctorPose doctorPose = DoctorPose.Idle;
        [Tooltip("What the doctor holds while talking.")]
        [SerializeField] DoctorProp doctorProp = DoctorProp.None;
        [SerializeField] string speaker = "Dr. Iris";
        [SerializeField, TextArea(2, 4)] string[] lines = new string[0];

        public override IEnumerator Run(IProcedureContext ctx)
        {
            var anchor = ctx.GetAnchor(doctorAnchorId);
            if (anchor != null && teleportDoctor) ctx.Doctor.PlaceAt(anchor);

            var shot = ctx.Shots.CutAndWait(shotId);
            if (anchor != null && !teleportDoctor)
            {
                ctx.CoroutineHost.StartCoroutine(shot);
                yield return ctx.Doctor.MoveTo(anchor);
            }
            else
            {
                yield return shot;
            }

            ctx.Doctor.SetProp(doctorProp);
            ctx.Doctor.Play(doctorPose);
            foreach (var line in lines)
                yield return ctx.Dialogue.Say(speaker, line);
            ctx.Dialogue.Hide();
        }

#if UNITY_EDITOR
        public DialogueStep EditorInit(string shot, string anchor, DoctorPose pose, params string[] text)
        {
            shotId = shot;
            doctorAnchorId = anchor;
            doctorPose = pose;
            lines = text;
            return this;
        }

        public DialogueStep EditorProp(DoctorProp prop)
        {
            doctorProp = prop;
            return this;
        }

        public DialogueStep EditorTeleport()
        {
            teleportDoctor = true;
            return this;
        }
#endif
    }
}
