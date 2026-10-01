using System.Threading;
using Hindsight.Core.Animation;
using Hindsight.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Hindsight.Procedures.Steps
{
    public sealed class DilationDemoStepView : StepView<DilationDemoStepDefinition>
    {
        [SerializeField] private EyeView eye;
        [SerializeField] private Button dropperButton;
        [SerializeField] private IdleMotion dropperPulse;
        [SerializeField] private GameObject tapHint;
        [SerializeField] private RectTransform drop;
        [SerializeField] private RectTransform dropTarget;
        [SerializeField] private CanvasGroup brightLightGlow;

        protected override async Awaitable RunStepAsync(DilationDemoStepDefinition definition, StepContext context, CancellationToken cancellationToken)
        {
            eye.SetPupilScale(definition.StartPupilScale);
            drop.gameObject.SetActive(false);
            brightLightGlow.alpha = 0f;

            context.Prompt(definition.TapPrompt);
            dropperPulse.enabled = true;
            await dropperButton.WaitForClickAsync(cancellationToken);
            dropperPulse.enabled = false;
            dropperButton.interactable = false;
            tapHint.SetActive(false);

            await DropFallsAsync(cancellationToken);
            await eye.DilateAsync(definition.EndPupilScale, definition.DilationSeconds, cancellationToken);
            context.PlaySuccess();

            // Dilated eyes are sensitive to light; the glow previews the "things look bright" line.
            Tween.FadeAsync(brightLightGlow, 0.85f, 1.2f, EaseType.InOutQuad, cancellationToken).Forget();
            await context.SayAllAsync(definition.ExplanationLines, cancellationToken);
        }

        private async Awaitable DropFallsAsync(CancellationToken cancellationToken)
        {
            var start = drop.anchoredPosition;
            drop.gameObject.SetActive(true);
            await Tween.MoveAsync(drop, dropTarget.anchoredPosition, 0.6f, EaseType.InQuad, cancellationToken);
            drop.gameObject.SetActive(false);
            drop.anchoredPosition = start;
            await Tween.PunchScaleAsync(eye.transform, 0.05f, 0.25f, cancellationToken);
        }
    }
}
