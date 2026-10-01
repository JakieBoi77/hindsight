using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    /// <summary>Tap the highlighted object; the child's avatar then appears there (e.g. seated in the chair).</summary>
    public sealed class TapTargetStepView : StepView<TapTargetStepDefinition>
    {
        [SerializeField] private Button target;
        [SerializeField] private IdleMotion targetPulse;
        [SerializeField] private CanvasGroup highlight;
        [SerializeField] private RectTransform avatar;
        [SerializeField] private CanvasGroup avatarGroup;
        [SerializeField] private float avatarDropDistance = 120f;

        protected override async Awaitable RunStepAsync(TapTargetStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            var seatedPosition = avatar.anchoredPosition;
            avatarGroup.alpha = 0f;

            context.Prompt(definition.Prompt);
            targetPulse.enabled = true;
            highlight.alpha = 1f;
            await target.WaitForClickAsync(cancellationToken);
            targetPulse.enabled = false;
            target.interactable = false;
            highlight.alpha = 0f;

            avatar.anchoredPosition = seatedPosition + new Vector2(0f, avatarDropDistance);
            var fade = Tween.FadeAsync(avatarGroup, 1f, 0.3f, EaseType.OutQuad, cancellationToken);
            await Tween.MoveAsync(avatar, seatedPosition, 0.5f, EaseType.OutBounce, cancellationToken);
            await fade;
            context.PlaySuccess();

            await context.SayAllAsync(definition.SuccessLines, cancellationToken);
        }
    }
}
