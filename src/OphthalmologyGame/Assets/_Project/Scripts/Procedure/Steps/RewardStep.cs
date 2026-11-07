using System.Collections;
using System.Collections.Generic;
using Hindsight.Ophthalmology.Core;
using Hindsight.Ophthalmology.SceneObjects;
using Hindsight.Ophthalmology.UI;
using UnityEngine;

namespace Hindsight.Ophthalmology.Procedure.Steps
{
    /// <summary>
    /// Celebrates with the player's own kid beside the doctor: both dance, the sticker pops up, the
    /// coins fly into the purse, then Home and Play again appear. Nothing to tap until then.
    /// </summary>
    [CreateAssetMenu(menuName = "Ophthalmology/Steps/Reward", fileName = "RewardStep")]
    public sealed class RewardStep : ProcedureStep
    {
        [SerializeField] string shotId = "Reward";
        [SerializeField] string doctorAnchorId = "DoctorStandPoint";
        [SerializeField] string kidAnchorId = "KidRewardPoint";
        [SerializeField] string title = "Brave Eyes!";
        [Tooltip("Optional line said while the sticker appears.")]
        [SerializeField] StoryLine line = new();

        public override IEnumerable<StoryLine> Lines => NonEmpty(line);

        public override IEnumerator Run(IProcedureContext ctx)
        {
            ctx.Effects.SetBlur(0f);
            ctx.Doctor.PlaceAt(ctx.GetAnchor(doctorAnchorId));
            if (ctx.PlayerKid != null) ctx.PlayerKid.PlaceAt(ctx.GetAnchor(kidAnchorId));
            yield return ctx.Shots.CutAndWait(shotId);
            ctx.Doctor.SetProp(DoctorProp.None);
            ctx.Doctor.Play(DoctorPose.Cheer);
            if (ctx.PlayerKid != null) ctx.CoroutineHost.StartCoroutine(ctx.PlayerKid.Dance(3.5f));
            Sfx.Play(SfxId.Reward);

            if (!line.IsEmpty) ctx.CoroutineHost.StartCoroutine(ctx.Captions.Speak(line));
            yield return ctx.Reward.ShowSticker(title);
            var coins = GameServices.FinishLevel(ctx.Level);
            yield return ctx.Reward.ShowCoins(coins);
            yield return ctx.Reward.ShowEndButtons();
            // The end card's buttons leave the scene; nothing else to wait for.
        }

#if UNITY_EDITOR
        public RewardStep EditorInit(string stickerTitle, StoryLine text = null)
        {
            title = stickerTitle;
            line = text ?? new StoryLine();
            return this;
        }
#endif
    }
}
