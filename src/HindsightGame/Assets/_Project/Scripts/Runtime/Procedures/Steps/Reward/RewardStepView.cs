using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using Hindsight.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class RewardStepView : StepView<RewardStepDefinition>
    {
        [SerializeField] private TMP_Text headline;
        [SerializeField] private Image sticker;
        [SerializeField] private RectTransform stickerRoot;
        [SerializeField] private UiConfetti confetti;
        [SerializeField] private Button finishButton;
        [SerializeField] private TMP_Text finishLabel;

        protected override async Awaitable RunStepAsync(RewardStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            headline.text = definition.Headline;
            finishLabel.text = definition.FinishLabel;
            sticker.sprite = context.Procedure.StickerSprite;
            sticker.enabled = sticker.sprite != null;
            finishButton.gameObject.SetActive(false);
            stickerRoot.localScale = Vector3.zero;

            context.SetDoctorPose(DoctorPose.Cheer);
            confetti.Burst();
            context.PlaySuccess();
            await Tween.ScaleAsync(stickerRoot, Vector3.one, 0.6f, EaseType.OutBack, cancellationToken);
            context.Prompt(definition.CelebrationLine);

            // Listen before animating so an eager first tap is never lost.
            finishButton.gameObject.SetActive(true);
            var finished = finishButton.WaitForClickAsync(cancellationToken);
            Tween.PunchScaleAsync(finishButton.transform, 0.15f, 0.35f, cancellationToken).Forget();
            await finished;
        }
    }
}
