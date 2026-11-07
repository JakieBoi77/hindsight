using System.Collections;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>Celebrates, records completion, and leaves the reward screen up until the player continues.</summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Reward", fileName = "RewardStep")]
    public sealed class RewardStep : ProcedureStep
    {
        [SerializeField] string shotId = "Reward";
        [SerializeField] string doctorAnchorId = "DoctorStandPoint";
        [SerializeField] string rewardTitle = "Eye Drop Champion!";
        [SerializeField, TextArea(2, 3)] string rewardBody = "You held still, kept your eyes open, and blinked like a pro.";

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Effects.SetBlur(0f);
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            yield return ctx.Shots.CutAndWait(shotId);
            ctx.Doctor.SetProp(DoctorProp.None);
            ctx.Doctor.Play(DoctorPose.Cheer);
            LevelProgress.MarkCompleted(ctx.Level);
            yield return ctx.Reward.Show(rewardTitle, rewardBody);
            // The reward panel's button leaves the scene; nothing else to wait for.
        }
    }
}
