using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using Hindsight.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class HoldStillStepView : StepView<HoldStillStepDefinition>
    {
        [SerializeField] private CanvasGroup tiltDemo;
        [SerializeField] private CanvasGroup eyeScene;
        [SerializeField] private EyeView eye;
        [SerializeField] private RectTransform drop;
        [SerializeField] private RectTransform dropStart;
        [SerializeField] private RectTransform dropTarget;
        [SerializeField] private RectTransform splashTarget;
        [SerializeField] private HoldButton holdButton;
        [SerializeField] private Image holdRing;
        [SerializeField] private IdleMotion holdButtonPulse;
        [SerializeField] private RectTransform sensationBurst;
        [SerializeField] private TMP_Text sensationLabel;

        protected override async Awaitable RunStepAsync(HoldStillStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            holdButton.Interactable = false;
            holdButton.gameObject.SetActive(false);
            sensationBurst.gameObject.SetActive(false);
            eyeScene.alpha = 0f;
            tiltDemo.alpha = 1f;

            await context.SayAllAsync(definition.IntroLines, cancellationToken);

            await Tween.FadeAsync(tiltDemo, 0f, 0.25f, EaseType.OutQuad, cancellationToken);
            await Tween.FadeAsync(eyeScene, 1f, 0.25f, EaseType.OutQuad, cancellationToken);
            ResetDrop();
            holdButton.gameObject.SetActive(true);
            holdButtonPulse.enabled = true;
            context.Prompt(definition.HoldPrompt);

            await HoldInteraction.RunAsync(
                holdButton,
                definition.HoldSeconds,
                ShowHoldProgress,
                token => OnBlinkedAsync(definition, context, token),
                cancellationToken);

            holdButtonPulse.enabled = false;
            holdButton.gameObject.SetActive(false);
            drop.gameObject.SetActive(false);
            context.PlaySuccess();
            await ShowSensationAsync(definition.SensationCaption, cancellationToken);
            await context.SayAllAsync(definition.SuccessLines, cancellationToken);
        }

        private void ShowHoldProgress(float progress)
        {
            holdRing.fillAmount = progress;
            drop.anchoredPosition = Vector2.Lerp(dropStart.anchoredPosition, dropTarget.anchoredPosition, progress);
        }

        private async Awaitable OnBlinkedAsync(HoldStillStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            context.PlayGentleError();
            await eye.CloseAsync(cancellationToken);
            await Tween.MoveAsync(drop, splashTarget.anchoredPosition, 0.2f, EaseType.InQuad, cancellationToken);
            drop.gameObject.SetActive(false);

            await context.SayAsync(definition.InterruptedLine, cancellationToken);

            await eye.OpenAsync(cancellationToken);
            ResetDrop();
            context.Prompt(definition.HoldPrompt);
        }

        private void ResetDrop()
        {
            drop.gameObject.SetActive(true);
            drop.anchoredPosition = dropStart.anchoredPosition;
            holdRing.fillAmount = 0f;
        }

        private async Awaitable ShowSensationAsync(string caption, CancellationToken cancellationToken)
        {
            sensationLabel.text = caption;
            sensationBurst.gameObject.SetActive(true);
            sensationBurst.localScale = Vector3.zero;
            await Tween.ScaleAsync(sensationBurst, Vector3.one, 0.35f, EaseType.OutBack, cancellationToken);
            await eye.BlinkAsync(cancellationToken);
        }
    }
}
